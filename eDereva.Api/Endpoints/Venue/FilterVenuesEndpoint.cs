using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.Venue;

public class FilterVenuesEndpoint(IVenueRepository venueRepository)
    : Endpoint<VenueFilterRequest, Results<Ok<PagedResponse<VenueResponse>>, Conflict<string>, NoContent>>
{
    public override void Configure()
    {
        Post("/venues/filter");
        AllowAnonymous();
        Version(2);
        Description(options =>
        {
            options.WithTags("Venue");
            options.WithSummary("Filters venues");
            options.WithDescription("The region Id is a must");
        });
    }

    public override async Task<Results<Ok<PagedResponse<VenueResponse>>, Conflict<string>, NoContent>> ExecuteAsync(
        VenueFilterRequest req, CancellationToken ct)
    {
        if (req.RegionId == Guid.Empty) return TypedResults.Conflict("Region Id is invalid");

        var response = await venueRepository.GetFilteredVenuesAsync(req, ct);

        if (response.Items.Count == 0) return TypedResults.NoContent();

        return TypedResults.Ok(response);
    }
}