namespace eDereva.Domain.SQL_Queries;

public static class UserQueries
{
    public static string CheckIfUserExists =>
        """
            SELECT 1
            FROM [Identity].Users
            WHERE (@NationalID IS NULL OR LowerNationalID = LOWER(@NationalID))
               OR (@Email IS NULL OR LowerEmail = LOWER(@Email))
               OR (@PhoneNumber IS NULL OR LowerPhoneNumber = LOWER(@PhoneNumber));
        """;

    public static string InsertUser =>
        """
            BEGIN
                INSERT INTO [Identity].Users
                    (NationalID, NationalIDType, FirstName, MiddleName, Surname, Sex, DateOfBirth, Email, PhoneNumber, PasswordHash, CreatedAt)
                VALUES
                    (@NationalID, @NationalIDType, @FirstName, @MiddleName, @Surname, @Sex, @DateOfBirth, @Email, @PhoneNumber, @PasswordHash, GETDATE());
            END
        """;

    public static string AuthenticateUser =>
        """
            SELECT 
                PasswordHash 
            FROM [Identity].Users 
            WHERE LowerPhoneNumber = @PhoneNumber;
        """;

    public static string GetUserDetails =>
        """
            SELECT 
                UserID, NationalID, FirstName as GivenName, Surname, PhoneNumber, COALESCE(Email, 'unknown email') as Email 
            FROM [Identity].Users 
            WHERE LowerPhoneNumber = @PhoneNumber;
        """;
}