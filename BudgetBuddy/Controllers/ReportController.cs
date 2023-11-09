using BudgetBuddy.Data;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BudgetBuddy.Controllers
{
    public class ReportController:Controller
    {
        private readonly BudgetDbContext _dbcontext;
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger, BudgetDbContext dbcontext)      //Constructor 
        {
            _dbcontext = dbcontext;
            _logger = logger;
        }

        //GET: Report_Index
        public ActionResult Index()
        {
            try
            {
                _logger.LogInformation("Fetching transactions from the database.");
                var transactions = _dbcontext.Transactions.ToList();

                _logger.LogInformation($"Fetched {transactions.Count} transactions. Processing data for the report.");
                var reportData = transactions
                    .GroupBy(t => t.Date.Date)
                    .Select(group => new ReportViewModel
                    {
                        Date = group.Key.ToString("yyyy-MM-dd"),
                        TotalAmount = group.Sum(t => t.Amount)
                    })
                    .OrderBy(result => result.Date)
                    .ToList();

                _logger.LogInformation($"Report data processed. Number of grouped entries: {reportData.Count}");

                if (!reportData.Any())
                {
                    _logger.LogWarning("No data available to display in the report.");
                }

                return View("Report_Index", reportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the report.");
                return View("Error"); // Assuming you have an Error view to display errors
            }
        }









        // Action for displaying the transaction overview report
        public IActionResult TransactionOverview()
        {
            // Implementation for displaying the transaction overview report
            // Fetch the necessary data for the report
            // Pass the data to the view and display the report

            return View();  // Return the transaction overview report view
        }

        // Action for displaying the transaction history report
        public IActionResult TransactionHistory()
        {
            // Implementation for displaying the transaction history report
            // Fetch the necessary data for the report
            // Pass the data to the view and display the report

            return View();  // Return the transaction history report view
        }

        // Action for displaying the financial reports
        public IActionResult FinancialReports()
        {
            // Implementation for displaying the financial reports
            // Fetch the necessary data for the reports
            // Pass the data to the view and display the reports

            return View();  // Return the financial reports view
        }
    }
}
