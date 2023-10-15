﻿// <auto-generated />
using System;
using BudgetBuddy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BudgetBuddy.Migrations
{
    [DbContext(typeof(BudgetDbContext))]
    partial class BudgetDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.12");

            modelBuilder.Entity("BudgetBuddy.Models.Budget", b =>
                {
                    b.Property<int>("BudgetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("BudgetLimit")
                        .HasColumnType("TEXT");

                    b.Property<string>("BudgetName")
                        .IsRequired()
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("nvarchar(5)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BudgetId");

                    b.HasIndex("UserId");

                    b.ToTable("Budgets");
                });

            modelBuilder.Entity("BudgetBuddy.Models.Categories", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(10)");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("BudgetBuddy.Models.Transaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT");

                    b.Property<int>("BudgetId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoriesCategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("nvchar(75)");

                    b.Property<int>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("TransactionId");

                    b.HasIndex("BudgetId");

                    b.HasIndex("CategoriesCategoryId");

                    b.HasIndex("UserId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("BudgetBuddy.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvchar(20)");

                    b.Property<string>("First_Name")
                        .IsRequired()
                        .HasColumnType("nvchar(15)");

                    b.Property<string>("Last_Name")
                        .IsRequired()
                        .HasColumnType("nvchar(15)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvchar(20)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvchar(20)");

                    b.HasKey("UserId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("BudgetBuddy.Models.Budget", b =>
                {
                    b.HasOne("BudgetBuddy.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BudgetBuddy.Models.Transaction", b =>
                {
                    b.HasOne("BudgetBuddy.Models.Budget", "Budget")
                        .WithMany()
                        .HasForeignKey("BudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BudgetBuddy.Models.Categories", "Categories")
                        .WithMany()
                        .HasForeignKey("CategoriesCategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BudgetBuddy.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Budget");

                    b.Navigation("Categories");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
