using System;
using System.Collections.Generic;

namespace PosCs.Application.Models
{
    public sealed class CreateClientRequest { public string Name { get; set; } public string Phone { get; set; } public string Address { get; set; } public string Notes { get; set; } }
    public sealed class UpdateClientRequest { public string Name { get; set; } public string Phone { get; set; } public string Address { get; set; } public string Notes { get; set; } public bool? IsActive { get; set; } }
    public sealed class CreateEmployeeRequest { public string Name { get; set; } public string Phone { get; set; } }
    public sealed class UpdateEmployeeRequest { public string Name { get; set; } public string Phone { get; set; } public bool? IsActive { get; set; } }

    public sealed class DryCleanLineRequest
    {
        public string GarmentType { get; set; }
        public string ServiceDescription { get; set; }
        public int Quantity { get; set; }
        public string ColorOrDescription { get; set; }
        public string ConditionBefore { get; set; }
        public string DamageNotes { get; set; }
        public string ProcessingNotes { get; set; }
        public decimal UnitPrice { get; set; }
        public bool CountsForProductivity { get; set; } = true;
        public bool IsManualEntry { get; set; } = true;
    }

    public sealed class CreateDryCleanOrderRequest
    {
        public string ClientId { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReceivedByEmployeeId { get; set; }
        public string AssignedIronerId { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        public string Notes { get; set; }
        public List<DryCleanLineRequest> Lines { get; set; } = new List<DryCleanLineRequest>();
    }

    public sealed class CompleteDryCleanOrderRequest { public string IronerEmployeeId { get; set; } }
    public sealed class DeliverDryCleanOrderRequest { public string DeliveryEmployeeId { get; set; } }

    public sealed class CreateDryCleanPaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
    }

    public sealed class DryCleanProductivitySummary
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int OrdersCompleted { get; set; }
        public int CountedQuantity { get; set; }
    }
}
