using System;
using System.Collections.Generic;
using PosCs.Domain.Entities;

namespace PosCs.Application.Models
{
    public class CreateDryCleanCatalogItemRequest
    {
        public string GarmentType { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceId { get; set; }
        public decimal UnitPrice { get; set; }
        public bool CountsForProductivity { get; set; } = true;
        public bool IsManualEntry { get; set; }
    }

    public sealed class UpdateDryCleanCatalogItemRequest : CreateDryCleanCatalogItemRequest
    {
        public bool? IsActive { get; set; }
    }
}
