using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAssignmentService
    {
        string CreateAssignment(ClassExam classExam);
    }
    public class AssignmentService : IAssignmentService
    {
        private readonly EchoosingContext _context;
        private readonly IClassService _classService;
        public AssignmentService(EchoosingContext context, IClassService classService)
        {
            _context = context;
            _classService = classService;
        }

        public string CreateAssignment(ClassExam classExam)
        {
            try
            {
                string msg = _classService.GetClass(classExam.ClassId, out Class classInfo);
                if (msg.Length > 0) { throw new Exception(msg); }
                msg = _classService.GetUserInClass(classInfo, out List<User> users);
                if (msg.Length > 0) { throw new Exception(msg); }
                foreach (User user in users)
                {
                    _context.CronJobSentMails.Add(new CronJobSentMail
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClassId = classExam.ClassId,
                        ExamId = classExam.ExamId,
                        UserId = user.UserId,
                        Status = 0,
                    });
                }
                _context.ClassExams.Add(classExam);
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
