using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class CronJobSentMail
{
    public string Id { get; set; } = null!;

    public string? ClassId { get; set; }

    public int? ExamId { get; set; }

    public string? UserId { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? ErrorLog { get; set; }

    /// <summary>
    /// 1: Thành công, 2: Thất bại, 0: Chưa gửi 
    /// </summary>
    public int? Status { get; set; }

    public virtual Class? Class { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual User? User { get; set; }
}
