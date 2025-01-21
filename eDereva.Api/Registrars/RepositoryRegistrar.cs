using eDereva.Application.Repositories;
using eDereva.Infrastructure.Repository;

namespace eDereva.Api.Registrars;

public static class RepositoryRegistrar
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ILicenseClassRepository, LicenseClassRepository>();

        services.AddScoped<IVenueRepository, VenueRepository>();

        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }
}