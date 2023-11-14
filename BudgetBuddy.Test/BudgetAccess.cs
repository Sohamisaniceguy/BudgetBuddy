using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using BudgetBuddy.Service;
using BudgetBuddy.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Serilog.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BudgetBuddy.Test
{
    public class BudgetAccess
    {
        private readonly BudgetDbContext _context;
        private readonly BudgetController _controller;
        private readonly Mock<IUserService> _mockUserService;

        public BudgetAccess()
        {
            // Set up an in-memory database
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Budget")
                .Options;

            _context = new BudgetDbContext(options);

            // Mock user service
            _mockUserService = new Mock<IUserService>();

            // Mock the IUrlHelper
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("callbackUrl").Verifiable();

            

            // Set up HttpContext for session, as it is used in the controller
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();
            httpContext.Session.SetInt32("UserID", 1); // Assuming a logged-in user with ID 1

            // Initialize the controller with the in-memory context and a null logger (or a mock if needed)
            _controller = new BudgetController(_mockUserService.Object, _context, new NullLogger<BudgetController>())
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Assign the mocked IUrlHelper to the controller
            _controller.Url = mockUrlHelper.Object;

        }

        [Fact]
        public void Individual_Budget_Index_View()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Email = "john.doe@example.com",
                First_Name = "John",
                Last_Name = "Doe",
                PasswordHash = "hashed_password",
                ResetPasswordToken = "reset_token",
                UserName = "john.doe",
                VerifyUserToken = "verify_token"
                // Set any other required properties that your User entity may have
            };
            _context.User.Add(user);
            _context.Budgets.Add(new Budget { BudgetId = 1, BudgetName = "Test Budget 1", BudgetLimit = 1000, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Enterprise = 0, Users = new List<User> { user } });
            _context.Budgets.Add(new Budget { BudgetId = 2, BudgetName = "Test Budget 2", BudgetLimit = 2000, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Enterprise = 0, Users = new List<User> { user } });
            _context.SaveChanges();

            // Mock the IUrlHelper
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("callbackUrl").Verifiable();
            _controller.Url = mockUrlHelper.Object;

            // Act
            var result = _controller.Budget_Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Budget>>(viewResult.ViewData.Model);
            Assert.NotEmpty(model);
        }

        [Fact]
        public void Enterprise_Budget_Index()
        {
            // Arrange
            var enterpriseUser = new User
            {
                UserId = 1,
                Email = "john.doe@example.com",
                First_Name = "John",
                Last_Name = "Doe",
                PasswordHash = "hashed_password",
                ResetPasswordToken = "reset_token",
                UserName = "john.doe",
                VerifyUserToken = "verify_token"
                
            };
            _context.User.Add(enterpriseUser);
            _context.Budgets.Add(new Budget { BudgetId = 3, BudgetName = "Enterprise Budget 1", BudgetLimit = 3000, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Enterprise = 1, Users = new List<User> { enterpriseUser } });
            _context.Budgets.Add(new Budget { BudgetId = 4, BudgetName = "Enterprise Budget 2", BudgetLimit = 4000, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Enterprise = 1, Users = new List<User> { enterpriseUser } });
            _context.SaveChanges();

            

            // Act
            var result = _controller.Budget_Index_Enterprise();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Budget>>(viewResult.ViewData.Model);
            Assert.NotEmpty(model);
        }


        [Fact]
        public void Create_Individual_Budget()
        {
            // Arrange
            
            var user = new User
            {
                UserId = 1,
                Email = "user@example.com",
                First_Name = "John",
                Last_Name = "Doe",
                PasswordHash = "hashed_password",
                ResetPasswordToken = "reset_token",
                UserName = "john.doe",
                VerifyUserToken = "verify_token"
            };
            _context.User.Add(user);
            _context.SaveChanges();

            // Mock HttpContext and Session
            var httpContextMock = new Mock<HttpContext>();
            var sessionMock = new Mock<ISession>();
            byte[] userIdBytes = BitConverter.GetBytes(user.UserId); // Convert UserId to byte array
            sessionMock.Setup(_ => _.TryGetValue(It.IsAny<string>(), out userIdBytes)).Returns(true); // Setup TryGetValue to simulate GetInt32
            httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);
            var loggerMock = new Mock<ILogger<BudgetController>>();
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());



            var budget = new Budget
            {
                BudgetName = "New Budget",
                BudgetLimit = 5000,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Users = new List<User>(),
                // Set other required properties...
            };

            // Act
            var result = _controller.CreateBudget(budget);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Budget_Index", redirectToActionResult.ActionName);
            Assert.Equal("Budget", redirectToActionResult.ControllerName);

            // Verify that the budget was added to the database
            var createdBudget = _context.Budgets
                .Include(b => b.Users)
                .FirstOrDefault(b => b.BudgetName == budget.BudgetName && b.Enterprise == 0);
            Assert.NotNull(createdBudget);
            Assert.Contains(user, createdBudget.Users);
        }



        [Fact]
        public void Create_Enterprise_Budget()
        {
            // Arrange
            var budget = new Budget
            {
                BudgetName = "Enterprise Budget",
                BudgetLimit = 10000,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Users = new List<User>()
            };
            var user = new User
            {
                UserId = 1,
                Email = "user@example.com",
                First_Name = "John",
                Last_Name = "Doe",
                PasswordHash = "hashed_password",
                ResetPasswordToken = "reset_token",
                UserName = "john.doe",
                VerifyUserToken = "verify_token"
            };
            _context.User.Add(user);
            _context.SaveChanges();

            

            var httpContextMock = new Mock<HttpContext>();
            var sessionMock = new Mock<ISession>();
            byte[] userIdBytes = BitConverter.GetBytes(user.UserId); // Convert UserId to byte array
            sessionMock.Setup(_ => _.TryGetValue(It.IsAny<string>(), out userIdBytes)).Returns(true); // Setup TryGetValue to simulate GetInt32
            httpContextMock.Setup(h => h.Session).Returns(sessionMock.Object);
            var loggerMock = new Mock<ILogger<BudgetController>>();
            var tempDataDictionary = new TempDataDictionary(httpContextMock.Object, Mock.Of<ITempDataProvider>());

       
            // Act
            var result = _controller.CreateBudget_Ent(budget);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Budget_Index_Enterprise", redirectToActionResult.ActionName);
            Assert.Equal("Budget", redirectToActionResult.ControllerName);

            // Verify that a new budget was added to the database
            var createdBudget = _context.Budgets
                .Include(b => b.Users)
                .FirstOrDefault(b => b.BudgetName == budget.BudgetName && b.Enterprise == 1);
            Assert.NotNull(createdBudget);
            Assert.Contains(user, createdBudget.Users);
        }


        [Fact]
        public void Delete_Budget()
        {
            // Arrange
            var budget = new Budget
            {
                BudgetId = 1,
                BudgetName = "Test Budget",
                
            };
            _context.Budgets.Add(budget);
            _context.SaveChanges();

            // Act
            var result = _controller.DeleteBudget(budget.BudgetId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Budget_Index", redirectToActionResult.ActionName);

            // Ensure the budget has been removed from the database
            var deletedBudget = _context.Budgets.SingleOrDefault(b => b.BudgetId == budget.BudgetId);
            Assert.Null(deletedBudget);
        }



        [Fact]
        public async Task AddUser_to_Budget()
        {
            // Arrange
            var user = new User
            {
                Email = "john.doe@example.com",
                First_Name = "John",
                Last_Name = "Doe",
                PasswordHash = "some_hashed_password",
                ResetPasswordToken = Guid.NewGuid().ToString(),
                UserName = "johndoe",
                VerifyUserToken = Guid.NewGuid().ToString()
            };

            var budget = new Budget
            {
                BudgetId = 1,
                Users = new List<User>(),
                BudgetName = "TestBudget"
                // ... set any other properties that are required by your Budget entity
            };
            _context.User.Add(user);
            _context.Budgets.Add(budget);
            _context.SaveChanges();

            var httpContextMock = new DefaultHttpContext();
            httpContextMock.Session = new MockHttpSession();
            httpContextMock.Session.SetInt32("BudgetId", 1); // Mock session to have BudgetId
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContextMock };
            _controller.TempData = new TempDataDictionary(httpContextMock, Mock.Of<ITempDataProvider>());

            var model = new UserBudgetAssignmentViewModel
            {
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var result = _controller.AddUser(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Transaction", redirectToActionResult.ControllerName);

            // Verify user was added to the budget
            budget = _context.Budgets.Include(b => b.Users).First(b => b.BudgetId == 1);
            Assert.Contains(user, budget.Users);
        }




        // A mock HttpSession class to simulate session state
        public class MockHttpSession : ISession
        {
            private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();
            public bool IsAvailable => true;
            public string Id => Guid.NewGuid().ToString();
            public IEnumerable<string> Keys => _sessionStorage.Keys;

            public void Clear() => _sessionStorage.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                // Simulate the async method with a completed task
                return Task.CompletedTask;
            }

            public Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                // Simulate the async method with a completed task
                return Task.CompletedTask;
            }

            public void Remove(string key)
            {
                _sessionStorage.Remove(key);
            }

            public void Set(string key, byte[] value)
            {
                _sessionStorage[key] = value;
            }

            public bool TryGetValue(string key, out byte[] value)
            {
                return _sessionStorage.TryGetValue(key, out value);
            }

            // Additional methods for ISession, if any, go here.
        }

    }
}
