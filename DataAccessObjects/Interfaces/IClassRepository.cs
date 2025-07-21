using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IClassRepository
    {
        void CreateClass(string className, string userID, string codeJoinClass, String classId);
        Class GetClassById(string id);
        Class? GetClassWithUsersByCode(string classCode);

        object GetClassByClassAndUserId(string classId, string userId);

        List<Class> GetListClassByCreateBy(string userId);

        Class FindClassById(string classId);

        void DeleteClass(Class c);

        void RenameClass(string classId, string newName, out string classIs);

        
    }
}
