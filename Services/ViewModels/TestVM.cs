using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ViewModels
{
    public class TestVM
    {
    }
    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<AnswerViewModel> Answers { get; set; } = new List<AnswerViewModel>();
    }

    public class AnswerViewModel
    {
        public int OptionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
    public class UserAnswerModel
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new List<int>();
    }
    public class ResultTestVM
    {
        public Exam exam { get; set; }
        public ExamAttempt examAttempt { get; set; }
        public int numberCorrect { get; set; }
        public int numberQuestion { get; set; }
        public Dictionary<string, List<AnswerResult>> optionResult;
    }
    public class AnswerResult
    {
        public int OptionId { get; set; } // <--- ADD THIS LINE
        public string OptionContent { get; set; }
        public bool IsCorrect { get; set; }
        public bool UserAnswer { get; set; }
    }
    public class ExamDtoa
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Không bao gồm List<ExamAttempt> để tránh vòng lặp
        // Nếu cần, có thể thêm các thuộc tính khác của Exam
    }

    public class ExamAttemptDtoa
    {
        public string AttemptId { get; set; }
        public ExamDtoa Exam { get; set; }
        // Thêm các thuộc tính khác của ExamAttempt nếu cần
    }

    public class ResultTestDto
    {
        public ExamDtoa Exam { get; set; }
        public ExamAttemptDtoa ExamAttempt { get; set; }
        public int NumberCorrect { get; set; }
        public int NumberQuestion { get; set; }
        public Dictionary<string, List<AnswerResult>> OptionResult { get; set; }
    }

    public class ResultTestVMa
    {
        public int numberCorrect { get; set; }
        public int numberQuestion { get; set; }
        public Dictionary<string, List<AnswerResult>> optionResult { get; set; }
        public ExamAttempt examAttempt { get; set; } // << Bạn đang gán vào đây
                                                     // FE Controller đang mong muốn result.exam.ExamId, result.exam.ExamName
                                                     // NGHĨA LÀ, ResultTestVM PHẢI CÓ THUỘC TÍNH 'exam'
        public ExamVM exam { get; set; } // << THIẾU DÒNG NÀY ĐƯỢC GÁN Ở TestService
    }

    public class ExamVM
    {
        public int ExamId { get; set; } // Tên thuộc tính có thể là Id hoặc ExamId
        public string ExamName { get; set; } // Tên thuộc tính có thể là Name hoặc ExamName
                                             // Thêm bất kỳ thuộc tính nào khác của bài thi mà bạn muốn truyền đi
    }
}
