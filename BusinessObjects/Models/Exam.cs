using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Lưu trữ thông tin về bài kiểm tra.
/// </summary>
public partial class Exam
{
    /// <summary>
    /// Lưu trữ thông tin về bài kiểm tra.
    /// </summary>
    public int ExamId { get; set; }

    /// <summary>
    /// Tên bài kiểm tra
    /// </summary>
    public string ExamName { get; set; } = null!;

    /// <summary>
    /// Thời gian làm bài (phút)
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Tổng số câu hỏi
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Thời gian tạo ra bài kiểm tra
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Thời gian update bài kiểm tra
    /// </summary>
    public DateTime UpdatedDate { get; set; }

    /// <summary>
    /// Mô tả bài thi
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// FK với Users
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Thời gian bắt đầu bài kiểm tra
    /// </summary>
    public DateTime? TimeStart { get; set; }

    /// <summary>
    /// Có cho làm lại bài kiểm tra hay không
    /// </summary>
    public bool? IsRetake { get; set; }

    /// <summary>
    /// Xóa
    /// </summary>
    public bool? IsDelete { get; set; }

    public virtual ICollection<ClassExam> ClassExams { get; set; } = new List<ClassExam>();

    public virtual ICollection<CronJobSentMail> CronJobSentMails { get; set; } = new List<CronJobSentMail>();

    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();

    public virtual ICollection<ExamCode> ExamCodes { get; set; } = new List<ExamCode>();

    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
}
