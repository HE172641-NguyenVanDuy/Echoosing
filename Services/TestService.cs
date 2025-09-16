using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Services.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ITestService
    {
        string LoadExamData(int examId, out int examDuration, out List<QuestionViewModel> questions);
        string SubmitExam(List<UserAnswerModel> userAnswers, string userId, int examId, int status, string code, string userName, string classId, out string attemptId, DateTime timeStart);
        List<Exam> GetAllExams();
        public string GetResultTest(string attemptID, out ResultTestVMa result);
        public string GetResultTestGuest(string attemptID, out ExamAttempt result);

    }


    public class TestService : ITestService
    {
        private readonly EchoosingContext _context;
        private readonly IExamQuestionService _examQuestionService;

        public TestService(EchoosingContext context, IExamQuestionService examQuestionService)
        {
            _context = context;
            _examQuestionService = examQuestionService;
        }

        public string LoadExamData(int examId, out int examDuration, out List<QuestionViewModel> questions)
        {
            questions = new List<QuestionViewModel>();
            examDuration = 0;

            var exam = _context.Exams.FirstOrDefault(e => e.ExamId == examId);
            if (exam == null)
            {
                return "Exam not found.";
            }

            if (exam.TimeStart > DateTime.Now)
            {
                return "Not yet the test date";
            }

            examDuration = exam.Duration;
            if (examDuration <= 0)
            {
                return "Invalid exam duration.";
            }

            string msg = _examQuestionService.GetAllExamQuestionByExamID(examId, out List<ExamQuestion> examQuestionList);
            if (!string.IsNullOrEmpty(msg))
            {
                return "Error: " + msg;
            }

            var query = (from eq in _context.ExamQuestions
                         join q in _context.Questions on eq.QuestionId equals q.QuestionId
                         join o in _context.Options on q.QuestionId equals o.QuestionId
                         where eq.ExamId == examId
                         select new
                         {
                             QuestionId = q.QuestionId,
                             QuestionContent = q.Content,
                             OptionId = o.OptionId,
                             OptionContent = o.Content,
                             IsCorrect = o.IsCorrect
                         }).ToList();

            questions = query.GroupBy(q => new { q.QuestionId, q.QuestionContent })
                             .Select(g => new QuestionViewModel
                             {
                                 QuestionId = g.Key.QuestionId,
                                 Question = g.Key.QuestionContent,
                                 Answers = g.Select(o => new AnswerViewModel
                                 {
                                     OptionId = o.OptionId,
                                     Content = o.OptionContent,
                                     IsCorrect = o.IsCorrect ?? false
                                 }).ToList()
                             })
                             .ToList();

            return "";
        }

        public string SubmitExam(List<UserAnswerModel> userAnswers, string userId, int examId, int status, string code, string userName, string classId, out string attemptId, DateTime timeStart)
        {
            attemptId = "";

            var questions = (from eq in _context.ExamQuestions
                             join q in _context.Questions on eq.QuestionId equals q.QuestionId
                             join o in _context.Options on q.QuestionId equals o.QuestionId
                             where eq.ExamId == examId
                             select new
                             {
                                 QuestionId = q.QuestionId,
                                 OptionId = o.OptionId,
                                 IsCorrect = o.IsCorrect
                             }).ToList()
                             .GroupBy(q => q.QuestionId)
                             .Select(g => new QuestionViewModel
                             {
                                 QuestionId = g.Key,
                                 Answers = g.Select(o => new AnswerViewModel
                                 {
                                     OptionId = o.OptionId,
                                     IsCorrect = o.IsCorrect ?? false
                                 }).ToList()
                             })
                             .ToList();

            decimal totalScore = 0;
            int totalQuestions = questions.Count;

            attemptId = Guid.NewGuid().ToString();
            List<Answer> answerEntities = new List<Answer>();

            foreach (var question in questions)
            {
                var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == question.QuestionId);
                if (userAnswer != null)
                {
                    var correctAnswers = question.Answers.Where(a => a.IsCorrect).Select(a => a.OptionId).ToList();
                    if (userAnswer.SelectedOptionIds.OrderBy(id => id).SequenceEqual(correctAnswers.OrderBy(id => id)))
                    {
                        totalScore += 1;
                    }

                    if (string.IsNullOrEmpty(code))
                    {
                        foreach (var optionId in userAnswer.SelectedOptionIds)
                        {
                            answerEntities.Add(new Answer
                            {
                                AttemptId = attemptId,
                                QuestionId = question.QuestionId,
                                OptionId = optionId,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
            decimal finalScore;
            if (status == 11)
            {
                finalScore = (totalScore / totalQuestions) * 10;
            }
            else
            {
                finalScore = 0;
            }


            var examAttempt = new ExamAttempt
            {
                AttemptId = attemptId,
                UserId = string.IsNullOrEmpty(userId) ? Guid.Empty.ToString() : userId,
                ExamId = examId,
                StartTime = timeStart,
                EndTime = DateTime.Now,
                Score = finalScore,
                Status = status,
                ExamCode = code,
                UserName = userName,
                ClassId = classId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.ExamAttempts.Add(examAttempt);
            if (answerEntities.Count > 0)
            {
                _context.Answers.AddRange(answerEntities);
            }
            _context.SaveChanges();

            return "";
        }
        public List<Exam> GetAllExams()
        {
            return _context.Exams.Where(e => e.IsDelete == false || e.IsDelete == null).ToList();
        }

        public string GetResultTest(string attemptID, out ResultTestVMa result)
        {
            result = new ResultTestVMa();
            result.numberCorrect = 0;
            try
            {
                var attempt = _context.ExamAttempts
                                        .Include(a => a.Answers)
                                        .Include(a => a.Exam) // Đã bao gồm Exam, nên attempt.Exam không null ở đây
                                        .FirstOrDefault(a => a.AttemptId == attemptID);

                if (attempt == null)
                {
                    // Logging thêm thông tin khi không tìm thấy attempt
                    Console.WriteLine($"TestService: Attempt with ID {attemptID} not found.");
                    return "Không tìm thấy Attempt!";
                }

                // --- ĐIỂM SỬA CHỮA CHÍNH ---
                // Gán thuộc tính 'exam' của ResultTestVM từ attempt.Exam
                if (attempt.Exam == null)
                {
                    // Đây là một kiểm tra an toàn, nhưng với .Include(a => a.Exam) nó hiếm khi xảy ra
                    // trừ khi có vấn đề với dữ liệu hoặc cấu hình EF Core.
                    Console.WriteLine($"TestService: WARNING: attempt.Exam is null for attempt ID {attemptID} even after Include.");
                    return "Thông tin bài thi (Exam) bị thiếu.";
                }

                // GÁN THÔNG TIN BÀI THI VÀO result.exam
                result.exam = new ExamVM // Sử dụng ExamVM của bạn, không phải entity Exam trực tiếp
                {
                    ExamId = attempt.Exam.ExamId, // Giả sử Exam.Id là ID của bài thi
                    ExamName = attempt.Exam.ExamName // Giả sử Exam.Name là tên bài thi
                };
                // --- KẾT THÚC ĐIỂM SỬA CHỮA CHÍNH ---

                // Gán attempt entity vào result.examAttempt
                // Đảm bảo ResultTestVM.examAttempt có kiểu là ExamAttempt (entity)
                // Nếu ResultTestVM.examAttempt là một ViewModel khác, bạn cần ánh xạ nó.
                result.examAttempt = attempt;

                var questions = (from eq in _context.ExamQuestions
                                 join q in _context.Questions on eq.QuestionId equals q.QuestionId
                                 join o in _context.Options on q.QuestionId equals o.QuestionId
                                 where eq.ExamId == attempt.ExamId // Sử dụng ExamId từ attempt đã tải
                                 select new
                                 {
                                     Content = o.Content,
                                     QuestionId = q.QuestionId,
                                     OptionId = o.OptionId,
                                     IsCorrect = o.IsCorrect ?? false, // Đảm bảo xử lý null cho bool
                                     Question = q.Content
                                 }).ToList()
                                 .GroupBy(q => q.QuestionId)
                                 .Select(g => new QuestionViewModel
                                 {
                                     QuestionId = g.Key,
                                     Question = g.First().Question,
                                     Answers = g.Select(o => new AnswerViewModel
                                     {
                                         OptionId = o.OptionId,
                                         Content = o.Content,
                                         IsCorrect = o.IsCorrect
                                     }).ToList()
                                 })
                                 .ToList();

                result.numberQuestion = questions.Count;
                Dictionary<string, List<AnswerResult>> keyValueOption = new Dictionary<string, List<AnswerResult>>();
                List<AnswerResult> answerResults; // Khai báo ngoài vòng lặp

                foreach (var question in questions)
                {
                    answerResults = new List<AnswerResult>(); // Khởi tạo mỗi lần lặp
                    var userAnswer = attempt.Answers.Where(a => a.QuestionId == question.QuestionId).ToList();

                    foreach (var option in question.Answers)
                    {
                        AnswerResult answerResult = new AnswerResult();
                        answerResult.OptionContent = option.Content;
                        answerResult.IsCorrect = option.IsCorrect; // Đã xử lý IsCorrect??false ở trên

                        var statusOption = userAnswer.FirstOrDefault(u => u.OptionId == option.OptionId);
                        answerResult.UserAnswer = (statusOption != null); // Đơn giản hóa kiểm tra này
                        answerResults.Add(answerResult);
                    }

                    var correctOptionsForQuestion = question.Answers.Where(a => a.IsCorrect).Select(a => a.OptionId).OrderBy(id => id).ToList();
                    var userSelectedOptionsForQuestion = answerResults.Where(ar => ar.UserAnswer).Select(ar => ar.OptionId).OrderBy(id => id).ToList(); // Lấy OptionId từ AnswerResult của người dùng

                    // So sánh các đáp án đúng và đáp án người dùng đã chọn
                    bool isCorrectlyAnswered = correctOptionsForQuestion.SequenceEqual(userSelectedOptionsForQuestion);

                    if (isCorrectlyAnswered)
                    {
                        result.numberCorrect++;
                    }

                    keyValueOption.Add(question.Question, answerResults);
                }
                result.optionResult = keyValueOption;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestService.GetResultTest ERROR: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return $"Lỗi khi lấy kết quả bài làm: {ex.Message}";
            }
            return ""; // Trả về chuỗi rỗng nếu thành công
        }

        public string GetResultTestGuest(string attemptID, out ExamAttempt result)
        {
            result = null;
            try
            {
                result = _context.ExamAttempts.FirstOrDefault(a => a.AttemptId == attemptID);
                if (result == null) throw new Exception("Attempt exam not exist");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }


    }


}
