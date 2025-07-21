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
        public string OptionContent { get; set; }
        public bool IsCorrect { get; set; }
        public bool UserAnswer { get; set; }
    }
}
