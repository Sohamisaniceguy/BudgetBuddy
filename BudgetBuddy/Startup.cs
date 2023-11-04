using BudgetBuddy.Data;
using BudgetBuddy.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;


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

        services.AddSession(options =>
        {
            // Set a short timeout for easy testing.
            options.IdleTimeout = TimeSpan.FromHours(100);
            options.Cookie.HttpOnly = true;
            // Make the session cookie essential
            options.Cookie.IsEssential = true;
        });

        // Add services to the container.
        services.AddControllersWithViews();

        // Add DbContext and configure session
        services.AddDbContext<BudgetDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("BudgetDbConnection")));

        services.AddSession(options =>
        {
            options.Cookie.Name = "YourAppSessionCookie";
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your desired session timeout
            options.Cookie.IsEssential = true;
        });

        //Client side validation:
        services.AddControllersWithViews().AddViewOptions(options =>
        {
            options.HtmlHelperOptions.ClientValidationEnabled = true;
        });


        // Register IUserService and HttpContextAccessor
        services.AddScoped<IUserService, UserService>();
        services.AddHttpContextAccessor();

       




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

        app.UseSession(); 

        // Use Serilog request logging middleware
        app.UseSerilogRequestLogging();

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthorization();

        app.UseSession(); //  enable sessions

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
