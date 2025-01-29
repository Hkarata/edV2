using eDereva.Application.Context;
using eDereva.Application.Repositories;
using eDereva.Domain.Contracts.Requests;
using eDereva.Domain.Contracts.Responses;
using eDereva.Domain.SQL_Queries;
using Microsoft.Data.SqlClient;

namespace eDereva.Infrastructure.Repository;

public class SessionRepository(IDatabaseContext context) : ISessionRepository
{
    public async Task<PagedResponse<SessionResponse>> GetFilteredSessionsAsync
        (SessionFilterRequest request, CancellationToken cancellationToken)
    {
        await using var sqlCommand = new SqlCommand(SessionQueries.GetFilteredSessions);

        // Add parameters with null handling
        sqlCommand.Parameters.AddWithValue("@VenueId", (object)request.VenueId ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@RegionId", (object)request.RegionId ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@DistrictId", (object)request.DistrictId ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@DateFrom", (object)request.DateFrom ?? DBNull.Value);
        sqlCommand.Parameters.AddWithValue("@PageSize", request.PageSize > 0 ? request.PageSize : 10);
        sqlCommand.Parameters.AddWithValue("@Page", request.Page > 0 ? request.Page : 1);

        await using var sqlDataReader = await context.ExecuteReaderAsync(sqlCommand, cancellationToken);

        var sessions = new List<SessionResponse>();
        var totalCount = 0;

        while (await sqlDataReader.ReadAsync(cancellationToken))
        {
            if (totalCount == 0) totalCount = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("TotalCount"));

            sessions.Add(new SessionResponse
            {
                Id = sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("Id")),
                Date = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("Date")),
                Status = sqlDataReader.GetString(sqlDataReader.GetOrdinal("Status")),
                StartTime = sqlDataReader.GetTimeSpan(sqlDataReader.GetOrdinal("StartTime")),
                EndTime = sqlDataReader.GetTimeSpan(sqlDataReader.GetOrdinal("EndTime")),
                AvailableSeats = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("AvailableSeats")),
                VenueName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("VenueName")),
                VenueAddress = sqlDataReader.GetString(sqlDataReader.GetOrdinal("VenueAddress")),
                ContingencyId = sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContingencyId"))
                    ? null
                    : sqlDataReader.GetGuid(sqlDataReader.GetOrdinal("ContingencyId")),
                RegionName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("RegionName")),
                DistrictName = sqlDataReader.GetString(sqlDataReader.GetOrdinal("DistrictName")),
                ContingencyType = sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContingencyType"))
                    ? null
                    : sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContingencyType")),
                ContingencyExplanation = sqlDataReader.IsDBNull(sqlDataReader.GetOrdinal("ContingencyExplanation"))
                    ? null
                    : sqlDataReader.GetString(sqlDataReader.GetOrdinal("ContingencyExplanation"))
            });
        }

        var pageSize = request.PageSize > 0 ? request.PageSize : 10;
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var currentPage = request.Page > 0 ? request.Page : 1;

        return new PagedResponse<SessionResponse>
        {
            Items = sessions,
            TotalCount = totalCount,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            HasNextPage = currentPage < totalPages,
            HasPreviousPage = currentPage > 1
        };
    }
}