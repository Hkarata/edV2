using eDereva.Application.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.Otp;


public class ConfirmOtpEndpoint(IOtpService otpService) : EndpointWithoutRequest<Results<Ok, BadRequest>>
{
    public override void Configure()
    {
        Get("/identity/confirm-otp/{phone-number}/{otp}");
        Version(1);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("Identity")
                .WithSummary("Confirm OTP")
                .WithDescription("Confirm OTP for a phone number");
        });
        Throttle(
            5,
            60,
            "X-Client-Id" // this is optional
        );
    }

    public override async Task<Results<Ok, BadRequest>> ExecuteAsync(CancellationToken ct)
    {
        var phoneNumber = Route<string>("phone-number");
        var otp = Route<string>("otp");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return TypedResults.BadRequest();

        if (string.IsNullOrWhiteSpace(otp))
            return TypedResults.BadRequest();

        var isOtpValid = await otpService.ConfirmOtp(phoneNumber, otp, ct);

        return isOtpValid
            ? (Results<Ok, BadRequest>)TypedResults.Ok()
            : TypedResults.BadRequest();
    }
}