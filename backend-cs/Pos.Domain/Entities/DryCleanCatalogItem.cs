using System;

namespace PosCs.Domain.Entities
{
    public sealed class DryCleanCatalogItem
    {
        public string Id { get; set; }
        public string GarmentType { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public double UnitPrice { get; set; }
        public bool CountsForProductivity { get; set; }
        public bool IsManualEntry { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
