using eGrants.Common;
using eGrants.DAL;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using Serilog;

using SimpleECommerceCore.Middleware;

// Enable Serilog internal diagnostics. 
// This logs Serilog’s own configuration or sink failures (not application logs) 
// Useful only for troubleshooting when logs are not appearing as expected.
var selfLogPath = Path.Combine(AppContext.BaseDirectory, "serilog-selflog.txt");

Serilog.Debugging.SelfLog.Enable(message =>
{
    File.AppendAllText(selfLogPath, message + Environment.NewLine);
});

#region Setting up the database connection

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with connection string
var raw = builder.Configuration.GetConnectionString("DefaultConnection");

// Pull username/password from environment variables
var user = builder.Configuration["DB_USER"];
var password = builder.Configuration["DB_PASSWORD"];

// Replace placeholders
var finalConnectionString = raw
    .Replace("{DB_USER}", user)
    .Replace("{DB_PASSWORD}", password);

// Use the final connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(finalConnectionString));
#endregion

#region Setting up the Entra ID client secret

// Pull the Entra ID client secret from an environment variable and replace the
// "{eGrants_AzureAd_ClientSecret}" placeholder configured in appsettings, mirroring
// the DB_USER / DB_PASSWORD pattern used for the connection string above.
var azureAdClientSecret = builder.Configuration["eGrants_AzureAd_ClientSecret"];
var configuredClientSecret = builder.Configuration["AzureAd:ClientSecret"];

if (!string.IsNullOrEmpty(configuredClientSecret))
{
    builder.Configuration["AzureAd:ClientSecret"] =
        configuredClientSecret.Replace("{eGrants_AzureAd_ClientSecret}", azureAdClientSecret);
}
#endregion

#region Setting up the eRA client certificate password

// Pull the client certificate (.pfx) password from an environment variable and replace
// the "{CERT_PASSWORD}" placeholder configured in appsettings, mirroring the
// DB_USER / DB_PASSWORD and client secret patterns above.
var certPassword = builder.Configuration["CERT_PASSWORD"];
var configuredCertPass = builder.Configuration["AppSettings:certPass"];

if (!string.IsNullOrEmpty(configuredCertPass))
{
    builder.Configuration["AppSettings:certPass"] =
        configuredCertPass.Replace("{CERT_PASSWORD}", certPassword ?? string.Empty);
}
#endregion

#region Request Size Limits Configuration
// ====================================================================================
// LARGE FILE UPLOAD SUPPORT
// ====================================================================================
// These settings are required for the "Convert to PDF & Add" functionality.
//
// IMPORTANT: ERR_HTTP2_PROTOCOL_ERROR near the end of an upload is often caused by
// upstream timeouts or the server/proxy closing the HTTP/2 stream while the request
// body is still being sent.
//
// To reduce this:
// - Increase Kestrel keep-alive / header timeouts (so slow uploads don't get cut off)
// - Increase MaxRequestBodySize and multipart limits (so large bodies aren't rejected)
//
// We keep limits aligned to the legacy .NET Framework configuration (2GB).
// ====================================================================================

// Configure Kestrel server limits for large file uploads
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // Maximum request body size (2GB)
    // This is the total size of the HTTP request body including file uploads
    options.Limits.MaxRequestBodySize = 2147483648; //2GB

    // TIMEOUTS (helps prevent HTTP/2 stream resets during slow uploads)
    // - KeepAliveTimeout: how long to keep an idle connection open
    // - RequestHeadersTimeout: how long to wait for request headers
    // 
    // Note: Uploads can take time on congested networks. If these are too low,
    // the server or a proxy may terminate the connection mid-upload.
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);

    // Optional: allow more generous data rates for slow clients
    // The defaults can be overly aggressive for some environments.
    options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
    options.Limits.MinResponseDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
});

// Configure IIS server limits (when hosted in IIS)
builder.Services.Configure<IISServerOptions>(options =>
{
    // Maximum request body size (2GB)
    options.MaxRequestBodySize = 2147483648; //2GB
});

