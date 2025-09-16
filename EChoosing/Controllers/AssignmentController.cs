using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.ComponentModel.DataAnnotations;

namespace EChoosing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController : ControllerBase
    {

        private readonly IExamService _examService;
        private readonly IClassService _classService;
        private readonly IAccountService _accountService;
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(
            IExamService examService,
            IClassService classService,
            IAccountService accountService,
            IAssignmentService assignmentService)
        {
            _examService = examService;
            _classService = classService;
            _accountService = accountService;
            _assignmentService = assignmentService;
        }

        [HttpGet("init")]
        public IActionResult GetInitialData()
        {
            string userId = _accountService.GetUserIDLogin(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "Login required." });

            string msg = _examService.GetListExamByID(userId, out List<Exam> exams);
            if (msg.Length > 0)
                return BadRequest(new { message = "Get List Exam Error: " + msg });

            msg = _classService.GetListClassByCreateBy(userId, out List<Class> classes);
            if (msg.Length > 0)
                return BadRequest(new { message = "Get List Class Error: " + msg });

            var result = new
            {
                Exams = exams.Select(e => new { e.ExamId, e.ExamName }),
                Classes = classes.Select(c => new { c.ClassId, c.ClassName })
            };

            return Ok(result);
        }

        //[HttpGet("init-exam")]
        //public IActionResult GetInitialDataExam()
        //{
        //    string userId = _accountService.GetUserIDLogin(HttpContext);
        //    if (userId == null)
        //        return Unauthorized(new { message = "Login required." });

        //    string msg = _examService.GetListExamByID(userId, out List<Exam> exams);
        //    if (!string.IsNullOrEmpty(msg))
        //        return BadRequest(new { message = "Get List Exam Error: " + msg });

        //    // Convert Exam to DTO
        //    var result = exams.Select(e => new ExamSummaryDto
        //    {
        //        ExamId = e.ExamId,
        //        ExamName = e.ExamName
        //    }).ToList();

        //    return Ok(result);
        //}

        //[HttpGet("init-class")]
        //public IActionResult GetInitialDataClass()
        //{
        //    string userId = _accountService.GetUserIDLogin(HttpContext);
        //    if (userId == null)
        //        return Unauthorized(new { message = "Login required." });

        //    string msg = _classService.GetListClassByCreateBy(userId, out List<Class> classes);
        //    if (!string.IsNullOrEmpty(msg))
        //        return BadRequest(new { message = "Get List Exam Error: " + msg });

        //    // Convert Exam to DTO
        //    var result = classes.Select(e => new ClassSummaryDto
        //    {
        //        ClassId = e.ClassId,
        //        ClassName = e.ClassName
        //    }).ToList();

        //    return Ok(result);
        //}


        [HttpPost]
        public IActionResult CreateAssignment([FromBody] AssignmentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string msg = _examService.GetExamByID(request.SelectedQuizId, out Exam exam);
            if (msg.Length > 0)
                return BadRequest(new { message = "Get Exam By ID Error: " + msg });

            exam.Description = request.Description;
            exam.IsRetake = request.AllowRetake;

            msg = _examService.UpdateExam(exam);
            if (msg.Length > 0)
                return BadRequest(new { message = "Update Exam Error: " + msg });

            string userId = _accountService.GetUserIDLogin(HttpContext);
            if (userId == null)
                return Unauthorized(new { message = "Login required." });

            foreach (var classId in request.SelectedClassIds)
            {
                var assignment = new ClassExam
                {
                    ClassExamId = Guid.NewGuid().ToString(),
                    ClassId = classId,
                    ExamId = request.SelectedQuizId,
                    CreateUser = userId,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    IsDelete = false
                };

                msg = _assignmentService.CreateAssignment(assignment);
                if (msg.Length > 0)
                    return BadRequest(new { message = "Create Assignment Error: " + msg });
            }

            return Ok(new { message = "Assignment created successfully." });
        }

    }

    public class AssignmentRequest
    {
        [Required]
        public int SelectedQuizId { get; set; }

        [Required]
        public List<string> SelectedClassIds { get; set; }

        public string Description { get; set; }

        public bool AllowRetake { get; set; }
    }

    public class ExamSummaryDto
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = null!;
    }


}
