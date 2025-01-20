namespace eDereva.Domain.SQL_Queries;

public static class LicenseQueries
{
    public static string GetUserLicenseClasses =>
        """
                        SELECT 
                            lc.LicenseClassID,
                            lc.ClassName,
                            lc.Description
                        FROM [Identity].UserLicenseClasses ulc
                        INNER JOIN [Identity].LicenseClasses lc ON ulc.LicenseClassID = lc.LicenseClassID
                        WHERE ulc.UserID = @UserID
        """;

    public static string AddSingleLicenseClass =>
        """
                                INSERT INTO [Identity].UserLicenseClasses (UserID, LicenseClassID)
                                SELECT @UserID, @LicenseClassID
                                WHERE NOT EXISTS (
                                    SELECT 1 
                                    FROM [Identity].UserLicenseClasses 
                                    WHERE UserID = @UserID 
                                    AND LicenseClassID = @LicenseClassID
                                )
        """;

    public static string DeleteUserLicenseClass =>
        """
                    DELETE FROM [Identity].UserLicenseClasses 
                    WHERE UserID = @UserID 
                    AND LicenseClassID = @LicenseClassID
        """;



}