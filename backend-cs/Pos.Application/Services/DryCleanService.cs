using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;
using PosCs.Domain.Exceptions;

namespace PosCs.Application.Services
{
    public sealed class DryCleanService
    {
        private readonly IDryCleanOrderRepository _orders;

        public DryCleanService(IDryCleanOrderRepository orders) { _orders = orders; }

        public DryCleanOrder Create(CreateDryCleanOrderRequest request, string actorUserId)
        {
            if (request == null || request.Lines == null || request.Lines.Count == 0)
                throw new DomainValidationException("At least one garment line is required");
            if (string.IsNullOrWhiteSpace(request.ClientId))
                throw new DomainValidationException("Client is required");
            return _orders.Create(request, actorUserId);
        }

        public PagedResult<DryCleanOrder> GetPaged(string status, string query, int page, int pageSize)
        {
            return _orders.GetPaged(status, query, page, pageSize);
        }

        public DryCleanOrder GetById(string id) { return _orders.GetById(id); }
        public DryCleanOrder StartProcessing(string id, string actorUserId) { return _orders.StartProcessing(id, actorUserId); }
        public DryCleanOrder Complete(string id, string ironerEmployeeId, string actorUserId) { return _orders.Complete(id, ironerEmployeeId, actorUserId); }
        public DryCleanOrder Deliver(string id, string deliveryEmployeeId, string actorUserId) { return _orders.Deliver(id, deliveryEmployeeId, actorUserId); }
        public DryCleanOrderPayment AddPayment(string id, CreateDryCleanPaymentRequest request, string actorUserId) { return _orders.AddPayment(id, request, actorUserId); }
        public System.Collections.Generic.List<DryCleanProductivitySummary> GetProductivity(string from, string to) { return _orders.GetProductivity(from, to); }
    }
}
