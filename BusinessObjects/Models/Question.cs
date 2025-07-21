using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Lưu thông tin về từng câu hỏi trắc nghiệm.
/// </summary>
public partial class Question
{
    /// <summary>
    /// Câu hỏi
    /// </summary>
    public int QuestionId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
