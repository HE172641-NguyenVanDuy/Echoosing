using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace EChoosing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {

        private readonly IQuestionService _questionService;
        private readonly IExamService _examService;
        private readonly IExamQuestionService _examQuestionService;
        private readonly IOptionService _optionService;

        public QuestionController(
            IQuestionService questionService,
            IExamService examService,
            IExamQuestionService examQuestionService,
            IOptionService optionService)
        {
            _questionService = questionService;
            _examService = examService;
            _examQuestionService = examQuestionService;
            _optionService = optionService;
        }


        [HttpGet("InitQuestion/{examId}")]
        public IActionResult GetQuestionTemplate(int examId)
        {
            string msg = _examService.GetExamByID(examId, out Exam exam);
            if (!string.IsNullOrEmpty(msg) || exam == null)
                return NotFound(new { message = "Exam not found", error = msg });

            var questions = new List<QuestionInputModel>();
            for (int i = 1; i <= exam.TotalQuestions; i++)
            {
                var question = new QuestionInputModel
                {
                    SortOrder = i,
                    Options = Enumerable.Range(0, 4).Select(_ => new OptionInputModel()).ToList()
                };
                questions.Add(question);
            }

            return Ok(new
            {
                ExamId = exam.ExamId,
                ExamName = exam.ExamName,
                Duration = exam.Duration,
                TotalQuestions = exam.TotalQuestions,
                Questions = questions
            });
        }

        [HttpPost("CreateQuestion/{examId}")]
        public IActionResult CreateQuestions(int examId, [FromBody] List<QuestionInputModel> questions)
        {
            string msg = _examService.GetExamByID(examId, out Exam exam);
            if (!string.IsNullOrEmpty(msg) || exam == null)
                return NotFound(new { message = "Exam not found", error = msg });

            var errors = new List<string>();

            foreach (var questionInput in questions)
            {
                if (!questionInput.Options.Any(o => o.IsCorrect))
                {
                    errors.Add($"Question {questionInput.SortOrder} must have at least one correct answer.");
                    continue;
                }

                var question = new Question { Content = questionInput.Content };
                msg = _questionService.AddQuestion(question);
                if (!string.IsNullOrEmpty(msg))
                {
                    errors.Add($"Error adding question {questionInput.SortOrder}: {msg}");
                    continue;
                }

                foreach (var optionInput in questionInput.Options)
                {
                    var option = new Option
                    {
                        QuestionId = question.QuestionId,
                        Content = optionInput.Content,
                        IsCorrect = optionInput.IsCorrect
                    };
                    msg = _optionService.CreateOption(option);
                    if (!string.IsNullOrEmpty(msg))
                        errors.Add($"Error adding option for question {questionInput.SortOrder}: {msg}");
                }

                var examQuestion = new ExamQuestion
                {
                    ExamId = exam.ExamId,
                    QuestionId = question.QuestionId,
                    SortOrder = questionInput.SortOrder,
                    IsDelete = false
                };
                msg = _examQuestionService.AddExamQuestion(examQuestion);
                if (!string.IsNullOrEmpty(msg))
                    errors.Add($"Error linking question {questionInput.SortOrder} to exam: {msg}");
            }

            if (errors.Any())
                return BadRequest(new { message = "Some questions failed to add", errors });

            return Ok(new { message = "Questions created successfully" });
        }

        [HttpGet("GetExamQuestions/{examId}")]
        public IActionResult GetQuestions(int examId)
        {
            var msg = _examService.GetExamByID(examId, out Exam exam);
            if (!string.IsNullOrEmpty(msg) || exam == null)
                return NotFound(new { message = "Exam not found", error = msg });

            msg = _examQuestionService.GetAllExamQuestionByExamID(examId, out var examQuestions);
            if (!string.IsNullOrEmpty(msg) || examQuestions == null)
                return BadRequest(new { message = "Error fetching exam questions", error = msg });

            var result = new List<ExamQuestionInputModel>();

            foreach (var eq in examQuestions.Where(q => q.IsDelete == false))
            {
                msg = _questionService.GetQuestionByQuestionId(eq.QuestionId, out var question);
                if (!string.IsNullOrEmpty(msg) || question == null) continue;

                msg = _optionService.GetOptionsByQuestionID(question.QuestionId, out var options);
                if (!string.IsNullOrEmpty(msg) || options == null) continue;

                result.Add(new ExamQuestionInputModel
                {
                    QuestionId = question.QuestionId,
                    SortOrder = eq.SortOrder == 0 ? 1 : eq.SortOrder,
                    Content = question.Content,
                    Options = options.Select(o => new ExamOptionInputModel
                    {
                        OptionId = o.OptionId,
                        Content = o.Content,
                        IsCorrect = o.IsCorrect.Value
                    }).ToList()
                });
            }

            return Ok(new
            {
                ExamId = exam.ExamId,
                ExamName = exam.ExamName,
                Duration = exam.Duration,
                TotalQuestions = exam.TotalQuestions,
                Questions = result
            });
        }

        [HttpPut("UpdateQuestion/{examId}")]
        public IActionResult UpdateQuestions(int examId, [FromBody] List<ExamQuestionInputModel> questions)
        {
            var errors = new List<string>();

            foreach (var questionInput in questions)
            {
                if (!questionInput.Options.Any(o => o.IsCorrect))
                {
                    errors.Add($"Question {questionInput.SortOrder} must have at least one correct answer.");
                    continue;
                }

                var msg = _questionService.GetQuestionByQuestionId(questionInput.QuestionId, out var question);
                if (!string.IsNullOrEmpty(msg))
                {
                    errors.Add($"Error loading question {questionInput.SortOrder}: {msg}");
                    continue;
                }

                question.Content = questionInput.Content;

                msg = _questionService.UpdateQuestion(question);
                if (!string.IsNullOrEmpty(msg))
                {
                    errors.Add($"Error updating question {questionInput.SortOrder}: {msg}");
                    continue;
                }

                msg = UpdateOptions(question.QuestionId, questionInput.Options);
                if (!string.IsNullOrEmpty(msg))
                    errors.Add(msg);
            }

            if (errors.Count > 0)
                return BadRequest(new { message = "Some updates failed", errors });

            return Ok(new { message = "Questions updated successfully" });
        }

        // DELETE: api/ExamQuestion/{examId}/question/{questionId}
        [HttpDelete("ExamQuestions/{examId}/question/{questionId}")]
        public IActionResult DeleteQuestion(int examId, int questionId)
        {
            var msg = _examQuestionService.GetExamQuestionByQuestionIDAndExamID(questionId, examId, out var examQuestion);
            if (!string.IsNullOrEmpty(msg) || examQuestion == null)
                return BadRequest(new { message = "Exam question not found", error = msg });

            examQuestion.IsDelete = true;
            msg = _examQuestionService.UpdateExamQuestion(examQuestion);
            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = "Failed to delete question", error = msg });

            // Update Exam.TotalQuestions
            msg = _examService.GetExamByID(examId, out var exam);
            if (!string.IsNullOrEmpty(msg) || exam == null)
                return BadRequest(new { message = "Error retrieving exam", error = msg });

            exam.TotalQuestions = Math.Max(0, exam.TotalQuestions - 1);
            msg = _examService.UpdateExam(exam);
            if (!string.IsNullOrEmpty(msg))
                return BadRequest(new { message = "Failed to update total questions", error = msg });

            return Ok(new { message = "Question deleted successfully" });
        }

        private string UpdateOptions(int questionId, List<ExamOptionInputModel> options)
        {
            foreach (var option in options)
            {
                var msg = _optionService.UpdateOption(new Option
                {
                    OptionId = option.OptionId,
                    QuestionId = questionId,
                    Content = option.Content,
                    IsCorrect = option.IsCorrect,
                    UpdatedDate = DateTime.UtcNow
                });
                if (!string.IsNullOrEmpty(msg))
                    return $"Error updating option {option.OptionId}: {msg}";
            }
            return string.Empty;
        }

    }


    public class QuestionInputModel
    {
        public int SortOrder { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<OptionInputModel> Options { get; set; } = new();
    }

    public class OptionInputModel
    {
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class ExamQuestionInputModel
    {
        public int QuestionId { get; set; }
        public int SortOrder { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<ExamOptionInputModel> Options { get; set; } = new();
    }

    public class ExamOptionInputModel
    {
        public int OptionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
    }


}
