using System.Collections.Generic;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Exceptions;

namespace PosCs.Application.Services
{
    public sealed class DryCleanCatalogService
    {
        private readonly IDryCleanCatalogRepository _catalog;

        public DryCleanCatalogService(IDryCleanCatalogRepository catalog) { _catalog = catalog; }
        public List<PosCs.Domain.Entities.DryCleanCatalogItem> GetAll(bool includeInactive) { return _catalog.GetAll(includeInactive); }
        public PosCs.Domain.Entities.DryCleanCatalogItem GetById(string id) { return _catalog.GetById(id); }

        public PosCs.Domain.Entities.DryCleanCatalogItem Create(CreateDryCleanCatalogItemRequest request, string actorUserId)
        {
            Validate(request);
            return _catalog.Create(request, actorUserId);
        }

        public PosCs.Domain.Entities.DryCleanCatalogItem Update(string id, UpdateDryCleanCatalogItemRequest request, string actorUserId)
        {
            Validate(request);
            return _catalog.Update(id, request, actorUserId);
        }

        public void Delete(string id, string actorUserId) { _catalog.Delete(id, actorUserId); }

        private static void Validate(CreateDryCleanCatalogItemRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.GarmentType) || string.IsNullOrWhiteSpace(request.ServiceDescription)) throw new DomainValidationException("Garment type and service are required");
            if (request.UnitPrice < 0) throw new DomainValidationException("Unit price cannot be negative");
        }
    }
}
