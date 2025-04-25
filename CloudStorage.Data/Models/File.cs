using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class File
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Size { get; set; }

    public int DirectoryId { get; set; }

    public int? OldDirId { get; set; }

    public string? TrashName { get; set; }

    public virtual Directory Directory { get; set; } = null!;

    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    public virtual Directory? OldDir { get; set; }
}
