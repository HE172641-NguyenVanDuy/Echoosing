using BusinessObjects.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using EChoosing.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace EChoosing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamController : ControllerBase
    {


        private readonly IExcelImportService _excelImportService;
        private readonly IExamService _examService;
        private readonly IAccountService _accountService;
        private readonly ILogger<ExamController> _logger;
        private readonly ICodeExamService _codeExamService;
        private readonly IClassService _classService;
        private readonly IConverter _pdfConverter;


        private readonly EchoosingContext echoosingContext;

        public ExamController(
           IExcelImportService excelImportService,
           IExamService examService,
           EchoosingContext exchosingContext,
           ILogger<ExamController> _logger,
           ICodeExamService _codeExamService,
           IClassService _classService,
           IConverter _pdfConverter,
            IAccountService accountService)
        {
            _excelImportService = excelImportService;
            _examService = examService;
            _accountService = accountService;
            _logger = _logger;
            _classService = _classService;
            _pdfConverter = _pdfConverter;
            _codeExamService = _codeExamService;
            echoosingContext = exchosingContext;
        }

		// GET: api/exams
		[Authorize(Policy = "TeacherOnly")]
		[HttpGet("get-exams")]
		public IActionResult GetExams(
	[FromQuery] string? searchTerm,
	[FromQuery] string? sortColumn,
	[FromQuery] string? sortOrder)
		{
			string userId = _accountService.GetUserIDLogin(HttpContext);
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Please login again." });

			string msg = _examService.GetListExamByID(userId, out List<Exam> exams);
			if (!string.IsNullOrEmpty(msg) || exams == null)
				return BadRequest(new { message = "Get Exam List Error", detail = msg });

			// Filtering
			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				exams = exams.Where(e =>
					e.ExamName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
			}

			// Sorting
			if (!string.IsNullOrEmpty(sortColumn))
			{
				exams = sortOrder == "asc"
					? exams.OrderBy(e => GetSortValue(e, sortColumn)).ToList()
					: exams.OrderByDescending(e => GetSortValue(e, sortColumn)).ToList();
			}

			// Chuyển sang DTO
			var examDtos = exams.Select(e => new ExamDto
			{
				ExamId = e.ExamId,
				ExamName = e.ExamName,
				Duration = e.Duration,
				TimeStart = e.TimeStart ?? DateTime.MinValue,

				TotalQuestions = e.TotalQuestions,
				CreatedDate = e.CreatedDate
			}).ToList();

			return Ok(examDtos);
		}


		// POST: api/exams/upload
		[HttpPost("upload")]
        [RequestSizeLimit(10 * 1024 * 1024)] // Giới hạn file 10MB
        public async Task<IActionResult> UploadExam(
            [FromForm] IFormFile excelFile,
            [FromForm] string examName,
            [FromForm] int duration,
            [FromForm] DateTime timeStart)
        {
            string userId = _accountService.GetUserIDLogin(HttpContext);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Please login again." });

            if (excelFile == null || excelFile.Length == 0)
                return BadRequest(new { message = "Please upload a valid Excel file." });

            if (string.IsNullOrWhiteSpace(examName))
                return BadRequest(new { message = "Exam name is required." });

            try
            {
				var examId = await _excelImportService.ProcessExcelFileAsync(excelFile, examName, duration, timeStart, userId);
				return Ok(new { message = "Exam created successfully.", examId = examId });
			}
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing Excel file.", detail = ex.Message });
            }
        }

        [HttpPost("CreateExamManual")]
		[Authorize]
		public IActionResult CreateExam([FromBody] CreateExamManualRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body." });

            if (string.IsNullOrWhiteSpace(request.ExamName))
                return BadRequest(new { message = "Exam name is required." });

            if (request.TimeStart != null && request.TimeStart <= DateTime.UtcNow)
                return BadRequest(new { message = "Time must be greater than present." });

            string userId = _accountService.GetUserIDLogin(HttpContext);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not logged in.");
                return Unauthorized(new { message = "Please log in again." });
            }

            var exam = new Exam
            {
                ExamName = request.ExamName,
                TotalQuestions = request.QuestionCount,
                Duration = request.ExamDuration,
                TimeStart = request.TimeStart,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsDelete = false,
                IsRetake = false
            };

            string msg = _examService.CreateExam(exam);
            if (!string.IsNullOrEmpty(msg))
            {
                _logger.LogError("Failed to create exam: " + msg);
                return BadRequest(new { message = "Failed to create exam.", detail = msg });
            }

            return Ok(new
            {
                message = "Exam created successfully.",
                examId = exam.ExamId.ToString()
            }); 
        }

        [HttpGet("GetExamById/{id}")]
        public IActionResult GetExamByID(int id)
        {
            string msg = _examService.GetExamByID(id, out Exam exam);
            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { error = "Get Exam Error: " + msg });
            }

            return Ok(exam);
        }

        // DELETE: api/Exam/{id}
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteExam(int id)
        {
            string msg = _examService.GetExamByID(id, out Exam exam);
            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { error = "Get Exam Error: " + msg });
            }

            exam.IsDelete = true;

            msg = _examService.UpdateExam(exam);
            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { error = "Delete Exam Error: " + msg });
            }

            return Ok(new { message = "Exam deleted successfully." });
        }

        [HttpGet("EnterName")]
		[Authorize]
		public IActionResult GetEnterName([FromQuery] int examId, [FromQuery] string examCode)
        {
           

            return Ok(new { ExamId = examId, ExamCode = examCode });
        }

        [HttpPost("TestEntry")]
		[Authorize]
		public IActionResult Post([FromBody] TestEntryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest("User name is required.");
            }

            // Có thể kiểm tra logic ExamID + ExamCode nếu cần

            // Frontend sẽ dùng dữ liệu trả về để redirect qua DoingTest page
            return Ok(new
            {
                RedirectUrl = $"/Test/DoingTest?ExamID={request.ExamId}&ExamCode={request.ExamCode}&UserName={request.UserName}"
            });
        }

        [HttpPost("JoinExam")]
		[Authorize]
		public IActionResult Post([FromBody] JoinExamRequest request)
        {
            var msg = _codeExamService.GetExamByCode(request.CodeExam, out ExamCodeVM examCodeVM);

            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { Message = msg });
            }

            return Ok(new
            {
                RedirectUrl = $"/Exams/EnterName?ExamID={examCodeVM.ExamID}&ExamCode={examCodeVM.ExamCode}"
            });
        }

        [HttpGet("GetExam/{examId}")]
        public IActionResult GetExam(int examId)
        {
            string msg = _examService.GetExamByID(examId, out Exam exam);
            if (msg.Length > 0 || exam == null)
            {
                return BadRequest(new { message = "Get Exam Error: " + msg });
            }

            return Ok(new
            {
                exam.ExamId,
                ExamName = exam.ExamName,
                Duration = exam.Duration,
                TimeStart = exam.TimeStart
            });
        }

        [HttpGet("ResultExam/{examId}")]
        public IActionResult GetResultByExamId(int examId)
        {
            var msg = _examService.GetListResultByExamID(examId, out List<ExamAttempt> examAttempts);

            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { Message = msg });
            }

            var examName = _examService.GetExamNameById(examId);

            return Ok(new ResultExamResponse
            {
                ExamId = examId,
                ExamName = examName,
                ExamResults = examAttempts
            });
        }


        [HttpGet("GetExamNameById/{examId}")]
		[Authorize]
		public IActionResult GetExamNameById(int examId)
        {
            var examName = _examService.GetExamNameById(examId);
            if (!string.IsNullOrEmpty(examName))
            {
                return BadRequest(new { Message = examName });
            }
            return Ok(examName);
        }

        [HttpPost("UpdateExam")]
        public IActionResult UpdateExam([FromBody] UpdateExamDto dto)
        {
            if (dto.TimeStart < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Time Start Exam invalid" });
            }

            string msg = _examService.GetExamByID(dto.ExamId, out Exam exam);
            if (msg.Length > 0 || exam == null)
            {
                return NotFound(new { message = "Get Exam Error: " + msg });
            }

            exam.ExamName = dto.ExamName;
            exam.Duration = dto.ExamDuration;
            exam.TimeStart = dto.TimeStart;
            exam.UpdatedDate = DateTime.UtcNow;

            msg = _examService.UpdateExam(exam);
            if (msg.Length > 0)
            {
                return BadRequest(new { message = "Update Exam Error: " + msg });
            }

            return Ok(new { message = "Update Exam Successfully", examId = exam.ExamId });
        }

        [HttpGet("/GetListExamByUserId/{userid}")]
        public IActionResult GetListExamByUserID(string userid)
        {
            string msg = _examService.GetListExamByID(userid, out List<Exam> exams);
            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(new { error = "Get Exam Error: " + msg });
            }

            return Ok(msg);
        }

        [HttpPost("export-result")]
        public IActionResult ExportResult()
        {
            string content = @"
        <html>
        <head>
            <style>
                body { font-family: Arial, sans-serif; }
                .container { width: 80%; margin: auto; padding: 20px; border: 1px solid #ccc; }
                .header { font-size: 24px; font-weight: bold; text-align: center; }
                .highlight { font-size: 20px; font-weight: bold; text-align: center; color: red; }
                .divider { border-top: 2px solid black; margin: 10px 0; }
                .question-card { margin-bottom: 10px; padding: 10px; border: 1px solid #ddd; }
                .question { font-weight: bold; }
                .correct { color: green; font-weight: bold; }
                .option { color: black; }
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>ĐÁP ÁN</div>
                <div class='highlight'>ĐỀ SỐ 1</div>
                <div class='divider'></div>

                <div class='question-card'>
                    <div class='question'>Câu 1: Kim loại nào sau đây có độ cứng cao nhất?</div>
                    <div class='correct'>A. Cr.</div>
                    <div class='option'>B. Al.</div>
                    <div class='option'>C. Fe.</div>
                    <div class='option'>D. Ag.</div>
                </div>
                <div class='question-card'>
                    <div class='question'>Câu 2: Chất lỏng có công thức CH₃COOCH=CH₂ có tên gọi là?</div>
                    <div class='correct'>A. Vinyl axetat.</div>
                    <div class='option'>B. Metyl axetat.</div>
                    <div class='option'>C. Etyl axetat.</div>
                    <div class='option'>D. Metyl propionat.</div>
                </div>

                <div class='divider'></div>
            </div>
        </body>
        </html>";

            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects = {
                new ObjectSettings()
                {
                    PagesCount = true,
                    HtmlContent = content,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
            };

            byte[] fileBytes = _pdfConverter.Convert(pdfDoc);

            return File(fileBytes, "application/pdf", "ResultExam.pdf");
        }

        

        public class UpdateExamDto
        {
            public int ExamId { get; set; }
            public string ExamName { get; set; }
            public int ExamDuration { get; set; }
            public DateTime? TimeStart { get; set; }
        }


        public class ResultExamResponse
        {
            public int ExamId { get; set; }
            public string ExamName { get; set; }
            public List<ExamAttempt> ExamResults { get; set; }
        }

        public class JoinExamRequest
        {
            public string CodeExam { get; set; }
        }

        public class CreateExamManualRequest
        {
            public string ExamName { get; set; }
            public int QuestionCount { get; set; }
            public int ExamDuration { get; set; }
            public DateTime? TimeStart { get; set; }
        }

        public class TestEntryRequest
        {
            public int ExamId { get; set; }
            public string ExamCode { get; set; }
            public string UserName { get; set; }
        }

        private object GetSortValue(Exam exam, string sortColumn)
        {
            return sortColumn switch
            {
                "ExamName" => exam.ExamName,
                "TotalQuestions" => exam.TotalQuestions,
                "Duration" => exam.Duration,
                "TimeStart" => exam.TimeStart,
                "CreatedDate" => exam.CreatedDate,
                _ => exam.CreatedDate,
            };
        }

		public class ExamDto
		{
			public int ExamId { get; set; }
			public string ExamName { get; set; } = string.Empty;
			public int Duration { get; set; }
			public DateTime TimeStart { get; set; }
			public int TotalQuestions { get; set; }
			public DateTime CreatedDate { get; set; }
		}

	}

}

