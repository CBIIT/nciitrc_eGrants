using System;

using eGrants.Common;
using eGrants.DAL;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

using Serilog;

using SimpleECommerceCore.Middleware;

var builder = WebApplication.CreateBuilder(args);

#region Service Configuration

// System Web Adapters & HTTP utilities
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();
builder.Services.AddHttpContextAccessor();

// Application Services & Repositories (Dependency Injection)
builder.Services.AddScoped<IeGrantsService, eGrantsService>();
builder.Services.AddScoped<IeGrantsRepository, eGrantsRepository>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<ISessionInfoService, SessionInfoService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IInstitutionalFilesService, InstitutionalFilesService>();
builder.Services.AddScoped<IInstitutionalFilesRepository, InstitutionalFilesRepository>();
builder.Services.AddScoped<IManagementService, ManagementService>();

// Utility class
builder.Services.AddTransient<EgrantsCommon>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session configuration
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;                 // Make session cookie HTTP-only
    options.Cookie.IsEssential = true;              // Make session cookie essential
});

// Register DbContext with connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region Logging (Serilog)

// Original commented-out logging configs (kept for reference)
//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.File(
//        new CompactJsonFormatter(), // Structured format for Datadog
//        path: "/var/log/myapp/log.json", // Datadog agent will tail this
//        rollingInterval: RollingInterval.Day,
//        retainedFileCountLimit: 7,
//        fileSizeLimitBytes: 10_000_000,
//        rollOnFileSizeLimit: true,
//        shared: true)
//    .CreateLogger();

//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.File(
//        new CompactJsonFormatter(), // Structured format for Datadog
//        path: "Logs/log.json", // Datadog agent will tail this
//        rollingInterval: RollingInterval.Day,
//        retainedFileCountLimit: 7,
//        fileSizeLimitBytes: 10_000_000,
//        rollOnFileSizeLimit: true,
//        shared: true)
//    .CreateLogger();

//builder.Host.UseSerilog();

// Active Serilog configuration
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(@"C:\Logs\log-.txt", rollingInterval: RollingInterval.Day);
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

// Is this cookie really needed?    
//app.Run(async context =>
//{
//    context.Response.Cookies.Append("auditMode", "true", new CookieOptions
//    {
//        Expires = DateTimeOffset.UtcNow.AddDays(7), // Optional: set expiration
//        HttpOnly = true,                            // Optional: restrict access from client-side scripts
//        Secure = true                               // Optional: send only over HTTPS
//    });

//    await context.Response.WriteAsync("Cookie 'auditTest' has been set.");
//});

app.UseSession(); // Enable session middleware


// TODO: Determine better way to handle getting user id information if possible
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("userid")))
    {
        // Retrieve user ID from SiteMinder header or fallback to Windows identity
        string userId = context.GetServerVariable("HEADER_SM_USER");

        if (string.IsNullOrEmpty(userId))
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var fullName = context.User.Identity?.Name;
                userId = fullName?.Contains('\\') == true
                    ? fullName.Split('\\')[1]
                    : fullName;
            }
            else
            {
                userId = Environment.UserName; // Fallback to machine account
            }
        }

        context.Session.SetString("userid", userId);

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

        // Resolve EgrantsCommon service
        var egrantsCommon = app.Services.GetRequiredService<EgrantsCommon>();

        // Populate user session variables
        var usertype = egrantsCommon.UserType(context.Session.GetString("ic"), context.Session.GetString("userid"));
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

        // You can log or use the URL here
        // Load app settings into session
        context.Session.SetString("frpprAcceptance", builder.Configuration["AppSettings:frpprAcceptance"] ?? string.Empty);
        context.Session.SetString("irpprAcceptance", builder.Configuration["AppSettings:irpprAcceptance"] ?? string.Empty);
        context.Session.SetString("ImageServerUrl", builder.Configuration["AppSettings:imageServerUrl"] ?? string.Empty);
        context.Session.SetString("WebGrantUrl", builder.Configuration["AppSettings:webGrantUrl"] ?? string.Empty);
        context.Session.SetString("EgrantsDocEmail", builder.Configuration["AppSettings:egrantsDocEmail"] ?? string.Empty);
    }
    await next.Invoke();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
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
