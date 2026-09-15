using System.Collections.Generic;
using System.Linq;
using PosCs.Application.Models;
using PosCs.Application.Ports;
using PosCs.Domain.Entities;

namespace PosCs.Application.Services
{
    /// <summary>Printing use cases: receipt image printing and barcode label printing.</summary>
    public class PrintingService
    {
        private readonly IReceiptPrinter _receiptPrinter;
        private readonly IBarcodeLabelPrinter _labelPrinter;
        private readonly IDryCleanOrderRepository _orders;

        public PrintingService(IReceiptPrinter receiptPrinter, IBarcodeLabelPrinter labelPrinter, IDryCleanOrderRepository orders)
        {
            _receiptPrinter = receiptPrinter;
            _labelPrinter = labelPrinter;
            _orders = orders;
        }

        public PrintOutcome PrintDryClean(string id, bool readyCopy)
        {
            var order = _orders.GetById(id);
            if (order == null) return new PrintOutcome { Success = false, Status = 404, Message = "Order not found" };
            return _receiptPrinter.PrintReceipt(new ReceiptContent
            {
                Id = order.Id,
                InvoiceNumber = order.OrderNumber,
                CreatedAt = order.ReceivedAt,
                Discount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                CustomerName = order.Client == null ? null : order.Client.Name,
                CustomerPhone = order.Client == null ? null : order.Client.Phone,
                DeliveryAddress = order.DeliveryAddress,
                DocumentTitle = readyCopy ? "طلب جاهز للتسليم" : "إيصال استلام طلب",
                PaidAmount = order.PaidAmount,
                RemainingAmount = order.RemainingAmount,
                Items = order.Lines.Select(i => new ReceiptLineItem { Name = i.GarmentType + " - " + i.ServiceDescription, Quantity = i.Quantity, SalePrice = i.UnitPrice, FinalTotal = i.LineTotal }).ToList()
            });
        }

        public PrintOutcome PrintReceipt(PrintReceiptRequest request)
        {
            if (request?.Invoice == null)
                return new PrintOutcome { Success = false, Message = "Missing invoice data" };

            var invoice = request.Invoice;
            var content = new ReceiptContent
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CreatedAt = invoice.CreatedAt,
                Discount = invoice.Discount,
                TotalAmount = invoice.TotalAmount,
                Items = ExtractItems(invoice)
            };

            return _receiptPrinter.PrintReceipt(content);
        }

        public PrintOutcome PrintBarcodeLabel(PrintBarcodeLabelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Barcode))
                return new PrintOutcome { Success = false, Message = "Missing barcode" };

            var count = request.Count > 0 ? request.Count : 1;
            return _labelPrinter.PrintLabels(request.Barcode, request.Name, request.Price, count);
        }

        private static List<ReceiptLineItem> ExtractItems(PrintInvoicePayload invoice)
        {
            var items = invoice.Items ?? new List<PrintItemPayload>();
            if (items.Count == 0 && invoice.InvoiceDetail != null)
            {
                items = invoice.InvoiceDetail.Select(d => new PrintItemPayload
                {
                    Name = d.Product?.Name ?? d.Name ?? "Item",
                    Quantity = d.Quantity,
                    SalePrice = d.SalePrice,
                    UnitName = d.UnitName,
                    FinalTotal = d.FinalTotal,
                    BundleComponents = d.BundleComponents
                }).ToList();
            }
            return items.Select(i => new ReceiptLineItem
            {
                Name = i.Name ?? "Item",
                UnitName = i.UnitName,
                Quantity = i.Quantity,
                SalePrice = i.SalePrice,
                FinalTotal = i.FinalTotal,
                Components = (i.BundleComponents ?? new List<PrintBundleComponentPayload>()).Select(c => new ReceiptComponent
                {
                    Name = c.Name,
                    Quantity = c.Quantity
                }).ToList()
            }).ToList();
        }
    }
}
