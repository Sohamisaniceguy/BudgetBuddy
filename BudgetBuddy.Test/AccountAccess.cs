using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BudgetBuddy.Test
{
    public class AccountControllerTests
    {
        private AccountController CreateAccountController()
        {
            // Create an in-memory database
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create a mock for your IUserService and any other dependencies
            var userServiceMock = new Mock<IUserService>();
            var loggerMock = new Mock<ILogger<AccountController>>();

            using var context = new BudgetDbContext(options);
            // Add test data to the in-memory database
            context.User.Add(new User
            {
                UserId = 1,
                First_Name = "John",
                Last_Name = "Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashed_password",
                ResetPasswordToken = "reset_token",
                UserName = "john.doe",
                VerifyUserToken = "verify_token"
            });
            context.SaveChanges();

            // Create an instance of the controller with the mock dependencies
            var controller = new AccountController(context, userServiceMock.Object, loggerMock.Object, null);

            // Set up the HTTP context for the controller
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost");
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };

            return controller;
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsRedirectToAction()
        {
            // Arrange
            var controller = CreateAccountController();
            var loginModel = new LoginViewModel
            {
                Username = "testuser",
                Password = "password", // Assuming this is the correct password
                RememberMe = false // Set as needed
            };

            // Act
            var result = await controller.Login(loginModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WelcomeUser", result.ActionName);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsRedirectToAction()
        {
            // Arrange
            var userServiceMock = new Mock<IUserService>();
            var loggerMock = new Mock<ILogger<AccountController>>();

            var dbContextMock = new Mock<BudgetDbContext>();

            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var dataProtectorMock = new Mock<IDataProtector>();

            dataProtectionProviderMock.Setup(provider => provider.CreateProtector(It.IsAny<string>()))
                .Returns(dataProtectorMock.Object);

            var controller = new AccountController(dbContextMock.Object, userServiceMock.Object, loggerMock.Object, dataProtectionProviderMock.Object);

            var model = new RegisterViewModel
            {
                First_Name = "John",
                Last_Name = "Doe",
                UserName = "johndoe",
                Email = "johndoe@example.com",
                Password = "password",
                // Remove ConfirmPassword if not used in your Register action
            };

            // Assuming your validation methods return success for this test
            userServiceMock.Setup(service => service.ValidateUserUniqueness(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new Tuple<bool, string>(true, "")));
            userServiceMock.Setup(service => service.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                .Returns(Task.FromResult("emailConfirmationToken"));

            // Act
            var result = await controller.Register(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("VerifyEmail", result.ActionName);
        }
        

    }
}
