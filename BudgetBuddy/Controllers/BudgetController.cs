using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetBuddy.Controllers
{
    public class BudgetController:Controller
    {
        // DB Connection
        private readonly BudgetDbContext _dbContext;
        private readonly IUserService _userService;
        

        public BudgetController(IUserService userService, BudgetDbContext dbContext)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        public IActionResult Budget_Index()
        {
            int userId = _userService.GetLoggedInUserId(); //Using User Service to retrive logged in user information.

            List<Budget> budgets = _dbContext.Budgets
                .Where(b => b.UserId == userId)
                .ToList();
            // Add any logic needed for Budget_Index action

            return View(budgets); // Return the appropriate view (Budget_Index.cshtml)
        }



        [HttpPost]
        public IActionResult DeleteBudget(int budgetId)
        {
            // Retrieve the budget by ID
            var budget = _dbContext.Budgets.SingleOrDefault(b => b.BudgetId == budgetId);

            if (budget == null)
            {
                // Budget not found
                return NotFound();
            }

            // Remove the budget from the database
            _dbContext.Budgets.Remove(budget);
            _dbContext.SaveChanges();

            return RedirectToAction("Budget_Index");
        }




        //Get: Create
        public IActionResult Bud_CreateorChange()
        {
            
                return View(new Budget());
            
        }



        [HttpPost]
        public IActionResult CreateBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the UserId from the session
                int? userId = UserUtility.GetUserId(HttpContext.Session);

                // Check if UserId is available in the session
                if (userId.HasValue)
                {
                    // Assign the UserId to the Budget
                    budget.UserId = userId.Value;

                    
                    _dbContext.Budgets.Add(budget);
                    _dbContext.SaveChanges();
                    return RedirectToAction("Budget_Index", "Budget"); // Redirect to desired page after successful creation
                }
                else
                {
                    
                    return RedirectToAction("Error"); 
                }
            }
            return View("Bud_CreateorChange", budget); 

        }


            // New action to handle redirect to Transactions_Index --> Budget to Transaction(Detail button)
            public IActionResult RedirectToTransactions(int budgetId)
        {
            // You may perform any necessary processing before redirecting
            return RedirectToAction("Index", "Transaction", new { budgetId });
        }

        

    }


}
