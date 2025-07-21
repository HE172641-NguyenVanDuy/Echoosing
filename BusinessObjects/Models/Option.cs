using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// phương án trả lời cho từng câu hỏi.
/// </summary>
public partial class Option
{
    /// <summary>
    /// Phương án trả lời cho từng câu hỏi.
    /// </summary>
    public int OptionId { get; set; }

    public int? QuestionId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? IsCorrect { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Question? Question { get; set; }
}
