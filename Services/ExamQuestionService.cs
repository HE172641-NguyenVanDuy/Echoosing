using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IExamQuestionService
    {
        string AddExamQuestion(ExamQuestion examQuestion);
        string GetExamQuestionByID(int ExamQuestionID, out ExamQuestion examQuestion);
        string GetExamQuestionByQuestionIDAndExamID(int questionID, int examId, out ExamQuestion examQuestion);
        string GetAllExamQuestionByExamID(int examID, out List<ExamQuestion> examQuestionList);
        string UpdateExamQuestion(ExamQuestion examQuestion);
    }
    public class ExamQuestionService : IExamQuestionService
    {
        private readonly EchoosingContext _context;
        public ExamQuestionService(EchoosingContext context)
        {
            _context = context;
        }
        public string AddExamQuestion(ExamQuestion examQuestion)
        {
            try
            {
                _context.ExamQuestions.Add(examQuestion);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetExamQuestionByID(int ExamQuestionID, out ExamQuestion examQuestion)
        {
            examQuestion = null;
            try
            {
                examQuestion = _context.ExamQuestions.FirstOrDefault(e => e.ExamQuestionId == ExamQuestionID);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetAllExamQuestionByExamID(int examID, out List<ExamQuestion> examQuestionList)
        {
            examQuestionList = null;
            try
            {
                examQuestionList = _context.ExamQuestions
                    .Include(eq => eq.Exam)
                    .Include(eq => eq.Question)
                    .Where(u => u.ExamId == examID && u.IsDelete == false).ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetExamQuestionByQuestionIDAndExamID(int questionID, int examId, out ExamQuestion examQuestion)
        {
            examQuestion = null;
            try
            {
                examQuestion = _context.ExamQuestions
                    .FirstOrDefault(u => u.QuestionId == questionID && u.ExamId == examId);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateExamQuestion(ExamQuestion examQuestion)
        {
            try
            {
                _context.ExamQuestions.Update(examQuestion);
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
