using System.Text;
using eDereva.Application.Jobs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eDereva.Infrastructure.Jobs;

public class SessionSeederJob(IConfiguration configuration, ILogger<SessionSeederJob> logger)
    : ISessionSeederJob
{
    private readonly string _connectionString = configuration.GetConnectionString("AppDbConnection")
                                                ?? throw new ArgumentNullException(nameof(configuration));

    public async Task SeedSessionsAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting session seeding for venue {VenueId}", venueId);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = connection.BeginTransaction();
        try
        {
            var currentYear = DateTime.UtcNow.Year;
            var startDate = new DateTime(currentYear, 1, 1);
            var endDate = new DateTime(currentYear, 12, 31);
            var tanzanianHolidays = GetTanzanianHolidays(currentYear);

            var timeSlots = new[]
            {
                (start: new TimeSpan(8, 0, 0), end: new TimeSpan(10, 0, 0)),
                (start: new TimeSpan(11, 0, 0), end: new TimeSpan(13, 0, 0)),
                (start: new TimeSpan(14, 0, 0), end: new TimeSpan(16, 0, 0)),
                (start: new TimeSpan(17, 0, 0), end: new TimeSpan(19, 0, 0)),
                (start: new TimeSpan(20, 0, 0), end: new TimeSpan(22, 0, 0))
            };

            var sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine(
                "INSERT INTO Core.Sessions (Id, Date, Status, StartTime, EndTime, Capacity, IsDeleted, ModifiedAt, VenueId, ContingencyId) VALUES");

            var first = true;
            var totalRecords = 0;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Session seeding cancelled");
                    break;
                }

                if (date.DayOfWeek == DayOfWeek.Sunday || tanzanianHolidays.Contains(date))
                    continue;

                foreach (var (start, end) in timeSlots)
                {
                    if (!first)
                        sqlBuilder.AppendLine(",");

                    sqlBuilder.Append('(');
                    sqlBuilder.Append("NEWID(), "); // Id
                    sqlBuilder.Append($"'{date:yyyy-MM-dd}', "); // Date
                    sqlBuilder.Append("2, "); // Status (Scheduled)
                    sqlBuilder.Append($@"'{start:hh\:mm\:ss}', "); // StartTime
                    sqlBuilder.Append($@"'{end:hh\:mm\:ss}', "); // EndTime
                    sqlBuilder.Append("50, "); // Capacity
                    sqlBuilder.Append("0, "); // IsDeleted
                    sqlBuilder.Append("GETUTCDATE(), "); // ModifiedAt
                    sqlBuilder.Append($"'{venueId}', "); // VenueId
                    sqlBuilder.Append("NULL"); // ContingencyId
                    sqlBuilder.Append(')');

                    first = false;
                    totalRecords++;

                    // Execute in batches of 1000
                    if (sqlBuilder.Length <= 30000)
                        continue;

                    await using var command = new SqlCommand(sqlBuilder.ToString(), connection, transaction);
                    var inserted = await command.ExecuteNonQueryAsync(cancellationToken);
                    logger.LogInformation("Inserted {Count} sessions", inserted);

                    sqlBuilder.Clear();
                    sqlBuilder.AppendLine(
                        "INSERT INTO Core.Sessions (Id, Date, Status, StartTime, EndTime, Capacity, IsDeleted, ModifiedAt, VenueId, ContingencyId) VALUES");
                    first = true;
                }
            }

            // Execute any remaining inserts
            if (!first)
            {
                await using var command = new SqlCommand(sqlBuilder.ToString(), connection, transaction);
                var inserted = await command.ExecuteNonQueryAsync(cancellationToken);
                logger.LogInformation("Inserted final batch of {Count} sessions", inserted);
            }

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Successfully seeded {Count} sessions for venue {VenueId}", totalRecords, venueId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding sessions for venue {VenueId}", venueId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static HashSet<DateTime> GetTanzanianHolidays(int year)
    {
        var holidays = new HashSet<DateTime>
        {
            new(year, 1, 1), // New Year's Day
            new(year, 1, 12), // Zanzibar Revolution Day
            new(year, 4, 7), // Sheikh Abeid Karume Day
            new(year, 4, 26), // Union Day
            new(year, 5, 1), // Workers Day
            new(year, 7, 7), // Saba Saba Day
            new(year, 8, 8), // Nane Nane Day
            new(year, 10, 14), // Nyerere Day
            new(year, 12, 9), // Independence Day
            new(year, 12, 25), // Christmas Day
            new(year, 12, 26) // Boxing Day
        };

        // Add Easter-related holidays (these need to be calculated for each year)
        var easter = GetEasterSunday(year);
        holidays.Add(easter.AddDays(-2)); // Good Friday
        holidays.Add(easter.AddDays(1)); // Easter Monday

        return holidays;
    }

    private static DateTime GetEasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateTime(year, month, day);
    }
}