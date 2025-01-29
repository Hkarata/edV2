using eDereva.Application.Repositories;
using FastEndpoints;

namespace eDereva.Api.Endpoints.Venue;

public class DeleteVenueEndpoint(IVenueRepository venueRepository, ILogger<DeleteVenueEndpoint> logger)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/venues/{venueId}");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("Venue");
            options.WithSummary("Deletes a venue.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var venueId = Route<Guid>("venueId");

        try
        {
            logger.LogInformation("Starting delete for venue with ID: {VenueId}", venueId);

            await venueRepository.DeleteVenueAsync(venueId, ct);

            logger.LogInformation("Successfully deleted venue with ID: {VenueId}", venueId);
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting venue with ID: {VenueId}", venueId);
            await SendErrorsAsync(500, ct);
        }
    }
}