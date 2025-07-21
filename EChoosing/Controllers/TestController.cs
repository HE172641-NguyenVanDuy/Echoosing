using BusinessObjects.Models;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.ViewModels;
using System.Security.Claims;

namespace EChoosing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly IExamService _examService;
        private readonly IAccountService _accountService;
        private readonly IClassService _classService;
        private readonly IConverter _converter;
        private readonly IExportPDFservice _exporter;

        public TestController(ITestService testService, IExamService examService, IAccountService accountService, IClassService classService, IConverter converter, IExportPDFservice _exporter)
        {
            _testService = testService;
            _examService = examService;
            _accountService = accountService;
            _classService = classService;
            _converter = converter;
            _exporter = _exporter;
        }

        [HttpGet("by-class/{classID}")]
        public IActionResult GetExamsByClass(string classID)
        {
            string role = _accountService.GetUserRoleLogin(HttpContext);

            string msg = _examService.GetListExamByClassID(classID, out List<Exam> exams);
            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { error = msg });
            }

            return Ok(new
            {
                Role = role,
                ClassID = classID,
                Exams = exams
            });
        }

        // GET: api/test/do?examId=1&classId=abc123&username=duy&examCode=XYZ
        [HttpGet("do")]
        public IActionResult LoadExam([FromQuery] int examId, [FromQuery] string classId,
                                      [FromQuery] string? username, [FromQuery] string? examCode)
        {
            string actualUsername = username ?? User.FindFirst(ClaimTypes.Name)?.Value;
            DateTime timeStart = DateTime.Now;

            var error = _testService.LoadExamData(examId, out int duration, out List<QuestionViewModel> questions);
            if (!string.IsNullOrEmpty(error))
            {
                if (error == "Not yet the test date")
                {
                    return BadRequest(new { message = "It's not yet the day of the test!" });
                }
                return BadRequest(new { message = error });
            }

            string examName = _examService.GetExamNameById(examId);
            string className = _classService.GetClassNameById(classId);

            return Ok(new
            {
                ExamID = examId,
                ClassID = classId,
                ExamName = examName,
                ClassName = className,
                Duration = duration,
                Questions = questions,
                Username = actualUsername,
                Code = examCode,
                TimeStart = timeStart
            });
        }

        // POST: api/test/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTest([FromBody] SubmitExamRequest request)
        {
            if (request == null || request.UserAnswers == null || request.ExamID <= 0)
                return BadRequest(new { message = "Invalid data" });

            string userId = _accountService.GetUserIDLogin(HttpContext);
            string error = _testService.SubmitExam(request.UserAnswers, userId, request.ExamID,
                                                   request.Status, request.Code, request.Username,
                                                   request.ClassID, out string attemptId, request.TimeStart);

            if (!string.IsNullOrEmpty(error))
            {
                return BadRequest(new { message = error });
            }

            return Ok(new { attemptId });
        }

        // GET: /api/test-result/{attemptId}
        [HttpGet("{attemptId}")]
        public IActionResult GetTestResult(string attemptId)
        {
            string userId = _accountService.GetUserIDLogin(HttpContext);

            if (!string.IsNullOrEmpty(userId))
            {
                string msg = _testService.GetResultTest(attemptId, out ResultTestVM result);
                if (result != null)
                    return Ok(result);
            }
            else
            {
                string msg = _testService.GetResultTestGuest(attemptId, out ExamAttempt result);
                if (result != null)
                    return Ok(result);
            }

            return NotFound("Không tìm thấy kết quả làm bài.");
        }

        // POST: /api/test-result/{attemptId}/export
        [HttpPost("{attemptId}/export")]
        public IActionResult ExportPdfResult(string attemptId)
        {
            string userId = _accountService.GetUserIDLogin(HttpContext);

            if (!string.IsNullOrEmpty(userId))
            {
                string msg = _testService.GetResultTest(attemptId, out ResultTestVM result);
                if (result != null)
                {
                    _exporter.ExportPDF(result, out byte[] file);
                    return File(file, "application/pdf", "ResultExam.pdf");
                }
            }

            return Unauthorized("Khách không được phép xuất file PDF.");
        }

    }

    public class SubmitExamRequest
    {
        public int ExamID { get; set; }
        public string? ClassID { get; set; }
        public int Status { get; set; }
        public string? Code { get; set; }
        public string? Username { get; set; }
        public DateTime TimeStart { get; set; }
        public List<UserAnswerModel> UserAnswers { get; set; } = new();
    }
}
