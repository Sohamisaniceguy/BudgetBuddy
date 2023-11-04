using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetBuddy.Models;
using BudgetBuddy.Data;
using BudgetBuddy.Service;
using BudgetBuddy.Controllers;

namespace Expense_Tracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly BudgetDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<BudgetController> _logger; // Injected loggerd

        public TransactionController(IUserService userService,BudgetDbContext context, ILogger<BudgetController> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger; // Initializing logger
        }


        // GET: Report
        [HttpGet]
        public ActionResult ViewReport()
        {
            // Redirect to the Report_Index.cshtml page
            return RedirectToAction("Index", "Report", new { area = "Reports" });
        }


        // GET: Transaction
        [HttpGet]
        public async Task<IActionResult> Index(int? budgetId) // Make budgetId nullable to handle cases where it's not provided
        {
            int userId = _userService.GetLoggedInUserId();



            // If a budgetId is provided as a parameter, use it and store it in the session
            if (budgetId.HasValue)
            {
                HttpContext.Session.SetInt32("BudgetId", budgetId.Value);
                ViewBag.BudgetId = budgetId.Value;
            }
            else
            {
                // If no budgetId is provided, try to get it from the session
                budgetId = HttpContext.Session.GetInt32("BudgetId");
                if (!budgetId.HasValue)
                {
                    // Handle the case where budgetId is not available
                    return RedirectToAction("SelectBudget"); // Redirect to a view where the user can select a budget
                }

                ViewBag.BudgetId = budgetId.Value;
            }

            // Retrieve transactions for the specified budget and user
            var transactions = await _context.Transactions
                .Where(t => t.BudgetId == budgetId && t.Budget.Users.Any(u => u.UserId == userId))
                .Include(t => t.Categories)
                .Include(t => t.User)
                .ToListAsync();

            // Fetch the budget's name
            var budget = await _context.Budgets.FindAsync(budgetId.Value);
            if (budget != null)
            {
                ViewBag.BudgetName = budget.BudgetName; // Assuming your Budget model has a "Name" property
            }

            // Retrieve the users associated with the specified budget
            var users = await _context.Budgets
                .Where(b => b.BudgetId == budgetId.Value)
                .Include(b => b.Users)
                .SelectMany(b => b.Users)
                .ToListAsync();

            ViewBag.Users = users;


            // Retrieve the Enterprise value for the specified budgetId
            var enterprise = await _context.Budgets
                .Where(b => b.BudgetId == budgetId)
                .Select(b => b.Enterprise)
                .FirstOrDefaultAsync();

            ViewBag.Enterprise = enterprise;

            // Set the mode in the session based on the enterprise value
            string mode = enterprise == 0 ? "Individual" : "Enterprise";
            HttpContext.Session.SetString("Mode", mode);

            return View(transactions);
        }



        // GET: Transaction/Trans_CreateorChange
        public IActionResult Trans_CreateorChange()
        {
            int? budgetId = HttpContext.Session.GetInt32("BudgetId");
            ViewBag.BudgetId = budgetId;
            PopulateCategories();
            return View(new Transaction());
        }


        [NonAction]
        private void PopulateCategories()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateorChange([Bind("TransactionId,CategoryId,Amount,Description,Date")] Transaction transaction)
        {
            _logger.LogInformation("Processing CreateorChange POST request.");

            // Retrieve budgetId from the session
            int? sessionBudgetId = HttpContext.Session.GetInt32("BudgetId");
            if (!sessionBudgetId.HasValue)
            {
                _logger.LogError("BudgetId not found in session.");
                ModelState.AddModelError("BudgetId", "Budget session not found.");
                PopulateCategories();
                return View("Trans_CreateorChange", transaction);
            }
            transaction.BudgetId = sessionBudgetId.Value;
            _logger.LogInformation($"BudgetId found : {sessionBudgetId.Value}");

            // Retrieve userId from the session or your user utility
            int? userId = UserUtility.GetUserId(HttpContext.Session);
            if (!userId.HasValue || userId.Value <= 0)
            {
                _logger.LogError("UserId not found in session or user utility.");
                ModelState.AddModelError("UserId", "User not found.");
                PopulateCategories();
                return View("Trans_CreateorChange", transaction);
            }
            transaction.UserId = userId.Value;
            _logger.LogInformation($"UserId found: {userId.Value}");


            _logger.LogInformation($"Looking up category with ID: {transaction.CategoryId}");
            var selectedCategory = await _context.Categories.FindAsync(transaction.CategoryId);
            if (selectedCategory == null)
            {
                _logger.LogError($"Category with ID: {transaction.CategoryId} not found in database.");
                ModelState.AddModelError("CategoryId", "Selected category not found in database.");
                PopulateCategories();
                return View("Trans_CreateorChange", transaction);
            }
            transaction.Categories = selectedCategory;

            if (ModelState.IsValid)
            {
                // Check if the User exists for the given UserId.
                var userExists = _context.User.Any(u => u.UserId == transaction.UserId);
                if (!userExists)
                {
                    ModelState.AddModelError("UserId", "User not found.");
                    PopulateCategories();
                    return View("Trans_CreateorChange", transaction);
                }

                // Check if the Budget exists for the given BudgetId.
                var budgetExists = _context.Budgets.Any(b => b.BudgetId == transaction.BudgetId);
                if (!budgetExists)
                {
                    ModelState.AddModelError("BudgetId", "Budget not found.");
                    PopulateCategories();
                    return View("Trans_CreateorChange", transaction);
                }

                // Determine if this is a create or update operation based on TransactionId
                if (transaction.TransactionId == 0)
                {
                    _logger.LogInformation("Creating a new transaction.");
                    _context.Transactions.Add(transaction);
                }
                else
                {
                    _logger.LogInformation($"Updating transaction with ID: {transaction.TransactionId}");
                    var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
                    if (existingTransaction != null)
                    {
                        _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
                    }
                    else
                    {
                        _logger.LogError($"Transaction with ID: {transaction.TransactionId} not found.");
                        ModelState.AddModelError("TransactionId", "Transaction not found.");
                        PopulateCategories();
                        return View("Trans_CreateorChange", transaction);
                    }
                }

                _logger.LogInformation($"BudgetId from session: {sessionBudgetId}");
                _logger.LogInformation($"UserId from UserUtility: {userId}");


                _logger.LogInformation("Saving changes to the database.");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully.");

                _logger.LogInformation($"Redirecting to Index with budgetId: {transaction.BudgetId}");
                return RedirectToAction(nameof(Index), new { budgetId = transaction.BudgetId });
            }

            
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError($"Validation Error: {error.ErrorMessage}");
                    }
                }

                PopulateCategories();
                return View("Trans_CreateorChange", transaction);
            
        }



        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        
    }
}
