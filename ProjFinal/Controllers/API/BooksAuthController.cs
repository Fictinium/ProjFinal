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
    public class BooksAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BooksAuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona um novo livro
        /// </summary>
        /// <param name="book">Dados do livro</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        /// <summary>
        /// Edita um livro existente
        /// </summary>
        /// <param name="id">Id do livro</param>
        /// <param name="book">Dados atualizados</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            if (id != book.Id)
            {
                return BadRequest();
            }

            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
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
        /// Apaga um livro
        /// </summary>
        /// <param name="id">Id do livro</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Devolve um livro específico pelo id (admin)
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

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}

