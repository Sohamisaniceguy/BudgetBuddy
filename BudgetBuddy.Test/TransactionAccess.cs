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
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BudgetBuddy.Test
{
    public class TransactionControllerTests
    {
        private readonly DbContextOptions<BudgetDbContext> _dbContextOptions;
        private readonly Mock<ILogger<TransactionController>> _mockLogger;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ISession> _mockSession;

        public TransactionControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryTransactionDb")
                .Options;

            _mockLogger = new Mock<ILogger<TransactionController>>();

            _mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);
        }
        private void SeedDatabase()
        {
            using var context = new BudgetDbContext(_dbContextOptions);

            // Seed Users
            var user1 = new User { 
                UserId = 1, 
                First_Name = "John", 
                Last_Name = "Doe", 
                Email = "john.doe@example.com",
                PasswordHash = "hashed_password", 
                ResetPasswordToken = "reset_token", 
                UserName = "john.doe",
                VerifyUserToken = "verify_token" 
            };
            var user2 = new User { 
                UserId = 2, 
                First_Name = "Jane", 
                Last_Name = "Doe", 
                Email = "jane.doe@example.com",
                PasswordHash = "hashed_password", 
                ResetPasswordToken = "reset_token", 
                UserName = "jane.doe",
                VerifyUserToken = "verify_token" 
            };
            context.User.AddRange(user1, user2);

            // Seed Budgets
            var budget1 = new Budget { BudgetId = 1, BudgetName = "Test Budget 1", BudgetLimit = 1000m, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1) /* other properties */ };
            var budget2 = new Budget { BudgetId = 2, BudgetName = "Test Budget 2", BudgetLimit = 2000m, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(2) /* other properties */ };
            context.Budgets.AddRange(budget1, budget2);
                
            // Assign Users to Budgets
            budget1.Users.Add(user1);
            budget2.Users.Add(user2);

            // Seed Transactions
            var transaction1 = new Transaction { TransactionId = 1, BudgetId = 1, CategoryId = 1, UserId = 1, Amount = 100.00m, Date = DateTime.Now };
            var transaction2 = new Transaction { TransactionId = 2, BudgetId = 2, CategoryId = 2, UserId = 2, Amount = 200.00m, Date = DateTime.Now.AddDays(-1) };
            context.Transactions.AddRange(transaction1, transaction2);

            // Seed Categories if necessary

            context.SaveChanges();
        }



        [Fact]
        public async Task Create_New_Transaction()
        {
            using var context = new BudgetDbContext(_dbContextOptions);
            SeedDatabase();
            var controller = new TransactionController(context, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };

            // Set up session values
            var sessionItems = new Dictionary<string, byte[]>();
            sessionItems["BudgetId"] = BitConverter.GetBytes(1); // Simulated session BudgetId
            sessionItems["UserID"] = BitConverter.GetBytes(1); // Simulated session UserId
            _mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                 .Returns(new Func<string, byte[], bool>((key, value) =>
                 {
                     if (sessionItems.ContainsKey(key))
                     {
                         value = sessionItems[key];
                         return true;
                     }
                     value = null;
                     return false;
                 }));

            var newTransaction = new Transaction { TransactionId = 3, BudgetId = 1, CategoryId = 1, UserId = 1, Amount = 100.00m, Date = DateTime.Now };

            //Act
            var result = await controller.CreateorChange(newTransaction);

            //Assert
            Assert.IsType<ViewResult>(result);
            Assert.NotEmpty(context.Transactions);
        }



       


        [Fact]
        public async Task DeleteConfirmed_DeletesTransaction_AndRedirects()
        {
            // Arrange
            using var context = new BudgetDbContext(_dbContextOptions);
            SeedDatabase();
            var controller = new TransactionController(context, _mockLogger.Object);

            int transactionIdToDelete = 1; // Assuming this ID exists in the seeded data

            // Act
            var result = await controller.DeleteConfirmed(transactionIdToDelete);

            // Assert
            var deletedTransaction = context.Transactions.Find(transactionIdToDelete);
            Assert.Null(deletedTransaction); // Transaction should be deleted
            Assert.IsType<RedirectToActionResult>(result); // Should redirect to Index

            var redirectToActionResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        // Additional tests for other methods and failure scenarios...
    }

}
