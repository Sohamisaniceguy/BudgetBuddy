using BudgetBuddy.Models;
using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.ViewModels
{
    public class BudgetUser_Enterprise
    {
        [Key] // Add this attribute to specify a primary key
        public int BudgetUser_EnterpriseId { get; set; }



        public int BudgetId { get; set; }
        public Budget Budget { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
