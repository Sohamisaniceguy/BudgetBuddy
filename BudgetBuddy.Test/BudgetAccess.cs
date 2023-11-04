using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BudgetBuddy.Test
{
    public class BudgetControllerTests : IDisposable
    {
        private readonly Mock<ILogger<BudgetController>> _mockLogger;
        private readonly Mock<IUserService> _mockUserService;
        private readonly BudgetDbContext _dbContext;
        private readonly BudgetController _controller;
        private readonly Mock<ISession> _mockSession;


        public BudgetControllerTests()
        {
            // Mock ILogger
            _mockLogger = new Mock<ILogger<BudgetController>>();

            // Mock IUserService
            _mockUserService = new Mock<IUserService>();
            //_mockUserService.Setup(service => service.GetLoggedInUserId()).Returns(1); // Assuming a logged in user ID 1 for testing

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDbForTesting")
                .Options;
            _dbContext = new BudgetDbContext(options);

            // Mock HttpContext and Session
            _mockSession = new Mock<ISession>();


            // Set up the HttpContext and Session
            var httpContext = new DefaultHttpContext { Session = _mockSession.Object };
            _controller = new BudgetController(_mockUserService.Object, _dbContext, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };



            // Mock the UrlHelper
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
                     .Returns("callbackUrl"); // Mock the behavior you expect - this is an example

            // Set the UrlHelper for the controller
            _controller.Url = urlHelper.Object;

            // Seed the database if necessary
            SeedDatabase();

            // Before each test, clear the change tracker to prevent tracking conflicts
            _dbContext.ChangeTracker.Clear();
        }

        private void SetupSession(int budgetId)
        {
            var budgetIdBytes = BitConverter.GetBytes(budgetId);
            _mockSession.Setup(_ => _.TryGetValue(It.IsAny<string>(), out budgetIdBytes)).Returns(true);
        }

        private void SeedDatabase()
        {
            var users = new List<User>
    {
        new User { UserId = 1, UserName = "JOHNDOE", Password = "Password", 
            First_Name = "John", Last_Name = "Doe", Email = "john@example.com" },

        new User { UserId = 2, UserName = "Janes", Password = "Password1", 
            First_Name = "Jane", Last_Name = "Smith", Email = "jane@example.com" }
    };

            var budgets = new List<Budget>
    {
        new Budget { BudgetId = 1, BudgetName = "Home Expenses", BudgetLimit = 1000, 
            Enterprise = 0, StartDate = DateTime.Parse("11/1/2023"), EndDate = DateTime.Parse("10/31/2023"), Users = new List<User> { users[0] } },
        
        new Budget { BudgetId = 2, BudgetName = "Business", BudgetLimit = 2000, 
            Enterprise = 1, StartDate = DateTime.Parse("11/1/2023"), EndDate = DateTime.Parse("10/31/2023"), Users = users }
    };

            _dbContext.User.AddRange(users);
            _dbContext.Budgets.AddRange(budgets);
            _dbContext.SaveChanges();
        }


        [Fact]
        public async Task Budget_Index_Individual()
        {
            

            // Act
            var result = _controller.Budget_Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Budget>>(viewResult.Model);
            Assert.NotEmpty(model);
        }

        [Fact]
        public async Task Budget_Index_Enterprise()
        {
            

            // Act
            var result = _controller.Budget_Index_Enterprise();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Budget>>(viewResult.Model);
            Assert.All(model, budget => Assert.Equal(1, budget.Enterprise));
        }


        [Fact]
        public void DeleteBudget_WithValidId_DeletesBudget()
        {
            // Arrange
            int budgetIdToDelete = 1; // Assuming this ID exists in the seeded data

            // Act
            var result = _controller.DeleteBudget(budgetIdToDelete);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Budget_Index", redirectToActionResult.ActionName); // Assuming this is the action you want to redirect to after deletion
            Assert.False(_dbContext.Budgets.Any(b => b.BudgetId == budgetIdToDelete)); // Check if the budget has been deleted
        }


        [Fact]
        public void Individualmode_Buget_Creation() // Problem!!!
        {
            // Arrange
            var testUser = new User
            {
                UserId = 1, // Make sure this UserId is unique for the test
                UserName = "JOHNDOE",
                Password = "Password",
                First_Name = "John",
                Last_Name = "Doe",
                Email = "john@example.com"
            };

            // Ensure the user is not already added
            if (!_dbContext.User.Any(u => u.UserId == testUser.UserId))
            {
                _dbContext.User.Add(testUser);
                _dbContext.SaveChanges();
            }

            var model = new Budget
            {
                
                BudgetName = "New Budget",
                BudgetLimit = 500,
                Enterprise = 0,
                StartDate = DateTime.Parse("11/1/2023"),
                EndDate = DateTime.Parse("10/31/2023")
                // Set other properties as required
            };

            // Mocking the session to simulate the behavior of `GetInt32`
            var userIdBytes = BitConverter.GetBytes(testUser.UserId);
            _mockSession.Setup(_ => _.TryGetValue("UserId", out userIdBytes)).Returns(true);

            // If the CreateBudget action relies on the ModelState being valid
            _controller.ModelState.Clear(); // Clear any existing errors
                                            // Add any required properties to the model here to ensure it is valid



            // Act
            var result = _controller.CreateBudget(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Budget_Index", redirectToActionResult.ActionName);
            // If the ID is auto-generated, check for the existence of the budget based on its name or other unique properties
            Assert.True(_dbContext.Budgets.Any(b => b.BudgetName == model.BudgetName && b.BudgetLimit == model.BudgetLimit));
        }

        [Fact]
        public void Bud_CreateorChange_WithInvalidData_ReturnsViewWithModel()
        {
            // Arrange
            var model = new Budget
            {
                // Set up properties that would make the model invalid
            };
            _controller.ModelState.AddModelError("error", "some error message");

            // Act
            var result = _controller.CreateBudget(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.ViewData.Model); // Ensure the same model is returned for correction
        }

        // Dispose the in-memory database if needed
        public void Dispose()
        {
            _dbContext?.Dispose();
        }
        // Additional test methods for each action will follow...
    }
}
