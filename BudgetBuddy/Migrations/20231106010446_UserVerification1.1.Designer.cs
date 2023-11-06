﻿// <auto-generated />
using System;
using BudgetBuddy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BudgetBuddy.Migrations
{
    [DbContext(typeof(BudgetDbContext))]
    [Migration("20231106010446_UserVerification1.1")]
    partial class UserVerification11
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

                    b.Property<int>("Enterprise")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("nvarchar(5)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.HasKey("BudgetId");

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

                    b.HasIndex("CategoryId");

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

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FailedLoginAttempts")
                        .HasColumnType("INTEGER");

                    b.Property<string>("First_Name")
                        .IsRequired()
                        .HasColumnType("nvchar(15)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLockedOut")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Last_Name")
                        .IsRequired()
                        .HasColumnType("nvchar(15)");

                    b.Property<DateTime?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvchar(20)");

                    b.Property<string>("ResetPasswordToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ResetPasswordTokenExpiration")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ResetPasswordTokenUsed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("nvchar(20)");

                    b.Property<DateTime?>("VerifyTokenExpiration")
                        .HasColumnType("TEXT");

                    b.Property<bool>("VerifyTokenUsed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VerifyUserToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("BudgetUser", b =>
                {
                    b.Property<int>("BudgetsBudgetId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UsersUserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BudgetsBudgetId", "UsersUserId");

                    b.HasIndex("UsersUserId");

                    b.ToTable("UserBudgets", (string)null);
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
                        .HasForeignKey("CategoryId")
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

            modelBuilder.Entity("BudgetUser", b =>
                {
                    b.HasOne("BudgetBuddy.Models.Budget", null)
                        .WithMany()
                        .HasForeignKey("BudgetsBudgetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BudgetBuddy.Models.User", null)
                        .WithMany()
                        .HasForeignKey("UsersUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
