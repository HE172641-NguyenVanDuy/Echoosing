using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class Terminology
{
    public string TerminologyId { get; set; } = null!;

    public string QuizletId { get; set; } = null!;

    public string? Academic { get; set; }

    public string? Definition { get; set; }

    public bool? IsDelete { get; set; }

    public virtual Quizlet Quizlet { get; set; } = null!;
}
