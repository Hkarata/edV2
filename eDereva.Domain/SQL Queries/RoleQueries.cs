namespace eDereva.Domain.SQL_Queries;

public static class RoleQueries
{
    public static string GetBasicRole =>
        """
            SELECT TOP 1 p.Flags
            FROM [Identity].Roles r
            INNER JOIN [Identity].Permissions p ON r.Id = p.RoleId
            WHERE LOWER(r.Name) = 'basic user';
        """;
}