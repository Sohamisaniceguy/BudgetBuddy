using BudgetBuddy.Data;
using BudgetBuddy.Service;

using Expense_Tracker.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetBuddy;
using BudgetBuddy.Controllers;
using BudgetBuddy.Models;
using Microsoft.AspNetCore.Http;

namespace BudgetBuddy.Test
{
    public class TransactionControllerTests
    {
        private readonly TransactionController _controller;
        private readonly Mock<IUserService> _mockUserService;
        private readonly DbContextOptions<BudgetDbContext> _dbOptions;

        public TransactionControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            var mockLogger = new Mock<ILogger<TransactionController>>();

            _dbOptions = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new BudgetDbContext(_dbOptions);

            _controller = new TransactionController(_mockUserService.Object, context, mockLogger.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfTransactions() //Problem
        {
            // Arrange
            int budgetId = 1;
            using (var context = new BudgetDbContext(_dbOptions))
            {
                context.Transactions.Add(new Transaction { /* Initialize properties, e.g., Id, Amount, etc. */ });
                context.SaveChanges();
            }

            // Act
            var result = await _controller.Index(budgetId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Transaction>>(viewResult.Model);
            Assert.NotEmpty(model);
        }

        [Fact]
        public async Task CreateorChange_PostsValidTransaction_AndRedirects() //PROBLEM
        {
            // Arrange
            var newTransaction = new Transaction
            {
                TransactionId = 1,
                BudgetId = 1,
                CategoryId = 1,
                UserId = 1, // Assuming UserId is a string as it usually represents the user identifier
                Amount = 100.00m,
                Date = DateTime.Now,
                Description = "Example transaction description"
            };

            // Mock HttpContext and Session
            var session = new Mock<ISession>();
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(ctx => ctx.Session).Returns(session.Object);

            // Mock the session to return the BudgetId and UserId
            byte[] budgetIdBytes = BitConverter.GetBytes(newTransaction.BudgetId);
            byte[] userIdBytes = BitConverter.GetBytes(newTransaction.UserId); 

            session.Setup(_ => _.TryGetValue("BudgetId", out budgetIdBytes)).Returns(true);
            session.Setup(_ => _.TryGetValue("UserId", out userIdBytes)).Returns(true);



            // Assign the mocked HttpContext to the controller
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext.Object
            };

            // Assume the ModelState is valid
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.CreateorChange(newTransaction);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            using (var context = new BudgetDbContext(_dbOptions))
            {
                Assert.Equal(1, context.Transactions.Count()); // Assuming only one transaction should be in the database
                Assert.Contains(context.Transactions, t => t.TransactionId == newTransaction.TransactionId);
            }
        }


        [Fact]
        public async Task DeleteConfirmed_DeletesTransaction_AndRedirects()
        {
            // Arrange
            var transactionId = 1;
            using (var context = new BudgetDbContext(_dbOptions))
            {
                context.Transactions.Add(new Transaction { TransactionId = transactionId, /* Initialize other properties */ });
                context.SaveChanges();
            }

            // Act
            var result = await _controller.DeleteConfirmed(transactionId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            using (var context = new BudgetDbContext(_dbOptions))
            {
                Assert.DoesNotContain(context.Transactions, t => t.TransactionId == transactionId);
            }
        }

        // Additional tests for other methods and failure scenarios...
    }

}
