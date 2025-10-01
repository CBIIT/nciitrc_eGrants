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
                //Log.Error(ex, "Unhandled exception occurred while processing request: " + context.Request.Path);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var referer = context.Request.Headers["Referer"].ToString();

                var errorResponse = new
                {
                    Message = EgrantsCommon.ErrorMessages.UNEXPECTED_ERROR_OCCURRED,
                    Detail = ex.Message
                };

                var errorMessage = Uri.EscapeDataString(ex.Message);

                var fallbackUrl = "/Views/Index?error=" + errorMessage;
                var redirectUrl = string.IsNullOrWhiteSpace(referer)
                    ? fallbackUrl
                    : referer + "?error=" + errorMessage;

                context.Response.Redirect(redirectUrl);

                //await context.Response.WriteAsJsonAsync(errorResponse);
                //context.Response.Redirect(referer + "?error=" + ex.Message ?? "/Views/Index");
            }
        }
    }
}

