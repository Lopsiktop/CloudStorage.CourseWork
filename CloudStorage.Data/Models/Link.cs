using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class Link
{
    public int Id { get; set; }

    public int? FileId { get; set; }

    public int? DirId { get; set; }

    public string Code { get; set; } = null!;

    public virtual Directory? Dir { get; set; }

    public virtual File? File { get; set; }
}
