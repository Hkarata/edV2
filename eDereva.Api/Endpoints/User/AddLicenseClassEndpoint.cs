using eDereva.Application.Repositories;
using eDereva.Domain.DataProtection;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.User;

public class AddLicenseClassEndpoint (ILicenseClassRepository licenseClassRepository, ILogger<AddLicenseClassEndpoint> logger) 
    : Endpoint<List<short>, Results<Created, BadRequest<string>>>
{
    public override void Configure()
    {
        Post("/user/{userId}/addLicenseClass");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("User");
            options.WithSummary("Adds selected license class(s) to user");
        });
    }

    public override async Task<Results<Created, BadRequest<string>>> ExecuteAsync(List<short> req, CancellationToken ct)
    {
        var userId = Route<Guid>("userId");
        logger.LogInformation("Starting to add license classes for UserId: {UserId}", userId);
        
        try
        {
            logger.LogInformation("License Class IDs to be added: {LicenseClasses}", string.Join(",", req));
            await licenseClassRepository.BulkInsertUserLicensesAsync(userId, req, ct);

            logger.LogInformation("Successfully added license classes to UserId: {UserId}", userId);
            return TypedResults.Created();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding license classes to UserId: {UserId}", userId);

            return TypedResults.BadRequest("An error occurred while adding license classes.");
        }
    }
}