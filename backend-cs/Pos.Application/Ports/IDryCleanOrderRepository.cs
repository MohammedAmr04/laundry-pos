using System;
using System.Collections.Generic;
using PosCs.Application.Models;
using PosCs.Domain.Entities;

namespace PosCs.Application.Ports
{
    public interface IDryCleanOrderRepository
    {
        DryCleanOrder Create(CreateDryCleanOrderRequest request, string receivedByUserId);
        DryCleanOrder GetById(string id);
        PagedResult<DryCleanOrder> GetPaged(string status, string query, int page, int pageSize);
        DryCleanOrder StartProcessing(string id, string actorUserId);
        DryCleanOrder Complete(string id, string ironerEmployeeId, string actorUserId);
        DryCleanOrder Deliver(string id, string deliveryEmployeeId, string actorUserId);
        DryCleanOrderPayment AddPayment(string orderId, CreateDryCleanPaymentRequest request, string actorUserId);
        List<DryCleanProductivitySummary> GetProductivity(string from, string to);
    }
}
