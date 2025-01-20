using System.Security.Cryptography;
using System.Text;

namespace eDereva.Domain.DataProtection;

public static class RequestParameterDecryption
{
    private const string SecretKey = "Gf2*Xt9!Vkq8@dCm";

    public static string DecryptData(string encryptedBase64)
    {
        try
        {
            // Convert URL-safe Base64 back to standard Base64
            encryptedBase64 = encryptedBase64.Replace('_', '/').Replace('-', '+');
            
            var secretKey = SecretKey.PadRight(32, '*');
            var combinedBytes = Convert.FromBase64String(encryptedBase64);
            
            var iv = new byte[16];
            var encryptedData = new byte[combinedBytes.Length - 16];
            
            Array.Copy(combinedBytes, 0, iv, 0, 16);
            Array.Copy(combinedBytes, 16, encryptedData, 0, encryptedData.Length);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(secretKey);
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(encryptedData);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            throw new Exception("Decryption failed: " + ex.Message);
        }
    }
}