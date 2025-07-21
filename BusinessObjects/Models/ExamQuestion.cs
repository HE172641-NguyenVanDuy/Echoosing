using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Xác định danh sách câu hỏi có trong một bài kiểm tra.
/// </summary>
public partial class ExamQuestion
{
    public int ExamQuestionId { get; set; }

    public int ExamId { get; set; }

    public int QuestionId { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public bool? IsDelete { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
