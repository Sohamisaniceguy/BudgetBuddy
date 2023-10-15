using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BudgetBuddy.Test
{
    public class HomeAccess
    {
        private readonly HomeController _homeController;
        

        public HomeAccess()
        {
            var loggerMock = new Mock<ILogger<HomeController>>();
            _homeController = new HomeController(loggerMock.Object);


            

            
        }

        [Fact]
        public void HomeController_HomeAccess()
        {
            // Act
            var result = _homeController.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        
    }
}
