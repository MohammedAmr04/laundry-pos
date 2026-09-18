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
    public sealed class DryCleanServiceRepository : IDryCleanServiceRepository
    {
        public List<DryCleanServiceDefinition> GetAll(bool includeInactive)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) return conn.Query<DryCleanServiceDefinition>("SELECT * FROM DryCleanServiceDefinition" + (includeInactive ? "" : " WHERE isActive=1") + " ORDER BY name").ToList();
        }

        public DryCleanServiceDefinition Create(CreateDryCleanServiceRequest request, string actorUserId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var item = new DryCleanServiceDefinition { Id = Guid.NewGuid().ToString("N"), Name = request.Name.Trim(), IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                conn.Execute("INSERT INTO DryCleanServiceDefinition (id,name,isActive,createdAt,updatedAt) VALUES (@Id,@Name,1,@Now,@Now)", new { item.Id, item.Name, Now = now }, tx);
                WriteAudit(conn, tx, actorUserId, "dry_clean.service.created", item.Id, "Created dry clean service");
                tx.Commit();
                return item;
            }
        }

        public DryCleanServiceDefinition Update(string id, UpdateDryCleanServiceRequest request, string actorUserId)
        {
            GetById(id);
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                conn.Execute("UPDATE DryCleanServiceDefinition SET name=@Name,isActive=COALESCE(@IsActive,isActive),updatedAt=@Now WHERE id=@Id", new { Id = id, Name = request.Name.Trim(), request.IsActive, Now = now }, tx);
                WriteAudit(conn, tx, actorUserId, "dry_clean.service.updated", id, "Updated dry clean service");
                tx.Commit();
            }
            return GetById(id);
        }

        public void Delete(string id, string actorUserId)
        {
            GetById(id);
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                conn.Execute("UPDATE DryCleanServiceDefinition SET isActive=0,updatedAt=@Now WHERE id=@Id", new { id, Now = now }, tx);
                WriteAudit(conn, tx, actorUserId, "dry_clean.service.deactivated", id, "Deactivated dry clean service");
                tx.Commit();
            }
        }

        private static DryCleanServiceDefinition GetById(string id)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                var item = conn.QueryFirstOrDefault<DryCleanServiceDefinition>("SELECT * FROM DryCleanServiceDefinition WHERE id=@id", new { id });
                if (item == null) throw new NotFoundException("Dry clean service not found");
                return item;
            }
        }

        private static void WriteAudit(System.Data.Common.DbConnection conn, System.Data.Common.DbTransaction tx, string actor, string action, string entityId, string summary)
        {
            if (string.IsNullOrWhiteSpace(actor)) return;
            conn.Execute("INSERT INTO AuditLog (id,actorUserId,action,entityType,entityId,summary,createdAt) VALUES (@id,@actor,@action,'dry_clean_service',@entityId,@summary,@createdAt)", new { id = Guid.NewGuid().ToString("N"), actor, action, entityId, summary, createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }, tx);
        }
    }
}
