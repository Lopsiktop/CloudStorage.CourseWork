using System.IO;

namespace CloudStorage.Client.Utils;

public static class SessionHandler
{
    public static void LogoutSession()
    {
        if(File.Exists("session.txt"))
            File.Delete("session.txt");
    }
    public static async Task SaveSessionAsync(string token)
    {
        if (!File.Exists("session.txt"))
            File.Create("session.txt").Close();

        await File.WriteAllTextAsync("session.txt", token);
    }

    public static async Task<string?> GetSessionAsync()
    {
        if (!File.Exists("session.txt"))
            return null;

        var token = await File.ReadAllTextAsync("session.txt");
        return token;
    }
}
