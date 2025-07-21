using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ClassExam
{
    public string ClassExamId { get; set; } = null!;

    public string? ClassId { get; set; }

    public int? ExamId { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateUser { get; set; }

    public bool? IsDelete { get; set; }

    public virtual Class? Class { get; set; }

    public virtual Exam? Exam { get; set; }
}
