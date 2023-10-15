﻿using BudgetBuddy.Data;
using BudgetBuddy.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
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

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthorization();

        app.UseSession(); // Add this to enable sessions

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
