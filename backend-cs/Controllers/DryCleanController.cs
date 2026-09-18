using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PosCs.Api;
using PosCs.Application.Models;
using PosCs.Application.Services;
using PosCs.Attributes;

namespace PosCs.Controllers
{
    [RoutePrefix("api/dry-clean/orders")]
    public sealed class DryCleanController : ApiController
    {
        private readonly DryCleanService _service = CompositionRoot.DryCleanService;

        [Route("")]
        [HttpGet]
        [RequirePermission("dry_clean.orders.view", "dry_clean")]
        public HttpResponseMessage List(int page = 1, int pageSize = 20, string status = null, string q = null, string clientId = null, string paymentStatus = null)
        {
            return Handle(() => _service.GetPaged(status, q, clientId, paymentStatus, page, pageSize), "Failed to fetch dry clean orders");
        }

        [Route("{id}")]
        [HttpGet]
        [RequirePermission("dry_clean.orders.view", "dry_clean")]
        public HttpResponseMessage Get(string id) { return Handle(() => _service.GetById(id), "Failed to fetch dry clean order"); }

        [Route("")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.create", "dry_clean")]
        public HttpResponseMessage Create(CreateDryCleanOrderRequest request) { return Handle(() => _service.Create(request, Request.GetOwinContextUserId()), "Failed to create dry clean order"); }

        [Route("{id}/prepare")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.complete", "dry_clean")]
        public HttpResponseMessage Prepare(string id, CompleteDryCleanOrderRequest request) { return Handle(() => _service.Prepare(id, request?.IronerEmployeeId, Request.GetOwinContextUserId()), "Failed to prepare dry clean order"); }

        [Route("{id}/ironer")]
        [HttpPut]
        [RequirePermission("dry_clean.orders.process", "dry_clean")]
        public HttpResponseMessage AssignIroner(string id, CompleteDryCleanOrderRequest request) { return Handle(() => _service.AssignIroner(id, request?.IronerEmployeeId, Request.GetOwinContextUserId()), "Failed to assign dry clean ironer"); }

        [Route("{id}/payments")]
        [HttpPost]
        [RequirePermission("dry_clean.payments.create", "dry_clean")]
        public HttpResponseMessage Payment(string id, CreateDryCleanPaymentRequest request) { return Handle(() => _service.AddPayment(id, request, Request.GetOwinContextUserId()), "Failed to record dry clean payment"); }

        [Route("{id}/deliver")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.deliver", "dry_clean")]
        public HttpResponseMessage Deliver(string id, DeliverDryCleanOrderRequest request) { return Handle(() => _service.Deliver(id, request?.DeliveryEmployeeId, Request.GetOwinContextUserId()), "Failed to deliver dry clean order"); }

        [Route("productivity")]
        [HttpGet]
        [RequirePermission("dry_clean.productivity.view", "dry_clean")]
        public HttpResponseMessage Productivity(string from = null, string to = null) { return Handle(() => _service.GetProductivity(from, to), "Failed to fetch dry clean productivity"); }

        private HttpResponseMessage Handle(Func<object> action, string failedMessage)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, action());
            }
            catch (Exception ex)
            {
                if (ApiErrors.IsHandled(ex)) return ApiErrors.From(Request, ex, null);
                Console.Error.WriteLine($"[API ERR] {failedMessage}: {ex}");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, failedMessage);
            }
        }
    }
}
