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

namespace Expense_Tracker.Controllers
{
    public class TransactionController : Controller
    {
        private readonly BudgetDbContext _context;
        private readonly IUserService _userService;

        public TransactionController(IUserService userService,BudgetDbContext context)
        {
            _context = context;
            _userService = userService;
        }


        // GET: Report
        public ActionResult ViewReport()
        {
            // Redirect to the Report_Index.cshtml page
            return RedirectToAction("Index", "Report", new { area = "Reports" });
        }


        // GET: Transaction
        public async Task<IActionResult> Index(int budgetId)
        {
            int userId = _userService.GetLoggedInUserId();
            ViewBag.BudgetId = budgetId;
            

            // Retrieve transactions for the specified budget and user
            var transactions = _context.Transactions
                .Where(t => t.BudgetId == budgetId && t.Budget.UserId == userId)
                .Include(t => t.Categories)
                .ToList();

            // Retrieve the Enterprise value for the specified budgetId
            var enterprise = _context.Budgets
                .Where(b => b.BudgetId == budgetId)
                .Select(b => b.Enterprise)
                .FirstOrDefault();

            ViewBag.Enterprise = enterprise;

            return View(transactions);
        }

        // GET: Transaction/Trans_CreateorChange
        public IActionResult Trans_CreateorChange(int budgetId)
        {
            ViewBag.BudgetId = budgetId;
           
            PopulateCategories();
            PopulateBudgets();
            return View(new Transaction());
        }


        [NonAction]
        private void PopulateCategories()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = categories;
        }

        [NonAction]
        private void PopulateBudgets()
        {
            var budgets = _context.Budgets.ToList();
            ViewBag.Budgets = budgets;
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateorChange([Bind("TransactionId,CategoryId,BudgetId,UserId,Amount,Description,Date")] Transaction transaction, int ddlCategory)
        {
            if (ModelState.IsValid)
            {
                // Assuming you can get the UserId from the logged-in user
                int? userId = UserUtility.GetUserId(HttpContext.Session);
                if (userId == null)
                {
                    ModelState.AddModelError("UserId", "User not found.");
                    PopulateCategories();
                    PopulateBudgets();
                    return View("Trans_CreateorChange", transaction);
                }

                // Set the UserId 
                transaction.UserId = (int)userId;
                transaction.CategoryId = ddlCategory;  // Set the CategoryId from the dropdown

                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Transaction", new { budgetId = transaction.BudgetId });
            }

            PopulateCategories();
            PopulateBudgets();
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
