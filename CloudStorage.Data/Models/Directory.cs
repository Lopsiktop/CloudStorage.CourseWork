using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class Directory
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<Directory> InverseParent { get; set; } = new List<Directory>();

    public virtual ICollection<Link> Links { get; set; } = new List<Link>();

    public virtual Directory? Parent { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
