using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BudgetBuddy.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using BudgetBuddy.Utils;
using Utils;

namespace BudgetBuddy.Controllers
{
    public class AccountController : Controller
    {
        private readonly BudgetDbContext _dbcontext;
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDataProtector _protector;
        


        public AccountController(BudgetDbContext dbcontext, IUserService userService, ILogger<AccountController> logger, IDataProtectionProvider dataProtectionProvider)      //Constructor 
        {
            _dbcontext = dbcontext;
            _userService = userService;
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
            _protector = _dataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware", CookieAuthenticationDefaults.AuthenticationScheme, "v2");
            
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
        [ValidateAntiForgeryToken] // Protect against CSRF
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            _logger.LogInformation("Attempting to log in user: {Username}", model.Username.Trim().ToLower());
            // Check if the form data is valid
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Attempt to find the user by the username provided in the model
            var user = _dbcontext.User.SingleOrDefault(u => u.UserName == model.Username.Trim().ToLower());

            // Define the maximum number of attempts allowed
            int maxLoginAttempts = 5;

            if (user != null && user.EmailConfirmed != true)
            {
                _logger.LogWarning("Email not confirmed for user: {Username}", model.Username);
                ViewBag.EmailConfirmationError = "Verify your email first.";
                return View(model);
            }


            // If the user doesn't exist or the password verification fails
            if (user == null || !_userService.VerifyPassword(user, model.Password, user.PasswordHash))
            {
                // If the user exists, increment the failed login attempt
                if (user != null)
                {
                    int attemptsLeft = _userService.IncrementFailedLoginAttempt(user);
                    if (_userService.IsLockedOut(user))
                    {
                        _logger.LogWarning("Account locked out for user: {Username}", model.Username);
                        ModelState.AddModelError("", "Account locked. Please try again later.");
                        // Pass the lockout status to the view
                        TempData["Lockout"] = true;
                    }
                    else
                    {
                        _logger.LogWarning("Invalid login attempt for user: {Username}. Attempts left: {AttemptsLeft}", model.Username, attemptsLeft);
                        // Pass the number of attempts left to the view
                        TempData["AttemptsLeft"] = attemptsLeft;
                        ModelState.AddModelError("", $"Invalid login attempt. You have {attemptsLeft} more attempt(s) before your account is locked.");
                    }
                }
                else
                {
                    _logger.LogWarning("Login attempt with invalid username: {Username}", model.Username);
                    ModelState.AddModelError("", "Invalid login attempt.");
                }

                return View(model);
            }


            // If the user is locked out, handle the lockout
            if (_userService.IsLockedOut(user))
            {
                _logger.LogWarning("Account locked out for user: {Username}", model.Username);
                ModelState.AddModelError("", "Account locked. Please try again later.");
                return View(model);
            }

            // If the login is successful, reset the failed login attempt
            _userService.ResetFailedLoginAttempt(user);

            // Create the claims for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
                
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, authProperties, CookieAuthenticationDefaults.AuthenticationScheme);

            var ticketDataFormat = new TicketDataFormat(_protector);
            var cookieValue = ticketDataFormat.Protect(ticket);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                Expires = authProperties.ExpiresUtc?.UtcDateTime
            };

            Response.Cookies.Append(".AspNetCore.Cookies", cookieValue, cookieOptions);

            // Also set the user's ID in the session for session management
            HttpContext.Session.SetInt32("UserID", user.UserId);

            //Layout Use
            int? userId = HttpContext.Session.GetInt32("UserID");
            ViewBag.UserId = userId;
            

