using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;

namespace ProjFinal.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public class CategoriesAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesAuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona uma nova categoria
        /// </summary>
        /// <param name="category">Dados da categoria</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        /// <summary>
        /// Edita uma categoria existente
        /// </summary>
        /// <param name="id">Id da categoria</param>
        /// <param name="category">Dados atualizados</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Apaga uma categoria
        /// </summary>
        /// <param name="id">Id da categoria</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Devolve uma categoria específica pelo id (admin)
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

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}

