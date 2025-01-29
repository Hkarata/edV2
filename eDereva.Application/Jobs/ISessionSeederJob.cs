namespace eDereva.Application.Jobs;

public interface ISessionSeederJob
{
    Task SeedSessionsAsync(Guid venueId, CancellationToken cancellationToken = default);
}