using System.Net.Http.Headers;
using System.Net.Http;
using CloudStorage.Client.Models;
using System.Net.Http.Json;

namespace CloudStorage.Client.Utils;

public static class AdminApi
{
    private static string _url = "http://127.0.0.1:5044/api/";

    private static HttpClient _http = new HttpClient();

    public static void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<Result<List<AdminUserDto>>> GetUsersAdmin()
    {
        var response = await _http.GetAsync(_url + "Admin/Users");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<List<AdminUserDto>>();
            return new Result<List<AdminUserDto>>(content);
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<List<AdminUserDto>>("Ошибка сервера");
        else
            return new Result<List<AdminUserDto>>(error);
    }

    public static async Task<Result<UserDto>> GetUserToken(int userId)
    {
        var response = await _http.GetAsync(_url + "Admin/User/" + userId.ToString());
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return new Result<UserDto>(new UserDto(content, false, false));
        }

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<UserDto>("Ошибка сервера");
        else
            return new Result<UserDto>(error);
    }

    public static async Task<Result<bool>> Ban(int userId)
    {
        var response = await _http.GetAsync(_url + "Admin/User/Ban/" + userId.ToString());
        if (response.IsSuccessStatusCode)
            return new Result<bool>(true);

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> Unban(int userId)
    {
        var response = await _http.GetAsync(_url + "Admin/User/Unban/" + userId.ToString());
        if (response.IsSuccessStatusCode)
            return new Result<bool>(true);

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> Admin(int userId)
    {
        var response = await _http.GetAsync(_url + "Admin/User/Admin/" + userId.ToString());
        if (response.IsSuccessStatusCode)
            return new Result<bool>(true);

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }

    public static async Task<Result<bool>> Unadmin(int userId)
    {
        var response = await _http.GetAsync(_url + "Admin/User/Unadmin/" + userId.ToString());
        if (response.IsSuccessStatusCode)
            return new Result<bool>(true);

        var error = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(error))
            return new Result<bool>("Ошибка сервера");
        else
            return new Result<bool>(error);
    }
}
