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
        public string Password { get; set; }

        [Column(TypeName = "nvchar(20)")]
        public string Email { get; set; }

        [Column(TypeName = "nvchar(15)")]
        public string First_Name { get; set; }

        [Column(TypeName = "nvchar(15)")]
        public string Last_Name { get; set;}

        // Navigation property for the many-to-many relationship
        public ICollection<BudgetUser_Enterprise> BudgetUsers { get; set; }
    }

}
