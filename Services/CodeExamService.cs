using BusinessObjects.Models;
using EChoosing.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ICodeExamService
    {
        public string GetExamByCode(string codeExam, out ExamCodeVM examCodeVM);
    }
    public class CodeExamService : ICodeExamService
    {
        private readonly EchoosingContext _context;
        public CodeExamService(EchoosingContext context)
        {
            _context = context;
        }
        public string GetExamByCode(string codeExam, out ExamCodeVM examCodeVM)
        {
            examCodeVM = new ExamCodeVM();
            try
            {
                var examCode = _context.ExamCodes.Include(e => e.Exam).FirstOrDefault(e => e.Code == codeExam);
                if (examCode == null) { throw new Exception("Exam code not valid."); }
                examCodeVM.ExamID = examCode.ExamId;
                examCodeVM.ExamCode = examCode.Code;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
    }
}
