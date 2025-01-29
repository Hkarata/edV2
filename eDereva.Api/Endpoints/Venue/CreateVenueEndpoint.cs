using eDereva.Application.Jobs;
using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using FastEndpoints;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.Venue;

public class CreateVenueEndpoint(
    IVenueRepository venuesRepository,
    IBackgroundJobClientV2 jobClientV2,
    ILogger<CreateVenueEndpoint> logger)
    : Endpoint<CreateVenueRequest, Results<Created, Conflict<string>, Conflict<List<VenueMatchesResponse>>>>
{
    public override void Configure()
    {
        Post("/venue");
        AllowAnonymous();
        Version(2);
        Description(options =>
        {
            options.WithTags("Venue");
            options.WithSummary("Creates a new venue in the specified district");
        });
    }

    public override async Task<Results<Created, Conflict<string>, Conflict<List<VenueMatchesResponse>>>> ExecuteAsync
        (CreateVenueRequest req, CancellationToken ct)
    {
        logger.LogInformation("Received request to create a venue: {VenueName} in District: {DistrictId}", req.Name,
            req.DistrictId);

        var venueMatched = await venuesRepository.CheckVenueExistsAsync(req.Name, req.DistrictId, ct);

        if (venueMatched)
        {
            logger.LogWarning("Venue creation conflict: {VenueName} already exists in District: {DistrictId}", req.Name,
                req.DistrictId);
            return TypedResults.Conflict("Venue already exists in the specified district");
        }

        var venueMatches = await venuesRepository.CheckVenueMatchesAsync(req.Name, req.DistrictId, ct);

        if (venueMatches.Count != 0)
        {
            logger.LogWarning(
                "Venue creation conflict: {VenueName} has similar matches in District: {DistrictId}. Matches: {Matches}",
                req.Name, req.DistrictId, venueMatches);
            return TypedResults.Conflict(venueMatches);
        }

        try
        {
            await venuesRepository.CreateVenueAsync(req, ct);
            logger.LogInformation("Successfully created venue: {VenueName} in District: {DistrictId}", req.Name,
                req.DistrictId);
            return TypedResults.Created();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create venue: {VenueName} in District: {DistrictId}", req.Name,
                req.DistrictId);
            return TypedResults.Conflict(ex.Message);
        }
    }

    public override async Task OnAfterHandleAsync
    (CreateVenueRequest req, Results<Created, Conflict<string>, Conflict<List<VenueMatchesResponse>>> res,
        CancellationToken ct)
    {
        if (HttpContext.Response.StatusCode != 201)
            return;

        var venueId = await venuesRepository.GetVenueIdByVenueNameAsync(req.Name, ct);

        _ = jobClientV2.Enqueue<ISessionSeederJob>(
            job => job.SeedSessionsAsync(venueId, ct));
    }
}