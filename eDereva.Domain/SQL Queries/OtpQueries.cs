namespace eDereva.Domain.SQL_Queries;

public static class OtpQueries
{
    public static string CheckIfOtpCodeExists =>
        "SELECT IIF((SELECT COUNT(*) FROM [Identity].Otps WHERE LOWER(Code) = LOWER(@OtpCode)) > 0, CAST(0 AS BIT), CAST(1 AS BIT)) AS IsUnique;";

    public static string ConfirmOtpCode =>
        """
                IF EXISTS (
                    SELECT 1
                    FROM [Identity].Otps
                    WHERE PhoneNumber = @PhoneNumber
                      AND LOWER(Code) = LOWER(@OtpCode)
                      AND ExpiresAt > SYSUTCDATETIME()
                      AND IsUsed = 0
                )
                    BEGIN
                        -- Mark OTP as used and update audit fields
                        UPDATE [Identity].Otps
                        SET IsUsed = 1,
                            LastModifiedAt = SYSUTCDATETIME(),
                            LastModifiedBy = 'System' -- Change this based on your audit requirements
                        WHERE PhoneNumber = @PhoneNumber
                          AND Code = @OtpCode;
                
                        SELECT CAST(1 AS BIT) AS IsValid; -- OTP was valid and updated
                    END
                ELSE
                    BEGIN
                        SELECT CAST(0 AS BIT) AS IsValid; -- OTP was invalid or expired
                    END;
        """;

    public static string CreateOtpCode =>
        """
            DECLARE @Id UNIQUEIDENTIFIER = NEWSEQUENTIALID();
            DECLARE @ExpiresAt DATETIME2(7) = DATEADD(MINUTE, 5, SYSUTCDATETIME()); -- OTP expires in 5 minutes
            
            INSERT INTO dbo.Otps (Id, Code, PhoneNumber, ExpiresAt, IsUsed, AttemptCount, CreatedAt, CreatedBy)
            VALUES (@Id, @OtpCode, @PhoneNumber, @ExpiresAt, 0, 0, SYSUTCDATETIME(), 'System');
        """;
}