using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Thông tin Người dùng
/// </summary>
public partial class User
{
    /// <summary>
    /// Thông tin Người dùng
    /// </summary>
    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    /// <summary>
    /// Mật khẩu đã Hash
    /// </summary>
    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Lấy Role từ bảng systemkey [1]
    /// </summary>
    public int Role { get; set; }

    /// <summary>
    /// OTP đổi mật khẩu
    /// </summary>
    public string? Otp { get; set; }

    /// <summary>
    /// Thời gian hết hạn OTP
    /// </summary>
    public DateTime? OtpexpirationTime { get; set; }

    public virtual ICollection<ClassUser> ClassUsers { get; set; } = new List<ClassUser>();

    public virtual ICollection<CronJobSentMail> CronJobSentMails { get; set; } = new List<CronJobSentMail>();

    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();

    public virtual ICollection<Quizlet> Quizlets { get; set; } = new List<Quizlet>();
}
