using PosCs.Application.Models;

namespace PosCs.Application.Ports
{
    public interface IOperationsRepository
    {
        PagedResult<AuditLogEntry> GetAudit(string action, string entityType, int page, int pageSize);
    }
}
