using BudgetBuddy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using BudgetBuddy.Service;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console() // Logs to the console
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Logs to a file with daily rolling
    .CreateLogger();

builder.Host.UseSerilog(); // Tell the host to use Serilog for logging



// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext and configure session
builder.Services.AddDbContext<BudgetDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BudgetDbConnection")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "YourAppSessionCookie";
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your desired session timeout
    options.Cookie.IsEssential = true;
});

// Register IUserService
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.UseSession(); // Add this to enable sessions

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
