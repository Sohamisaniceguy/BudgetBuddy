using BudgetBuddy.Models;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BudgetBuddy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET HOME/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Home/Login
        public IActionResult GoToLogin()
        {
            // Redirect to the Login action in the Account controller
            return View("Login", "Account");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}