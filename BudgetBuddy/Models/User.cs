using BudgetBuddy.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace BudgetBuddy.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // Primary Key

        [Column(TypeName = "nvchar(20)")]
        public string UserName { get; set; }

        [Column(TypeName = "nvchar(20)")]
        
        //public string? Password { get; set; }
        public string PasswordHash { get; set; }

        [Column(TypeName = "nvchar(20)")]
        public string Email { get; set; }

        [Column(TypeName = "nvchar(15)")]
        public string First_Name { get; set; }

        [Column(TypeName = "nvchar(15)")]
        public string Last_Name { get; set;}


        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEnd { get; set; }

       

        // New fields for password reset functionality
        public string ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiration { get; set; }

        public bool ResetPasswordTokenUsed { get; set; }

        // Property to track whether the user's account is locked
        public bool IsLockedOut { get; set; }

        // Property to track whether the user has verified their email address
        public bool EmailConfirmed { get; set; }

        // New fields for user verifcation functionality
        public string VerifyUserToken { get; set; }
        public DateTime? VerifyTokenExpiration { get; set; }

        public bool VerifyTokenUsed { get; set; }

        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();


    }

}
