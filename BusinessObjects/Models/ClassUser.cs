using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

public partial class ClassUser
{
    public string ClassUserId { get; set; } = null!;

    public string? ClassId { get; set; }

    public string? UserId { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsDelete { get; set; }

    public virtual Class? Class { get; set; }

    public virtual User? User { get; set; }
}
