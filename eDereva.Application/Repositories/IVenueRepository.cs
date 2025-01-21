using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;

namespace eDereva.Application.Repositories;

public interface IVenueRepository
{
    Task<bool> CheckVenueExistsAsync(string venueName, Guid districtId, CancellationToken cancellationToken);

    Task<List<VenueMatchesResponse>> CheckVenueMatchesAsync(string venueName, Guid districtId,
        CancellationToken cancellationToken);

    Task CreateVenueAsync(CreateVenueRequest request, CancellationToken cancellationToken);

    Task<VenueResponse> GetVenueAsync(Guid venueId, CancellationToken cancellationToken);

    Task<PagedResponse<VenueResponse>> GetFilteredVenuesAsync(
        VenueFilterRequest request,
        CancellationToken cancellationToken);
}