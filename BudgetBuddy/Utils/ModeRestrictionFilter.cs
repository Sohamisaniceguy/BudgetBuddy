using BudgetBuddy.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;

namespace BudgetBuddy.Utils
{
    public class ModeRestrictionFilter : IAuthorizationFilter
    {
        private readonly ILogger _logger;
        private readonly BudgetDbContext _context; // Replace with your actual DbContext

        public ModeRestrictionFilter(ILogger<ModeRestrictionFilter> logger, BudgetDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var userMode = httpContext.Session.GetString("UserMode");
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier); // Or however you get the user ID
            var budgetId = (int?)context.RouteData.Values["budgetId"];

            if (budgetId.HasValue)
            {
                var budget = _context.Budgets.Include(b => b.Users).FirstOrDefault(b => b.BudgetId == budgetId.Value);

                // If the budget does not exist or the user doesn't have access to it, return a Forbidden result
                if (budget == null || !budget.Users.Any(u => u.UserId.ToString() == userId))
                {
                    context.Result = new ForbidResult();
                    return;
                }

                // Check if the user's mode matches the budget type
                bool isBudgetEnterprise = budget.Enterprise == 1;
                bool isUserAllowed = (userMode == "Individual" && !isBudgetEnterprise) || (userMode == "Enterprise" && isBudgetEnterprise);

                if (!isUserAllowed)
                {
                    // Log the unauthorized attempt and redirect to an error page or return a 403 Forbidden response
                    context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                }
            }
        }
    }
}
