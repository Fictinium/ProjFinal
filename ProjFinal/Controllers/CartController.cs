using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Helpers;
using ProjFinal.Models;
using ProjFinal.Models.ViewModels;
using System.Security.Claims;

namespace ProjFinal.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Mostra o conteúdo atual (items) do carrinho.
        /// </summary>
        public IActionResult Index()
        {
            // Obter items do carrinho da sessão (do utilizador)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>($"Cart_{userId}") ?? new List<CartItemViewModel>();
            return View(cart);
        }

        /// <summary>
        /// Adiciona um item ao carrinho (quantidade padrão = 1).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            // Verificar se o livro existe
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("Current IdentityUserId: " + userId);
            var cartKey = $"Cart_{userId}";
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>(cartKey) ?? new List<CartItemViewModel>();

            // Verificar se o livro já está no carrinho
            if (cart.Any(c => c.BookId == bookId))
            {
                TempData["CartMessage"] = "Este livro já está no seu carrinho.";
                return RedirectToAction("BookPage", "Books", new { id = bookId });
            }

            // Verificar se já foi comprado (através do UserProfile)
            var userProfile = await _context.UserProfiles
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (userProfile != null && userProfile.Books.Any(b => b.Id == bookId))
            {
                TempData["CartMessage"] = "Este livro já se encontra na sua biblioteca.";
                return RedirectToAction("BookPage", "Books", new { id = bookId });
            }

            // Adicionar ao carrinho
            cart.Add(new CartItemViewModel
            {
                BookId = book.Id,
                Title = book.Title,
                Price = book.Price,
                Quantity = quantity
            });

            HttpContext.Session.SetObject(cartKey, cart);

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Remove um item do carrinho.
        /// </summary>
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartKey = $"Cart_{userId}";
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>(cartKey) ?? new List<CartItemViewModel>();

            cart.RemoveAll(c => c.BookId == bookId);
            HttpContext.Session.SetObject(cartKey, cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Finaliza a compra, guardando-a na base de dados e removendo o carrinho.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Get user from Identity to ensure FK is valid
            var appUser = await _context.Users.FindAsync(userId);
            if (appUser == null)
                return Unauthorized(); // Or throw exception if it should never happen

            var cartKey = $"Cart_{userId}";
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>(cartKey);
            if (cart == null || !cart.Any())
                return RedirectToAction("Index");

            var purchase = new Purchase
            {
                ConnectedUserId = appUser.Id,
                PurchaseDate = DateTime.Now,
                Status = PurchaseStatus.Completed,
                Items = cart.Select(item => new PurchaseItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            purchase.TotalPrice = purchase.Items.Sum(i => i.Price * i.Quantity);
            _context.Purchases.Add(purchase);

            // Add books to user's library
            var userProfile = await _context.UserProfiles
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (userProfile != null)
            {
                var bookIds = cart.Select(i => i.BookId).ToList();

                var booksToAdd = await _context.Books
                    .Where(b => bookIds.Contains(b.Id))
                    .ToListAsync();

                foreach (var book in booksToAdd)
                {
                    if (!userProfile.Books.Any(b => b.Id == book.Id))
                    {
                        userProfile.Books.Add(book);
                    }
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(cartKey);

            return RedirectToAction("Index", "Library");
        }
    }
}

