namespace eDereva.Domain.SQL_Queries;

public static class SessionQueries
{
    public static string GetFilteredSessions =>
        """
                WITH SessionsCTE AS (
                    SELECT 
                        s.Id, s.Date,
                        CASE s.Status
                            WHEN 0 THEN 'Active'
                            WHEN 1 THEN 'Cancelled'
                            WHEN 2 THEN 'Scheduled'
                            WHEN 3 THEN 'Completed'
                            ELSE 'Unknown'
                        END AS Status,
                        s.StartTime, s.EndTime,
                        v.Capacity - s.Capacity as AvailableSeats,
                        s.VenueId, s.ContingencyId,
                        v.Name as VenueName, v.Address as VenueAddress,
                        CASE c.ContingencyType
                            WHEN 0 THEN 'None'
                            WHEN 1 THEN 'Power Failure'
                            WHEN 2 THEN 'System Crash'
                            WHEN 3 THEN 'Network Issue'
                            WHEN 4 THEN 'Natural Disaster'
                            WHEN 5 THEN 'Security Breach'
                            WHEN 6 THEN 'Pandemic'
                            WHEN 7 THEN 'Other'
                            ELSE 'Unknown'
                        END as ContingencyType,
                        c.ContingencyExplanation,
                        d.Name as DistrictName,
                        r.Name as RegionName,
                        COUNT(*) OVER() as TotalCount
                    FROM Core.Sessions s
                    INNER JOIN Core.Venues v ON s.VenueId = v.Id
                    INNER JOIN Locale.Districts d ON v.DistrictId = d.Id
                    INNER JOIN Locale.Regions r ON d.RegionId = r.Id
                    LEFT JOIN Core.Contingencies c ON s.ContingencyId = c.Id
                    WHERE 
                        s.IsDeleted = 0
                        AND v.IsDeleted = 0
                        AND d.IsDeleted = 0
                        AND r.IsDeleted = 0
                        AND (@VenueId IS NULL OR s.VenueId = @VenueId)
                        AND (@RegionId IS NULL OR d.RegionId = @RegionId)
                        AND (@DistrictId IS NULL OR v.DistrictId = @DistrictId)
                        AND s.Date >= @DateFrom
                )
                SELECT 
                    *,
                    CAST(CEILING(CAST(TotalCount AS FLOAT) / @PageSize) AS INT) as TotalPages
                FROM SessionsCTE
                ORDER BY Date DESC, StartTime
                OFFSET (@Page - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;
        """;
}