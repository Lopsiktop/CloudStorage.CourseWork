using CloudStorage.Client.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CloudStorage.Client.Utils;

public static class ApiHelper
{
    private static string _url = "http://localhost:5044/api/";

    private static HttpClient _http = new HttpClient();

    public static async Task<Result<bool>> LoginAsync(LoginModel model)
    {
        var response = await _http.PostAsJsonAsync(_url + "User/Login", model);
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
}
