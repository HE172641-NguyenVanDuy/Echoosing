using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Lịch sử làm bài của người dùng
/// </summary>
public partial class ExamAttempt
{
    /// <summary>
    /// Lịch sử làm bài của người dùng
    /// </summary>
    public string AttemptId { get; set; } = null!;

    public string? UserId { get; set; }

    public int ExamId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public decimal? Score { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Status lấy trong bảng systemkey [9]
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Tham gia kiểm tra bằng code
    /// </summary>
    public string? ExamCode { get; set; }

    /// <summary>
    /// Tên tham gia bằng code
    /// </summary>
    public string? UserName { get; set; }

    public string? ClassId { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Class? Class { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual User? User { get; set; }
}
