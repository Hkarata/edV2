using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Loggers;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.User;

public class CreateUserEndpoint(IUserRepository userRepository, ILogger<CreateUserEndpoint> logger)
    : Endpoint<CreateUserRequest, Results<Created, BadRequest<string>>>
{
    public override void Configure()
    {
        Post("users");
        AllowAnonymous();
        Version(2);
        Description(options =>
        {
            options.WithTags("User");
            options.WithSummary("Creates a new user");
        });
        Throttle(
            10, // Maximum requests allowed
            60, // Time window in seconds
            "X-Client-Id" // Optional client identifier for throttling
        );
    }

    public override async Task<Results<Created, BadRequest<string>>> ExecuteAsync(CreateUserRequest req,
        CancellationToken ct)
    {
        // Log the incoming request data
        logger.LogInformation("Received request to create user with email: {Email}.", req.Email);
        
        try
        {
            await userRepository.CreateUser(req, ct);
            
            logger.LogUserCreated(req);
            return TypedResults.Created();
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}