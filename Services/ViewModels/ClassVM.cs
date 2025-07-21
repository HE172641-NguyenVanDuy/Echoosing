using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ViewModels
{
    public class ClassVM
    {
    }
    public class CreateClassRequest
    {
        public string ClassName { get; set; }
    }

    public class RenameClassRequest
    {
        public string NewName { get; set; }
    }

    public class JoinClassRequest
    {
        public string ClassId { get; set; }
        public string UserId { get; set; }
        public string? ClassCode { get; set; } // nếu cần kiểm tra mã lớp
    }
}
