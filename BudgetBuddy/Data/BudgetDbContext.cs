
using BudgetBuddy.Controllers;
using BudgetBuddy.Models;
using BudgetBuddy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualBasic;
using System.IO;
using Microsoft.Extensions.Configuration;


namespace BudgetBuddy.Data
{
    public class BudgetDbContext : DbContext
    {
        
            public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) { }

            public DbSet<Budget> Budgets { get; set; }
            public DbSet<Transaction> Transactions { get; set; }
            public DbSet<User> User { get; set; }
            public DbSet<Categories> Categories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Budgets)
                .WithMany(b => b.Users)
                .UsingEntity(j => j.ToTable("UserBudgets"));
        }



    }
}
