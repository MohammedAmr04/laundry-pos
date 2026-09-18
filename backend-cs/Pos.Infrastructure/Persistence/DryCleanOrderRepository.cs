using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;
using PosCs.Domain.Exceptions;
using PosCs.Domain.Rules;

namespace PosCs.Infrastructure.Persistence
{
    public sealed class DryCleanOrderRepository : IDryCleanOrderRepository
    {
        public DryCleanOrder Create(CreateDryCleanOrderRequest request, string receivedByUserId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var clientAddress = conn.ExecuteScalar<string>("SELECT address FROM Client WHERE id=@id AND isActive=1", new { id = request.ClientId }, tx);
                if (clientAddress == null) throw new NotFoundException("Client not found or inactive");
                var receivedEmployeeId = RequireEmployee(conn, request.ReceivedByEmployeeId);
                var order = new DryCleanOrder { Id = Guid.NewGuid().ToString("N"), OrderNumber = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(orderNumber),0)+1 FROM DryCleanOrder", transaction: tx), ClientId = request.ClientId, DeliveryAddress = clientAddress, Status = DryCleanOrderFlow.Received, ReceivedByEmployeeId = receivedEmployeeId, AssignedIronerId = string.IsNullOrWhiteSpace(request.AssignedIronerId) ? null : RequireEmployee(conn, request.AssignedIronerId), ReceivedAt = DateTime.Now, ExpectedDeliveryAt = request.ExpectedDeliveryAt, Notes = request.Notes, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                order.Subtotal = request.Lines.Sum(x => (double)x.Quantity * (double)x.UnitPrice);
                order.TotalAmount = order.Subtotal;
                conn.Execute("INSERT INTO DryCleanOrder (id,orderNumber,clientId,deliveryAddress,status,receivedByEmployeeId,assignedIronerId,receivedAt,expectedDeliveryAt,subtotal,discountAmount,totalAmount,notes,createdAt,updatedAt) VALUES (@Id,@OrderNumber,@ClientId,@DeliveryAddress,@Status,@ReceivedByEmployeeId,@AssignedIronerId,@ReceivedAt,@ExpectedDeliveryAt,@Subtotal,@DiscountAmount,@TotalAmount,@Notes,@CreatedAt,@UpdatedAt)", order, tx);
                foreach (var line in request.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line.GarmentType) || string.IsNullOrWhiteSpace(line.ServiceDescription) || line.Quantity <= 0 || line.UnitPrice < 0) throw new DomainValidationException("Invalid garment line");
                    conn.Execute("INSERT INTO DryCleanOrderLine (id,orderId,garmentType,serviceDescription,quantity,colorOrDescription,conditionBefore,damageNotes,processingNotes,unitPrice,lineTotal,countsForProductivity,isManualEntry) VALUES (@id,@orderId,@garmentType,@serviceDescription,@quantity,@colorOrDescription,@conditionBefore,@damageNotes,@processingNotes,@unitPrice,@lineTotal,@countsForProductivity,@isManualEntry)", new { id = Guid.NewGuid().ToString("N"), orderId = order.Id, garmentType = line.GarmentType.Trim(), serviceDescription = line.ServiceDescription.Trim(), quantity = line.Quantity, colorOrDescription = line.ColorOrDescription, conditionBefore = line.ConditionBefore, damageNotes = line.DamageNotes, processingNotes = line.ProcessingNotes, unitPrice = line.UnitPrice, lineTotal = line.Quantity * line.UnitPrice, countsForProductivity = line.CountsForProductivity ? 1 : 0, isManualEntry = line.IsManualEntry ? 1 : 0 }, tx);
                }
                if (request.PaymentStatus == "paid")
                {
                    if (request.PaymentMethod != "cash" && request.PaymentMethod != "wallet" && request.PaymentMethod != "instapay") throw new DomainValidationException("Invalid payment method");
                    conn.Execute("INSERT INTO DryCleanOrderPayment (id,orderId,amount,paymentMethod,paidAt,createdByUserId) VALUES (@id,@orderId,@amount,@paymentMethod,@paidAt,@createdByUserId)", new { id = Guid.NewGuid().ToString("N"), orderId = order.Id, amount = order.TotalAmount, paymentMethod = request.PaymentMethod, paidAt = DateTime.Now, createdByUserId = receivedByUserId }, tx);
                }
                AddHistory(conn, tx, order.Id, null, DryCleanOrderFlow.Received, receivedByUserId);
                tx.Commit(); return GetById(order.Id);
            }
        }

        public DryCleanOrder GetById(string id)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                var order = conn.QueryFirstOrDefault<DryCleanOrder>("SELECT * FROM DryCleanOrder WHERE id=@id", new { id });
                if (order == null) throw new NotFoundException("Dry clean order not found");
                order.Client = conn.QueryFirstOrDefault<Client>("SELECT * FROM Client WHERE id=@id", new { id = order.ClientId });
                order.ClientName = order.Client == null ? null : order.Client.Name;
                order.Lines = conn.Query<DryCleanOrderLine>("SELECT * FROM DryCleanOrderLine WHERE orderId=@id ORDER BY rowid", new { id }).ToList();
                order.PaidAmount = conn.ExecuteScalar<double>("SELECT COALESCE(SUM(amount),0) FROM DryCleanOrderPayment WHERE orderId=@id", new { id });
                order.RemainingAmount = Math.Max(0, order.TotalAmount - order.PaidAmount);
                return order;
            }
        }

        public PagedResult<DryCleanOrder> GetPaged(string status, string query, string clientId, string paymentStatus, int page, int pageSize)
        {
            page = Math.Max(1, page); pageSize = Math.Min(100, Math.Max(1, pageSize));
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                var where = new List<string>(); var args = new DynamicParameters(); args.Add("offset", (page - 1) * pageSize); args.Add("pageSize", pageSize);
                if (!string.IsNullOrWhiteSpace(status)) { where.Add("o.status=@status"); args.Add("status", status); }
                if (!string.IsNullOrWhiteSpace(clientId)) { where.Add("o.clientId=@clientId"); args.Add("clientId", clientId); }
                if (paymentStatus == "paid") { where.Add("o.totalAmount <= COALESCE((SELECT SUM(amount) FROM DryCleanOrderPayment p WHERE p.orderId=o.id), 0)"); }
                else if (paymentStatus == "credit") { where.Add("o.totalAmount > COALESCE((SELECT SUM(amount) FROM DryCleanOrderPayment p WHERE p.orderId=o.id), 0)"); }
                if (!string.IsNullOrWhiteSpace(query)) { where.Add("(CAST(o.orderNumber AS TEXT) LIKE @query OR o.clientId IN (SELECT id FROM Client WHERE name LIKE @query OR phone LIKE @query))"); args.Add("query", "%" + query.Trim() + "%"); }
                var filter = where.Count == 0 ? "" : " WHERE " + string.Join(" AND ", where);
                var total = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM DryCleanOrder o" + filter, args);
                var items = conn.Query<DryCleanOrder>("SELECT o.*, c.name AS ClientName FROM DryCleanOrder o LEFT JOIN Client c ON c.id=o.clientId" + filter + " ORDER BY o.createdAt DESC LIMIT @pageSize OFFSET @offset", args).ToList();
                return new PagedResult<DryCleanOrder> { Items = items, Total = total };
            }
        }

        public DryCleanOrder Prepare(string id, string ironerEmployeeId, string actorUserId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var order = GetForUpdate(conn, id, tx); EnsureOrderHasPrices(conn, order, tx); if (!DryCleanOrderFlow.CanPrepare(order.Status)) throw new DomainValidationException("Only received orders can be prepared");
                var employee = RequireEmployee(conn, ironerEmployeeId); var count = conn.ExecuteScalar<int>("SELECT COALESCE(SUM(CASE WHEN countsForProductivity=1 THEN quantity ELSE 0 END),0) FROM DryCleanOrderLine WHERE orderId=@id", new { id }, tx);
                conn.Execute("UPDATE DryCleanOrder SET status=@prepared,assignedIronerId=@employeeId,completedAt=@now,readyAt=@now,updatedAt=@now WHERE id=@id", new { id, employeeId = employee, prepared = DryCleanOrderFlow.Prepared, now = DateTime.Now }, tx);
                conn.Execute("INSERT INTO DryCleanProductivity (id,orderId,ironerEmployeeId,countedQuantity,completedAt,createdAt) VALUES (@pid,@id,@employee,@count,@now,@now)", new { pid = Guid.NewGuid().ToString("N"), id, employee, count, now = DateTime.Now }, tx);
                AddHistory(conn, tx, id, order.Status, DryCleanOrderFlow.Prepared, actorUserId); tx.Commit(); return GetById(id);
            }
        }

        public DryCleanOrder AssignIroner(string id, string ironerEmployeeId, string actorUserId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var order = GetForUpdate(conn, id, tx);
                if (order.Status != DryCleanOrderFlow.Received && order.Status != DryCleanOrderFlow.Prepared) throw new DomainValidationException("Only received or prepared orders can have an ironer assigned");
                var employee = RequireEmployee(conn, ironerEmployeeId);
                conn.Execute("UPDATE DryCleanOrder SET assignedIronerId=@employee,updatedAt=@now WHERE id=@id", new { id, employee, now = DateTime.Now }, tx);
                AddHistory(conn, tx, id, order.Status, order.Status, actorUserId);
                tx.Commit();
                return GetById(id);
            }
        }

        public DryCleanOrder Deliver(string id, string deliveryEmployeeId, string actorUserId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            { var order = GetForUpdate(conn, id, tx); EnsureOrderHasPrices(conn, order, tx); if (!DryCleanOrderFlow.CanDeliver(order.Status)) throw new DomainValidationException("Only prepared orders can be delivered"); var employee = RequireEmployee(conn, deliveryEmployeeId); conn.Execute("UPDATE DryCleanOrder SET status=@delivered,deliveredByEmployeeId=@employee,deliveredAt=@now,updatedAt=@now WHERE id=@id", new { id, employee, delivered = DryCleanOrderFlow.Delivered, now = DateTime.Now }, tx); AddHistory(conn, tx, id, order.Status, DryCleanOrderFlow.Delivered, actorUserId); tx.Commit(); return GetById(id); }
        }

        public DryCleanOrderPayment AddPayment(string orderId, CreateDryCleanPaymentRequest request, string actorUserId)
        {
            if (request == null || request.Amount <= 0 || (request.PaymentMethod != "cash" && request.PaymentMethod != "wallet" && request.PaymentMethod != "instapay")) throw new DomainValidationException("Invalid payment");
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            { var order = GetForUpdate(conn, orderId, tx); EnsureOrderHasPrices(conn, order, tx); var paid = conn.ExecuteScalar<decimal>("SELECT COALESCE(SUM(amount),0) FROM DryCleanOrderPayment WHERE orderId=@orderId", new { orderId }, tx); if (paid + request.Amount > (decimal)order.TotalAmount) throw new DomainValidationException("Payment exceeds remaining balance"); var payment = new DryCleanOrderPayment { Id = Guid.NewGuid().ToString("N"), OrderId = orderId, Amount = (double)request.Amount, PaymentMethod = request.PaymentMethod, PaidAt = request.PaidAt ?? DateTime.Now, CreatedByUserId = actorUserId, Reference = request.Reference, Notes = request.Notes }; conn.Execute("INSERT INTO DryCleanOrderPayment (id,orderId,amount,paymentMethod,paidAt,createdByUserId,reference,notes) VALUES (@Id,@OrderId,@Amount,@PaymentMethod,@PaidAt,@CreatedByUserId,@Reference,@Notes)", payment, tx); tx.Commit(); return payment; }
        }

        public List<DryCleanProductivitySummary> GetProductivity(string from, string to)
        {
            using (var conn = DbConnectionFactory.CreateConnection())
            {
                var start = string.IsNullOrWhiteSpace(from) ? "0000-01-01" : from.Trim();
                var end = string.IsNullOrWhiteSpace(to) ? "9999-12-31" : to.Trim();
                return conn.Query<DryCleanProductivitySummary>(@"SELECT p.ironerEmployeeId AS EmployeeId, e.name AS EmployeeName, COUNT(1) AS OrdersCompleted, COALESCE(SUM(p.countedQuantity),0) AS CountedQuantity FROM DryCleanProductivity p JOIN Employee e ON e.id=p.ironerEmployeeId WHERE date(p.completedAt) BETWEEN @start AND @end GROUP BY p.ironerEmployeeId, e.name ORDER BY CountedQuantity DESC, EmployeeName", new { start, end }).ToList();
            }
        }

        private static DryCleanOrder GetForUpdate(SqliteConnection conn, string id, SqliteTransaction tx) { var order = conn.QueryFirstOrDefault<DryCleanOrder>("SELECT * FROM DryCleanOrder WHERE id=@id", new { id }, tx); if (order == null) throw new NotFoundException("Dry clean order not found"); return order; }
        private static string RequireEmployee(SqliteConnection conn, string id) { if (string.IsNullOrWhiteSpace(id) || conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Employee WHERE id=@id AND isActive=1", new { id }) != 1) throw new NotFoundException("Employee not found or inactive"); return id; }
        private static void EnsureOrderHasPrices(SqliteConnection conn, DryCleanOrder order, SqliteTransaction tx)
        {
            var invalidLines = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM DryCleanOrderLine WHERE orderId=@id AND (unitPrice <= 0 OR lineTotal <= 0)", new { id = order.Id }, tx);
            if (invalidLines > 0 || order.TotalAmount <= 0) throw new DomainValidationException("All order items must have a price greater than zero");
        }
        private static void AddHistory(SqliteConnection conn, SqliteTransaction tx, string id, string from, string to, string actor) { if (string.IsNullOrWhiteSpace(actor)) return; conn.Execute("INSERT INTO DryCleanOrderStatusHistory (id,orderId,fromStatus,toStatus,changedByUserId,createdAt) VALUES (@id,@orderId,@fromStatus,@toStatus,@actor,@createdAt)", new { id = Guid.NewGuid().ToString("N"), orderId = id, fromStatus = from, toStatus = to, actor, createdAt = DateTime.Now }, tx); }
    }
}
