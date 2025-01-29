using eDereva.Application.Repositories;
using FastEndpoints;

namespace eDereva.Api.Endpoints.Venue;

public class SoftDeleteVenueEndpoint(IVenueRepository venueRepository, ILogger<SoftDeleteVenueEndpoint> logger)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/venues/{venueId}/soft-delete");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithName("SoftDelete");
            options.WithTags("Venue");
            options.WithSummary("Soft deletes a venue.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var venueId = Route<Guid>("venueId");

        try
        {
            logger.LogInformation("Starting soft delete for venue with ID: {VenueId}", venueId);

            await venueRepository.SoftDeleteVenueAsync(venueId, ct);

            logger.LogInformation("Successfully soft deleted venue with ID: {VenueId}", venueId);
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while soft deleting venue with ID: {VenueId}", venueId);
            await SendErrorsAsync(500, ct);
        }
    }
}