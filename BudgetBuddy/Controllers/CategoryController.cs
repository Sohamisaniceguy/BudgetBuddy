using BudgetBuddy.Data;
using BudgetBuddy.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BudgetBuddy.Controllers
{
    public class CategoryController: Controller
    {
        private readonly BudgetDbContext _dbcontext;

        public CategoryController(BudgetDbContext dbcontext)      //Constructor 
        {
            _dbcontext = dbcontext;
        }

        // Get: Category
        public async Task<IActionResult> Index()
        {
            return (IActionResult)(_dbcontext.Categories != null ?
                View(await _dbcontext.Categories.ToListAsync()) :
                Problem("Entity set 'BudgetDbContext.Categories' is null"));
        }

        // GET: Category/Create
        public IActionResult Cat_CreateorChange(int id = 0)
        {
            if (id == 0)
                return View(new Categories());
            else
                return View(_dbcontext.Categories.Find(id));
        }
        
        [HttpPost]
        public IActionResult CreateCategory(Categories model)  
        {
            if (ModelState.IsValid)
            {
                // Save the model to the database
                _dbcontext.Categories.Add(model);  
                _dbcontext.SaveChanges();

                return RedirectToAction("Index", "Category");  
            }

            
            return View("Create", model);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_dbcontext.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _dbcontext.Categories.FindAsync(id);
            if (category != null)
            {
                _dbcontext.Categories.Remove(category);
            }

            await _dbcontext.SaveChangesAsync();
            return RedirectToAction("Index", "Category");
        }

        private ViewResult Problem(string v)
        {
            throw new NotImplementedException();
        }
    }
}
