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

    public static async Task<string?> LoadFileAsync(string pathToDir, IFormFile file)
    {
        try
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Cloud", pathToDir, file.FileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Path.Combine("Cloud", pathToDir, file.FileName);
        }
        catch
        {
            return null;
        }
    }

    public static string GetFullPathByLogin(string login)
    {
        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Root_{login}");
        return rootPath;
    }
}
