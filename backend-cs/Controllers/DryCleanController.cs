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
        public HttpResponseMessage List(int page = 1, int pageSize = 20, string status = null, string q = null)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _service.GetPaged(status, q, page, pageSize));
        }

        [Route("{id}")]
        [HttpGet]
        [RequirePermission("dry_clean.orders.view", "dry_clean")]
        public HttpResponseMessage Get(string id) { return Request.CreateResponse(HttpStatusCode.OK, _service.GetById(id)); }

        [Route("")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.create", "dry_clean")]
        public HttpResponseMessage Create(CreateDryCleanOrderRequest request) { return Request.CreateResponse(HttpStatusCode.OK, _service.Create(request, Request.GetOwinContextUserId())); }

        [Route("{id}/processing")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.process", "dry_clean")]
        public HttpResponseMessage StartProcessing(string id) { return Request.CreateResponse(HttpStatusCode.OK, _service.StartProcessing(id, Request.GetOwinContextUserId())); }

        [Route("{id}/complete")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.complete", "dry_clean")]
        public HttpResponseMessage Complete(string id, CompleteDryCleanOrderRequest request) { return Request.CreateResponse(HttpStatusCode.OK, _service.Complete(id, request?.IronerEmployeeId, Request.GetOwinContextUserId())); }

        [Route("{id}/payments")]
        [HttpPost]
        [RequirePermission("dry_clean.payments.create", "dry_clean")]
        public HttpResponseMessage Payment(string id, CreateDryCleanPaymentRequest request) { return Request.CreateResponse(HttpStatusCode.OK, _service.AddPayment(id, request, Request.GetOwinContextUserId())); }

        [Route("{id}/deliver")]
        [HttpPost]
        [RequirePermission("dry_clean.orders.deliver", "dry_clean")]
        public HttpResponseMessage Deliver(string id, DeliverDryCleanOrderRequest request) { return Request.CreateResponse(HttpStatusCode.OK, _service.Deliver(id, request?.DeliveryEmployeeId, Request.GetOwinContextUserId())); }

        [Route("productivity")]
        [HttpGet]
        [RequirePermission("dry_clean.productivity.view", "dry_clean")]
        public HttpResponseMessage Productivity(string from = null, string to = null) { return Request.CreateResponse(HttpStatusCode.OK, _service.GetProductivity(from, to)); }
    }
}
