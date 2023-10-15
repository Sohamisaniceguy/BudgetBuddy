using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace BudgetBuddy.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; } // Primary Key

        // Foreign Key
        public int BudgetId { get; set; }
        public Budget? Budget { get; set; }


        // Foreign Key
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public Categories Categories { get; set; }

        // Foreign Key
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int UserId { get; set; }
        public User? User { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than 0.")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;


        [Column(TypeName = "nvchar(75)")]
        public string? Description { get; set; }

        [NotMapped]
        public string? FormattedAmount
        {
            get
            {
                return ((Categories == null || Categories.Type == "Expense") ? "- " : "+ ") + Amount.ToString("C0");
            }
        }

        [NotMapped]
        public string? CategoryTitleWithIcon
        {
            get
            {
                return Categories == null ? "" : Categories.Icon + " " + Categories.Title;
            }
        }
    }
}
