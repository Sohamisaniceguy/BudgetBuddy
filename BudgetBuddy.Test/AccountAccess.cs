using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBuddy.Test
{

    public class AccountControllerTests
    {
        private readonly AccountController _controller;
        private readonly Mock<IUserService> _mockUserService;
        private readonly BudgetDbContext _dbContext;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;
        private readonly IDataProtector _stubDataProtector;

        private class StubDataProtector : IDataProtector
        {
            // Implement IDataProtector methods, can be no-op or basic implementations
            public IDataProtector CreateProtector(string purpose)
            {
                return this; // Return self for simplicity, assuming no actual data protection is needed
            }

            public byte[] Protect(byte[] plaintext)
            {
                return plaintext; // Simple pass-through implementation
            }

            public byte[] Unprotect(byte[] protectedData)
            {
                return protectedData; // Simple pass-through implementation
            }
        }
        public AccountControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _dbContext = new BudgetDbContext(options);

            // Mock User Service
            _mockUserService = new Mock<IUserService>();

            // Mock ILogger
            _mockLogger = new Mock<ILogger<AccountController>>();

            // Create a stub for IDataProtector
            _stubDataProtector = new StubDataProtector();

            // Mock IDataProtectionProvider
            _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            _mockDataProtectionProvider
                .Setup(p => p.CreateProtector(It.IsAny<string>()))
                .Returns(_stubDataProtector); // Use the StubDataProtector here

            // Initialize AccountController with the mocked dependencies
            _controller = new AccountController(_dbContext, _mockUserService.Object, _mockLogger.Object, _mockDataProtectionProvider.Object);
        }



        [Fact]
        public async Task Login_ValidCredentials_RedirectsToWelcomeUser()
        {
            // Arrange
            var testUser = new User
            {
                
                Email = "john.doe@example.com",
                First_Name = "John",
                Last_Name = "Doe",
                
                ResetPasswordToken = "reset_token",
                
                VerifyUserToken = "verify_token",
                UserId = 1,
                UserName = "testuser",
                PasswordHash = "hashed_testpassword", // This should be a hash of "testpassword"
                EmailConfirmed = true,
                FailedLoginAttempts = 0,
                LockoutEnd = null,
                IsLockedOut = false,

                // ... set other necessary properties ...
            };

            _dbContext.User.Add(testUser);
            _dbContext.SaveChanges();


            var loginViewModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "testpassword",
                RememberMe = false
            };

            // Mocking the necessary conditions for a successful login
            _mockUserService.Setup(x => x.VerifyPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockUserService.Setup(x => x.IsLockedOut(It.IsAny<User>())).Returns(false);

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result); // Verify if the result is a redirection
            Assert.Equal("WelcomeUser", redirectResult.ActionName);
        }


        [Fact]
        public async Task Login_InvalidCredentials_ReturnsToLoginView()
        {
            // Arrange
            var loginViewModel = new LoginViewModel
            {
                Username = "wronguser",
                Password = "wrongpassword",
                RememberMe = false
            };

            _mockUserService.Setup(x => x.VerifyPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = await _controller.Login(loginViewModel);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(loginViewModel, viewResult.Model);
        }

        // Additional tests for other scenarios like account lockout, email confirmation, etc.
    }







    //public void Dispose()
    //    {
    //        throw new NotImplementedException();
    //    }
        // ... Other tests
   // }
}
