using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetBuddy.Controllers
{
    public class EnterpriseController : Controller
    {

        private readonly BudgetDbContext _dbcontext;
        private readonly IUserService _userService;


        public EnterpriseController(IUserService userService, BudgetDbContext dbContext)
        {
            _dbcontext = dbContext;
            _userService = userService;
        }


        //Get: Enterprise
        public ActionResult Enterprise()
        {
            return View("Enterprise");
        }


        public IActionResult Budget_Index()
        {
            int userId = _userService.GetLoggedInUserId(); //Using User Service to retrive logged in user information.

            List<Budget> budgets = _dbcontext.Budgets
                .Where(b => b.Users.Any(u => u.UserId == userId) && b.Enterprise == 1)  //Only Displaying for budgets linked to the Loggedin user and where Enterprise mode is on
                .ToList();

            // Add any logic needed for Budget_Index action

            return View(budgets); // Return the appropriate view (Budget_Index.cshtml)
        }




    }
}
