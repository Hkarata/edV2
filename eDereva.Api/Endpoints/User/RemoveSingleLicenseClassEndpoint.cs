using eDereva.Application.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.User;

public class RemoveSingleLicenseClassEndpoint(
    ILicenseClassRepository licenseClassRepository,
    ILogger<RemoveSingleLicenseClassEndpoint> logger)
    : EndpointWithoutRequest<Results<Ok, Conflict>>
{
    public override void Configure()
    {
        Delete("/users/{userId}/remove-license-class/{licenseClassId}");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("User");
            options.WithSummary("Removes a selected license class.");
        });
    }

    public override async Task<Results<Ok, Conflict>> ExecuteAsync(CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        var licenseClassId = Route<short>("licenseClassId");

        // Log the received userId and licenseClassId
        logger.LogInformation("Received request to remove license class {LicenseClassId} for user {UserId}.",
            licenseClassId, userId);


        try
        {
            await licenseClassRepository.RemoveLicenseClassAsync(userId, licenseClassId, ct);

            logger.LogInformation("Successfully removed license class {LicenseClassId} for user {UserId}.",
                licenseClassId, userId);
            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the process
            logger.LogError(ex, "An error occurred while removing license class {LicenseClassId} for user {UserId}.",
                licenseClassId, userId);
            return TypedResults.Conflict();
        }
    }
}