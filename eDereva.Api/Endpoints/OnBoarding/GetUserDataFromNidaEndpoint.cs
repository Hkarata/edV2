using eDereva.Application.Services;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.DataProtection;
using eDereva.Domain.Loggers;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.OnBoarding;

/// <summary>
///     Endpoint to retrieve user data from NIDA using a hashed National Identification Number (NIN).
/// </summary>
public class GetUserDataFromNidaEndpoint(ILogger<GetUserDataFromNidaEndpoint> logger, INidaService nidaService)
    : EndpointWithoutRequest<Results<Ok<NidaUserDataResponse>, NoContent>>
{
    /// <summary>
    ///     Configures the endpoint settings, including route, versioning, and policies.
    /// </summary>
    public override void Configure()
    {
        Get("/retrieve-user-data-from-nida/{hashedNin}");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("User")
                .WithSummary("Retrieves user data from Nida");
        });
        Throttle(
            10, // Maximum requests allowed
            60, // Time window in seconds
            "X-Client-Id" // Optional client identifier for throttling
        );
    }

    /// <summary>
    ///     Executes the logic to retrieve user data from NIDA based on the hashed NIN.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>HTTP response containing user data or no content if the user is not found.</returns>
    public override async Task<Results<Ok<NidaUserDataResponse>, NoContent>> ExecuteAsync(CancellationToken ct)
    {
        // Retrieve the hashed NIN from the route parameter.
        var hashedNin = Route<string>("hashedNin");

        // Validate the hashed NIN parameter.
        if (string.IsNullOrEmpty(hashedNin))
        {
            logger.LogWarning("Invalid or missing hashedNin parameter.");
            return TypedResults.NoContent();
        }

        try
        {
            // Decrypt the hashed NIN to get the original NIN.
            var nin = RequestParameterDecryption.DecryptData(hashedNin);

            // Retrieve user data from the NIDA service.
            var response = await nidaService.RetrieveUserData(nin, ct);

            logger.LogUserDataRetrievedFromNida(response);
            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            // Log unexpected errors for debugging purposes.
            logger.LogError(ex, "Error occurred while retrieving user data from NIDA.");
            throw;
        }
    }
}