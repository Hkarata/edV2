namespace eDereva.Application.Services;

public interface IPasswordService
{
    /// <summary>
    ///     Hashes the given password using PBKDF2 with a random salt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>A hex string representing the hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    ///     Verifies if the provided password matches the hashed password.
    /// </summary>
    /// <param name="hashedPassword">The stored hashed password as a hex string.</param>
    /// <param name="password">The password to verify.</param>
    /// <returns>True if the password matches the hash, otherwise false.</returns>
    bool VerifyHashedPassword(string hashedPassword, string password);
}