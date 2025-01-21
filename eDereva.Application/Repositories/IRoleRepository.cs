using eDereva.Domain.Enums;

namespace eDereva.Application.Repositories;

public interface IRoleRepository
{
    Task<PermissionFlag> GetBasicRolePermissionFlag(CancellationToken cancellationToken);
}