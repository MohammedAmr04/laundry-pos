using System;
using System.Collections.Generic;

namespace PosCs.Domain.Entities
{
    public class DryCleanOrder
    {
        public string Id { get; set; }
        public int OrderNumber { get; set; }
        public string ClientId { get; set; }
        public Client Client { get; set; }
        public string DeliveryAddress { get; set; }
        public string Status { get; set; } = "received";
        public string ReceivedByEmployeeId { get; set; }
        public Employee ReceivedByEmployee { get; set; }
        public string AssignedIronerId { get; set; }
        public Employee AssignedIroner { get; set; }
        public string DeliveredByEmployeeId { get; set; }
        public Employee DeliveredByEmployee { get; set; }
        public DateTime ReceivedAt { get; set; }
        public DateTime? ExpectedDeliveryAt { get; set; }
        public DateTime? ProcessingStartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelledReason { get; set; }
        public double Subtotal { get; set; }
        public double DiscountAmount { get; set; }
        public double TotalAmount { get; set; }
        public double PaidAmount { get; set; }
        public double RemainingAmount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DryCleanOrderLine> Lines { get; set; } = new List<DryCleanOrderLine>();
    }

    public class DryCleanOrderLine
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string GarmentType { get; set; }
        public string ServiceDescription { get; set; }
        public int Quantity { get; set; }
        public string ColorOrDescription { get; set; }
        public string ConditionBefore { get; set; }
        public string DamageNotes { get; set; }
        public string ProcessingNotes { get; set; }
        public double UnitPrice { get; set; }
        public double LineTotal { get; set; }
        public bool CountsForProductivity { get; set; }
        public bool IsManualEntry { get; set; }
    }

    public class DryCleanProductivity
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string IronerEmployeeId { get; set; }
        public int CountedQuantity { get; set; }
        public DateTime CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DryCleanOrderPayment
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaidAt { get; set; }
        public string CreatedByUserId { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
    }
}
