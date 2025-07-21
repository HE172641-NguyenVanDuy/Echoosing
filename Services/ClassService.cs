using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IClassService
    {
        public string CreateClass(string className, string userID, out string classID);
        public string GetClass(string classID, out Class classInfo);
        public string JoinClass(string classCode, string userID, out Class classInfo);
        public string GetUserInClass(Class classInfo, out List<User> users);
        public string RemoveStudent(string userId, string classId, out string classID);
        public string GetListClassByCreateBy(string userID, out List<Class> listClass);
        public string GetListClassByUserId(string userID, out List<Class> listClass);
        public string RenameClass(string classId, string newName, out string classID);
        public string DeleteClass(string classId);
        public string GetClassForStudent(string userID, out Dictionary<Class, List<ClassExam>> values);
        string GetClassNameById(string classId);
        string GetNumberStudentOfClass(string classId, out int studentCount);
        string GetExamsInClassByClassId(string classId, out List<ClassExam> exams);
        string GetListExamIdInClassByClassId(string classId, out List<int> examIds);
        string GetScoresForClass(string classId, List<int> examIds, out Dictionary<int, List<ExamAttempt>> examAttempts);
    }
    public class ClassService : IClassService
    {
        private readonly EchoosingContext _context;
        private readonly ICodeJoinClassService _codeJoinClassService;
        public ClassService(EchoosingContext context, ICodeJoinClassService codeJoinClassService)
        {
            _context = context;
            _codeJoinClassService = codeJoinClassService;
        }
        public string CreateClass(string className, string userID, out string ClassID)
        {
            string classID = Guid.NewGuid().ToString();
            ClassID = classID;
            try
            {
                _context.Classes.Add(new Class
                {
                    ClassId = classID,
                    ClassName = className,
                    CreateDate = DateTime.Now,
                    CreateBy = userID,
                    IsDelete = false,
                    CodeJoinClass = _codeJoinClassService.Encode(classID)
                });
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }

        public string GetClass(string classID, out Class classInfo)
        {
            classInfo = new Class();
            try
            {
                Class classIn = _context.Classes.Include(c => c.ClassUsers).FirstOrDefault(c => c.ClassId == classID);
                if (classIn != null) { classInfo = classIn; }
                else { throw new Exception("Không có class hợp lệ"); }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
        public string JoinClass(string classCode, string userID, out Class classInfo)
        {
            classInfo = new Class();
            try
            {
                Class classIn = _context.Classes.Include(c => c.ClassUsers).FirstOrDefault(c => c.CodeJoinClass.Trim() == classCode.Trim() && c.IsDelete == false);
                if (classIn == null) throw new Exception("Không có class hợp lệ");
                var user = classIn.ClassUsers.FirstOrDefault(u => u.UserId.Trim() == userID.Trim());
                if (user != null) throw new Exception("You have already taken this class");
                string classUserID = Guid.NewGuid().ToString();
                _context.ClassUsers.Add(new ClassUser
                {
                    ClassUserId = classUserID,
                    ClassId = classIn.ClassId,
                    UserId = userID,
                    CreateDate = DateTime.Now,
                    IsDelete = false
                });
                _context.SaveChanges();
                classInfo = _context.Classes.Include(c => c.ClassUsers).FirstOrDefault(c => c.ClassId == classIn.ClassId);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
        public string GetUserInClass(Class classInfo, out List<User> users)
        {
            users = new List<User>();
            try
            {
                var classUser = classInfo.ClassUsers.Where(x => x.IsDelete == false).ToList();
                foreach (var userInClass in classUser)
                {
                    var user = _context.Users.FirstOrDefault(u => u.UserId == userInClass.UserId);
                    if (user != null) users.Add(user);
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }
        public string RemoveStudent(string classId, string userId, out string classID)
        {
            classID = "";
            try
            {
                var classUserUpdate = _context.ClassUsers.FirstOrDefault(c => c.ClassId == classId && c.UserId == userId);
                if (classUserUpdate != null)
                {
                    classUserUpdate.IsDelete = true;
                    _context.ClassUsers.Update(classUserUpdate);
                    _context.SaveChanges();
                    classID = classUserUpdate.ClassId;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }
        public string GetListClassByCreateBy(string userID, out List<Class> listClass)
        {
            listClass = null;
            try
            {
                listClass = _context.Classes.Where(c => c.CreateBy == userID && c.IsDelete == false).ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string RenameClass(string classId, string newName, out string classID)
        {
            classID = classId;
            try
            {
                var classUpdate = _context.Classes.FirstOrDefault(c => c.ClassId == classId);
                classUpdate.ClassName = newName;
                _context.Classes.Update(classUpdate);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string DeleteClass(string classId)
        {
            try
            {
                var classUpdate = _context.Classes.FirstOrDefault(c => c.ClassId == classId);
                classUpdate.IsDelete = true;
                _context.Classes.Update(classUpdate);
                _context.SaveChanges();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetClassForStudent(string userID, out Dictionary<Class, List<ClassExam>> values)
        {
            values = new Dictionary<Class, List<ClassExam>>();
            try
            {
                var listClass = _context.ClassUsers.Include(c => c.Class).Where(u => u.UserId == userID && u.IsDelete == false).ToList();
                if (listClass.Count == 0) { throw new Exception("You are not attending any classes"); }
                foreach (var item in listClass)
                {
                    var listExam = _context.ClassExams.Include(c => c.Class).Include(c => c.Exam).Where(e => e.ClassId == item.ClassId).ToList();
                    if (listExam.Count == 0)
                        values.Add(item.Class, new List<ClassExam>());
                    else
                        values.Add(item.Class, listExam);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetClassNameById(string classId)
        {
            return _context.Classes
                           .Where(c => c.ClassId == classId)
                           .Select(c => c.ClassName)
                           .FirstOrDefault();
        }

        public string GetNumberStudentOfClass(string classId, out int studentCount)
        {
            studentCount = 0;
            try
            {
                string msg = GetClass(classId, out Class classInfo);
                if (msg.Length > 0) return msg;

                msg = GetUserInClass(classInfo, out List<User> users);
                if (msg.Length > 0) return msg;

                studentCount = users.Count;
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetExamsInClassByClassId(string classId, out List<ClassExam> exams)
        {
            exams = null;
            try
            {
                exams = _context.ClassExams
                .Where(c => c.ClassId.Contains(classId)).ToList();
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string GetListExamIdInClassByClassId(string classId, out List<int> examIds)
        {
            examIds = null;

            try
            {
                examIds = _context.ClassExams
                    .Where(ce => ce.ClassId == classId && ce.IsDelete == false)
                    .Select(ce => ce.ExamId ?? 0)
                    .Distinct()
                    .ToList();

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetScoresForClass(string classId, List<int> examIds, out Dictionary<int, List<ExamAttempt>> examAttempts)
        {
            examAttempts = new Dictionary<int, List<ExamAttempt>>();
            if (examIds == null || examIds.Count == 0) return "No exam IDs provided";
            try
            {
                // Lấy toàn bộ dữ liệu của lớp và danh sách bài kiểm tra tương ứng
                var attempts = _context.ExamAttempts
                    .Where(ea => ea.ClassId == classId && examIds.Contains(ea.ExamId))
                    .ToList();

                // Nhóm dữ liệu theo ExamID
                examAttempts = attempts
                    .GroupBy(e => e.ExamId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string GetListClassByUserId(string userId, out List<Class> listClass)
        {
            listClass = new List<Class>();
            try
            {
                listClass = _context.Classes
                    .Join(_context.ClassUsers,
                        c => c.ClassId,
                        cu => cu.ClassId,
                        (c, cu) => new { Class = c, ClassUser = cu })
                    .Where(joined => joined.ClassUser.UserId == userId)
                    .Select(joined => joined.Class)
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
