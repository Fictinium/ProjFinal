using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;

namespace ProjFinal.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devolve a lista de todos os livros
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books
                .Include(b => b.Categories)
                .ToListAsync();
        }

        /// <summary>
        /// Devolve um livro específico pelo id
        /// </summary>
        /// <param name="id">Identificador do livro</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }
    }
}
