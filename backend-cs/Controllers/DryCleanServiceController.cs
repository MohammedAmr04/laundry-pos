using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PosCs.Api;
using PosCs.Application.Models;
using PosCs.Attributes;

namespace PosCs.Controllers
{
    [RoutePrefix("api/dry-clean/services")]
    public sealed class DryCleanServiceController : ApiController
    {
        [HttpGet, Route("")]
        [RequirePermission("dry_clean.catalog.view", "dry_clean")]
        public HttpResponseMessage List(bool includeInactive = false) { return Handle(() => PosCs.CompositionRoot.DryCleanServiceDefinitionService.GetAll(includeInactive), "Failed to fetch dry clean services"); }

        [HttpPost, Route("")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Create(CreateDryCleanServiceRequest request) { return Handle(() => PosCs.CompositionRoot.DryCleanServiceDefinitionService.Create(request, Request.GetOwinContextUserId()), "Failed to create dry clean service"); }

        [HttpPut, Route("{id}")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Update(string id, UpdateDryCleanServiceRequest request) { return Handle(() => PosCs.CompositionRoot.DryCleanServiceDefinitionService.Update(id, request, Request.GetOwinContextUserId()), "Failed to update dry clean service"); }

        [HttpDelete, Route("{id}")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Delete(string id) { return Handle(() => { PosCs.CompositionRoot.DryCleanServiceDefinitionService.Delete(id, Request.GetOwinContextUserId()); return new { success = true }; }, "Failed to delete dry clean service"); }

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
