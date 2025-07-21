using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ClassRepsitory : IClassRepository
    {
        private readonly EchoosingContext _context;

        public ClassRepsitory(EchoosingContext context)
        {
            _context = context;
        }

        public void CreateClass(string className, string userID, string codeJoinClass, String classId)
        {
                _context.Classes.Add(new Class
                {
                    ClassId = classId,
                    ClassName = className,
                    CreateDate = DateTime.Now,
                    CreateBy = userID,
                    IsDelete = false,
                    CodeJoinClass = codeJoinClass
                });
                _context.SaveChanges();
        }

        public void DeleteClass(Class classDeleted)
        {
            classDeleted.IsDelete = true;
            _context.Classes.Update(classDeleted);
            _context.SaveChanges();
        }

        public Class FindClassById(string classId)
        {
            return _context.Classes.FirstOrDefault(c => c.ClassId == classId);
        }

        public object GetClassByClassAndUserId(string classId, string userId)
        {
            return _context.ClassUsers.FirstOrDefault(c => c.ClassId == classId && c.UserId == userId);
        }

        public Class GetClassById(string id)
        {
            return _context.Classes.Include(c => c.ClassUsers).FirstOrDefault(c => c.ClassId == id);
        }

        public Class? GetClassWithUsersByCode(string classCode)
        {
            return _context.Classes
                .Include(c => c.ClassUsers)
                .FirstOrDefault(c => c.CodeJoinClass.Trim() == classCode.Trim() && c.IsDelete == false);
        }

        public List<Class> GetListClassByCreateBy(string userId)
        {
            return _context.Classes.Where(c => c.CreateBy == userId && c.IsDelete == false).ToList();
        }

        public void RenameClass(string classId, string newName, out string classID)
        {
            classID = classId;
            try
            {
                var classUpdate = FindClassById(classId);
                classUpdate.ClassName = newName;
                _context.Classes.Update(classUpdate);
                _context.SaveChanges();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
