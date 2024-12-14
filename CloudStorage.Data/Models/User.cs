using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IsAdmin { get; set; }

    public int RootDirId { get; set; }

    public int TrashDirId { get; set; }

    public int IsBan { get; set; }

    public virtual ICollection<History> Histories { get; set; } = new List<History>();

    public virtual Directory RootDir { get; set; } = null!;

    public virtual Directory TrashDir { get; set; } = null!;
}
