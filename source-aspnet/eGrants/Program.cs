using eGrants.DAL;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;
using eGrants.Services;
using eGrants.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

using Serilog;

using SimpleECommerceCore.Middleware;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IeGrantsService, eGrantsService>();
builder.Services.AddScoped<IeGrantsRepository, eGrantsRepository>();
builder.Services.AddScoped<ICommonService, CommonService>();
builder.Services.AddScoped<ICommonRepository, CommonRepository>();
builder.Services.AddScoped<ISessionInfoService, SessionInfoService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IInstitutionalFilesService, InstitutionalFilesService>();
builder.Services.AddScoped<IInstitutionalFilesRepository, InstitutionalFilesRepository>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make session cookie HTTP-only
    options.Cookie.IsEssential = true; // Make session cookie essential
});

// Register DbContext with connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(@"C:\Logs\log-.txt", rollingInterval: RollingInterval.Day);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandling>();

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
    string userId = context.GetServerVariable("HEADER_SM_USER");

    if (string.IsNullOrEmpty(userId))
    {
        // Prefer authenticated Windows identity
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var fullName = context.User.Identity?.Name;
            userId = fullName?.Contains('\\') == true
                ? fullName.Split('\\')[1]
                : fullName;
        }
        else
        {
            // Fallback to machine account
            userId = Environment.UserName;
        }
    }

    context.Session.SetString("userid", userId);
    context.Session.SetString("Validation", "OK");
    context.Session.SetString("ic", "NCI");
    context.Session.SetString("Personid", "3941");
    context.Session.SetInt32("position_id", 8);
    context.Session.SetString("UserName", "Daryl Dehuff");
    context.Session.SetString("UserEmail", "daryl.dehuff@nih.gov");
    context.Session.SetString("Menus", ",Management|M,Admin|A,Dashboard|D");
    context.Session.SetString("browser", "Chrome");

    var frpprAcceptance = builder.Configuration["AppSettings:frpprAcceptance"] ?? string.Empty;
    var irpprAcceptance = builder.Configuration["AppSettings:irpprAcceptance"] ?? string.Empty;
    var ImageServerUrl = builder.Configuration["AppSettings:imageServerUrl"] ?? string.Empty;
    context.Session.SetString("frpprAcceptance", frpprAcceptance);
    context.Session.SetString("irpprAcceptance", irpprAcceptance);
    context.Session.SetString("ImageServerUrl", ImageServerUrl);

    // You can log or use the URL here
    await next.Invoke();
});

//app.Use(async (context, next) =>
//{
//    //context.Request.Headers["SM_USER"] = "localtestuser";
//    //await next();
//    if (context.Request.Headers.TryGetValue("SM_USER", out var smUser))
//    {
//        // Store in session
//        context.Session.SetString("SM_USER", smUser);

//        // Or attach to claims
//        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, smUser) }, "SiteMinder");
//        context.User = new ClaimsPrincipal(identity);
//    }

//    await next();
//});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();

app.MapControllerRoute("Default", "{controller=Egrants}/{action=Index}/{id?}");

app.MapControllerRoute("Integration", "{controller=Integration}/{action=Trigger}/{id?}");
//app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();