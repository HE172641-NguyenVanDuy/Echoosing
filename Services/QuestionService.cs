using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IQuestionService
    {
        string AddQuestion(Question question);
        string GetQuestionByQuestionId(int examId, out Question question);
        string UpdateQuestion(Question question);
    }

    public class QuestionService : IQuestionService
    {
        private readonly EchoosingContext _context;

        public QuestionService(EchoosingContext context)
        {
            _context = context;
        }

        public string AddQuestion(Question question)
        {
            try
            {
                _context.Questions.Add(question);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetQuestionByQuestionId(int questionId, out Question question)
        {
            question = null;
            try
            {
                question = _context.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateQuestion(Question question)
        {
            try
            {
                _context.Questions.Update(question);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
