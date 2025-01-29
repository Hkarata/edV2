using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;

namespace eDereva.Application.Repositories;

public interface ISessionRepository
{
    Task<PagedResponse<SessionResponse>> GetFilteredSessionsAsync(SessionFilterRequest request,
        CancellationToken cancellationToken);
}