using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class Directory
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public int? OldDirId { get; set; }

    public string? TrashName { get; set; }

    public DateTime? CreationTime { get; set; }

    public virtual ICollection<File> FileDirectories { get; set; } = new List<File>();

    public virtual ICollection<File> FileOldDirs { get; set; } = new List<File>();

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual ICollection<Directory> InverseOldDir { get; set; } = new List<Directory>();

    public virtual ICollection<Directory> InverseParent { get; set; } = new List<Directory>();

    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    public virtual Directory? OldDir { get; set; }

    public virtual Directory? Parent { get; set; }

    public virtual ICollection<User> UserRootDirs { get; set; } = new List<User>();

    public virtual ICollection<User> UserTrashDirs { get; set; } = new List<User>();

    public Directory()
    {
        CreationTime = DateTime.Now;
    }
}
