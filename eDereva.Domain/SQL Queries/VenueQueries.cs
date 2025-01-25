namespace eDereva.Domain.SQL_Queries;

public static class VenueQueries
{
    public static string CreateVenue =>
        """
                INSERT INTO Core.Venues (Name, Address, ImageUrls, Capacity, DistrictId)
                VALUES (@Name, @Address, @ImageUrls, @Capacity, @DistrictId);
        """;

    public static string CheckVenueExists =>
        """
                SELECT COUNT(*) 
                FROM Core.Venues 
                WHERE LOWER(Name) = LOWER(@VenueName) 
                  AND DistrictId = @DistrictId 
                  AND IsDeleted = 0;
        """;

    public static string CheckVenueNameMatch =>
        """
            SELECT Name
            FROM Core.Venues
            WHERE LOWER(Name) LIKE LOWER('%' + @VenueName + '%')
              AND DistrictId = @DistrictId
              AND IsDeleted = 0;
        """;

    //Full-featured match with language-aware ranking
    public static string CheckVenueNameMatchRanked =>
        """
                WITH VenueMatches AS (
                    SELECT 
                        Name,
                        CASE
                            -- Exact matches (highest priority)
                            WHEN Name COLLATE SQL_Latin1_General_CP1_CI_AI = @VenueName THEN 100
                            
                            -- Prefix matches
                            WHEN Name COLLATE SQL_Latin1_General_CP1_CI_AI LIKE @VenueName + '%' THEN 90
                            
                            -- Contains matches
                            WHEN Name COLLATE SQL_Latin1_General_CP1_CI_AI LIKE '%' + @VenueName + '%' THEN 80
                            
                            -- Swahili variations matches
                            WHEN REPLACE(REPLACE(REPLACE(
                                    Name COLLATE SQL_Latin1_General_CP1_CI_AI,
                                    'dh', 'd'),
                                    'th', 't'),
                                    'ng''', 'ng') = 
                                 REPLACE(REPLACE(REPLACE(
                                    @VenueName,
                                    'dh', 'd'),
                                    'th', 't'),
                                    'ng''', 'ng') THEN 85
                            
                            -- Phonetic matching as fallback
                            WHEN SOUNDEX(Name) = SOUNDEX(@VenueName) THEN 70
                            ELSE 0
                        END as MatchScore
                    FROM Core.Venues
                    WHERE DistrictId = @DistrictId
                      AND IsDeleted = 0
                )
                SELECT Name, MatchScore
                FROM VenueMatches
                WHERE MatchScore > 0
                ORDER BY MatchScore DESC;
        """;

    public static string GetVenueDetailsById =>
        """
                SELECT 
                    v.Id as VenueId, v.Name as VenueName, v.Address, v.ImageUrls,v.Capacity,
                    d.Id as DistrictId, d.Name as DistrictName,
                    r.Id as RegionId, r.Name as RegionName
                FROM Core.Venues v
                INNER JOIN Locale.Districts d ON v.DistrictId = d.Id
                INNER JOIN Locale.Regions r ON d.RegionId = r.Id
                WHERE v.Id = @VenueId
                  AND v.IsDeleted = 0
                  AND d.IsDeleted = 0
                  AND r.IsDeleted = 0;
        """;

    public static string GetFilteredVenues =>
        """
                WITH FilteredVenues AS (
                    SELECT 
                        v.Id as VenueId, v.Name as VenueName, v.Address, v.ImageUrls, v.Capacity,
                        d.Id as DistrictId, d.Name as DistrictName,
                        COUNT(*) OVER() as TotalCount,
                        ROW_NUMBER() OVER(ORDER BY 
                            CASE 
                                WHEN @OrderBy = 'name' THEN v.Name 
                                WHEN @OrderBy = 'capacity' THEN CAST(v.Capacity as NVARCHAR(20))
                            END
                        ) as RowNum
                    FROM Core.Venues v
                    INNER JOIN Locale.Districts d ON v.DistrictId = d.Id
                    WHERE d.RegionId = @RegionId
                        AND v.IsDeleted = 0 
                        AND d.IsDeleted = 0
                        AND (@DistrictId IS NULL OR d.Id = @DistrictId)
                        AND (@MinCapacity IS NULL OR v.Capacity >= @MinCapacity)
                        AND (@MaxCapacity IS NULL OR v.Capacity <= @MaxCapacity)
                        AND (@SearchTerm IS NULL 
                            OR v.Name COLLATE SQL_Latin1_General_CP1_CI_AI LIKE '%' + @SearchTerm + '%'
                            OR v.Address COLLATE SQL_Latin1_General_CP1_CI_AI LIKE '%' + @SearchTerm + '%'
                        )
                )
                SELECT 
                    VenueId, VenueName, Address, ImageUrls, Capacity,
                    DistrictId, DistrictName,
                    TotalCount
                FROM FilteredVenues
                WHERE RowNum BETWEEN @Offset + 1 AND @Offset + @PageSize
        """;

    public static string SoftDeleteVenue =>
        """
                UPDATE Core.Venues 
                SET IsDeleted = 1,
                    ModifiedAt = GETUTCDATE()
                WHERE Id = @VenueId
                  AND IsDeleted = 0;
        """;
    
    public static string UndoSoftDeleteVenue =>
        """
                UPDATE Core.Venues 
                SET IsDeleted = 0,
                    ModifiedAt = GETUTCDATE()
                WHERE Id = @VenueId
                  AND IsDeleted = 1;
        """;

    public static string DeleteVenue =>
        "Delete FROM Core.Venues WHERE Id = @VenueId;";
}