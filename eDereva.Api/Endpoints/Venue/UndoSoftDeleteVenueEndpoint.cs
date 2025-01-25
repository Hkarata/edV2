using eDereva.Application.Repositories;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace eDereva.Api.Endpoints.Venue;

public class UndoSoftDeleteVenueEndpoint(IVenueRepository venueRepository, ILogger<UndoSoftDeleteVenueEndpoint> logger)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Patch("/venues/{venueId}");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("Venue");
            options.WithSummary("Undo Soft deletes a venue.");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var venueId = Route<Guid>("venueId");

        try
        {
            logger.LogInformation("Starting undo soft delete for venue with ID: {VenueId}", venueId);
            
            await venueRepository.UnSoftDeleteVenueAsync(venueId, ct);

            logger.LogInformation("Successfully undid soft deleted venue with ID: {VenueId}", venueId);
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while undoing soft deleting venue with ID: {VenueId}", venueId);
            await SendErrorsAsync(500, ct);
        }
    }
}