using PosCs.Application.Models;
using PosCs.Application.Ports;

namespace PosCs.Application.Services
{
    public class OperationsService
    {
        private readonly IOperationsRepository _repo;
        public OperationsService(IOperationsRepository repo) { _repo = repo; }
        public PagedResult<AuditLogEntry> Audit(string action, string entity, int page, int size) { return _repo.GetAudit(action, entity, NormalizePage(page), NormalizeSize(size)); }
        private static int NormalizePage(int page) { return page < 1 ? 1 : page; }
        private static int NormalizeSize(int size) { return size < 1 ? 20 : (size > 100 ? 100 : size); }
    }
}
