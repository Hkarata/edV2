using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Loggers;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.OnBoarding;

public class CheckIfUserExistsEndpoint(IUserRepository userRepository, ILogger<CheckIfUserExistsEndpoint> logger)
    : Endpoint<UserCheckRequest, Results<Ok, NoContent>>
{
    public override void Configure()
    {
        Post("/users/check-if-user-exists");
        AllowAnonymous();
        Version(2);
        Description(options =>
        {
            options.WithTags("OnBoarding");
            options.WithSummary("Checks if user exists");
        });
        Throttle(
            10, // Maximum requests allowed
            60, // Time window in seconds
            "X-Client-Id" // Optional client identifier for throttling
        );
    }

    public override async Task<Results<Ok, NoContent>> ExecuteAsync(UserCheckRequest req, CancellationToken ct)
    {
        logger.LogCheckingIfUserExists(req);

        var result = await userRepository.CheckIfUserExists(req.Nin, req.Email, req.Phone, ct);

        if (result) 
            return TypedResults.Ok();

        return TypedResults.NoContent();
    }
}