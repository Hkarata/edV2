using System.Text.Json;
using eDereva.Application.Context;
using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;

namespace eDereva.Infrastructure.Repository;

public class VenueRepository(IDatabaseContext context) : IVenueRepository
{
    public async Task<bool> CheckVenueExistsAsync(string venueName, Guid districtId,
        CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(VenueQueries.CheckVenueExists);
        sqlCommand.Parameters.AddWithValue("@VenueName", venueName);
        sqlCommand.Parameters.AddWithValue("@DistrictId", districtId);

        var result = await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);

        return result > 0;
    }

    public async Task<List<VenueMatchesResponse>> CheckVenueMatchesAsync
        (string venueName, Guid districtId, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(VenueQueries.CheckVenueNameMatchRanked);
        sqlCommand.Parameters.AddWithValue("@VenueName", venueName);
        sqlCommand.Parameters.AddWithValue("@DistrictId", districtId);

        await using var sqlDataReader = await context.ExecuteReaderAsync(sqlCommand, cancellationToken);

        var result = new List<VenueMatchesResponse>();

        if (!sqlDataReader.HasRows) return result;

        while (await sqlDataReader.ReadAsync(cancellationToken))
            result.Add(new VenueMatchesResponse
            {
                Name = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Name")),
                MatchScore = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("MatchScore"))
            });

        sqlDataReader.Close();

        return result;
    }

    public async Task CreateVenueAsync(CreateVenueRequest request, CancellationToken cancellationToken)
    {
        // Serialize the list to JSON
        var imageUrlsJson = JsonSerializer.Serialize(request.ImageUrls);

        await using var sqlCommand = new SqlCommand(VenueQueries.CreateVenue);
        sqlCommand.Parameters.AddWithValue("@Name", request.Name);
        sqlCommand.Parameters.AddWithValue("@Address", request.Address);
        sqlCommand.Parameters.AddWithValue("@ImageUrls", imageUrlsJson); // Pass the JSON string
        sqlCommand.Parameters.AddWithValue("@Capacity", request.Capacity);
        sqlCommand.Parameters.AddWithValue("@DistrictId", request.DistrictId);

        await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);
    }

    public async Task<VenueResponse> GetVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(VenueQueries.GetVenueDetailsById);
        sqlCommand.Parameters.AddWithValue("@VenueId", venueId);

        await using var sqlDataReader = await context.ExecuteReaderAsync(sqlCommand, cancellationToken);

        var venueResponse = new VenueResponse();

        if (!sqlDataReader.HasRows) return venueResponse;

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            venueResponse.VenueId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("VenueId"));
            venueResponse.VenueName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("VenueName"));
            venueResponse.VenueAddress = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address"));
            venueResponse.Capacity = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Capacity"));
            venueResponse.DistrictId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("DistrictId"));
            venueResponse.DistrictName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("DistrictName"));
            venueResponse.RegionId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("RegionId"));
            venueResponse.RegionName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("RegionName"));

            // Handling nullable Images column
            if (sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ImageUrls")))
                continue;
            var imagesString =
                sqlDataReader.GetString(
                    sqlDataReader.GetOrdinal("ImageUrls")); // Assuming images are stored as a delimited string
            venueResponse.Images = JsonSerializer.Deserialize<List<string>>(imagesString); // Convert to a List<string>
        }

        return venueResponse;
    }

    public async Task<PagedResponse<VenueResponse>> GetFilteredVenuesAsync(VenueFilterRequest request,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(VenueQueries.GetFilteredVenues);

        // Add parameters with null handling
        command.Parameters.AddWithValue("@RegionId", request.RegionId);
        command.Parameters.AddWithValue("@DistrictId", (object?)request.DistrictId ?? DBNull.Value);
        command.Parameters.AddWithValue("@MinCapacity", (object?)request.MinCapacity ?? DBNull.Value);
        command.Parameters.AddWithValue("@MaxCapacity", (object?)request.MaxCapacity ?? DBNull.Value);
        command.Parameters.AddWithValue("@SearchTerm",
            !string.IsNullOrWhiteSpace(request.SearchTerm) ? request.SearchTerm : DBNull.Value);
        command.Parameters.AddWithValue("@Offset", (request.Page - 1) * request.PageSize);
        command.Parameters.AddWithValue("@PageSize", request.PageSize);
        command.Parameters.AddWithValue("@OrderBy", "name");


        await using var sqlDataReader = await context.ExecuteReaderAsync(command, cancellationToken);

        var venues = new List<VenueResponse>();
        var totalCount = 0;

        if (!sqlDataReader.HasRows) return new PagedResponse<VenueResponse>();

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            if (totalCount == 0) totalCount = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("TotalCount"));

            venues.Add(new VenueResponse
            {
                VenueId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("VenueId")),
                VenueName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("VenueName")),
                VenueAddress = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Address")),
                Images = !sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ImageUrls"))
                    ? JsonSerializer.Deserialize<List<string>>(
                        sqlDataReader.GetString(sqlDataReader.GetOrdinal("ImageUrls")))
                    : null,
                Capacity = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("Capacity")),
                DistrictId = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("DistrictId")),
                DistrictName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("DistrictName"))
            });
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new PagedResponse<VenueResponse>
        {
            Items = venues,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = request.Page,
            HasNextPage = request.Page < totalPages,
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task SoftDeleteVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(VenueQueries.SoftDeleteVenue);
        sqlCommand.Parameters.AddWithValue("@VenueId", venueId);
        
        await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);
    }

    public async Task UnSoftDeleteVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(VenueQueries.UndoSoftDeleteVenue);
        sqlCommand.Parameters.AddWithValue("@VenueId", venueId);
        
        await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);
    }

    public async Task DeleteVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(VenueQueries.SoftDeleteVenue);
        sqlCommand.Parameters.AddWithValue("@VenueId", venueId);
        
        await context.ExecuteNonQueryAsync(sqlCommand, cancellationToken);
    }
}