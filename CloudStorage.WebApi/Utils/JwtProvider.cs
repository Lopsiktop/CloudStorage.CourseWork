using CloudStorage.Data.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CloudStorage.WebApi.Utils;

public class JwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(User user)
    {
        var claims = new Claim[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        };

        var security = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var signIn = new SigningCredentials(security, SecurityAlgorithms.HmacSha256);
        var sec = new JwtSecurityToken(
            claims: claims,
            signingCredentials: signIn,
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:Expires"))
        );

        return new JwtSecurityTokenHandler().WriteToken(sec);
    }
}
