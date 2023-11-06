using BudgetBuddy.Data;
using BudgetBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BudgetBuddy.Service
{

    public interface IUserService
    {
        public bool VerifyPassword(User user, string providedPassword, string storedHash);
        public string HashPassword(User user, string password);
        int GetLoggedInUserId();

        int IncrementFailedLoginAttempt(User user);
        bool IsLockedOut(User user);
        void ResetFailedLoginAttempt(User user);

        bool IsUserActive(User user);

        Task<bool> ResetPasswordAsync(User user, string token, string newPassword);

        Task<string> GeneratePasswordResetTokenAsync(User user);

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<bool> ConfirmEmailAsync(User user, string token);




    }

    public class UserService : IUserService
    {
        private readonly BudgetDbContext _dbcontext;

        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;


        // The threshold for how many failed attempts before lockout
        private const int LockoutThreshold = 5;
        // The duration of the lockout in minutes
        private const int LockoutDurationInMinutes = 30;


        public UserService(IHttpContextAccessor httpContextAccessor, BudgetDbContext dbcontext, ILogger<UserService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = new PasswordHasher<User>();
            _dbcontext = dbcontext;
            _logger = logger;
        }

        public int GetLoggedInUserId()
        {
            if (_httpContextAccessor.HttpContext?.Session.GetInt32("UserId") != null)
            {
                return (int)_httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            }

            return -1;
        }

        public string HashPassword(User user, string password)
        {
            return _passwordHasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string providedPassword, string storedHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, storedHash, providedPassword);
            return result == PasswordVerificationResult.Success;
        }







        public int IncrementFailedLoginAttempt(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null when attempting to increment failed login attempts.");
            }

            if (_dbcontext == null)
            {
                throw new InvalidOperationException("Database context (_dbContext) is not initialized.");
            }

            // Assuming that user has a property FailedLoginAttempts that is an integer
            user.FailedLoginAttempts += 1;

            // Calculate the remaining attempts
            int remainingAttempts = LockoutThreshold - user.FailedLoginAttempts;

            // If the failed login attempts reach a certain threshold, set the lockout end time
            if (user.FailedLoginAttempts >= LockoutThreshold)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationInMinutes);
                remainingAttempts = 0; // No more attempts remaining once locked out
            }

            // Save changes to the database
            _dbcontext.SaveChanges();

            // Return the number of remaining attempts
            return remainingAttempts;
        }


        public bool IsLockedOut(User user)
        {
            return user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow;
        }

        public void ResetFailedLoginAttempt(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "The user cannot be null.");
            }

            if (_dbcontext == null)
            {
                throw new InvalidOperationException("The database context is not initialized.");
            }

            // Proceed with resetting the failed login attempt count
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            // Save the changes to the database
            _dbcontext.SaveChanges();
        }


        public bool IsUserActive(User user)
        {
            // Your logic to determine if the user is active
            return user.EmailConfirmed; // Assuming 'IsActive' is a boolean property of the User class
        }


        public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
        {
            _logger.LogInformation("Attempting to reset password for user: {UserId}", user.UserId);

            if (user.ResetPasswordTokenUsed || user.ResetPasswordTokenExpiration < DateTime.UtcNow)
            {
                _logger.LogWarning("Reset password failed: Token has been used or is expired for user: {UserId}", user.UserId);
                return false;
            }

            // Verify the provided token with the stored hashed token
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.ResetPasswordToken, token);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogWarning("Reset password failed: Token mismatch for user: {UserId}", user.UserId);
                return false;
            }


            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            user.ResetPasswordTokenUsed = true;
            user.ResetPasswordToken = "";
            user.ResetPasswordTokenExpiration = null;

            _logger.LogInformation("Updating user's password and marking token as used for user: {UserId}", user.UserId);
            _dbcontext.User.Update(user);

            try
            {
                int result = await _dbcontext.SaveChangesAsync();
                bool success = result > 0;
                if (success)
                {
                    _logger.LogInformation("Password reset successfully for user: {UserId}", user.UserId);
                }
                else
                {
                    _logger.LogWarning("No changes saved to the database for user: {UserId}", user.UserId);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred when resetting password for user: {UserId}", user.UserId);
                throw; // Re-throw the exception to handle it further up the call stack
            }
        }



        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            _logger.LogInformation("Starting token generation for password reset.");
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            string token = Convert.ToBase64String(tokenBytes);
            _logger.LogInformation("Token generated successfully.");

            string hashedToken = _passwordHasher.HashPassword(user, token);
            _logger.LogInformation("Token hashed and ready to be stored.");

            user.ResetPasswordToken = hashedToken;
            user.ResetPasswordTokenExpiration = DateTime.UtcNow.AddHours(1);
            user.ResetPasswordTokenUsed = false;

            _logger.LogInformation("Updating user with password reset token information.");
            _dbcontext.User.Update(user);
            try
            {
                await _dbcontext.SaveChangesAsync();
                _logger.LogInformation("User updated successfully in the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with password reset token information.");
                throw; // Rethrow the exception to maintain the flow of handling it outside this method
            }

            return token;
        }




        public async Task<bool> ConfirmEmailAsync(User user, string token)
        {
            _logger.LogInformation("Attempting to confirm email for user: {UserId}", user.UserId);

            if (user.VerifyTokenUsed || user.VerifyTokenExpiration < DateTime.UtcNow)
            {
                _logger.LogWarning("Reset password failed: Token has been used or is expired for user: {UserId}", user.UserId);
                return false;
            }

            // Verify the provided token with the stored hashed token
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.VerifyUserToken, token);
            if (verificationResult != PasswordVerificationResult.Success)
            {
                _logger.LogWarning("Reset password failed: Token mismatch for user: {UserId}", user.UserId);
                return false;
            }

                user.EmailConfirmed = true; // Assuming true means the email is confirmed.
                user.VerifyTokenUsed = true; // Mark the token as used.
                user.VerifyUserToken = "";
                user.VerifyTokenExpiration = null;

                _logger.LogInformation("Updating user's password and marking token as used for user: {UserId}", user.UserId);
                _dbcontext.User.Update(user);
            

                try
                {
                    int result = await _dbcontext.SaveChangesAsync();
                    bool success = result > 0;
                    if (success)
                    {
                        _logger.LogInformation("Email verification successfully for user: {UserId}", user.UserId);
                    }
                    else
                    {
                        _logger.LogWarning("No changes saved to the database for user: {UserId}", user.UserId);
                    }
                    return success;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred when confirming email: {UserId}", user.UserId);
                    throw; // Re-throw the exception to handle it further up the call stack
                }

        }








        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            _logger.LogInformation("Starting token generation for email confirmation.");
            byte[] tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            string token = Convert.ToBase64String(tokenBytes);
            _logger.LogInformation("Token generated successfully.");

            string hashedToken = _passwordHasher.HashPassword(user, token);
            _logger.LogInformation("Token hashed and ready to be stored.");

            user.VerifyUserToken = hashedToken;
            user.VerifyTokenExpiration = DateTime.UtcNow.AddHours(1);
            user.ResetPasswordTokenUsed = false;

            _logger.LogInformation("Updating user with email confirmation token information.");
            _dbcontext.User.Update(user);
            try
            {
                await _dbcontext.SaveChangesAsync();
                _logger.LogInformation("User updated successfully in the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with email confirmation token information.");
                throw; // Rethrow the exception to maintain the flow of handling it outside this method
            }

            return token;
        }


    }

}
