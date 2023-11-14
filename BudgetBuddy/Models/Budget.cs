using BudgetBuddy.Utils;
using BudgetBuddy.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace BudgetBuddy.Models
{
    public class Budget
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BudgetId { get; set; } //Primary Key

        [Column(TypeName = "nvarchar(5)")]
        public string Icon { get; set; } = "";

        // Foreign Key
        //public int UserId { get; set; }
        //public User? User { get; set; }



        [Column(TypeName = "nvarchar(20)")]
        [Required(ErrorMessage = "Budget Name is required.")]
        public string BudgetName { get; set; }

        
        public decimal BudgetLimit { get; set; }

        [DateRange(ErrorMessage = "Start date must be earlier than the end date.")]
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;


        // New property for Enterprise (0 or 1)
        public int Enterprise { get; set; }



        [NotMapped]
        public string? BudgetWithIcon
        {
            get
            {
                return this.Icon + " " + this.BudgetName;
            }
        }

        
        public ICollection<User> Users { get; set; } = new List<User>();



    }
}
