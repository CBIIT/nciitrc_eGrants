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
                //var client = new SmtpClient("mailfwd.nih.gov", 25); 
                //client.Send("eGrants@nih.gov", "daryl.dehuff@nih.gov", "Test Email", "Hello from SmtpClient");
                Log.Error(exceptionFeature.Error,
                    "Unhandled exception at path {Path}", exceptionFeature.Path);
            }

            return View("GeneralError");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            //var client = new SmtpClient("mailfwd.nih.gov", 25);
            //client.Send("eGrants@nih.gov", "daryl.dehuff@nih.gov", "Test Email", "Hello from SmtpClient");
            Log.Error("HTTP {StatusCode} at path {Path}",
                statusCode,
                HttpContext.Request.Path);

            return View("StatusCodeError", statusCode);
        }
    }

}
