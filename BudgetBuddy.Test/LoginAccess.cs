using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

public class LoginAccess
{
    private readonly AccountController _accountController;

    public LoginAccess()
    {
        // Mock DbContext and DbSet
        var users = new[]
        {
                new User { UserId = 1, UserName = "testuser", Password = "password123" },
                // Add more users as needed for testing different scenarios
            }.AsQueryable();

        var usersDbSetMock = new Mock<DbSet<User>>();
        usersDbSetMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        usersDbSetMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        usersDbSetMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        usersDbSetMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());

        // Mock DbContext
        var dbContextMock = new Mock<BudgetDbContext>(new DbContextOptions<BudgetDbContext>());
        dbContextMock.Setup(m => m.Set<User>()).Returns(usersDbSetMock.Object);  // Mock the Set method

        // Mock HttpContext and Session
        var httpContext = new DefaultHttpContext();
        var session = new Mock<ISession>();
        httpContext.Request.HttpContext.Session = session.Object;

        // Mock IUserService
        var userServiceMock = new Mock<IUserService>();

        // Pass the mocked dependencies to the controller
        _accountController = new AccountController(dbContextMock.Object, userServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
        public void Login_ValidCredentials_RedirectsToWelcomeUser()
        {
            // Arrange
            var username = "Test";
            var password = "test";

            // Act
            var result = _accountController.Login(username, password);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("WelcomeUser", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsLoginView()
        {
            // Arrange
            var username = "invaliduser";
            var password = "invalidpassword";

            // Act
            var result = _accountController.Login(username, password);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Login", viewResult.ViewName);
        }
    
}

