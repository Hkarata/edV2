using eDereva.Domain.Contracts.Responses;

namespace eDereva.Application.Repositories;

public interface ILicenseClassRepository
{
    Task BulkInsertUserLicensesAsync(Guid userId, List<short> licenseClassIds,
        CancellationToken cancellationToken);

    Task<List<LicenseClassInfo>> GetUserLicensesAsync(Guid userId, CancellationToken cancellationToken);

    Task AddLicenseClassAsync(Guid userId, int licenseClassId, CancellationToken cancellationToken);

    Task RemoveLicenseClassAsync(Guid userId, int licenseClassId, CancellationToken cancellationToken);
}