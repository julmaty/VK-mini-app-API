using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VK_mini_app.Models;

namespace VK_mini_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Categories/GetCategories
        [HttpGet]
        [Route("GetCategories")]
        public async Task<ActionResult<List<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // GET: api/Categories/GetCategoriesRus
        [HttpGet]
        [Route("GetCategoriesRus")]
        public async Task<ActionResult<List<Category>>> GetCategoriesRus()
        {
            var categories = await _context.CategoriesRus.ToListAsync();
            return Ok(categories);
        }
    }
}
