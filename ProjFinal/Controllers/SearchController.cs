using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;
using ProjFinal.Models.ViewModels;

namespace ProjFinal.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? query, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var booksQuery = _context.Books
                .Include(b => b.Categories)
                .Include(b => b.Images)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                booksQuery = booksQuery.Where(b => b.Title.Contains(query));
            }

            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.Categories.Any(c => c.Id == categoryId));
            }

            if (minPrice.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.Price <= maxPrice.Value);
            }

            var viewModel = new SearchViewModel
            {
                SearchTerm = query,
                SelectedCategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync(),
                Results = await booksQuery.ToListAsync()
            };

            return View(viewModel);
        }
    }
}

