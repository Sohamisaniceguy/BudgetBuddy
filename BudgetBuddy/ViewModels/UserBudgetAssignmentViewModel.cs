using System.ComponentModel.DataAnnotations;

namespace BudgetBuddy.ViewModels
{
    public class UserBudgetAssignmentViewModel
    {
        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        public int BudgetId { get; set; }

        public int UserId { get; set; }
    }
}
