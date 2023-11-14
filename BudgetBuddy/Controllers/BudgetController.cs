using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BudgetBuddy.Controllers
{
    public class BudgetController : Controller
    {
        // DB Connection
        private readonly BudgetDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly ILogger<BudgetController> _logger; // Injected logger

        public BudgetController(IUserService userService, BudgetDbContext dbContext, ILogger<BudgetController> logger)
        {
            _dbContext = dbContext;
            _userService = userService;
            _logger = logger; // Initializing logger
        }

        public IActionResult Budget_Index()
        {

            HttpContext.Session.SetString("Mode", "Individual");
            var userId = HttpContext.Session.GetInt32("UserID"); //Using User Service to retrive logged in user information.



            List<Budget> budgets = _dbContext.Budgets
                .Where(b => b.Users.Any(u => u.UserId == userId) && b.Enterprise == 0)  //Only Displaying for Loggedin user and Enterprise mode off
                .ToList();
            // Add any logic needed for Budget_Index action

            // Set the ViewData for Individual mode button
            ViewData["PageActionUrl"] = Url.Action("YourActionForIndividual", "Budget");
            ViewData["PageActionText"] = "Add Budget - Individual";

            return View(budgets); // Return the appropriate view (Budget_Index.cshtml)
        }

        public IActionResult Budget_Index_Enterprise()
        {
            HttpContext.Session.SetString("Mode", "Enterprise");
            var userId = HttpContext.Session.GetInt32("UserID"); //Using User Service to retrive logged in user information.

            List<Budget> Enterprisebudgets = _dbContext.Budgets
                .Where(b => b.Users.Any(u => u.UserId == userId) && b.Enterprise == 1)  //Only Displaying for Loggedin user and Enterprise mode off
                .ToList();
            // Add any logic needed for Budget_Index action

            return View("Budget_Index", Enterprisebudgets); // Return the appropriate view (Budget_Index.cshtml)
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
        [HttpGet]
        public IActionResult Bud_CreateorChange()
        {

            _logger.LogWarning("Got Budget Createpage.");

            return View(new Budget());
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBudget(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid for CreateBudget.");
                var userId = HttpContext.Session.GetInt32("UserID");

                if (userId.HasValue)
                {
                    var user = _dbContext.User.Find(userId.Value);
                    if (user != null)
                    {
                        _logger.LogInformation("User found in database for CreateBudget.");
                        budget.Users.Add(user);
                        budget.Enterprise = 0;
                        _dbContext.Budgets.Add(budget);
                        _dbContext.SaveChanges();
                        return RedirectToAction("Budget_Index", "Budget");
                    }
                    else
                    {
                        _logger.LogError("User object not found in DB for CreateBudget.");
                        TempData["ErrorMessage"] = "User object not found in the database.";
                        return RedirectToAction("Error");
                    }
                }
                else
                {
                    _logger.LogError("UserId not found in session for CreateBudget.");
                    TempData["ErrorMessage"] = "User ID not found in session.";
                    return RedirectToAction("Error");
                }
            }

            // DEBUG
            foreach (var entry in ModelState)
            {
                if (entry.Value.Errors.Any())
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Property: {entry.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }
            // END DEBUG

            _logger.LogWarning("Model state is invalid for CreateBudget.");

            // Store model state errors in TempData
            TempData["ModelStateErrors"] = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                .ToList();

            return View("Bud_CreateorChange", budget);
        }



        // ENTERPRISE MODE:
        //Get: Create_ent
        [HttpGet]
        public IActionResult Bud_CreateorChange_Ent()
        {
            // Retrieve the mode from session
            string mode = HttpContext.Session.GetString("Mode");

            // Check if the mode is set to "Individual"
            if (mode == "Individual")
            {
                // If it is, redirect the user to the Bud_CreateorChange action
                return RedirectToAction("Bud_CreateorChange");
            }

            // Otherwise, continue with the enterprise budget creation
            return View(new Budget());

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBudget_Ent(Budget budget)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid for CreateBudget_Ent.");
                var userId = HttpContext.Session.GetInt32("UserID");

                if (userId.HasValue)
                {
                    var user = _dbContext.User.Find(userId.Value);
                    if (user != null)
                    {
                        _logger.LogInformation("User found in database for CreateBudget_Ent.");
                        budget.Users.Add(user);
                        budget.Enterprise = 1;
                        _dbContext.Budgets.Add(budget);
                        _dbContext.SaveChanges();
                        return RedirectToAction("Budget_Index_Enterprise", "Budget");
                    }
                    else
                    {
                        _logger.LogError("User object not found in DB for CreateBudget_Ent.");
                        TempData["ErrorMessage"] = "User object not found in the database.";
                        return RedirectToAction("Error");
                    }
                }
                else
                {
                    _logger.LogError("UserId not found in session for CreateBudget_Ent.");
                    TempData["ErrorMessage"] = "User ID not found in session.";
                    return RedirectToAction("Error");
                }
            }

            // If model state is invalid or any other error occurs, return the creation view with the budget data
            _logger.LogWarning("Model state is invalid or error occurred in CreateBudget_Ent.");
            // Store model state errors in TempData
            TempData["ModelStateErrors"] = ModelState.Values
                .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                .ToList();
            return View("Bud_CreateorChange", budget);
        }



        [HttpGet]
        public IActionResult AddUser()
        {
            return View(new UserBudgetAssignmentViewModel());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(UserBudgetAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Processing user addition with provided model.");
                var existingUser = _dbContext.User.FirstOrDefault(u => u.Email == model.Email && u.First_Name == model.FirstName && u.Last_Name == model.LastName);

                if (existingUser != null)
                {
                    int? sessionBudgetId = HttpContext.Session.GetInt32("BudgetId");

                    if (sessionBudgetId.HasValue)
                    {
                        var budget = _dbContext.Budgets.Include(b => b.Users).FirstOrDefault(b => b.BudgetId == sessionBudgetId.Value);

                        if (budget != null)
                        {
                            budget.Users.Add(existingUser);
                            _dbContext.SaveChanges();
                            _logger.LogInformation($"User {existingUser.Email} added to budget {budget.BudgetId} successfully.");
                            TempData["SuccessMessage"] = "User added to the budget successfully!";
                            return RedirectToAction("Index", "Transaction");
                        }
                        else
                        {
                            _logger.LogWarning($"Budget with ID {sessionBudgetId.Value} not found.");
                            TempData["ErrorMessage"] = "Budget not found!";
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Attempt to add a user without a selected budget.");
                        TempData["ErrorMessage"] = "No budget selected!";
                    }
                }
                else
                {
                    _logger.LogWarning($"User not found with email {model.Email}.");
                    TempData["ErrorMessage"] = "User not found!";
                }
            }
            else
            {
                _logger.LogWarning("Attempt to add a user with invalid form data.");
                TempData["ErrorMessage"] = "Form data is not valid!";
            }

            return View(model);
        }






            // New action to handle redirect to Transactions_Index --> Budget to Transaction(Detail button)
            public IActionResult RedirectToTransactions(int budgetId)
        {
            
            return RedirectToAction("Index", "Transaction", new { budgetId });
        }

        

    }


}
