using eDereva.Application.Context;
using eDereva.Application.Repositories;
using eDereva.Domain.Enums;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;

namespace eDereva.Infrastructure.Repository;

public class RoleRepository (IDatabaseContext context) : IRoleRepository
{
    public async Task<PermissionFlag> GetBasicRolePermissionFlag(CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(RoleQueries.GetBasicRole);
        
        var flag = await context.ExecuteScalarAsync(sqlCommand, cancellationToken);
        
        return (PermissionFlag)flag;
    }
}