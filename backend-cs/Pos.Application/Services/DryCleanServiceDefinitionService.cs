using System.Collections.Generic;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;
using PosCs.Domain.Exceptions;

namespace PosCs.Application.Services
{
    public sealed class DryCleanServiceDefinitionService
    {
        private readonly IDryCleanServiceRepository _services;

        public DryCleanServiceDefinitionService(IDryCleanServiceRepository services) { _services = services; }
        public List<DryCleanServiceDefinition> GetAll(bool includeInactive) { return _services.GetAll(includeInactive); }

        public DryCleanServiceDefinition Create(CreateDryCleanServiceRequest request, string actorUserId)
        {
            Validate(request == null ? null : request.Name);
            return _services.Create(request, actorUserId);
        }

        public DryCleanServiceDefinition Update(string id, UpdateDryCleanServiceRequest request, string actorUserId)
        {
            Validate(request == null ? null : request.Name);
            return _services.Update(id, request, actorUserId);
        }

        public void Delete(string id, string actorUserId) { _services.Delete(id, actorUserId); }

        private static void Validate(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException("Service name is required");
        }
    }
}
