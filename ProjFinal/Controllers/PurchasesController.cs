using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;
using System.Security.Claims;

namespace ProjFinal.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Purchase/Index
        public async Task<IActionResult> Index()
        {
            // Obter todas as compras com os livros incluídos
            var purchases = await _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(i => i.Book)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // GET: Purchase/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
                return NotFound();

            return View(purchase);
        }

        // GET: Purchase/Create
        public IActionResult Create()
        {
            // Para simular o carrinho: mostrar lista de livros para selecionar
            ViewBag.Books = new SelectList(_context.Books.OrderBy(b => b.Title), "Id", "Title");
            return View();
        }

        // POST: Purchase/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<int> bookIds, List<int> quantities)
        {
            if (bookIds == null || !bookIds.Any() || quantities == null || bookIds.Count != quantities.Count)
            {
                ModelState.AddModelError("", "Tem de selecionar pelo menos um livro com quantidade válida.");
                ViewBag.Books = new SelectList(_context.Books.OrderBy(b => b.Title), "Id", "Title");
                return View();
            }

            // Obter o id do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var purchase = new Purchase
            {
                PurchaseDate = DateTime.Now,
                Status = PurchaseStatus.Pending,
                Items = new List<PurchaseItem>(),
                ConnectedUserId = userId
            };

            for (int i = 0; i < bookIds.Count; i++)
            {
                var bookId = bookIds[i];
                var quantity = quantities[i];

                if (quantity <= 0)
                    continue;

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                    continue;

                var item = new PurchaseItem
                {
                    BookId = book.Id,
                    Quantity = quantity,
                    Price = book.Price
                };

                purchase.Items.Add(item);
            }

            if (!purchase.Items.Any())
            {
                ModelState.AddModelError("", "Nenhum item válido foi adicionado à compra.");
                ViewBag.Books = new SelectList(_context.Books.OrderBy(b => b.Title), "Id", "Title");
                return View();
            }

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Purchase/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var purchase = await _context.Purchases
                .Include(p => p.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
                return NotFound();

            // Preparar dropdown com os valores do enum PurchaseStatus
            ViewBag.StatusList = Enum.GetValues(typeof(PurchaseStatus))
                                     .Cast<PurchaseStatus>()
                                     .Select(s => new SelectListItem
                                     {
                                         Value = s.ToString(),
                                         Text = s.ToString()
                                     })
                                     .ToList();

            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseStatus Status)
        {
            var purchase = await _context.Purchases.FindAsync(id);

            if (purchase == null)
                return NotFound();

            // Atualizar apenas o estado da compra
            purchase.Status = Status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Purchases/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase != null)
            {
                _context.Purchases.Remove(purchase);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchases.Any(e => e.Id == id);
        }
    }
}
