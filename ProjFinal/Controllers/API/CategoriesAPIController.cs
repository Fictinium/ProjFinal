using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;

namespace ProjFinal.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devolve todas as categorias
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories
                .Include(c => c.Books)
                .ToListAsync();
        }

        /// <summary>
        /// Devolve uma categoria específica pelo id
        /// </summary>
        /// <param name="id">Identificador da categoria</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }
    }
}

