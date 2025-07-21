using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Lưu lại phương án trả lời của người dùng trong một lần làm bài.
/// </summary>
public partial class Answer
{
    /// <summary>
    /// Câu trả lời của người dùng
    /// </summary>
    public int AnswerId { get; set; }

    public string AttemptId { get; set; } = null!;

    public int QuestionId { get; set; }

    public int OptionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ExamAttempt Attempt { get; set; } = null!;

    public virtual Option Option { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
