namespace CloudStorage.WebApi.Utils;

public static class CloudProvider
{
    public static void CreateUserDir(string login)
    {
        var dirPath = Path.Combine(Environment.CurrentDirectory, "Cloud");
        if (!System.IO.Directory.Exists(dirPath))
            System.IO.Directory.CreateDirectory(dirPath);

        var rootPath = Path.Combine(dirPath, $"Root_{login}");
        System.IO.Directory.CreateDirectory(rootPath);
    }

    public static bool CreateFolder(string pathn)
    {
        var path = Path.Combine(Environment.CurrentDirectory, "Cloud", pathn);
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string GetFullPathByLogin(string login)
    {
        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Root_{login}");
        return rootPath;
    }
}
