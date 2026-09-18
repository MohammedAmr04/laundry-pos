using System.Collections.Generic;
using PosCs.Application.Models;
using PosCs.Domain.Entities;

namespace PosCs.Application.Ports
{
    public interface IDryCleanServiceRepository
    {
        List<DryCleanServiceDefinition> GetAll(bool includeInactive);
        DryCleanServiceDefinition Create(CreateDryCleanServiceRequest request, string actorUserId);
        DryCleanServiceDefinition Update(string id, UpdateDryCleanServiceRequest request, string actorUserId);
        void Delete(string id, string actorUserId);
    }
}
