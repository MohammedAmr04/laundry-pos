using PosCs.Application.Models;
using PosCs.Domain.Entities;
using System.Collections.Generic;

namespace PosCs.Application.Ports
{
    /// <summary>
    /// Clients are never deleted — credit sales require a stable identity to answer
    /// "who owes us money?" (spec §10.2).
    /// </summary>
    public interface IClientRepository
    {
        List<Client> GetAll();
        PagedResult<Client> GetPaged(int page, int pageSize, string query, string balanceFilter);
        Client GetById(string id);
        Client Create(Client client);
        Client Update(Client client);
    }

    public interface IEmployeeRepository
    {
        List<Employee> GetAll();
        PagedResult<Employee> GetPaged(int page, int pageSize, string query);
        Employee GetById(string id);
        Employee Create(Employee employee);
        Employee Update(Employee employee);
    }
}
