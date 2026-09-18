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
        public HttpResponseMessage List(bool includeInactive = false) { return Request.CreateResponse(HttpStatusCode.OK, PosCs.CompositionRoot.DryCleanServiceDefinitionService.GetAll(includeInactive)); }

        [HttpPost, Route("")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Create(CreateDryCleanServiceRequest request) { return Request.CreateResponse(HttpStatusCode.OK, PosCs.CompositionRoot.DryCleanServiceDefinitionService.Create(request, Request.GetOwinContextUserId())); }

        [HttpPut, Route("{id}")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Update(string id, UpdateDryCleanServiceRequest request) { return Request.CreateResponse(HttpStatusCode.OK, PosCs.CompositionRoot.DryCleanServiceDefinitionService.Update(id, request, Request.GetOwinContextUserId())); }

        [HttpDelete, Route("{id}")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public IHttpActionResult Delete(string id) { try { PosCs.CompositionRoot.DryCleanServiceDefinitionService.Delete(id, Request.GetOwinContextUserId()); return Ok(); } catch (Exception ex) { return Content(HttpStatusCode.BadRequest, new { message = ex.Message }); } }
    }
}
