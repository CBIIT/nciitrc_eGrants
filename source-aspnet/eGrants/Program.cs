using eGrants.Common;
using eGrants.DAL;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

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
// ====================================================================================;

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.UseSession(); // Enable session middleware

// TODO: Determine better way to handle getting user id information if possible

// Middleware to initialize and validate the user session.
//
// - Strips server-identifying response headers.
// - If no session user exists:
//      • Resolve user ID from SiteMinder, Windows identity, or machine account.
//    • Store IC code, browser type, and default view.
//      • Load user type and profile via EgrantsCommon; redirect if invalid.
//      • Populate session with user details and app configuration values.
//      • Fetch latest GitHub release tag and store cookies.
// - Continues request pipeline afterward.
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

    if (string.IsNullOrEmpty(context.Session.GetString("userid")))
    {
        // ===================================================================================
        // SITEMINDER BYPASS CONFIGURATION
        // ===================================================================================
        // When enabled, completely bypasses SiteMinder authentication.
        // The SiteMinder header will NOT be checked at all when bypass is enabled.
        // Configure in appsettings.json:
        //   "SiteMinderBypass": {
        //       "Enabled": true,
        //       "AllowedUsers": ["user1", "user2"]
        //   }
        // 
        // IMPORTANT: This should only be enabled in development/test environments.
        // In production, set "Enabled": false to require SiteMinder authentication.
        //
        // NOTE: When bypass is enabled, ONLY users in the AllowedUsers list can access
        // the application. Environment.UserName is NOT used as it returns the app pool
        // identity, not the actual user.
        // ===================================================================================

        var bypassEnabled = builder.Configuration.GetValue<bool>("SiteMinderBypass:Enabled");
        var allowedUsers = builder.Configuration.GetSection("SiteMinderBypass:AllowedUsers").Get<string[]>() ?? Array.Empty<string>();

        string userId = string.Empty;
        bool bypassUsed = false;

        if (bypassEnabled)
        {
            // ===================================================================================
            // BYPASS MODE: Skip SiteMinder entirely
            // ===================================================================================
            // When bypass is enabled, we do NOT check the SiteMinder header at all.
            // 
            // User identification methods (in order of priority):
            // 1. X-Dev-User HTTP header (for API tools like Postman)
            // 2. dev_user query parameter (for browser testing: ?dev_user=username)
            // 3. First user in AllowedUsers list (default fallback)
            //
            // The identified user must be in the AllowedUsers list to gain access.
            // ===================================================================================

            if (allowedUsers.Length == 0)
            {
                // No allowed users configured - deny access
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger?.LogError("SiteMinder bypass is enabled but no AllowedUsers are configured. Access denied.");
                context.Response.Redirect("/egrants_default.htm");
                return;
            }

            string? requestedUser = null;

            // Method 1: Check X-Dev-User header (useful for API testing tools)
            if (context.Request.Headers.TryGetValue("X-Dev-User", out var headerUser) && !string.IsNullOrWhiteSpace(headerUser))
            {
                requestedUser = headerUser.ToString().Trim();
            } 
            // Method 2: Check dev_user query parameter (useful for browser testing)
            else if (context.Request.Query.TryGetValue("dev_user", out var queryUser) && !string.IsNullOrWhiteSpace(queryUser))
            {
                requestedUser = queryUser.ToString().Trim();
                
                // Store in session so subsequent requests don't need the query param
                context.Session.SetString("dev_user_override", requestedUser);
            }
             // Method 3: Check if we previously stored a dev_user in session
         else if (!string.IsNullOrEmpty(context.Session.GetString("dev_user_override")))
       {
   requestedUser = context.Session.GetString("dev_user_override");
   }

    // Validate and match the user against AllowedUsers
            if (!string.IsNullOrEmpty(requestedUser))
     {
        var matchedUser = allowedUsers.FirstOrDefault(u =>
        string.Equals(u, requestedUser, StringComparison.OrdinalIgnoreCase));

   if (matchedUser != null)
       {
    userId = matchedUser;
           var loggerMatch = context.RequestServices.GetService<ILogger<Program>>();
         loggerMatch?.LogInformation("SiteMinder bypass: User '{UserId}' authenticated via header/query parameter", userId);
          }
                else
                {
   // Requested user not in allowed list - deny access
  var logger = context.RequestServices.GetService<ILogger<Program>>();
    logger?.LogWarning("SiteMinder bypass: Requested user '{RequestedUser}' is not in AllowedUsers list. Access denied.", requestedUser);
     context.Response.Redirect("/egrants_default.htm");
  return;
            }
  }
      else
  {
      // No user specified - use first user in the list as default
          userId = allowedUsers[0];
     var loggerDefault = context.RequestServices.GetService<ILogger<Program>>();
      loggerDefault?.LogInformation("SiteMinder bypass: No user specified, using default: {UserId}", userId);
            }

            bypassUsed = true;
        }
     else
        {
            // ===================================================================================
            // NORMAL MODE: Use SiteMinder authentication
            // ===================================================================================
            // When bypass is NOT enabled, check the SiteMinder header for the user.
            // ===================================================================================

            string siteMinderUser = context.GetServerVariable("HEADER_SM_USER");

            if (!string.IsNullOrEmpty(siteMinderUser))
            {
                userId = siteMinderUser;
            }
        }

        // If still no userId (SiteMinder mode but no header), deny access
        if (string.IsNullOrEmpty(userId))
        {
            var logger = context.RequestServices.GetService<ILogger<Program>>();
            logger?.LogWarning("No user identity found. SiteMinder header missing or empty.");
            context.Response.Redirect("/egrants_default.htm");
            return;
        }

        context.Session.SetString("userid", userId);

        // Store bypass flag in session for potential audit/display purposes
        if (bypassUsed)
        {
            context.Session.SetString("SiteMinderBypassed", "true");
        }

        // Capture IC (Institute/Org Code)
        var ic = context.GetServerVariable("HEADER_USER_SUB_ORG") ?? "NCI";
        context.Session.SetString("ic", ic);

        // Detect browser from User-Agent
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        string browserName = userAgent.Contains("Chrome") ? "Chrome" :
 userAgent.Contains("Firefox") ? "Firefox" :
 (userAgent.Contains("Safari") && !userAgent.Contains("Chrome")) ? "Safari" :
 userAgent.Contains("Edg") ? "Edge" :
 (userAgent.Contains("MSIE") || userAgent.Contains("Trident")) ? "Internet Explorer" :
 "Unknown";

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

        if (context.Session.GetString("Validation").ToString() != "OK")
        {
            context.Response.Redirect("/egrants_default.htm");
            return;
        }

        // You can log or use the URL here
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
        string token = context.Session.GetString("GitHubToken").ToString();
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

//app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

#endregion

app.Run();
