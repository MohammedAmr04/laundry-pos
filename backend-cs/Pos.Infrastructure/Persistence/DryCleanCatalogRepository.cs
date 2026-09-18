using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;
using PosCs.Domain.Exceptions;

namespace PosCs.Infrastructure.Persistence
{
    public sealed class DryCleanCatalogRepository : IDryCleanCatalogRepository
    {
        public List<DryCleanCatalogItem> GetAll(bool includeInactive)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) return conn.Query<DryCleanCatalogItem>("SELECT c.*,s.name AS serviceName FROM DryCleanCatalogItem c LEFT JOIN DryCleanServiceDefinition s ON s.id=c.serviceId" + (includeInactive ? "" : " WHERE c.isActive=1") + " ORDER BY c.garmentType, COALESCE(s.name,c.serviceDescription)").ToList();
        }

        public DryCleanCatalogItem GetById(string id)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                var item = conn.QueryFirstOrDefault<DryCleanCatalogItem>("SELECT c.*,s.name AS serviceName FROM DryCleanCatalogItem c LEFT JOIN DryCleanServiceDefinition s ON s.id=c.serviceId WHERE c.id=@id", new { id });
                if (item == null) throw new NotFoundException("Catalog item not found");
                return item;
            }
        }

        public DryCleanCatalogItem Create(CreateDryCleanCatalogItemRequest request, string actorUserId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var now = DateTime.Now;
                var item = new DryCleanCatalogItem { Id = Guid.NewGuid().ToString("N"), GarmentType = request.GarmentType.Trim(), ServiceDescription = request.ServiceDescription.Trim(), ServiceId = request.ServiceId, UnitPrice = (double)request.UnitPrice, CountsForProductivity = request.CountsForProductivity, IsManualEntry = request.IsManualEntry, IsActive = true, CreatedAt = now, UpdatedAt = now };
                conn.Execute("INSERT INTO DryCleanCatalogItem (id,garmentType,serviceDescription,serviceId,unitPrice,countsForProductivity,isManualEntry,isActive,createdAt,updatedAt) VALUES (@Id,@GarmentType,@ServiceDescription,@ServiceId,@UnitPrice,@CountsForProductivity,@IsManualEntry,1,@CreatedAt,@UpdatedAt)", new { item.Id, item.GarmentType, item.ServiceDescription, item.ServiceId, item.UnitPrice, CountsForProductivity = item.CountsForProductivity ? 1 : 0, IsManualEntry = item.IsManualEntry ? 1 : 0, CreatedAt = now.ToString("yyyy-MM-dd HH:mm:ss"), UpdatedAt = now.ToString("yyyy-MM-dd HH:mm:ss") }, tx);
                WriteAudit(conn, tx, actorUserId, "dry_clean.catalog.created", item.Id, "Created catalog item");
                tx.Commit();
                return GetById(item.Id);
            }
        }

        public DryCleanCatalogItem Update(string id, UpdateDryCleanCatalogItemRequest request, string actorUserId)
        {
            GetById(id);
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                conn.Execute("UPDATE DryCleanCatalogItem SET garmentType=@GarmentType,serviceDescription=@ServiceDescription,serviceId=@ServiceId,unitPrice=@UnitPrice,countsForProductivity=@CountsForProductivity,isManualEntry=@IsManualEntry,isActive=COALESCE(@IsActive,isActive),updatedAt=@UpdatedAt WHERE id=@Id", new { Id = id, request.GarmentType, request.ServiceDescription, request.ServiceId, UnitPrice = (double)request.UnitPrice, CountsForProductivity = request.CountsForProductivity ? 1 : 0, IsManualEntry = request.IsManualEntry ? 1 : 0, request.IsActive, UpdatedAt = now }, tx);
                WriteAudit(conn, tx, actorUserId, "dry_clean.catalog.updated", id, "Updated catalog item");
                tx.Commit();
            }
            return GetById(id);
        }

        public void Delete(string id, string actorUserId)
        {
            GetById(id);
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                conn.Execute("UPDATE DryCleanCatalogItem SET isActive=0,updatedAt=@now WHERE id=@id", new { id, now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, tx);
                WriteAudit(conn, tx, actorUserId, "dry_clean.catalog.deactivated", id, "Deactivated catalog item");
                tx.Commit();
            }
        }

        private static void WriteAudit(System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction tx, string actor, string action, string entityId, string summary)
        {
            if (string.IsNullOrWhiteSpace(actor)) return;
            conn.Execute("INSERT INTO AuditLog (id,actorUserId,action,entityType,entityId,summary,createdAt) VALUES (@id,@actor,@action,'dry_clean_catalog',@entityId,@summary,@createdAt)", new { id = Guid.NewGuid().ToString("N"), actor, action, entityId, summary, createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, tx);
        }
    }
}
