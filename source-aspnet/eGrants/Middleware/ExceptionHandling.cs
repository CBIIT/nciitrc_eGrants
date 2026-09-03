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

                // SECURITY: The Referer header is client-controlled, so redirecting to it
                // directly would allow an open redirect to an arbitrary external site.
                // Only honor the referer when it is a local URL that targets the same host
                // as the current request; otherwise fall back to the safe local URL.
                var redirectUrl = fallbackUrl;
                if (!string.IsNullOrWhiteSpace(referer) &&
                    Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
                    string.Equals(refererUri.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase))
                {
                    // Rebuild the target from trusted, validated components only (never the
                    // raw header string) so no attacker-supplied data flows into the redirect.
                    var separator = string.IsNullOrEmpty(refererUri.Query) ? "?" : "&";
                    redirectUrl = refererUri.PathAndQuery + separator + "error=" + errorMessage;
                }

                context.Response.Redirect(redirectUrl);

                //await context.Response.WriteAsJsonAsync(errorResponse);
                //context.Response.Redirect(referer + "?error=" + ex.Message ?? "/Views/Index");
            }
        }
    }
}

