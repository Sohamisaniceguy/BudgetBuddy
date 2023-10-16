using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BudgetBuddy.Service;
using Microsoft.EntityFrameworkCore;

namespace BudgetBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly BudgetDbContext _dbcontext;
        private readonly IUserService _userService;

        public AccountController(BudgetDbContext dbcontext, IUserService userService)      //Constructor 
        {
            _dbcontext = dbcontext;
            _userService = userService;
        }


        //Get Login
        public ActionResult Index()
        {
            return View("Login");
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View("Login"); 
        }



        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please provide valid credentials.";
                return View("Login");
            }

            var user = _dbcontext.User.SingleOrDefault(u => u.UserName == username && u.Password == password);

            if (user != null)
            {
                // Store user ID in session using UserUtility
                UserUtility.SetUserId(user.UserId, HttpContext.Session);

                // Redirect to WelcomeUser action
                return RedirectToAction("WelcomeUser", "Account");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid credentials. Please try again.";
                return View("Login");
            }
        }


        public IActionResult WelcomeUser()
        {
            // Retrieve the user ID from the session
            var userId = UserUtility.GetUserId(HttpContext.Session);

            if (userId.HasValue)
            {
                // Retrieve the user info based on the user ID
                var user_info = _dbcontext.User.FirstOrDefault(u => u.UserId == userId.Value);

                if (user_info != null)
                {
                    // Create the ViewModel and pass it to the view
                    var viewModel = new WelcomeUserViewModel
                    {
                        UserInfo = user_info
                    };

                    return View(viewModel);
                }
            }

            // If user is not logged in or user info is not found, show an error
            ViewBag.ErrorMessage = "User not logged in or user information not found.";
            return View("Error"); // Create an Error view for this
        }


        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(UserRegistrationViewModel model, int budgetId)
        {
            if (ModelState.IsValid)
            {
                // Create a new user
                var user = new User
                {
                    First_Name = model.FirstName,
                    Last_Name = model.LastName,
                    Email = model.Email,
                    
                };

                // Save the user to the database
               // _dbcontext.EmailList.Add(user);
               // _dbcontext.SaveChanges();

                // Retrieve the budget
                var budget = _dbcontext.Budgets.FirstOrDefault(b => b.BudgetId == budgetId);

                if (budget != null)
                {
                    // Assign the user to the budget
                    budget.BudgetUsers.Add(new BudgetUser_Enterprise { Budget = budget, User = user });
                    _dbcontext.SaveChanges();

                    return View("Index", "Transaction");
                }

                return RedirectToAction("Budget_Index_Enterprise", "Budget");
            }

            // Handle validation errors
            return View(model);
        }






        // GET: Register
        public IActionResult Register()
        {
            return View();
        }




        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save the model to the database
                var user = new User
                {
                    First_Name = model.First_Name,
                    Last_Name = model.Last_Name,
                    UserName = model.UserName,
                    Email = model.Email,
                    Password = model.Password
                    // Assuming you have a 'User' model with appropriate properties
                };

                _dbcontext.User.Add(user);
                _dbcontext.SaveChanges();

                ViewBag.SuccessMessage = "Registration successful!";
                return View("Register"); // Replace with the appropriate action and controller

                
            }

            // If the model is not valid, return to the create view with validation errors
            return View("Register", model);
        }
    }
}