            _logger.LogInformation("User logged in successfully: {Username}", model.Username);
            return RedirectToAction("WelcomeUser", "Account");
        }
        



        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF
        public async Task<IActionResult> Logout()
        {

            try
            {
                _logger.LogInformation("Logout initiated.");

                // Manually remove the cookie
                Response.Cookies.Delete(".AspNetCore.Cookies");
                Response.Cookies.Delete("YourAppSessionCookie", new CookieOptions { Path = "/" });

                HttpContext.Session.Clear(); // Clear the session upon logout
                _logger.LogInformation("User logged out successfully.");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout.");
                throw;
            }
        }



        [HttpGet]
        public IActionResult WelcomeUser()
        {
            var userId = HttpContext.Session.GetInt32("UserID");

            if (userId.HasValue)
            {
                var user = _dbcontext.User.Find(userId.Value);
                if (user != null)
                {
                    // Create the ViewModel and pass it to the view
                    var viewModel = new WelcomeUserViewModel
                    {
                        UserInfo = user
                    };
                    return View(viewModel);
                }
            }

            // If user is not logged in or user information not found, show an error
            ViewBag.ErrorMessage = "User not logged in or user information not found.";
            return View("Error"); // Ensure there is an Error view to handle this case
        }



        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("User trying to Register on the register screen");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            _logger.LogInformation("Starting user registration process.");
            if (ModelState.IsValid)
            {

                // Initialize an empty string to hold all error messages
                string errorMessages = "";

                // Normalize input
                var normalizedUsername = model.UserName.Trim().ToLower();
                var normalizedEmail = model.Email.Trim().ToLower();

                // Check for unique username and email
                _logger.LogInformation("Validating user uniqueness.");
                var uniquenessResult = await _userService.ValidateUserUniqueness(normalizedUsername, normalizedEmail);
                if (!uniquenessResult.Item1)
                {
                    errorMessages += uniquenessResult.Item2;
                    _logger.LogWarning($"User uniqueness validation failed: {uniquenessResult.Item2}");
                }

                // Validate the password
                _logger.LogInformation("Validating password.");
                var validationResult = PasswordValidator.Validate(model.UserName, model.Password); // Assuming model.Email is the user identifier
                if (!validationResult.Item1)
                {
                    errorMessages += validationResult.Item2;
                    _logger.LogWarning($"Password validation failed: {validationResult.Item2}");
                }

                // If there are any error messages, add them to TempData and return the view
                if (!string.IsNullOrEmpty(errorMessages))
                {
                    TempData["CustomError"] = errorMessages;
                    _logger.LogWarning("Registration process encountered errors, returning to view with errors.");
                    return View(model);
                }

                // Save the model to the database
                var user = new User
                {
                    First_Name = model.First_Name,
                    Last_Name = model.Last_Name,
                    UserName = normalizedUsername,
                    Email = normalizedEmail,
                    ResetPasswordToken = " ",
                    VerifyUserToken = " ",
                    // ... other properties as necessary
                };

                // Hash the password and set the PasswordHash property
                user.PasswordHash = _userService.HashPassword(user, model.Password);

                // Add the user to the DbContext
                _logger.LogInformation("Adding new user to the database.");
                _dbcontext.User.Add(user);
                await _dbcontext.SaveChangesAsync();

                // Generate email confirmation token
                _logger.LogInformation("Sending confirmation email.");
                var emailConfirmationToken = await _userService.GenerateEmailConfirmationTokenAsync(user);

                // Create a confirmation link
                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.UserId, token = emailConfirmationToken }, Request.Scheme);

                var emailBody = $"To access your Budget Buddy account, please verify your email address: <a href='{confirmationLink}'>here</a>.";

                // Send the email using the EmailSender's static method
                bool emailSentSuccessfully = EmailSender.Send(user.Email, "Confirm Email", emailBody);

                if (emailSentSuccessfully)
                {
                    _logger.LogInformation("Registration successful, email sent.");
                    ViewBag.SuccessMessage = "Registration successful! Please check your email to confirm your account.";
                    return View("VerifyEmail"); // Or redirect to a confirmation page
                }
                else
                {
                    // Handle the case when the email couldn't be sent
                    _logger.LogError("Failed to send confirmation email.");
                    ModelState.AddModelError(string.Empty, "An error occurred while sending confirmation email.");
                }
            }

            // If the model is not valid, return to the create view with validation errors
            _logger.LogWarning("Registration process terminated due to invalid model state.");
            return View("Register", model);
        }



        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            _logger.LogInformation("ConfirmEmail started for user ID {UserId}", userId);

            if (userId == 0 || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Invalid email confirmation token for user ID {UserId}", userId);
                ViewBag.ErrorMessage = "Invalid email confirmation token.";
                return View("Error"); // Make sure to create an Error view to handle this.
            }

            var user = await _dbcontext.User.FindAsync(userId);
            if (user != null && await _userService.ConfirmEmailAsync(user, token))
            {
                _logger.LogInformation("Email confirmed for user ID {UserId}", userId);
                ViewBag.SuccessMessage = "Your email has been confirmed. You can now login.";

                var emailBody = $" Your email has been successfully verified. You can now log in to your Budget Buddy account using your new password.";

                // Send the email using the EmailSender's static method
                bool emailSentSuccessfully = EmailSender.Send(user.Email, "Email Confirmation", emailBody);
                _logger.LogInformation("Email sent successfully: {EmailSentSuccessfully} for user ID {UserId}", emailSentSuccessfully, userId);
                return View("VerifyEmailConfirmation"); // Or redirect to a confirmation success page.
            }

            _logger.LogError("Error while confirming email for user ID {UserId}", userId);
            ViewBag.ErrorMessage = "Error while confirming your email.";
            return View("Error"); // Make sure to create an Error view to handle this.
        }



        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }



        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            _logger.LogInformation("ForgotPassword POST action called with email: {Email}", model.Email);

            if (ModelState.IsValid)
            {
                _logger.LogDebug("Model state is valid for ForgotPassword.");

                var user = await _dbcontext.User
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null || !_userService.IsUserActive(user))
                {
                    _logger.LogWarning("User is null or not active for email: {Email}", model.Email);
                    // Don't reveal that the user does not exist or is not active
                    ViewBag.UserEmail = model.Email;
                    return View("ForgotPasswordConfirmation");
                }

                _logger.LogDebug("User found and active: {Email}", user.Email);

                // Generate a reset password token
                var token = await _userService.GeneratePasswordResetTokenAsync(user);
                _logger.LogDebug("Password reset token generated for user: {UserId}", user.UserId);

                // Create reset link
                var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = user.Email, token = token }, Request.Scheme);
                _logger.LogDebug("Password reset link generated: {PasswordResetLink}", passwordResetLink);

                var emailBody = $"Please confirm your email by clicking <a href='{passwordResetLink}'>here</a>.";

                // Send the email using the EmailSender's static method
                bool emailSentSuccessfully = EmailSender.Send(user.Email, "Password Reset", emailBody);
                if (emailSentSuccessfully)
                {
                    _logger.LogInformation("Password reset email sent successfully to: {Email}", user.Email);
                }
                else
                {
                    _logger.LogError("Failed to send password reset email to: {Email}", user.Email);
                }

                // Redirect to confirmation page
                return View("ForgotPasswordConfirmation");
            }

            _logger.LogWarning("Model state is invalid for ForgotPassword.");
            // If we got this far, something failed, redisplay form
            return View(model);
        }




        // GET: Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token.");
            }
            return View();
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            _logger.LogInformation("ResetPassword POST action called with email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid for ResetPassword.");
                return View(model);
            }

            // Validate the password
            var validationResult = PasswordValidator.Validate(model.Email, model.Password); // Assuming model.Email is the user identifier
            if (!validationResult.Item1)
            {
                // Use TempData to pass the specific error message to the view
                TempData["CustomError"] = validationResult.Item2;
                return View(model);
            }

            var user = await _dbcontext.User
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                // Log the fact that the user was not found
                _logger.LogWarning("User not found with email: {Email}", model.Email);
            }
            else
            {
                _logger.LogInformation("User found with email: {Email}, attempting to reset password.", model.Email);
                var result = await _userService.ResetPasswordAsync(user, model.Token, model.Password);
                if (result)
                {
                    _logger.LogInformation("Password reset was successful for user with email: {Email}", model.Email);
                    var emailBody = $" Your password has been successfully reset. You can now log in to your Budget Buddy account using your new password." +
                                    $" If this is not you click <a href=' '>here</a> to REPORT. ";

                    // Send the email using the EmailSender's static method
                    bool emailSentSuccessfully = EmailSender.Send(user.Email, "Password Reset", emailBody);
                    return View("ResetPasswordConfirmation");
                }
                else
                {
                    // Log the failure of the password reset attempt
                    _logger.LogWarning("Password reset attempt failed for user with email: {Email}", model.Email);
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to reset password.");
            _logger.LogWarning("Redisplaying ResetPassword form due to failure to reset password for email: {Email}", model.Email);
            return View(model);
        }


    }
}
