namespace eGrants.Controllers
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Mvc;

    using Serilog;

    //using System.Net.Mail;

    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionFeature != null)
            {
                Log.Error(exceptionFeature.Error,
                    "Unhandled exception at path {Path}", exceptionFeature.Path);
            }

            return View("GeneralError");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            Log.Error("HTTP {StatusCode} at path {Path}",
                statusCode,
                HttpContext.Request.Path);

            return View("StatusCodeError", statusCode);
        }
    }

}
