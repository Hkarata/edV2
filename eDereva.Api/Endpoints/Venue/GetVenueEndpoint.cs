using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.Venue;

public class GetVenueEndpoint(IVenueRepository venueRepository)
    : EndpointWithoutRequest<Results<Ok<VenueResponse>, NoContent>>
{
    public override void Configure()
    {
        Get("/venue/{venueId}");
        AllowAnonymous();
        Version(2);
        Description(options =>
        {
            options.WithTags("Venue");
            options.WithSummary("Gets a venue by id");
        });
    }

    public override async Task<Results<Ok<VenueResponse>, NoContent>> ExecuteAsync(CancellationToken ct)
    {
        var venueId = Route<Guid>("venueId");

        var response = await venueRepository.GetVenueAsync(venueId, ct);

        if (response.VenueId == Guid.Empty) return TypedResults.NoContent();

        return TypedResults.Ok(response);
    }
}