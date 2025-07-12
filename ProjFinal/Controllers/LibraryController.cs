using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;
using System.Security.Claims;

namespace ProjFinal.Controllers
{
    [Authorize]
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string sort = "title")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Busca todos os livros comprados pelo utilizador autenticado
            var query = _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(i => i.Book)
                        .ThenInclude(b => b.Images)
                .Where(p => p.ConnectedUserId == userId && p.Status == PurchaseStatus.Completed)
                .SelectMany(p => p.Items.Select(i => i.Book))
                .Distinct();

            // Aplicar o filtro
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.Categories.Any(c => c.Id == categoryId));
            }

            // Aplicar a ordenação
            query = sort switch
            {
                "price" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                "title_desc" => query.OrderByDescending(b => b.Title),
                _ => query.OrderBy(b => b.Title)
            };

            var books = await query.ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.CurrentSort = sort;
            ViewBag.SelectedCategoryId = categoryId;

            return View(books);
        }
    }
}

