using BudgetBuddy.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BudgetBuddy.Controllers
{
    public class ReportController:Controller
    {
        private readonly BudgetDbContext _dbcontext;

        public ReportController(BudgetDbContext dbcontext)      //Constructor 
        {
            _dbcontext = dbcontext;
        }

        //GET: Report_Index
        public ActionResult Index()
        {
            return View("Report_Index");
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
