using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.User;

public class AddSingleLicenseClassEndpoint(
    ILicenseClassRepository licenseClassRepository,
    ILogger<AddSingleLicenseClassEndpoint> logger)
    : Endpoint<AddSingleLicenseClassRequest, Results<Created, Conflict<string>>>
{
    public override void Configure()
    {
        Post("/user/{userId}/add-single-License-class");
        AllowAnonymous();
        Version(2);
        Description(options =>
        {
            options.WithTags("User");
            options.WithSummary("Adds single license class.");
        });
    }


    public override async Task<Results<Created, Conflict<string>>> ExecuteAsync(AddSingleLicenseClassRequest req,
        CancellationToken ct)
    {
        var userId = Route<Guid>("userId");

        try
        {
            logger.LogInformation("Processing request to add license class {LicenseClassId} for user {UserId}.", req,
                userId);
            await licenseClassRepository.AddLicenseClassAsync(userId, req.LicenseClassId, ct);

            logger.LogInformation("Successfully added license class {LicenseClassId} for user {UserId}.", req, userId);
            return TypedResults.Created();
        }
        catch (Exception e)
        {
            logger.LogWarning("License class {LicenseClassId} already exists for user {UserId}.", req, userId);
            return TypedResults.Conflict($"License class {req} is already associated with the user.");
        }
    }
}