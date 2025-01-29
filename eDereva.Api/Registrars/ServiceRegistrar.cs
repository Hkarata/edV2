using eDereva.Application.Services;
using eDereva.Infrastructure.Services;

namespace eDereva.Api.Registrars;

public static class ServiceRegistrar
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<INidaService, NidaService>();

        services.AddSingleton<IPasswordService, PasswordService>();

        services.AddSingleton<ITokenService, TokenService>();

        services.AddScoped<IOtpService, OtpService>();

        services.AddScoped<ISmsService, SmsService>();

        return services;
    }
}