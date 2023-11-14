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


        
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        

        

        services.AddMvc();

        // Add framework services
        services.AddControllersWithViews();

        //services.AddSingleton<ITicketStore, InMemoryTicketStore>();

        // Configure cookie authentication
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    //options.SessionStore = new InMemoryTicketStore();
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Home/Index";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = System.TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;


                    
                });

        //services.AddLogging();


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
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });






        // Register application services.
        services.AddScoped<IUserService, UserService>(); 
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

        //app.UseSerilogRequestLogging(); // Log requests
        app.UseStaticFiles();

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
