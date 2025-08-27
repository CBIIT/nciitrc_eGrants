
using Microsoft.EntityFrameworkCore;
using eGrants.DAL;
using eGrants.Services.Interfaces;
using eGrants.Services;
using eGrants.Repositories;
using eGrants.Repositories.Interfaces;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSystemWebAdapters();
builder.Services.AddHttpForwarder();

builder.Services.AddScoped<IeGrantsService, eGrantsService>();
builder.Services.AddScoped<IeGrantsRepository, eGrantsRepository>();

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Enable session middleware
app.UseAuthorization();
app.UseSystemWebAdapters();

app.MapDefaultControllerRoute();

app.MapControllerRoute("Default", "{controller=Egrants}/{action=Index}/{id?}");

app.MapControllerRoute("Integration", "{controller=Integration}/{action=Trigger}/{id?}");
//app.MapForwarder("/{**catch-all}", app.Configuration["ProxyTo"]).Add(static builder => ((RouteEndpointBuilder)builder).Order = int.MaxValue);

app.Run();
