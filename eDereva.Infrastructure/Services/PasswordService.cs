using System.Globalization;
using System.Security.Cryptography;
using eDereva.Application.Services;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace eDereva.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int IterCount = 100_000;

    public string HashPassword(string password)
    {
        var hashedPassword = HashPassword(password, RandomNumberGenerator.Create());
        return hashedPassword.Length switch
        {
            0 => "null",
            1 => "empty",
            _ => Convert.ToHexString(hashedPassword)
        };
    }

    public bool VerifyHashedPassword(string hashedPassword, string password)
    {
        var bytesValue = GetByteArrayFromStringValue(hashedPassword);
        return VerifyHashedPassword(bytesValue, password, out _, out _);
    }

    private static byte[] HashPassword(string password, RandomNumberGenerator rng,
        KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA512, int iterCount = IterCount, int saltSize = 128 / 8,
        int numBytesRequested = 256 / 8)
    {
        // Produce a version 3 (see comment above) text hash.
        var salt = new byte[saltSize];
        rng.GetBytes(salt);
        var subKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subKey.Length];
        outputBytes[0] = 0x01; // format marker
        WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
        WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subKey, 0, outputBytes, 13 + saltSize, subKey.Length);
        return outputBytes;
    }

    private static bool VerifyHashedPassword(byte[] hashedPassword, string password, out int iterCount,
        out KeyDerivationPrf prf)
    {
        iterCount = default;
        prf = default;

        try
        {
            // Read header information
            prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
            iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
            var saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

            // Read the salt: must be >= 128 bits
            if (saltLength < 128 / 8) return false;
            var salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

            // Read the subKey (the rest of the payload): must be >= 128 bits
            var subKeyLength = hashedPassword.Length - 13 - salt.Length;
            if (subKeyLength < 128 / 8) return false;
            var expectedSubKey = new byte[subKeyLength];
            Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubKey, 0, expectedSubKey.Length);

            // Hash the incoming password and verify it
            var actualSubKey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subKeyLength);

            return CryptographicOperations.FixedTimeEquals(actualSubKey, expectedSubKey);
        }
        catch
        {
            // This should never occur except in the case of a malformed payload
            return false;
        }
    }

    private static uint ReadNetworkByteOrder(IReadOnlyList<byte> buffer, int offset)
    {
        return ((uint)buffer[offset + 0] << 24)
               | ((uint)buffer[offset + 1] << 16)
               | ((uint)buffer[offset + 2] << 8)
               | buffer[offset + 3];
    }

    private static void WriteNetworkByteOrder(IList<byte> buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }

    private static byte[] GetByteArrayFromStringValue(string stringValue)
    {
        return stringValue switch
        {
            "null" => [],
            "empty" => [0],
            _ => ConvertHexStringToByteArray(stringValue)
        };
    }

    private static byte[] ConvertHexStringToByteArray(string hexString)
    {
        if (hexString.Length % 2 != 0) throw new ArgumentException("Hex string must have an even length");

        var length = hexString.Length / 2;
        var byteArray = new byte[length];

        for (var i = 0; i < length; i++)
            byteArray[i] = byte.Parse(hexString.Substring(i * 2, 2), NumberStyles.HexNumber);

        return byteArray;
    }
}