using System.Linq;
using Dapper;
using PosCs.Application.Models;
using PosCs.Application.Ports;

namespace PosCs.Infrastructure.Persistence
{
    public sealed class OperationsRepository : IOperationsRepository
    {
        public PagedResult<AuditLogEntry> GetAudit(string action, string entity, int page, int size)
        {
            using (var connection = DbConnectionFactory.CreateConnection())
            {
                var parameters = new DynamicParameters();
                var where = " WHERE 1=1";
                if (!string.IsNullOrWhiteSpace(action)) { where += " AND action=@action"; parameters.Add("action", action); }
                if (!string.IsNullOrWhiteSpace(entity)) { where += " AND entityType=@entity"; parameters.Add("entity", entity); }
                parameters.Add("size", size);
                parameters.Add("offset", (page - 1) * size);
                var total = connection.ExecuteScalar<int>("SELECT COUNT(1) FROM AuditLog" + where, parameters);
                var items = connection.Query<AuditLogEntry>("SELECT id,actorUserId,action,entityType,entityId,summary,createdAt FROM AuditLog" + where + " ORDER BY createdAt DESC LIMIT @size OFFSET @offset", parameters).ToList();
                return new PagedResult<AuditLogEntry> { Items = items, Total = total };
            }
        }
    }
}
