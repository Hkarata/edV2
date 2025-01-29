using eDereva.Application.Repositories;
using eDereva.Application.Services;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.DataProtection;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.User;

public class AuthenticationEndpoint(
    IUserRepository userRepository,
    IPasswordService passwordService,
    ITokenService tokenService,
    IRoleRepository roleRepository)
    : Endpoint<AuthenticationRequest, Results<Ok<string>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        Post("/user/authenticate");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("User");
            options.WithSummary("Authentication");
        });
    }

    public override async Task<Results<Ok<string>, UnauthorizedHttpResult>> ExecuteAsync(AuthenticationRequest req,
        CancellationToken ct)
    {
        var unHashedClientPassword = RequestParameterDecryption.DecryptData(req.ClientHashedPassword);

        var hashedPassword = await userRepository.Authenticate(req.PhoneNumber, ct);

        if (string.IsNullOrEmpty(hashedPassword)) return TypedResults.Unauthorized();

        var result = passwordService.VerifyHashedPassword(hashedPassword, unHashedClientPassword);

        if (!result) return TypedResults.Unauthorized();

        var userData = await userRepository.GetUserData(req.PhoneNumber, ct);

        var basicFlag = await roleRepository.GetBasicRolePermissionFlag(ct);

        var accessToken = tokenService.GenerateToken(userData, basicFlag);

        return TypedResults.Ok(accessToken);
    }
}