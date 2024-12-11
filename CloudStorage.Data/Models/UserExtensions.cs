namespace CloudStorage.Data.Models;

public partial class User
{
    public void SetPassword(string password)
    {
        PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
    }

    public bool Verify(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, PasswordHash);
    }
}
