using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PosCs.Api;
using PosCs.Application.Models;
using PosCs.Attributes;

namespace PosCs.Controllers
{
    [RoutePrefix("api/dry-clean/catalog")]
    public sealed class DryCleanCatalogController : ApiController
    {
        [HttpGet, Route("")]
        [RequirePermission("dry_clean.catalog.view", "dry_clean")]
        public HttpResponseMessage List(bool includeInactive = false) { return Request.CreateResponse(HttpStatusCode.OK, PosCs.CompositionRoot.DryCleanCatalogService.GetAll(includeInactive)); }

        [HttpPost, Route("")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Create(CreateDryCleanCatalogItemRequest request) { return Request.CreateResponse(HttpStatusCode.OK, PosCs.CompositionRoot.DryCleanCatalogService.Create(request, Request.GetOwinContextUserId())); }

        [HttpPut, Route("{id}")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public HttpResponseMessage Update(string id, UpdateDryCleanCatalogItemRequest request) { return Request.CreateResponse(HttpStatusCode.OK, PosCs.CompositionRoot.DryCleanCatalogService.Update(id, request, Request.GetOwinContextUserId())); }

        [HttpDelete, Route("{id}")]
        [RequirePermission("dry_clean.catalog.manage", "dry_clean")]
        public IHttpActionResult Delete(string id) { try { PosCs.CompositionRoot.DryCleanCatalogService.Delete(id, Request.GetOwinContextUserId()); return Ok(); } catch (Exception ex) { return Content(HttpStatusCode.BadRequest, new { message = ex.Message }); } }
    }
}
