namespace eDereva.Application.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string phoneNumber, CancellationToken ct);
    Task<bool> ConfirmOtp(string phoneNumber, string otp, CancellationToken ct);
}