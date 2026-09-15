using System;
using System.Collections.Generic;
using System.Linq;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;
using PosCs.Domain.Exceptions;

namespace PosCs.Application.Services
{
    public sealed class ClientService
    {
        private readonly IClientRepository _repo;

        public ClientService(IClientRepository repo) { _repo = repo; }

        public List<Client> GetActive() { return _repo.GetAll().Where(c => c.IsActive).ToList(); }

        public PagedResult<Client> GetPaged(int page, int pageSize, string query, string balanceFilter)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));
            return _repo.GetPaged(page, pageSize, query?.Trim(), NormalizeBalanceFilter(balanceFilter));
        }

        public Client GetById(string id)
        {
            var client = _repo.GetById(id);
            if (client == null) throw new NotFoundException("Client not found");
            return client;
        }

        public Client Create(CreateClientRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name)) throw new DomainValidationException("Client name is required");
            return _repo.Create(new Client { Name = request.Name.Trim(), Phone = Clean(request.Phone), Address = Clean(request.Address), Notes = Clean(request.Notes), IsActive = true });
        }

        public Client Update(string id, UpdateClientRequest request)
        {
            if (request == null) throw new DomainValidationException("Invalid client data");
            var existing = GetById(id);
            if (!string.IsNullOrWhiteSpace(request.Name)) existing.Name = request.Name.Trim();
            if (request.Phone != null) existing.Phone = Clean(request.Phone);
            if (request.Address != null) existing.Address = Clean(request.Address);
            if (request.Notes != null) existing.Notes = Clean(request.Notes);
            if (request.IsActive.HasValue) existing.IsActive = request.IsActive.Value;
            return _repo.Update(existing);
        }

        internal static string NormalizeBalanceFilter(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().ToLowerInvariant() == "all") return null;
            var normalized = value.Trim().ToLowerInvariant();
            if (normalized == "positive" || normalized == "negative" || normalized == "zero") return normalized;
            throw new DomainValidationException("Invalid balance filter");
        }

        private static string Clean(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
    }
}
