using System;
using System.Net;
using System.Web.Http;
using PosCs.Api;
using PosCs.Application.Services;
using PosCs.Attributes;

namespace PosCs.Controllers
{
    [RoutePrefix("api")]
    public sealed class OperationsController : ApiController
    {
        private readonly OperationsService _service = CompositionRoot.OperationsService;

        [Route("audit-logs")]
        [HttpGet]
        [RequirePermission("audit.view")]
        public IHttpActionResult Audit(string action = null, string entityType = null, int page = 1, int pageSize = 20)
        {
            return Execute(() => _service.Audit(action, entityType, page, pageSize));
        }

        private IHttpActionResult Execute(Func<object> operation)
        {
            try { return Ok(operation()); }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[API ERR] Audit request failed: " + ex);
                if (ApiErrors.IsHandled(ex)) return Content(HttpStatusCode.BadRequest, new { message = ex.Message });
                return InternalServerError();
            }
        }
    }
}
