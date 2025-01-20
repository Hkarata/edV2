using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.User;

public class GetUserLicenseClassesEndpoint(
    ILicenseClassRepository licenseClassRepository,
    ILogger<GetUserLicenseClassesEndpoint> logger)
    : EndpointWithoutRequest<Results<Ok<List<LicenseClassInfo>>, NoContent>>
{
    public override void Configure()
    {
        Get("/user/{userId}/selected-license-classes");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("User");
            options.WithSummary("Gets all selected user's license classes.");
        });
    }

    public override async Task<Results<Ok<List<LicenseClassInfo>>, NoContent>> ExecuteAsync(CancellationToken ct)
    {
        var userId = Route<Guid>("userId");

        try
        {
            logger.LogInformation("Fetching license classes for UserId: {UserId}", userId);

            var response = await licenseClassRepository.GetUserLicensesAsync(userId, ct);

            if (response.Count == 0)
                logger.LogWarning("No license classes found for UserId: {UserId}", userId);
            else
                logger.LogInformation("{Count} license classes found for UserId: {UserId}", response.Count, userId);

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching license classes for UserId: {UserId}", userId);
            return TypedResults.NoContent();
        }
    }
}