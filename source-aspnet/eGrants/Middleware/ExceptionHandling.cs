using eGrants.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Serilog;
using System.Text.Json;

namespace SimpleECommerceCore.Middleware
{
    // Middleware for global exception handling and logging
    public class ExceptionHandling
    {
        private readonly RequestDelegate _next;

        public ExceptionHandling(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unhandled exception occurred while processing request: " + context.Request.Path);

                // Avoid trying to modify the response if it has already started.
                if (context.Response.HasStarted)
                {
                    throw;
                }

                // Redirect to the application's error page (handled by ErrorController at "/Error").
                // Note: the previous fallback redirected to "/Views/Index", which is not a valid
                // route and resulted in an HTTP 404 that masked the real error.
                context.Response.Redirect("/Error");
            }
        }
    }
}

