using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eDereva.Application.Services;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eDereva.Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _audience = configuration["Jwt:Audience"] ??
                                        throw new ArgumentNullException(nameof(configuration),
                                            "Audience cannot be null");

    private readonly string _issuer = configuration["Jwt:Issuer"] ??
                                      throw new ArgumentNullException(nameof(configuration), "Issuer cannot be null");

    private readonly string _secretKey = configuration["Jwt:SecretKey"] ??
                                         throw new ArgumentNullException(nameof(configuration),
                                             "SecretKey cannot be null");
    
    public string GenerateToken(UserDataResponse userData, PermissionFlag permissions)
    {
        if (string.IsNullOrEmpty(userData.Nin))
            throw new ArgumentException("Username cannot be null or empty", nameof(userData.Nin));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("UserID", userData.UserId.ToString()),
            new(ClaimTypes.Sid, userData.Nin),
            new(ClaimTypes.GivenName, userData.GivenName),
            new(ClaimTypes.Surname, userData.Surname),
            new(ClaimTypes.Email, userData.Email),
            new(ClaimTypes.MobilePhone, userData.PhoneNumber),
            new("RefreshTimes", 0.ToString())
        };

        claims.AddRange(Enum.GetValues<PermissionFlag>()
            .Where(permission => permissions.HasFlag(permission) && permission != PermissionFlag.None)
            .Select(permission => new Claim("Permission", permission.ToString())));

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(20), // Using UTC time
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool IsTokenCloseToExpiring(JwtSecurityToken? token)
    {
        ArgumentNullException.ThrowIfNull(token);

        var timeToExpire = (token.ValidTo - DateTime.UtcNow).Duration();
        return timeToExpire < TimeSpan.FromMinutes(5);
    }

    public string RefreshToken(UserDataResponse userData, PermissionFlag permissions, int refreshTimes)
    {
        if (string.IsNullOrEmpty(userData.Nin))
            throw new ArgumentException("Username cannot be null or empty", nameof(userData.Nin));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("UserID", userData.UserId.ToString()),
            new(ClaimTypes.Sid, userData.Nin),
            new(ClaimTypes.GivenName, userData.GivenName),
            new(ClaimTypes.Surname, userData.Surname),
            new(ClaimTypes.Email, userData.Email),
            new(ClaimTypes.MobilePhone, userData.PhoneNumber),
            new("RefreshTimes", 0.ToString())
        };

        claims.AddRange(Enum.GetValues<PermissionFlag>()
            .Where(permission => permissions.HasFlag(permission) && permission != PermissionFlag.None)
            .Select(permission => new Claim("Permission", permission.ToString())));

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(6), // Using UTC time
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}