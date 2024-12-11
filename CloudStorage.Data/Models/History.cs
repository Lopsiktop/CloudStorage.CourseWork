using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class History
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ActionType { get; set; }

    public string Text { get; set; } = null!;

    public int? FileId { get; set; }

    public int? DirectoryId { get; set; }

    public virtual Directory? Directory { get; set; }

    public virtual File? File { get; set; }

    public virtual User User { get; set; } = null!;
}
