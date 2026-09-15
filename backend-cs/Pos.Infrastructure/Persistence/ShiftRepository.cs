using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.Sqlite;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;
using PosCs.Domain.Exceptions;

namespace PosCs.Infrastructure.Persistence
{
    public sealed class ShiftRepository : IShiftRepository
    {
        public Shift Create(Shift shift)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                if (conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Shift WHERE status='open'", transaction: tx) > 0) throw new DomainValidationException("An open shift already exists");
                shift.Id = Guid.NewGuid().ToString("N"); shift.Number = conn.ExecuteScalar<int>("SELECT COALESCE(MAX(number),0)+1 FROM Shift", transaction: tx); shift.OpenedAt = DateTime.Now; shift.Status = "open";
                conn.Execute("INSERT INTO Shift (id,number,openedBy,openingCash,openedAt,notes,status) VALUES (@Id,@Number,@OpenedBy,@OpeningCash,@OpenedAt,@Notes,@Status)", new { shift.Id, shift.Number, shift.OpenedBy, shift.OpeningCash, OpenedAt = shift.OpenedAt.ToString("yyyy-MM-dd HH:mm:ss"), shift.Notes, shift.Status }, tx); tx.Commit(); return GetById(shift.Id);
            }
        }

        public Shift GetActive() { using (var conn = DbConnectionFactory.CreateConnection()) return conn.QueryFirstOrDefault<Shift>("SELECT * FROM Shift WHERE status='open' ORDER BY number DESC LIMIT 1"); }
        public Shift GetById(string id) { using (var conn = DbConnectionFactory.CreateConnection()) { var shift = GetByIdCore(conn, id); if (shift == null) throw new NotFoundException("Shift not found"); return shift; } }

        public ShiftPageResult GetPaged(string status, int page, int pageSize)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) { var where = string.IsNullOrWhiteSpace(status) || status == "all" ? "" : " WHERE status=@status"; var args = new { status, pageSize, offset = (page - 1) * pageSize }; return new ShiftPageResult { Items = conn.Query<Shift>("SELECT * FROM Shift" + where + " ORDER BY number DESC LIMIT @pageSize OFFSET @offset", args).ToList(), Total = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Shift" + where, args) }; }
        }

        public Shift Close(string shiftId, double countedCash, string closedBy)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction())
            {
                var shift = conn.QueryFirstOrDefault<Shift>("SELECT * FROM Shift WHERE id=@shiftId", new { shiftId }, tx); if (shift == null) throw new NotFoundException("Shift not found"); if (shift.Status != "open") throw new DomainValidationException("Only an open shift can be closed");
                var now = DateTime.Now; var expected = shift.OpeningCash + CashInWindow(conn, shift.OpenedAt, now, tx) - ExpensesInWindow(conn, shift.OpenedAt, now, tx); shift.ClosedAt = now; shift.CountedCash = Math.Round(countedCash, 2); shift.ExpectedCash = Math.Round(expected, 2); shift.Difference = Math.Round(countedCash - expected, 2); shift.Status = "closed";
                conn.Execute("UPDATE Shift SET closedAt=@ClosedAt,countedCash=@CountedCash,expectedCash=@ExpectedCash,difference=@Difference,status='closed' WHERE id=@Id", new { Id = shift.Id, ClosedAt = now.ToString("yyyy-MM-dd HH:mm:ss"), shift.CountedCash, shift.ExpectedCash, shift.Difference }, tx); conn.Execute("INSERT INTO AuditLog (id,actorUserId,action,entityType,entityId,summary,createdAt) VALUES (@id,@user,'shift.closed','shift',@shiftId,@summary,@at)", new { id = Guid.NewGuid().ToString("N"), user = closedBy, shiftId, summary = "Closed shift #" + shift.Number, at = now.ToString("yyyy-MM-dd HH:mm:ss") }, tx); tx.Commit(); return shift;
            }
        }

        public ShiftReport GetReport(string shiftId)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) { var shift = GetByIdCore(conn, shiftId); if (shift == null) throw new NotFoundException("Shift not found"); var end = shift.ClosedAt ?? DateTime.Now; var cash = CashInWindow(conn, shift.OpenedAt, end, null); var expenses = ExpensesInWindow(conn, shift.OpenedAt, end, null); return new ShiftReport { Shift = shift, OpeningCash = shift.OpeningCash, OtherCashIn = cash, ExpensesOut = expenses, ExpectedCash = Math.Round(shift.OpeningCash + cash - expenses, 2), Entries = new List<ShiftReportEntry> { new ShiftReportEntry { Date = shift.OpenedAt, Description = "Opening cash", Amount = shift.OpeningCash } } }; }
        }

        public CashDrawerMovement CreateDrawerMovement(CashDrawerMovement movement)
        {
            using (var conn = DbConnectionFactory.CreateConnection()) using (var tx = conn.BeginTransaction()) { if (conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Shift WHERE id=@id AND status='open'", new { id = movement.ShiftId }, tx) == 0) throw new DomainValidationException("Cash drawer movements require an open shift"); movement.Id = Guid.NewGuid().ToString("N"); movement.CreatedAt = DateTime.Now; conn.Execute("INSERT INTO CashDrawerMovement (id,shiftId,type,amount,reason,createdBy,createdAt) VALUES (@Id,@ShiftId,@Type,@Amount,@Reason,@CreatedBy,@CreatedAt)", new { movement.Id, movement.ShiftId, movement.Type, movement.Amount, movement.Reason, movement.CreatedBy, CreatedAt = movement.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss") }, tx); tx.Commit(); return movement; }
        }

        private static Shift GetByIdCore(SqliteConnection conn, string id) { var shift = conn.QueryFirstOrDefault<Shift>("SELECT * FROM Shift WHERE id=@id", new { id }); if (shift != null) shift.OpenedBy = conn.ExecuteScalar<string>("SELECT username FROM User WHERE id=@id", new { id = shift.OpenedBy }) ?? shift.OpenedBy; return shift; }
        private static double CashInWindow(SqliteConnection conn, DateTime from, DateTime to, SqliteTransaction tx) { return conn.ExecuteScalar<double>("SELECT COALESCE(SUM(p.amount),0) FROM DryCleanOrderPayment p WHERE p.paymentMethod='cash' AND p.paidAt >= @from AND p.paidAt <= @to", new { from = from.ToString("yyyy-MM-dd HH:mm:ss"), to = to.ToString("yyyy-MM-dd HH:mm:ss") }, tx); }
        private static double ExpensesInWindow(SqliteConnection conn, DateTime from, DateTime to, SqliteTransaction tx) { return conn.ExecuteScalar<double>("SELECT COALESCE(SUM(amount),0) FROM Expense WHERE paymentMethod='cash' AND date >= @from AND date <= @to", new { from = from.ToString("yyyy-MM-dd HH:mm:ss"), to = to.ToString("yyyy-MM-dd HH:mm:ss") }, tx); }
    }
}
