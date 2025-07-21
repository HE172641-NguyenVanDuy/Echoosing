using BusinessObjects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;
using Services.ViewModels;

namespace EChoosing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassController : ControllerBase
    {
        private readonly EchoosingContext _context;
        private readonly IClassService _classService;
        private readonly IAccountService _accountService;
        private readonly ICodeJoinClassService _codeJoinClassService;
        private readonly IExamService _examService;


        public ClassController(EchoosingContext context, IClassService classService, IAccountService accountService, ICodeJoinClassService codeJoinClassService, IExamService examService)
        {
            _context = context;
            _classService = classService;
            _accountService = accountService;
            _codeJoinClassService = codeJoinClassService;
            _examService = examService;
        }

        [Authorize(Policy = "TeacherOnly")]
        [HttpPost("create")]
        public IActionResult CreateClass([FromBody] CreateClassRequest request)
        {
            string userID = _accountService.GetUserIDLogin(HttpContext);

            if (userID == null)
            {
                return Unauthorized(new { message = "Please login again." });
            }

            _classService.CreateClass(request.ClassName, userID, out string classID);
            return Ok(new { message = "Class created successfully", classID });
        }

        [HttpGet("list")]
        public IActionResult GetClassList()
        {
            string userRole = _accountService.GetUserRoleLogin(HttpContext);
            string userId = _accountService.GetUserIDLogin(HttpContext);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
            {
                return Unauthorized(new { message = "Please login again." });
            }

            if (userRole != "5") // Student or others
            {
                var msg = _classService.GetClassForStudent(userId, out Dictionary<Class, List<ClassExam>> values);
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(new { message = msg });
                }

                // Convert Dictionary<Class, List<ClassExam>> to a DTO if necessary
                var result = values.Select(kv => new
                {
                    Class = kv.Key,
                    Exams = kv.Value
                });

                return Ok(new
                {
                    role = userRole,
                    userId = userId,
                    classExams = result
                });
            }
            else // Role 5: creator/admin
            {
                var msg = _classService.GetListClassByCreateBy(userId, out List<Class> listClass);
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(new { message = msg });
                }

                return Ok(new
                {
                    role = userRole,
                    userId = userId,
                    classes = listClass
                });
            }
        }
        // GET: api/class-management/{classId}
        [HttpGet("get-class-info/{classId}")]
        public IActionResult GetClassInfo(string classId)
        {
            classId = classId?.Trim();
            string msg = _classService.GetClass(classId, out Class classInfo);

            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = msg });

            msg = _classService.GetUserInClass(classInfo, out List<User> users);
            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = msg });

            string role = _accountService.GetUserRoleLogin(HttpContext);

            return Ok(new
            {
                classInfo,
                users,
                userRole = role
            });
        }

        [Authorize(Policy = "TeacherOnly")]
        // DELETE: api/class-management/{classId}/students/{studentId}
        [HttpDelete("{classId}/students/{studentId}")]
        public IActionResult RemoveStudentFromClass(string classId, string studentId)
        {
            string msg = _classService.RemoveStudent(classId, studentId, out string classID);
            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = msg, classID });

            return Ok(new { message = "Student removed successfully", classID });
        }

        [Authorize(Policy = "TeacherOnly")]
        // PUT: api/class-management/{classId}/rename
        [HttpPut("{classId}/rename")]
        public IActionResult RenameClass(string classId, [FromBody] RenameClassRequest request)
        {
            string msg = _classService.RenameClass(classId, request.NewName, out string classID);
            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = msg, classID });

            return Ok(new { message = "Class renamed successfully", classID });
        }

        [Authorize(Policy = "TeacherOnly")]
        // DELETE: api/class-management/{classId}
        [HttpDelete("delete/{classId}")]
        public IActionResult DeleteClass(string classId)
        {
            string msg = _classService.DeleteClass(classId);
            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = msg });

            return Ok(new { message = "Class deleted successfully" });
        }

        [HttpGet("selects")]
        public async Task<IActionResult> GetDropdownOptions()
        {
            var classIds = await _context.Classes
                .Select(c => new { c.ClassId })
                .ToListAsync();

            var userIds = await _context.Users
                .Select(u => new { u.UserId })
                .ToListAsync();

            return Ok(new
            {
                classIds,
                userIds
            });
        }

        // POST: api/join-class
        [HttpPost("join-class")]
        public async Task<IActionResult> JoinClass([FromBody] JoinClassDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.ClassId))
            {
                return BadRequest(new { message = "Invalid input data." });
            }

            // Optional: Validate ClassCode logic here if needed

            var classUser = new ClassUser
            {
                ClassId = request.ClassId,
                UserId = request.UserId,
                // Thêm các field khác nếu cần
            };

            _context.ClassUsers.Add(classUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Joined class successfully." });
        }

        [HttpGet("/statistic/{classId}")]
        public IActionResult GetStatisticsForClass(string classId)
        {
            // 1. Get class info
            var msg = _classService.GetClass(classId, out Class classInfo);
            if (!string.IsNullOrEmpty(msg)) return BadRequest(new { message = "Get class error", detail = msg });

            // 2. Get students in class
            msg = _classService.GetUserInClass(classInfo, out List<User> users);
            if (!string.IsNullOrEmpty(msg)) return BadRequest(new { message = "Get student list error", detail = msg });

            // 3. Get exam IDs
            msg = _classService.GetListExamIdInClassByClassId(classId, out List<int> examIds);
            if (!string.IsNullOrEmpty(msg) || examIds == null)
                return BadRequest(new { message = "Get exam ID list error", detail = msg });

            // 4. Get exam details
            msg = _examService.GetListExamByListExamID(examIds, out List<Exam> exams);
            if (!string.IsNullOrEmpty(msg)) return BadRequest(new { message = "Get exams error", detail = msg });

            // 5. Get exam attempts (scores)
            msg = _classService.GetScoresForClass(classId, examIds, out Dictionary<int, List<ExamAttempt>> examAttempts);
            if (!string.IsNullOrEmpty(msg)) return BadRequest(new { message = "Get exam scores error", detail = msg });

            // 6. Get exam status list
            msg = _examService.GetListStatusExam(out List<Systemkey> listStatusExam);
            if (!string.IsNullOrEmpty(msg)) return BadRequest(new { message = "Get exam status error", detail = msg });

            // Convert listStatusExam to dictionary for lookup
            var statusLookup = listStatusExam.ToDictionary(s => s.Id, s => s.Description);

            return Ok(new
            {
                classInfo,
                students = users,
                exams,
                examAttempts,
                examStatusLookup = statusLookup
            });
        }
    }

    public class JoinClassDto
    {
        public string UserId { get; set; } = default!;
        public string ClassId { get; set; } = default!;
        public string? ClassCode { get; set; } // Nếu cần xử lý mã lớp riêng
    }


}

