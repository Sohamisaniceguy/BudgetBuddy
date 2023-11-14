using BudgetBuddy.Data;
using BudgetBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BudgetBuddy.Controllers
{
    public class CategoryController: Controller
    {
        private readonly BudgetDbContext _dbcontext;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(BudgetDbContext dbcontext, ILogger<CategoryController> logger)      //Constructor 
        {
            _dbcontext = dbcontext;
            _logger = logger;
        }

       

        // Get: Category
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return (IActionResult)(_dbcontext.Categories != null ?
                View(await _dbcontext.Categories.ToListAsync()) :
                Problem("Entity set 'BudgetDbContext.Categories' is null"));
        }

        // GET: Category/Create
        [HttpGet]
        public IActionResult Cat_CreateorChange(int id = 0)
        {
                return View(new Categories());
            
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Categories model)  
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Attempting to create a new category."); // Log information

                _dbcontext.Categories.Add(model);
                _dbcontext.SaveChanges();

                _logger.LogInformation($"Category created successfully: {model.Title}"); // Log success

                return RedirectToAction("Index", "Category");
            }

            _logger.LogWarning("Model state is invalid. Redirecting to Cat_CreateorChange view."); 

            return View("Cat_CreateorChange", model);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation($"Initiating deletion of category with ID: {id}");
            if (_dbcontext.Categories == null)
            {
                _logger.LogError("Entity set 'ApplicationDbContext.Categories' is null.");
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _dbcontext.Categories.FindAsync(id);
            if (category != null)
            {
                _dbcontext.Categories.Remove(category);
                await _dbcontext.SaveChangesAsync();
                _logger.LogInformation($"Category with ID: {id} deleted successfully."); // Log successful deletion
            }
            else
            {
                _logger.LogWarning($"Category with ID: {id} not found. Deletion could not be performed."); // Log warning for not found
            }

            return RedirectToAction("Index", "Category");
        }

        private ViewResult Problem(string v)
        {
            throw new NotImplementedException();
        }
    }
}
