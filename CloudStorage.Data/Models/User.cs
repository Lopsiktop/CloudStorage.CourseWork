using System;
using System.Collections.Generic;

namespace CloudStorage.Data.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int StorageVolume { get; set; }

    public int RootDirId { get; set; }

    public virtual Directory RootDir { get; set; } = null!;

    public void SetPassword(string password)
    {
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
    }

    public bool Verify(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, PasswordHash);
    }
}
