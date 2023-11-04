using Abp.Web.Mvc.Models;
using BudgetBuddy.Controllers;
using BudgetBuddy.Service;
using BudgetBuddy.Data;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BudgetBuddy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace BudgetBuddy.Test
{
    public class AccountControllerTests : IDisposable
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly BudgetDbContext _dbContext;
        private readonly AccountController _controller;
        private readonly Mock<ISession> _mockSession;

        public AccountControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockSession = new Mock<ISession>();

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryBudgetDb")
                .EnableSensitiveDataLogging()
                .Options;
            // Create instance of DbContext
            _dbContext = new BudgetDbContext(options);

            // Instantiate the controller
            _controller = new AccountController(_dbContext, _mockUserService.Object);

            // Mock HttpContext and Session
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(ctx => ctx.Session).Returns(_mockSession.Object);

            // Set the ControllerContext with the mock HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            };
        }

        [Fact]
        public void Get_LoginPage1()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Get_LoginPage2()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Login_With_ValidCredentials()
        {
            // Arrange - seed the database
            var user = new User
            {
                UserName = "testUser",
                Password = "testPassword",
                Email = "testUser@example.com",
                First_Name = "Test",
                Last_Name = "User"
            };

            _dbContext.User.Add(user);
            _dbContext.SaveChanges();

            // Mock the session behavior
            var sessionDict = new Dictionary<string, byte[]>();
            _mockSession.Setup(_ => _.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                        .Callback<string, byte[]>((key, value) => {
                            sessionDict[key] = value;
                        });
            _mockSession.Setup(_ => _.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                        .Returns((string key, out byte[] value) => {
                            return sessionDict.TryGetValue(key, out value);
                        });

            // Act
            var result = _controller.Login("testUser", "testPassword") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WelcomeUser", result.ActionName);

            // Verify that the session was called to set the UserId
            _mockSession.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }


        [Fact]
        public void Login_With_Invalid_Credentials()
        {
            // Arrange - no need to seed the database with a user
            // since we want to test the invalid credentials scenario

            // Mock the session behavior
            var sessionDict = new Dictionary<string, byte[]>();
            _mockSession.Setup(_ => _.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                        .Callback<string, byte[]>((key, value) => {
                            sessionDict[key] = value;
                        });
            _mockSession.Setup(_ => _.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                        .Returns((string key, out byte[] value) => {
                            return sessionDict.TryGetValue(key, out value);
                        });

            // Act
            var result = _controller.Login("invalidUser", "invalidPassword") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ViewName);
            Assert.NotNull(result.ViewData["ErrorMessage"]);
            Assert.Equal("Invalid credentials. Please try again.", result.ViewData["ErrorMessage"]);

            // Verify that the session Set method was never called since the credentials are invalid
            _mockSession.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }


        [Fact]
        public void Register_Get_ReturnsAViewResult()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        


        [Fact]
        public void Register_With_ValidModel_AddsUser_And_ReturnsRegisterView_WithSuccessMessage()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                First_Name = "John",
                Last_Name = "Doe",
                UserName = "johndoe",
                Email = "john@example.com",
                Password = "password123"
            };

            // Act
            var result = _controller.Register(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Register", result.ViewName);
            Assert.Equal("Registration successful!", result.ViewData["SuccessMessage"]);

            // Verify that a user was added to the database
            var user = _dbContext.User.FirstOrDefault(u => u.UserName == model.UserName);
            Assert.NotNull(user);
        }

        [Fact]
        public void Register_With_InvalidModel_DoesNotAddUser_And_ReturnsRegisterView_WithModel()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                // Don't set any properties to simulate an invalid model
            };
            _controller.ModelState.AddModelError("error", "some error message"); // Force model state to be invalid

            // Act
            var result = _controller.Register(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Register", result.ViewName);
            var returnedModel = result.Model as RegisterViewModel;
            Assert.Equal(model, returnedModel); // The same model should be returned for correction

            // Verify that no user was added to the database
            var userCount = _dbContext.User.Count();
            Assert.Equal(0, userCount); // Assuming the database was empty before this test ran
        }



        // Dispose the in-memory database if needed
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
