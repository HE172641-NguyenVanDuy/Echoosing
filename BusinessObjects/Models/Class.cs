using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Class
{
    public string ClassId { get; set; } = null!;

    public string? ClassName { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateBy { get; set; }

    public bool? IsDelete { get; set; }

    /// <summary>
    /// code join class mã hóa từ classID
    /// </summary>
    public string? CodeJoinClass { get; set; }

    public virtual ICollection<ClassExam> ClassExams { get; set; } = new List<ClassExam>();

    public virtual ICollection<ClassUser> ClassUsers { get; set; } = new List<ClassUser>();

    public virtual ICollection<CronJobSentMail> CronJobSentMails { get; set; } = new List<CronJobSentMail>();

    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
}
