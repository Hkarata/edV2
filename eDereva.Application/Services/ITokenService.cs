using System.IdentityModel.Tokens.Jwt;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.Enums;

namespace eDereva.Application.Services;

public interface ITokenService
{
    string GenerateToken(UserDataResponse userData,
        PermissionFlag permissions);

    bool IsTokenCloseToExpiring(JwtSecurityToken? token);

    string RefreshToken(UserDataResponse userData,
        PermissionFlag permissions, int refreshTimes);
}