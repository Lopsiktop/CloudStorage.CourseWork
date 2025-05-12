using CloudStorage.WebApi.DTOs;
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
    
    public static string MoveDirectoryToBin(string trash, string oldPath)
    {
        var name = Guid.NewGuid().ToString();
        var path = Path.Combine(Environment.CurrentDirectory, "Cloud", trash, name);
        if (Directory.Exists(oldPath))
            Directory.Move(oldPath, path);

        return name;
    }
    public static string MoveFileToBin(string trash, string oldPath)
    {
        var ext = Path.GetExtension(oldPath);
        var name = Guid.NewGuid().ToString() + ext;
        var path = Path.Combine(Environment.CurrentDirectory, "Cloud", trash, name);
        if (File.Exists(oldPath))
            File.Move(oldPath, path);

        return name;
    }

    public static string? ReturnFileFromBin(string trashPath, string newPath)
    {
        if(File.Exists(trashPath))
        {
            if (File.Exists(newPath))
            {
                var name = Guid.NewGuid().ToString() + Path.GetExtension(newPath);
                var fileName = Path.GetFileName(newPath);
                var path = Path.Combine(newPath.Remove(newPath.Length - fileName.Length, fileName.Length), name);
                File.Move(trashPath, path);
                return name;
            }

            var defName = Path.GetFileName(newPath);
            File.Move(trashPath, newPath);
            return defName;
        }

        return null;
    }

    public static string? ReturnDirectoryFromBin(string trashPath, string newPath)
    {
        if (Directory.Exists(trashPath))
        {
            if (Directory.Exists(newPath))
            {
                var name = Guid.NewGuid().ToString();
                var fileName = Path.GetFileName(newPath);
                var path = Path.Combine(newPath.Remove(newPath.Length - fileName.Length, fileName.Length), name);
                Directory.Move(trashPath, path);
                return name;
            }

            var defName = Path.GetFileName(newPath);
            Directory.Move(trashPath, newPath);
            return defName;
        }

        return null;
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

    public static string CreateArchive(string folderPath, string? toPath = null)
    {
        defineFolders();

        var folderName = Path.GetFileName(folderPath);
        string archivePath = "";

        if (toPath == null)
            archivePath = Path.Combine(Environment.CurrentDirectory, "Zips", folderName + ".zip");
        else
            archivePath = toPath;

        if (File.Exists(archivePath))
            File.Delete(archivePath);

        ZipFile.CreateFromDirectory(folderPath, archivePath);
        return archivePath;
    }

    public static string ExtractArchive(string filePath)
    {
        defineFolders();

        if (!File.Exists(filePath))
            throw new Exception();

        var fileName = Path.GetFileName(filePath);
        var dirName = Path.GetFileNameWithoutExtension(filePath);
        var rootPath = filePath.Remove(filePath.Length - fileName.Length, fileName.Length);
        var destPath = Path.Combine(rootPath, dirName);

        ZipFile.ExtractToDirectory(filePath, destPath);
        return destPath;
    }

    public static List<ReturnFileDto> GetFiles(string dirPath)
    {
        var files = System.IO.Directory.GetFiles(dirPath);
        var list = new List<ReturnFileDto>();

        foreach (var file in files)
        {
            var info = new FileInfo(file);

            list.Add(new ReturnFileDto(0, info.Name, info.Length));
        }

        return list;
    }

    public static List<ReturnDirDto> GetFolders(string dirPath)
    {
        var dirs = System.IO.Directory.GetDirectories(dirPath);
        var list = new List<ReturnDirDto>();

        foreach (var dir in dirs)
        {
            var name = Path.GetFileName(dir);
            list.Add(new ReturnDirDto(0, name));
        }

        return list;
    }

    public static string GetFullPathByLogin(string login)
    {
        defineFolders();

        var rootPath = Path.Combine(Environment.CurrentDirectory, "Cloud", $"Root_{login}");
        return rootPath;
    }
}
