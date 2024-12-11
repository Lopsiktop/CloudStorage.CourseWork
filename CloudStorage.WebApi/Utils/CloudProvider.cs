using Microsoft.OpenApi.Validations;
using System.IO.Compression;

namespace CloudStorage.WebApi.Utils;

public static class CloudProvider
{
    private static void defineFolders()
    {
        var cloud = Path.Combine(Environment.CurrentDirectory, "Cloud");
        if(!Directory.Exists(cloud))
            Directory.CreateDirectory(cloud);

        var zips = Path.Combine(Environment.CurrentDirectory, "Zips");
        if(!Directory.Exists(zips))
            Directory.CreateDirectory(zips);
    }

    public static void CreateUserDir(string login)
    {
        defineFolders();

        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Root_{login}");
        Directory.CreateDirectory(rootPath);
    }

    public static void CreateTrashDir(string login)
    {
        defineFolders();

        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Trash_{login}");
        Directory.CreateDirectory(rootPath);
    }

    public static bool CreateFolder(string pathn)
    {
        defineFolders();

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
        defineFolders();

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

    public static void MoveDirectoryToBin(string trash, string oldPath)
    {
        var path = Path.Combine(Environment.CurrentDirectory, "Cloud", trash, Guid.NewGuid().ToString());
        if (Directory.Exists(oldPath))
            Directory.Move(oldPath, path);
    }

    public static void DeleteDirectory(string path)
    {
        defineFolders();

        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path);
        if (Directory.Exists(fullPath))
            Directory.Delete(fullPath, true);
    }
    public static void DeleteFile(string path, string fileName)
    {
        defineFolders();

        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path, fileName);
        if (File.Exists(fullPath)) 
            File.Delete(fullPath);
    }

    public static void RenameFile(string path, string fileName, string newName)
    {
        defineFolders();

        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path, fileName);
        var newPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path, newName);
        if (File.Exists(fullPath))
            File.Move(fullPath, newPath);
    }

    public static void RenameDirectory(string path, string newName)
    {
        defineFolders();

        var dir = Path.GetFileName(path);
        var npath = Path.Combine(path.Remove(path.Length - dir.Length, dir.Length), newName);
        var fullPath = Path.Combine(Environment.CurrentDirectory, "Cloud", path);
        var newPath = Path.Combine(Environment.CurrentDirectory, "Cloud", npath);
        if (Directory.Exists(fullPath))
            Directory.Move(fullPath, newPath);
    }

    public static string GetFilePath(string dirPath, string fileName)
    {
        defineFolders();

        return Path.Combine(Environment.CurrentDirectory, "Cloud", dirPath, fileName);
    }

    public static string GetFolderPath(string dirPath)
    {
        defineFolders();

        return Path.Combine(Environment.CurrentDirectory, "Cloud", dirPath);
    }

    public static string CreateArchive(string folderPath)
    {
        defineFolders();

        var folderName = Path.GetFileName(folderPath);
        var archivePath = Path.Combine(Environment.CurrentDirectory, "Zips", folderName + ".zip");

        if (File.Exists(archivePath))
            File.Delete(archivePath);

        ZipFile.CreateFromDirectory(folderPath, archivePath);
        return archivePath;
    }

    public static string GetFullPathByLogin(string login)
    {
        defineFolders();

        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Root_{login}");
        return rootPath;
    }
}
