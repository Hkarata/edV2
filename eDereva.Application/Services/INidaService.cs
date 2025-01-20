using eDereva.Domain.Contracts.Responses;

namespace eDereva.Application.Services;

public interface INidaService
{
    Task<NidaUserDataResponse> RetrieveUserData(string nin, CancellationToken cancellationToken);
}