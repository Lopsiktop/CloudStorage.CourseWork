using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class Link
{
    public int Id { get; set; }

    public int? FileId { get; set; }

    public int? DirectoryId { get; set; }

    public string Url { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public int CanDownload { get; set; }

    public string? PasswordHash { get; set; }

    public virtual Directory? Directory { get; set; }

    public virtual File? File { get; set; }
}
