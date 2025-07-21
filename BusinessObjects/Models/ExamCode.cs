using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Mã Code cho từng đề thi
/// </summary>
public partial class ExamCode
{
    public string CodeId { get; set; } = null!;

    public int ExamId { get; set; }

    public string Code { get; set; } = null!;

    /// <summary>
    /// Thời gian hết hạn
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Đã dùng hay chưa
    /// </summary>
    public bool? IsUsed { get; set; }

    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Thời gian mở
    /// </summary>
    public DateTime? TimeStart { get; set; }

    public virtual Exam Exam { get; set; } = null!;
}
