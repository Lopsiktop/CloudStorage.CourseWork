using Microsoft.OpenApi.Validations;

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

    public static void DeleteDirectory(string path)
    {
        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path);
        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, true);
    }
    public static void DeleteFile(string path, string fileName)
    {
        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path, fileName);
        if (File.Exists(fullPath)) 
            File.Delete(fullPath);
    }

    public static void RenameFile(string path, string fileName, string newName)
    {
        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path, fileName);
        var newPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path, newName);
        if (File.Exists(fullPath))
            File.Move(fullPath, newPath);
    }

    public static void RenameDirectory(string path, string newName)
    {
        var dir = Path.GetFileName(path);
        var npath = Path.Combine(path.Remove(path.Length - dir.Length, dir.Length), newName);
        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path);
        var newPath = Path.Combine(Environment.CurrentDirectory, "Cloud", npath);
        if (Directory.Exists(fullPath))
            Directory.Move(fullPath, newPath);
    }

    public static string GetFullPathByLogin(string login)
    {
        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Root_{login}");
        return rootPath;
    }
}
