using CloudStorage.Client.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Windows.Controls;
using System.Xml.Linq;

namespace CloudStorage.Client.Utils;

public static class ApiHelper
{
    private static string _url = "http://127.0.0.1:5044/api/";

    private static HttpClient _http = new HttpClient();

    public static void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<Result<FilterReturnDto>> Search(string searchField, int searchType, int dirId)
    {
        var response = await _http.GetAsync(_url + $"Filter/Search?searchField={searchField}&searchType={searchType}&dirId={dirId}");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FilterReturnDto>();
            return new Result<FilterReturnDto>(result);
        }

        var error = await response.Content.ReadAsStringAsync();
        return new Result<FilterReturnDto>($"Не удалось выполнить поиск; ({error})");
    }

    public static async Task<Result<UserDto>> LoginAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync(_url + "User/Login", model);
        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            UserHandler.IsAdmin = user.IsAdmin;

            if (user.IsAdmin)
            {
                AdminApi.SetToken(user.Token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
                await SessionHandler.SaveSessionAsync(user.Token);
            }

            return new Result<UserDto>(user);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return new Result<UserDto>("Неверный логин или пароль");
        }
    }

    public static async Task<Result<bool>> RegisterAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync(_url + "User/Register", model);
        if (response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
            UserHandler.IsAdmin = user.IsAdmin;
            await SessionHandler.SaveSessionAsync(user.Token);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            if (error == "This login is busy")
                return new Result<bool>("Данный логин занят");
        }

        return new Result<bool>(response.IsSuccessStatusCode);
    }

    public static async Task<Result<ReturnUserDto>> GetMeAsync()
    {
        var response = await _http.GetAsync(_url + "User/Me");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<ReturnUserDto>();
            return new Result<ReturnUserDto>(content);
        }

        return new Result<ReturnUserDto>("Ошибка сервера");
    }

    public static async Task<Result<List<DirStructureDto>>> GetStructure()
    {
        var response = await _http.GetAsync(_url + "Directory/GetStructure");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<DirStructureDto>>();
            return new Result<List<DirStructureDto>>(content);
        }

        return new Result<List<DirStructureDto>>("Ошибка сервера");
    }

    public static async Task<Result<List<ReturnDirDto>>> GetDirsByFolderId(int dirId)
    {
        var response = await _http.GetAsync(_url + $"Directory/GetDirsByDirId/{dirId}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<ReturnDirDto>>();
            return new Result<List<ReturnDirDto>>(content);
        }

        return new Result<List<ReturnDirDto>>("Ошибка сервера");
    }

    public static async Task<Result<List<ReturnFileDto>>> GetFilesByFolderId(int dirId)
    {
        var response = await _http.GetAsync(_url + $"File/GetFilesByDirId/{dirId}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<ReturnFileDto>>();
            return new Result<List<ReturnFileDto>>(content);
        }

        return new Result<List<ReturnFileDto>>("Ошибка сервера");
    }

    public static async Task<Result<ReturnDirDto>> CreateDirectory(CreateDirDto model)
    {
        var response = await _http.PostAsJsonAsync(_url + "Directory", model);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<ReturnDirDto>();
            return new Result<ReturnDirDto>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<ReturnDirDto>("Ошибка сервера");
        else
            return new Result<ReturnDirDto>(error);
    }

    public static async Task<Result<ReturnFileDto>> LoadFiles(string file, int dirId)
    {
        using (var form = new MultipartFormDataContent())
        {
            using (var fs = File.OpenRead(file))
            {
                using (var streamContent = new StreamContent(fs))
                {
                    using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                    {
                        form.Add(fileContent, "File", Path.GetFileName(file));
                        form.Add(new StringContent(dirId.ToString()), "DirId");

                        var response = await _http.PostAsync(_url + "File", form);
                        if (response.IsSuccessStatusCode)
                        {
                            var dto = await response.Content.ReadFromJsonAsync<ReturnFileDto>();
                            return new Result<ReturnFileDto>(dto);
                        }

                        var error = await response.Content.ReadAsStringAsync();
                        return new Result<ReturnFileDto>(error);
                    }
                }
            }
        }
    }

    public static async Task<Result<bool>> DeleteFile(int fileID)
    {
        var response = await _http.DeleteAsync(_url + "File/" + fileID.ToString());
        if (response.IsSuccessStatusCode)
            return new Result<bool>(true);

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> DeleteDirectory(int dirId)
    {
        var response = await _http.DeleteAsync(_url + "Directory/" + dirId.ToString());
        if (response.IsSuccessStatusCode)
            return new Result<bool>(true);

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<ReturnFileDto>> RenameFile(int fileId, string name)
    {
        var response = await _http.PostAsJsonAsync(_url + "File/Rename", new RenameFileDto(fileId, name));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<ReturnFileDto>();
            return new Result<ReturnFileDto>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<ReturnFileDto>("Ошибка сервера");
        else
            return new Result<ReturnFileDto>(error);
    }

    public static async Task<Result<ReturnDirDto>> RenameDirectory(int dirId, string name)
    {
        var response = await _http.PostAsJsonAsync(_url + "Directory/Rename", new RenameDirectoryDto(dirId, name));
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<ReturnDirDto>();
            return new Result<ReturnDirDto>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<ReturnDirDto>("Ошибка сервера");
        else
            return new Result<ReturnDirDto>(error);
    }

    public static async Task<Result<bool>> DownloadFile(int fileId, string path)
    {
        var response = await _http.GetAsync(_url + "File/Download/" + fileId.ToString());
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using (var fs = new FileStream(path, FileMode.CreateNew))
            {
                await stream.CopyToAsync(fs);
            }

            return new Result<bool>(true);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> DownloadFolder(int dirId, string path)
    {
        var response = await _http.GetAsync(_url + "Directory/Download/" + dirId.ToString());
        if (response.IsSuccessStatusCode)
        {
            using var stream = await response.Content.ReadAsStreamAsync();
            using (var fs = new FileStream(path, FileMode.CreateNew))
            {
                await stream.CopyToAsync(fs);
            }

            return new Result<bool>(true);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> MoveDirToBin(int dirId)
    {
        var response = await _http.PostAsync(_url + "Directory/Bin/" + dirId.ToString(), null);
        if (response.IsSuccessStatusCode)
        {
            return new Result<bool>(true);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> MoveFileToBin(int fileId)
    {
        var response = await _http.PostAsync(_url + "File/Bin/" + fileId.ToString(), null);
        if (response.IsSuccessStatusCode)
        {
            return new Result<bool>(true);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> RefreshFileFromBin(int fileId)
    {
        var response = await _http.PostAsync(_url + "File/Refresh/" + fileId.ToString(), null);
        if (response.IsSuccessStatusCode)
        {
            return new Result<bool>(true);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> RefreshDirectoryFromBin(int dirId)
    {
        var response = await _http.PostAsync(_url + "Directory/Refresh/" + dirId.ToString(), null);
        if (response.IsSuccessStatusCode)
        {
            return new Result<bool>(true);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<List<ReturnHistoryDto>>> GetAllHistory()
    {
        var response = await _http.GetAsync(_url + "History/All");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<ReturnHistoryDto>>();
            return new Result<List<ReturnHistoryDto>>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<List<ReturnHistoryDto>>("Ошибка сервера");
        else
            return new Result<List<ReturnHistoryDto>>(error);
    }

    public static async Task<Result<List<ReturnHistoryDto>>> GetFileHistory(int id)
    {
        var response = await _http.GetAsync(_url + "History/File/" + id.ToString());
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<ReturnHistoryDto>>();
            return new Result<List<ReturnHistoryDto>>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<List<ReturnHistoryDto>>("Ошибка сервера");
        else
            return new Result<List<ReturnHistoryDto>>(error);
    }

    public static async Task<Result<List<ReturnHistoryDto>>> GetFolderHistory(int id)
    {
        var response = await _http.GetAsync(_url + "History/Directory/" + id.ToString());
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<ReturnHistoryDto>>();
            return new Result<List<ReturnHistoryDto>>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<List<ReturnHistoryDto>>("Ошибка сервера");
        else
            return new Result<List<ReturnHistoryDto>>(error);
    }

    public static async Task<Result<bool>> CheckBan()
    {
        var response = await _http.GetAsync(_url + "User/Ban");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<UserDto>();
            return new Result<bool>(content.IsBan);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<FileProperties>> GetFileProperties(int fileId)
    {
        var response = await _http.GetAsync(_url + "File/Properties/" + fileId.ToString());
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<FileProperties>();
            return new Result<FileProperties>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<FileProperties>("Ошибка сервера");
        else
            return new Result<FileProperties>(error);
    }

    public static async Task<Result<FolderProperties>> GetFolderProperties(int dirId)
    {
        var response = await _http.GetAsync(_url + "Directory/Properties/" + dirId.ToString());
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<FolderProperties>();
            return new Result<FolderProperties>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<FolderProperties>("Ошибка сервера");
        else
            return new Result<FolderProperties>(error);
    }
}
