using BudgetBuddy.Controllers;
using BudgetBuddy.Data;
using BudgetBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetBuddy.Test
{
    public class CategoryAccess
    {
        private readonly BudgetDbContext _context;
        private readonly CategoryController _controller;

        public CategoryAccess()
        {
            // Set up an in-memory database
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // Make sure the database name is unique for each test method
                .Options;

            _context = new BudgetDbContext(options);

            
            // Initialize the controller with the in-memory context
            _controller = new CategoryController(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public async Task Catergory_IndexPage()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_Index") // Unique name for this in-memory database
                .Options;

            // Insert test data into the in-memory database
            using (var context = new BudgetDbContext(options))
            {
                context.Categories.AddRange(
                    new Categories { CategoryId = 1, Title = "Utilities", Type = "Expense", Icon = "U" },
                    new Categories { CategoryId = 2, Title = "Salary", Type = "Income", Icon = "S" }
                );
                context.SaveChanges();
            }

            // Use a separate instance of the context to verify correct data was saved to the database
            using (var context = new BudgetDbContext(options))
            {
                var controller = new CategoryController(context);

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<Categories>>(viewResult.ViewData.Model);
                var categoriesList = model.ToList();

                // Verifying that the categories were retrieved from the in-memory database
                Assert.Equal(2, categoriesList.Count); // Make sure two categories are returned
                Assert.Equal("Utilities", categoriesList[0].Title); // Validate the data of the first category
                Assert.Equal("Salary", categoriesList[1].Title); // Validate the data of the second category
            }
        }





        [Fact]
        public void Get_Cat_CreateorChange_page()
        {
            // Arrange
            using var context = new BudgetDbContext(new DbContextOptions<BudgetDbContext>());
            var controller = new CategoryController(context);

            // Act
            var result = controller.Cat_CreateorChange();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Categories>(viewResult.ViewData.Model);
        }


        [Fact]
        public void CreateCategory_When_ModelState_is_Valid()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_CreateCategory_Valid")
                .Options;

            using var context = new BudgetDbContext(options);
            var controller = new CategoryController(context);
            var newCategory = new Categories { Title = "Test", Icon = "Icon", Type = "Expense" };

            // Act
            var result = controller.CreateCategory(newCategory);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Category", redirectToActionResult.ControllerName);
        }


        [Fact]
        public void CreateCategory_When_ModelState_is_Invalid()
        {
            // Arrange
            using var context = new BudgetDbContext(new DbContextOptions<BudgetDbContext>());
            var controller = new CategoryController(context);
            controller.ModelState.AddModelError("Title", "Required");
            var newCategory = new Categories { Icon = "Icon", Type = "Expense" };

            // Act
            var result = controller.CreateCategory(newCategory);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Categories>(viewResult.ViewData.Model);
        }


        [Fact]
        public async Task Delete_Category()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_DeleteConfirmed_Found")
                .Options;

            var categoryId = 1;
            using (var context = new BudgetDbContext(options))
            {
                context.Categories.Add(new Categories { CategoryId = categoryId, Title = "Test", Icon = "Icon", Type = "Expense" });
                context.SaveChanges();
            }

            using (var context = new BudgetDbContext(options))
            {
                var controller = new CategoryController(context);

                // Act
                var result = await controller.DeleteConfirmed(categoryId);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectToActionResult.ActionName);
                Assert.Equal("Category", redirectToActionResult.ControllerName);
            }
        }

    }
}
