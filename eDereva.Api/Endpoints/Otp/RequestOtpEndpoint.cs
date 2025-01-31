using eDereva.Application.Services;
using eDereva.Domain.Contracts.Requests;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace eDereva.Api.Endpoints.Otp;


public class RequestOtpEndpoint(IOtpService otpService, ISmsService smsService)
    : EndpointWithoutRequest<Results<Ok, BadRequest>>
{
    public override void Configure()
    {
        Get("identity/request-otp/{phone-number}");
        Version(1);
        AllowAnonymous();
        Description(options =>
        {
            options.WithTags("Identity")
                .WithSummary("Request OTP")
                .WithDescription("Request an OTP for a phone number");
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

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return TypedResults.BadRequest();

        var otp = await otpService.GenerateOtpAsync(phoneNumber, ct);

        if (string.IsNullOrEmpty(otp))
            return TypedResults.BadRequest();

        var phone = phoneNumber.StartsWith("0") ? "255" + phoneNumber.Substring(1) : "255" + phoneNumber;


        var sms = new Sms
        {
            source_addr = "RSAllies",
            schedule_time = string.Empty,
            encoding = "0",
            message =
                $"Hello, your OTP is {otp}. Use it before it expires.\nHabari, OTP yako ni {otp}. Tumia kabla muda wake hujaisha.",
            recipients =
            [
                new Recipient { recipient_id = "1", dest_addr = phone }
            ]
        };

        await smsService.SendMessageAsync(sms);

        return TypedResults.Ok();
    }
}