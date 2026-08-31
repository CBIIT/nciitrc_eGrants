using eGrants.Common;
using eGrants.DAL;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
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

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session configuration
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make session cookie essential
});

// Microsoft Entra ID (OIDC) Authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Use the authorization code flow (back-channel token exchange) rather than the
// hybrid/implicit flow. This avoids AADSTS700054 ("response_type 'id_token' is not
// enabled for the application") by requesting "response_type=code" instead of an
// id_token at the authorize endpoint. The ID token is then returned via the token
// endpoint using the configured client secret.
builder.Services.Configure<OpenIdConnectOptions>(
    OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.ResponseType = OpenIdConnectResponseType.Code;
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

#region Middleware Pipeline

// Global exception handling middleware
app.UseMiddleware<ExceptionHandling>();

// Enforce HSTS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

#if !DEBUG
 // Handles unhandled exceptions (500 errors)
 app.UseExceptionHandler("/Error");

 // Handles HTTP status codes (404,403, etc.)
 app.UseStatusCodePagesWithReExecute("/Error/{0}");
#endif

app.UseSession(); // Enable session middleware

// Middleware to initialize and validate the user session.
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

    // ===================================================================================
    // TEST AUTH SEAM (integration smoke tests only)
    // ===================================================================================
    // When TestAuth:Enabled is true (set exclusively by the integration test host, never
    // in production configuration) we seed the session with a fully-validated fake user
    // and skip the SiteMinder / database / GitHub driven initialization below. This is the
    // equivalent of a "fake auth handler" for this session-based application and lets the
    // route smoke tests exercise every page without a live database or SiteMinder header.
    // ===================================================================================
    if (builder.Configuration.GetValue<bool>("TestAuth:Enabled"))
    {
        if (string.IsNullOrEmpty(context.Session.GetString("userid")))
        {
            var testUser = builder.Configuration.GetValue<string>("TestAuth:UserId") ?? "testuser";
            var testIc = builder.Configuration.GetValue<string>("TestAuth:Ic") ?? "NCI";

            context.Session.SetString("userid", testUser);
            context.Session.SetString("ic", testIc);
            context.Session.SetString("Validation", "OK");
            context.Session.SetString("UserName", "Test User");
            context.Session.SetString("UserEmail", "testuser@example.com");
            context.Session.SetString("Menus", string.Empty);
            context.Session.SetString("browser", "Chrome");
            context.Session.SetString("CurrentView", "standardForm");
            context.Session.SetInt32("Personid", 0);
            context.Session.SetInt32("position_id", 0);
            context.Session.SetInt32("dashboard", 0);
        }

        await next.Invoke();
        return;
    }

    if (string.IsNullOrEmpty(context.Session.GetString("userid")))
    {
        var bypassEnabled = builder.Configuration.GetValue<bool>("SiteMinderBypass:Enabled");
        var allowedUser = builder.Configuration.GetValue<string>("SiteMinderBypass:AllowedUser") ?? string.Empty;

        string userId = string.Empty;

        if (bypassEnabled)
        {
            // ===================================================================================
            // BYPASS MODE: Use the configured AllowedUser
            // ===================================================================================
            // When bypass is enabled, use the single configured user.
            // This is intended for development/testing environments only.
            // ===================================================================================
            if (string.IsNullOrEmpty(allowedUser))
            {
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger?.LogError("SiteMinder bypass is enabled but AllowedUser is not configured.");
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Access denied: AllowedUser not configured for bypass mode.");
                return;
            }

            userId = allowedUser;
            context.Session.SetString("SiteMinderBypassed", "true");

            var logger2 = context.RequestServices.GetService<ILogger<Program>>();
            logger2?.LogWarning("SiteMinder bypass active. Using configured user: {UserId}", userId);
        }
        else
        {
            // ===================================================================================
            // NORMAL MODE: Use SiteMinder authentication
            // ===================================================================================
            // When running locally in Development and SiteMinder is not available,
            // fall back to the Windows username from the environment.
            // ===================================================================================
            string siteMinderUser = context.GetServerVariable("HEADER_SM_USER");

            if (!string.IsNullOrEmpty(siteMinderUser))
            {
                userId = siteMinderUser;
            }
            else if (app.Environment.IsDevelopment())
            {
                // Local development fallback: use the Windows username
                userId = Environment.UserName;
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger?.LogWarning("SiteMinder header not found. Using local Windows username: {UserId}", userId);
            }
            else
            {
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger?.LogWarning("No user identity found. SiteMinder header missing or empty.");
                context.Response.Redirect("/egrants_default.htm");
                return;
            }
        }

        context.Session.SetString("userid", userId);

        // Capture IC (Institute/Org Code)
        var ic = context.GetServerVariable("HEADER_USER_SUB_ORG") ?? "NCI";
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
        context.Session.SetString("ImageServerUrl", builder.Configuration["AppSettings:imageServerUrl"] ?? string.Empty);
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
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

// Exposes the implicit Program class to the integration test project so it can be used
// as the entry point for WebApplicationFactory<Program>.
public partial class Program { }