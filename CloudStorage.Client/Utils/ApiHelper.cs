using CloudStorage.Client.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Windows.Controls;

namespace CloudStorage.Client.Utils;

public static class ApiHelper
{
    private static string _url = "http://localhost:5044/api/";

    private static HttpClient _http = new HttpClient();

    public static void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<Result<bool>> LoginAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync(_url + "User/Login", model);
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await SessionHandler.SaveSessionAsync(token);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            return new Result<bool>("Неверный логин или пароль");
        }

        return new Result<bool>(response.IsSuccessStatusCode);
    }

    public static async Task<Result<bool>> RegisterAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync(_url + "User/Register", model);
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await SessionHandler.SaveSessionAsync(token);
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
}
