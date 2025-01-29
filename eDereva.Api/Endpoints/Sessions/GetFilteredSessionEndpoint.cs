using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.Sessions;

public class GetFilteredSessionEndpoint(
    ISessionRepository sessionRepository,
    ILogger<GetFilteredSessionEndpoint> logger)
    : Endpoint<SessionFilterRequest, Results<Ok<PagedResponse<SessionResponse>>, Conflict<string>, NoContent>>
{
    public override void Configure()
    {
        Post("/sessions/getFilteredSession");
        Version(2);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("Sessions");
            options.WithSummary("Get filtered sessions");
        });
    }

    public override async Task<Results<Ok<PagedResponse<SessionResponse>>, Conflict<string>, NoContent>> ExecuteAsync(
        SessionFilterRequest req, CancellationToken ct)
    {
        if (req.RegionId == Guid.Empty || req.DistrictId == Guid.Empty || req.DateFrom == default)
            return TypedResults.Conflict("RegionId, DistrictId, and DateFrom must be provided.");

        var sessions = await sessionRepository.GetFilteredSessionsAsync(req, ct);

        if (sessions.Items.Count == 0) return TypedResults.NoContent();

        return TypedResults.Ok(sessions);
    }
}