// Configure form options for multipart uploads (file uploads via form data)
builder.Services.Configure<FormOptions>(options =>
{
    // Maximum length of the entire multipart body (2GB)
    options.MultipartBodyLengthLimit = 2147483648; //2GB

    // Maximum length of individual form values (50MB for large text fields)
    options.ValueLengthLimit = 52428800; //50MB

    // Maximum length of form key names
    options.KeyLengthLimit = 2048;

    // Maximum number of form entries (files + form fields)
    options.ValueCountLimit = 1024;

    // Maximum header section size
    options.MultipartHeadersLengthLimit = 16384;
});

#endregion

#region Service Configuration

// System Web Adapters & HTTP utilities
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();
builder.Services.AddHttpContextAccessor();

// Application Services & Repositories (Dependency Injection)
builder.Services.AddScoped<EgrantsCommon>();
builder.Services.AddScoped<IeGrantsService, eGrantsService>();
builder.Services.AddScoped<IeGrantsRepository, eGrantsRepository>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<ISessionInfoService, SessionInfoService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IInstitutionalFilesService, InstitutionalFilesService>();
builder.Services.AddScoped<IInstitutionalFilesRepository, InstitutionalFilesRepository>();
builder.Services.AddScoped<ICategoryEditService, CategoryEditService>();
builder.Services.AddScoped<IManagementService, ManagementService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IEgrantsAccessService, EgrantsAccessService>();
builder.Services.AddScoped<IFlagMaintenanceService, FlagMaintenanceService>();
builder.Services.AddScoped<IGPMATWorkReportService, GPMATWorkReportService>();
builder.Services.AddScoped<IApplDestructedService, ApplDestructedService>();
builder.Services.AddScoped<ISupplementService, SupplementService>();
builder.Services.AddScoped<IEgrantsFundingService, EgrantsFundingService>();
builder.Services.AddScoped<IApplService, ApplService>();

// Session configuration
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make session cookie essential
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Microsoft Entra ID (OIDC) Authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.HttpOnly = true;
    // The eGrants application session cookie is first-party (top-level navigation
    // to its own host). Use Lax so it is reliably sent when users re-enter eGrants
    // from an external referrer (for example authdev.nih.gov). SameSite=None can be
    // treated as third-party and dropped by modern browsers on top-level re-entry.
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // Persist the auth cookie across browser restarts. ExpireTimeSpan controls the
    // ticket lifetime, but the browser only keeps the cookie past close-out when the
    // cookie itself carries a Max-Age/Expires attribute, which Cookie.MaxAge sets.
    options.Cookie.MaxAge = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);

    // Cookie auth diagnostics to determine why a request is redirected to login
    // even when users report that cookies are present in their browser.
    options.Events = new CookieAuthenticationEvents
    {
        OnSigningIn = context =>
        {
            // Authoritative place to force a PERSISTENT cookie. This runs for the
            // actual cookie being written and is not overridden by Microsoft.Identity.Web's
            // OIDC wiring. Without this the cookie is issued as a session cookie (no
            // Expires/Max-Age) and is deleted when the browser is closed, forcing re-login.
            context.Properties.IsPersistent = true;
            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);

            Log.Information(
                "Cookie signing in. Name={Name}, IsPersistent={IsPersistent}, ExpiresUtc={ExpiresUtc}, TraceId={TraceId}",
                context.Principal?.Identity?.Name,
                context.Properties?.IsPersistent,
                context.Properties?.ExpiresUtc,
                context.HttpContext.TraceIdentifier);

            return Task.CompletedTask;
        },
        OnSignedIn = context =>
        {
            Log.Information(
                "Cookie signed in. Name={Name}, IsPersistent={IsPersistent}, ExpiresUtc={ExpiresUtc}, TraceId={TraceId}",
                context.Principal?.Identity?.Name,
                context.Properties?.IsPersistent,
                context.Properties?.ExpiresUtc,
                context.HttpContext.TraceIdentifier);

            return Task.CompletedTask;
        },
        OnValidatePrincipal = context =>
        {
            Log.Information(
                "Cookie validate principal. IsAuthenticated={IsAuthenticated}, Name={Name}, ExpiresUtc={ExpiresUtc}, IssuedUtc={IssuedUtc}, TraceId={TraceId}",
                context.Principal?.Identity?.IsAuthenticated == true,
                context.Principal?.Identity?.Name,
                context.Properties?.ExpiresUtc,
                context.Properties?.IssuedUtc,
                context.HttpContext.TraceIdentifier);

            return Task.CompletedTask;
        },
        OnRedirectToLogin = context =>
        {
            Log.Warning(
                "Cookie redirect to login. Path={Path}, RedirectUri={RedirectUri}, TraceId={TraceId}",
                context.Request.Path,
                context.RedirectUri,
                context.HttpContext.TraceIdentifier);

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

// Use the authorization code flow (back-channel token exchange) rather than the
// hybrid/implicit flow. This avoids AADSTS700054 ("response_type 'id_token' is not
// enabled for the application") by requesting "response_type=code" instead of an
// id_token at the authorize endpoint. The ID token is then returned via the token
// endpoint using the configured client secret.
builder.Services.Configure<OpenIdConnectOptions>(
    OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ResponseType = OpenIdConnectResponseType.Code;

        // Force an interactive login prompt on EVERY authentication redirect
        // (including after the browser is closed and reopened). Without this,
        // Entra performs a silent SSO re-login using the still-valid Microsoft
        // session cookie and the user is never challenged. Setting prompt=login
        // instructs Entra to ignore the existing SSO session and re-prompt for
        // credentials.
        options.Prompt = "login";
    });

