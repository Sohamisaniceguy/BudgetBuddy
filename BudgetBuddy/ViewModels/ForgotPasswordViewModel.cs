using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.ViewModels
{
    public class ForgotPasswordViewModel
    {
        

        [Required]
        [EmailAddress]
        public string Email { get; set; }

      
    }

}
