using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Quizlet
{
    public string QuizletId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string QuizletName { get; set; } = null!;

    public DateTime? CreateDate { get; set; }

    public bool? IsDelete { get; set; }

    public string? IsPublic { get; set; }

    public virtual ICollection<Terminology> Terminologies { get; set; } = new List<Terminology>();

    public virtual User User { get; set; } = null!;
}
