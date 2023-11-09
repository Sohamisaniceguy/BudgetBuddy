using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using BudgetBuddy.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebGoatCore.Utils;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;


        // Initialize Serilog configuration
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(Configuration) // This reads the Serilog settings from appsettings.json (if you have them)
            .WriteTo.Console() // Optional: if you want to log to the console as well
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Logs will be stored in a "logs" folder with daily rolling
            .CreateLogger();
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {

        
        // Add framework services
        services.AddControllersWithViews();

        // Configure cookie authentication
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = System.TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

        services.AddLogging();


        // Configure DbContext
        services.AddDbContext<BudgetDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Add Identity registers the services
        services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<BudgetDbContext>()
        .AddDefaultTokenProviders();


        // Configure sessions
        services.AddSession(options =>
        {
            options.IdleTimeout = System.TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });






        // Register application services.
        services.AddScoped<IUserService, UserService>(); // Ensure this matches your actual implementation
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher<IdentityUser>, Argon2Hasher<IdentityUser>>();










    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseSerilogRequestLogging(); // Log requests
          
        app.UseHttpsRedirection();
        app.UseStaticFiles();

       
        app.UseRouting();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
