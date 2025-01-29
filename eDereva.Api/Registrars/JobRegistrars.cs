using eDereva.Application.Jobs;
using eDereva.Infrastructure.Jobs;

namespace eDereva.Api.Registrars;

public static class JobRegistrars
{
    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddScoped<ISessionSeederJob, SessionSeederJob>();

        return services;
    }
}