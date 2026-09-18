using System.Collections.Generic;
using PosCs.Application.Models;
using PosCs.Domain.Entities;

namespace PosCs.Application.Ports
{
    public interface IDryCleanCatalogRepository
    {
        List<DryCleanCatalogItem> GetAll(bool includeInactive);
        DryCleanCatalogItem GetById(string id);
        DryCleanCatalogItem Create(CreateDryCleanCatalogItemRequest request, string actorUserId);
        DryCleanCatalogItem Update(string id, UpdateDryCleanCatalogItemRequest request, string actorUserId);
        void Delete(string id, string actorUserId);
    }
}
