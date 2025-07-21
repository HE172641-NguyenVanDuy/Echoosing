using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IExamService
    {
        string CreateExam(Exam exam);
        string GetExamByID(int ExamID, out Exam exam);
        string GetListExamByID(string userID, out List<Exam> exams);
        string GetListExamByClassID(string classID, out List<Exam> exams);
        string UpdateExam(Exam exam);
        string DeleteExamByID(int examID);
        string GetListStatusExam(out List<Systemkey> listStatusExam);
        string GetExamNameById(int examId);
        string GetListExamByListExamID(List<int> examIds, out List<Exam> exams);

        string GetListResultByExamID(int examID, out List<ExamAttempt> exams);
    }
    public class ExamService : IExamService
    {
        private readonly EchoosingContext _context;
        private readonly ICodeJoinClassService _codeJoinClassService;
        public ExamService(EchoosingContext context, ICodeJoinClassService codeJoinClassService)
        {
            _context = context;
            _codeJoinClassService = codeJoinClassService;
        }
        public string CreateExam(Exam exam)
        {
            try
            {
                _context.Exams.Add(exam);
                _context.SaveChanges();

                int examId = exam.ExamId;
                string codeId = Guid.NewGuid().ToString();

                ExamCode examCode = new ExamCode
                {
                    CodeId = codeId,
                    ExamId = examId,
                    Code = _codeJoinClassService.Encode(codeId),
                    CreatedDate = DateTime.Now,
                    TimeStart = exam.TimeStart,
                };
                _context.ExamCodes.Add(examCode);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteExamByID(int examId)
        {
            try
            {
                GetExamByID(examId, out Exam exam);
                exam.IsDelete = true;

                _context.Exams.Update(exam);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetExamByID(int ExamID, out Exam exam)
        {
            exam = null;
            try
            {
                exam = _context.Exams.FirstOrDefault(e => e.ExamId == ExamID);
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetListStatusExam(out List<Systemkey> listStatusExam)
        {
            listStatusExam = null;
            try
            {
                listStatusExam = _context.Systemkeys.Where(s => s.ParentId == 9).ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetListExamByClassID(string classID, out List<Exam> exams)
        {
            exams = new List<Exam>();
            try
            {
                var listExam = _context.ClassExams.Where(u => u.ClassId == classID).Include(c => c.Exam).ToList();
                if (listExam.Any())
                {
                    foreach (var item in listExam)
                    {
                        exams.Add(item.Exam);
                    }
                }
                else throw new Exception("Class has no test");
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetListExamByID(string userID, out List<Exam> exams)
        {
            exams = null;
            try
            {
                exams = _context.Exams.Include(x => x.ExamCodes).Where(e => e.CreatedBy == userID && e.IsDelete == false).OrderByDescending(e => e.CreatedDate).ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string UpdateExam(Exam exam)
        {
            try
            {
                _context.Exams.Update(exam);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetExamNameById(int examId)
        {
            return _context.Exams
                           .Where(e => e.ExamId == examId)
                           .Select(e => e.ExamName)
                           .FirstOrDefault() ?? "Unknown Exam";
        }

        public string GetListExamByListExamID(List<int> examIds, out List<Exam> exams)
        {
            exams = new List<Exam>();
            try
            {
                if (examIds == null || !examIds.Any())
                {
                    return "Danh sách examIds rỗng.";
                }

                // Truy vấn lấy tất cả các exam có ID nằm trong danh sách examIds
                exams = _context.Exams
                                .Where(e => examIds.Contains(e.ExamId))
                                .ToList();
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetListResultByExamID(int examID, out List<ExamAttempt> exams)
        {
            exams = new List<ExamAttempt>();
            try
            {
                if (examID == null)
                {
                    return "Danh sách examIds rỗng.";
                }

                // Truy vấn lấy tất cả các exam có ID nằm trong danh sách examIds
                exams = _context.ExamAttempts
                                .Where(e => e.ExamId == examID)
                                .ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
