using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace DngnApiBackend.Controllers
{
    public class BaseController : Controller
    {
        protected virtual IActionResult ModelStateErrorResult()
        {
            return UserError("INVALID_REQUEST_DATA",
                ModelState.FirstOrDefault().Value?.Errors?.FirstOrDefault()?.ErrorMessage);
        }

        protected virtual IActionResult UserError(string code, string? message)
        {
            return BadRequest(new
            {
                status = "failed",
                error  = new {code, message}
            });
        }

        protected virtual IActionResult ServiceError(string code, string message)
        {
            return StatusCode(500, new
            {
                status = "failed",
                error  = new {code, message}
            });
        }
    }
}