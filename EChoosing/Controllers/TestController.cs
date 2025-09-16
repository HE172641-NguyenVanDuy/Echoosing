using BusinessObjects.Models;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.ViewModels;
using System.Data;
using System.Security.Claims;
using static EChoosing.Controllers.ExamController;

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
        private readonly EchoosingContext _context;

        public TestController(EchoosingContext _context, ITestService testService, IExamService examService, IAccountService accountService, IClassService classService, IConverter converter, IExportPDFservice _exporter)
        {
            _testService = testService;
            _examService = examService;
            _accountService = accountService;
            _classService = classService;
            _converter = converter;
            _exporter = _exporter;
            this._context = _context;
        }


        [HttpGet("by-class/{classID}")]
        public IActionResult GetExamsByClass(string classID)
        {
            string role = _accountService.GetUserRoleLogin(HttpContext);

            string msg = _examService.GetListExamByClassExamID(classID, out List<ClassExam> classExams);
            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { error = msg });
            }

            // Map dữ liệu
            var result = classExams.Select(ce => new ExamResponseByClassDto
            {
                ClassExamId = ce.ClassExamId,
                ClassId = ce.ClassId,
                ExamId = ce.ExamId,
                ExamName = ce.Exam?.ExamName,
                TimeStart = ce.Exam?.TimeStart,
                Duration = ce.Exam?.Duration
            }).ToList();

            return Ok(new
            {
                Role = role,
                ClassID = classID,
                Exams = result
            });
        }
        [HttpGet("GetExamCode/{examID}")]
        public IActionResult GetCodeExam(int examID)
        {
            var msg = _context.Exams.FirstOrDefault(a => a.ExamId == examID);
            var msga = _context.ExamCodes.FirstOrDefault(a => a.ExamId == examID);
            if (msg != null)
            {
                return Ok(new
                {
                    examName = msg.ExamName,
                    timeStart = msg.TimeStart,
                    examCode = msga.Code
                });
            }
            return null;
        }

        [HttpGet("validate/{examcode}")]
        public IActionResult ValidateExamCode(string examcode)
        {
            string msg = "";
            var code = _context.ExamCodes.Where(a => a.Code.Equals(examcode));
            if (!code.Any())
            {
                return BadRequest(msg);
            }
            return Ok();
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
        // EChoosing\Controllers\TestController.cs

        [HttpGet("{attemptId}")]
        public IActionResult GetTestResult(string attemptId)
        {
            string userId = _accountService.GetUserIDLogin(HttpContext);

            if (!string.IsNullOrEmpty(userId))
            {
                string msg = _testService.GetResultTest(attemptId, out ResultTestVMa result);

                if (result != null) // Check if the main 'result' object is null
                {
                    // --- Crucial Null Checks for User Case ---
                    if (result.exam == null)
                    {
                        // Log this for debugging purposes
                        Console.WriteLine($"WARNING: result.exam is null for attemptId: {attemptId}, userId: {userId}");
                        return BadRequest("Exam data is missing in the test result (result.exam is null).");
                    }
                    if (result.examAttempt == null)
                    {
                        Console.WriteLine($"WARNING: result.examAttempt is null for attemptId: {attemptId}, userId: {userId}");
                        return BadRequest("Exam Attempt data is missing in the test result (result.examAttempt is null).");
                    }
                    if (result.examAttempt.Exam == null) // This is the most common place for the NRE
                    {
                        Console.WriteLine($"WARNING: result.examAttempt.Exam is null for attemptId: {attemptId}, userId: {userId}");
                        return BadRequest("Exam data within Exam Attempt is missing (result.examAttempt.Exam is null).");
                    }
                    // --- End of Crucial Null Checks ---

                    var resultDto = new ResultTestDto
                    {
                        NumberCorrect = result.numberCorrect,
                        NumberQuestion = result.numberQuestion,
                        OptionResult = result.optionResult, // Ensure OptionResult and its contents are not null

                        Exam = new ExamDtoa
                        {
                            Id = result.exam.ExamId,
                            Name = result.exam.ExamName
                        },

                        ExamAttempt = new ExamAttemptDtoa
                        {
                            // Ensure result.examAttempt.AttemptId is not null or empty before parsing
                            AttemptId = result.examAttempt.AttemptId, // <--- DIRECT ASSIGNMENT (no int.Parse)
                                                                      // Use "0" as fallback for safety
                            Exam = new ExamDtoa
                            {
                                Id = result.examAttempt.Exam.ExamId,
                                Name = result.examAttempt.Exam.ExamName
                            }
                        }
                    };
                    return Ok(resultDto);
                }
                else // result itself was null
                {
                    Console.WriteLine($"INFO: No ResultTestVM found for attemptId: {attemptId}, userId: {userId}");
                    return NotFound($"Result for attempt {attemptId} not found for user {userId}.");
                }
            }
            return BadRequest();
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

        public class ExamResponseByClassDto
        {
            public string ClassExamId { get; set; } = null!;
            public string? ClassId { get; set; }
            public int? ExamId { get; set; }
            public string? ExamName { get; set; }
            public DateTime? TimeStart { get; set; }
            public int? Duration { get; set; }
        }

    }
