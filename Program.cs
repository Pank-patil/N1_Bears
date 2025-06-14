using Unique.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
// Configure Kestrel to listen on port 81

builder.Services.AddControllersWithViews();
builder.Services.AddSession(); // ? Add session
builder.Services.AddDistributedMemoryCache(); // ? Required for session

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("WhitelistLimiter", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2000,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0 
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
    };
});


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add EF Core DbContext with SQL Server connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSession(); // ? Add this BEFORE UseRouting()

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Redirects to HomeController's Error action
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