// Make the application authentication cookie a non-persistent (session) cookie so
// it is discarded when the browser is closed. Combined with prompt=login above,
// reopening the browser then triggers a fresh interactive login rather than a
// silent SSO restore.
//
// NOTE: Browser "session restore" (e.g. Chrome/Edge "reopen tabs on startup")
// can hand session cookies back after a reopen, so we cannot rely on cookie
// deletion alone. The OnValidatePrincipal event below enforces an absolute
// server-side lifetime that holds regardless of what the browser does with the
// cookie.
builder.Services.Configure<CookieAuthenticationOptions>(
    CookieAuthenticationDefaults.AuthenticationScheme, (CookieAuthenticationOptions options) =>
    {
        options.Cookie.MaxAge = null;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = false;

        options.Events.OnValidatePrincipal = async context =>
        {
            // Enforce an absolute session lifetime based on when the auth ticket
            // was issued. If the ticket is older than the allowed window, reject
            // it and sign out so the next request triggers a fresh (interactive,
            // prompt=login) OIDC challenge. This is immune to browsers restoring
            // session cookies on reopen.
            var absoluteLifetime = TimeSpan.FromMinutes(10);
            var issuedUtc = context.Properties?.IssuedUtc;

            if (issuedUtc == null ||
                DateTimeOffset.UtcNow - issuedUtc.Value > absoluteLifetime)
            {
                context.RejectPrincipal();
                context.HttpContext.Session.Clear();
                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

#endregion

#region Logging (Serilog)

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext();
});

#endregion

var app = builder.Build();

#region IronPdf Initialization & Warm-up

// ====================================================================================
// IRONPDF STARTUP INITIALIZATION & WARM-UP
// ====================================================================================
// WHY: IronPdf uses an embedded Chrome rendering engine. The FIRST render in a process
// pays a large one-time cost (engine/Chromium initialization). Previously the license
// and Installation settings were applied lazily inside every conversion call, so this
// cold-start cost was paid by the first user action (e.g. "Replace document"), which
// could take minutes in the development environment.
//
// By configuring IronPdf once here and performing a tiny warm-up render at startup, the
// Chrome engine is initialized before any user request, removing that delay from the
// first document conversion/replace.
// ====================================================================================
try
{
    IronPdf.License.LicenseKey =
        "IRONPDF.NATIONALINSTITUTESOFHEALTH.IRO240906.3804.91129-DA12E4CBF3-DBQNBY5HLE5VALY-Q5R6HRQZIG3H-QKT3YRHJTBUH-PNRD6KJMHI5C-G7MCDB5LXYT3-Y5V5MI-LNUL6X3VZT6VUA-IRONPDF.DOTNET.PLUS.5YR-P3KMXU.RENEW.SUPPORT.05.SEP.2029";

    // Disable local disk access or cross-origin requests during rendering.
    IronPdf.Installation.EnableWebSecurity = true;

    // Keep IronPdf's Chrome/engine temp files on a fast local disk. A slow or network
    // temp location makes every cold start dramatically slower.
    var ironPdfTempPath = Path.Combine(AppContext.BaseDirectory, "ironpdf_temp");
    Directory.CreateDirectory(ironPdfTempPath);
    IronPdf.Installation.TempFolderPath = ironPdfTempPath;

    // Warm up the Chrome rendering engine with a minimal render so the first real
    // conversion doesn't pay the initialization cost.
    var warmupRenderer = new IronPdf.ChromePdfRenderer();
    using var warmupPdf = warmupRenderer.RenderHtmlAsPdf("<p>warmup</p>");

    Log.Information("IronPdf initialized and warmed up. TempFolderPath={TempPath}", ironPdfTempPath);
}
catch (Exception ex)
{
    // A warm-up failure should never prevent the application from starting; conversions
    // will still fall back to lazy initialization on first use.
    Log.Warning(ex, "IronPdf warm-up failed during startup; conversions will initialize lazily.");
}

#endregion

#region Middleware Pipeline

// Global exception handling middleware
app.UseMiddleware<ExceptionHandling>();

// Enforce HSTS in non-development environments.
// "Local" is treated as a development-like environment so local runs behave the
// same as Development (no HSTS) even though appsettings.Development.json is not loaded.
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Local"))
{
    app.UseHsts();
}

#if !DEBUG
 // Handles unhandled exceptions (500 errors)
 app.UseExceptionHandler("/Error");

 // Handles HTTP status codes (404,403, etc.)
 app.UseStatusCodePagesWithReExecute("/Error/{0}");
#endif

app.UseHttpsRedirection();
app.UseStaticFiles();

#if DEBUG
// Local debug-only document URL mappings for viewing files from local disk.
var localPdfOutputPath = @"C:\PdfFileOutput";
Directory.CreateDirectory(localPdfOutputPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(localPdfOutputPath),
    RequestPath = "/data/funded2/nci/main"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(localPdfOutputPath),
    RequestPath = "/data/funded2/nci/main1"
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(localPdfOutputPath),
    RequestPath = "/data/funded/nci/modify"
});
#endif

app.UseRouting();

// Cross-site diagnostics middleware for requests entering eGrants from other sites.
// Logs key headers/cookie presence and flags unsuccessful outcomes for correlation.
app.Use(async (context, next) =>
{
    var request = context.Request;
    var referer = request.Headers.Referer.ToString();
    var origin = request.Headers.Origin.ToString();
    var forwardedProto = request.Headers["X-Forwarded-Proto"].ToString();
    var forwardedHost = request.Headers["X-Forwarded-Host"].ToString();
    var forwardedFor = request.Headers["X-Forwarded-For"].ToString();
    var hasAuthCookie = request.Cookies.Keys.Any(k => k.Contains(".AspNetCore.Cookies", StringComparison.OrdinalIgnoreCase));
    var hasNonceCookie = request.Cookies.Keys.Any(k => k.StartsWith(".AspNetCore.OpenIdConnect.Nonce", StringComparison.OrdinalIgnoreCase));
    var hasCorrelationCookie = request.Cookies.Keys.Any(k => k.StartsWith(".AspNetCore.Correlation.", StringComparison.OrdinalIgnoreCase));

    // Treat request as cross-site when referer host differs from current host.
    var isCrossSite = !string.IsNullOrWhiteSpace(referer) &&
                      Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
                      !string.Equals(refererUri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase);

    if (isCrossSite)
    {
        Log.Information(
            "Cross-site inbound request. Method={Method}, Path={Path}, Query={Query}, Host={Host}, Referer={Referer}, Origin={Origin}, XForwardedProto={XForwardedProto}, XForwardedHost={XForwardedHost}, XForwardedFor={XForwardedFor}, UserAgent={UserAgent}, RemoteIp={RemoteIp}, IsAuthenticated={IsAuthenticated}, HasAuthCookie={HasAuthCookie}, HasNonceCookie={HasNonceCookie}, HasCorrelationCookie={HasCorrelationCookie}, TraceId={TraceId}",
            request.Method,
            request.Path,
            request.QueryString.Value,
            request.Host.Value,
            referer,
            origin,
            forwardedProto,
            forwardedHost,
            forwardedFor,
            request.Headers.UserAgent.ToString(),
            context.Connection.RemoteIpAddress?.ToString(),
            context.User?.Identity?.IsAuthenticated == true,
            hasAuthCookie,
            hasNonceCookie,
            hasCorrelationCookie,
            context.TraceIdentifier);
    }

    var isAuthEndpoint = request.Path.StartsWithSegments("/MicrosoftIdentity") ||
                         request.Path.StartsWithSegments("/signin-oidc") ||
                         request.Path.StartsWithSegments("/signout-callback-oidc");

    if (!isCrossSite &&
        !isAuthEndpoint &&
        HttpMethods.IsGet(request.Method) &&
        !hasAuthCookie &&
        context.User?.Identity?.IsAuthenticated != true)
    {
        Log.Warning(
            "Direct inbound request has no auth cookie and user is unauthenticated. Path={Path}, Query={Query}, Host={Host}, XForwardedProto={XForwardedProto}, XForwardedHost={XForwardedHost}, XForwardedFor={XForwardedFor}, UserAgent={UserAgent}, TraceId={TraceId}",
            request.Path,
            request.QueryString.Value,
            request.Host.Value,
            forwardedProto,
            forwardedHost,
            forwardedFor,
            request.Headers.UserAgent.ToString(),
            context.TraceIdentifier);
    }

    await next.Invoke();

    // Log failures for all requests, plus redirects/errors for cross-site traffic.
    if (context.Response.StatusCode >= 400 || (isCrossSite && context.Response.StatusCode >= 300))
    {
        Log.Warning(
            "Inbound request completed with notable status. Method={Method}, Path={Path}, Query={Query}, StatusCode={StatusCode}, Host={Host}, Referer={Referer}, Origin={Origin}, IsAuthenticated={IsAuthenticated}, TraceId={TraceId}",
            request.Method,
            request.Path,
            request.QueryString.Value,
            context.Response.StatusCode,
            request.Host.Value,
            referer,
            origin,
            context.User?.Identity?.IsAuthenticated == true,
            context.TraceIdentifier);
    }
});

app.UseSession(); // Enable session middleware

app.UseAuthentication();

// Post-auth diagnostics: logs the effective principal state after cookie processing.
// This helps distinguish between "cookie exists" and "cookie produced an authenticated user".
app.Use(async (context, next) =>
{
    var request = context.Request;
    var referer = request.Headers.Referer.ToString();
    var hasAuthCookie = request.Cookies.Keys.Any(k =>
        k.Contains(".AspNetCore.Cookies", StringComparison.OrdinalIgnoreCase));

    Log.Information(
        "Post-auth state. Method={Method}, Path={Path}, Host={Host}, Referer={Referer}, IsAuthenticated={IsAuthenticated}, AuthType={AuthType}, Name={Name}, HasAuthCookie={HasAuthCookie}, TraceId={TraceId}",
        request.Method,
        request.Path,
        request.Host.Value,
        referer,
        context.User?.Identity?.IsAuthenticated == true,
        context.User?.Identity?.AuthenticationType,
        context.User?.Identity?.Name,
        hasAuthCookie,
        context.TraceIdentifier);

    if (hasAuthCookie && context.User?.Identity?.IsAuthenticated != true)
    {
        Log.Warning(
            "Auth cookie is present but request is still unauthenticated after cookie auth. Path={Path}, Host={Host}, TraceId={TraceId}",
            request.Path,
            request.Host.Value,
            context.TraceIdentifier);
    }

    await next.Invoke();
});

app.UseAuthorization();

// Middleware to initialize and validate the user session from Entra ID claims.
app.Use(async (context, next) =>
{
    // Remove unwanted headers
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-AspNetMvc-Version");
        context.Response.Headers.Remove("X-AspNet-Version");
        context.Response.Headers.Remove("X-UA-Compatible");
        return Task.CompletedTask;
    });

    // Skip session initialization for auth endpoints
    if (context.Request.Path.StartsWithSegments("/MicrosoftIdentity") ||
        context.Request.Path.StartsWithSegments("/signin-oidc") ||
        context.Request.Path.StartsWithSegments("/signout-callback-oidc"))
    {
        await next.Invoke();
        return;
    }

    if (string.IsNullOrEmpty(context.Session.GetString("userid")))
    {
        // User must be authenticated via Entra ID OIDC at this point
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            // Not authenticated — the [Authorize] policy will trigger OIDC challenge
            await next.Invoke();
            return;
        }

        // ===================================================================================
        // Extract user identity from Entra ID OIDC claims
        // ===================================================================================
        // The "preferred_username" claim contains the UPN (e.g., "dehuffdc@nih.gov").
        // We extract the username portion to match the existing person table.
        // Resolution logic lives in EntraIdUserResolver so it can be unit tested.
        // ===================================================================================
        string userId = eGrants.Common.EntraIdUserResolver.ResolveUserId(context.User);

        if (string.IsNullOrEmpty(userId))
        {
            var logger = context.RequestServices.GetService<ILogger<Program>>();
            logger?.LogWarning("No user identity found in Entra ID claims.");
            context.Response.Redirect("/egrants_default.htm");
            return;
        }

        context.Session.SetString("userid", userId);

        // Determine IC (Institute/Org Code) - default to NCI
        var ic = eGrants.Common.EntraIdUserResolver.ResolveIc(context.User);
        context.Session.SetString("ic", ic);

        // Detect browser
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        string browserName = userAgent.Contains("Chrome") ? "Chrome" :
        userAgent.Contains("Firefox") ? "Firefox" :
                (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) ? "Safari" :
       userAgent.Contains("Edg") ? "Edge" :
            (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) ? "Internet Explorer" : "Unknown";

        context.Session.SetString("browser", browserName);
        context.Session.SetString("CurrentView", "standardForm");

        // Resolve EgrantsCommon service
        var egrantsCommon = context.RequestServices.GetRequiredService<EgrantsCommon>();

        var usertype = egrantsCommon.UserType(context.Session.GetString("ic"), context.Session.GetString("userid"));

        if (string.IsNullOrEmpty(usertype) || usertype == "NULL")
        {
            context.Response.Redirect("/egrants_default.htm");
            return;
        }

        // Populate user session variables
        var users = egrantsCommon.uservar(context.Session.GetString("userid"), context.Session.GetString("ic"), usertype);

        foreach (var usr in users)
        {
            context.Session.SetString("Validation", usr.Validation);
            context.Session.SetString("userid", usr.UserId);
            context.Session.SetString("ic", usr.ic);
            context.Session.SetInt32("Personid", usr.personID);
            context.Session.SetInt32("position_id", usr.positionID);
            context.Session.SetString("UserName", usr.PersonName);
            context.Session.SetString("UserEmail", usr.PersonEmail);
            context.Session.SetString("Menus", usr.menulist);
        }

        if (context.Session.GetString("Validation")?.ToString() != "OK")
        {
            context.Response.Redirect("/egrants_default.htm");
            return;
        }

        // Load app settings into session
        context.Session.SetString("WebGrantUrl", builder.Configuration["AppSettings:webGrantUrl"] ?? string.Empty);
        context.Session.SetString("WebGrantRelativePath", builder.Configuration["AppSettings:webGrantRelativePath"] ?? string.Empty);
#if DEBUG
        var localImageServerUrl = $"{context.Request.Scheme}://{context.Request.Host}/";
        context.Session.SetString("ImageServerUrl", localImageServerUrl);
#else
        context.Session.SetString("ImageServerUrl", builder.Configuration["AppSettings:imageServerUrl"] ?? string.Empty);
#endif
        context.Session.SetInt32("dashboard", 0);
        context.Session.SetString("EgrantsDocNewRelativePath", builder.Configuration["AppSettings:egrantsDocNewRelativePath"] ?? string.Empty);
        context.Session.SetString("EgrantsDocModifyRelativePath", builder.Configuration["AppSettings:egrantsDocModifyRelativePath"] ?? string.Empty);
        context.Session.SetString("EgrantsFundingRelativePath", builder.Configuration["AppSettings:egrantsFundingRelativePath"] ?? string.Empty);
        context.Session.SetString("EgrantsInstRelativePath", builder.Configuration["AppSettings:egrantsInstRelativePath"] ?? string.Empty);
        context.Session.SetString("EgrantsFundingModifyRelativePath", builder.Configuration["AppSettings:egrantsFundingModifyRelativePath"] ?? string.Empty);
        context.Session.SetString("EgrantsDocEmail", builder.Configuration["AppSettings:egrantsDocEmail"] ?? string.Empty);
        context.Session.SetString("closeoutAcceptance", builder.Configuration["AppSettings:closeoutAcceptance"] ?? string.Empty);
        context.Session.SetString("frpprAcceptance", builder.Configuration["AppSettings:frpprAcceptance"] ?? string.Empty);
        context.Session.SetString("irpprAcceptance", builder.Configuration["AppSettings:irpprAcceptance"] ?? string.Empty);
        context.Session.SetString("GitHubToken", builder.Configuration["AppSettings:GitHubToken"] ?? string.Empty);
        context.Session.SetString("CertPath", builder.Configuration["AppSettings:certPath"] ?? string.Empty);
        context.Session.SetString("CertPass", builder.Configuration["AppSettings:certPass"] ?? string.Empty);
        context.Session.SetString("EraUrlBase", builder.Configuration["AppSettings:eraUrlBase"] ?? string.Empty);

        egrantsCommon.UpdateUsersLastLoginDate(userId);
        string token = context.Session.GetString("GitHubToken")?.ToString() ?? string.Empty;
        var latestReleaseFull = egrantsCommon.GetLatestReleaseTagAsync("CBIIT", "nciitrc_eGrants", token);
        var latestRelease = latestReleaseFull.Split(' ')[0];
        context.Session.SetString("Release", latestRelease);

        var browserCookies = context.Request.Headers["Cookie"].ToString();
        context.Session.SetString("BrowserCookies", browserCookies);
    }

    await next.Invoke();
});

app.UseSystemWebAdapters();

#endregion

#region Routing

// Default MVC route
app.MapDefaultControllerRoute();

// Explicit routes
app.MapControllerRoute("Default", "{controller=Egrants}/{action=Index}/{id?}");
app.MapControllerRoute("Integration", "{controller=Integration}/{action=Trigger}/{id?}");

#endregion

app.Run();

// Exposes the top-level-statements Program class as public so the integration
// test project (eGrants.Tests) can reference it via WebApplicationFactory<Program>.
public partial class Program
{
}