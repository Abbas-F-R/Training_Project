using System.Data;
using Dapper;
using OC_System_Training.Features.AuditLogs.Dtos;
using OC_System_Training.Infrastructure.Persistence;
using OC_System_Training.Shared.Attributes;

namespace OC_System_Training.Features.AuditLogs.Repositories;

/// <summary>
/// Repository for inserting and querying append-only audit trail entries via Stored Procedures.
/// </summary>
[Scoped]
public class AuditLogRepository(DapperContext context) : IAuditLogRepository
{
    public async Task LogAsync(
        long? userId,
        string action,
        string entityName,
        string? entityId = null,
        string? changes = null,
        string? ipAddress = null,
        string? userAgent = null,
        bool isSuccess = true)
    {
        try
        {
            using var connection = context.CreateConnection();
            await connection.ExecuteAsync(
                "AuditLogsInsert",
                new
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Changes = changes,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    IsSuccess = isSuccess
                },
                commandType: CommandType.StoredProcedure
            );
        }
        catch
        {
            // Suppress non-critical external audit logging failures outside transactional boundaries
        }
    }

    public async Task<(List<AuditLogResponse>? data, int totalCount)> GetAll(AuditLogFilter filter)
    {
        using var connection = context.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(
            "AuditLogsGetAll",
            new
            {
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                EntityName = filter.EntityName,
                Action = filter.Action,
                UserId = filter.UserId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate
            },
            commandType: CommandType.StoredProcedure
        );

        var totalCount = await multi.ReadFirstAsync<int>();
        var data = (await multi.ReadAsync<AuditLogResponse>()).ToList();

        return (data, totalCount);
    }
}
