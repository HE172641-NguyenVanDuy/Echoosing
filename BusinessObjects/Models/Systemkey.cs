using System;
using System.Collections.Generic;

namespace BusinessObjects.Models;

/// <summary>
/// Lưu trữ các cấu hình chung hoặc danh mục hệ thống.
/// </summary>
public partial class Systemkey
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string? CodeKey { get; set; }

    public string? CodeValue { get; set; }

    public string? Description { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsDelete { get; set; }
}
