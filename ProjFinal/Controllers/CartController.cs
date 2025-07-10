using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            // Obter items do carrinho da sessão
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
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

            // Obter o carrinho da sessão
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            // Verificar se o livro já está no carrinho
            var existing = cart.FirstOrDefault(c => c.BookId == bookId);
            if (existing != null)
            {
                // Se existir, apenas aumentar a quantidade
                existing.Quantity += quantity;
            }
            else
            {
                // Caso contrário, adicionar novo item
                cart.Add(new CartItemViewModel
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Price = book.Price,
                    Quantity = quantity
                });
            }

            // Atualizar carrinho na sessão
            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Remove um item do carrinho.
        /// </summary>
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            cart.RemoveAll(c => c.BookId == bookId);

            HttpContext.Session.SetObject("Cart", cart);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Finaliza a compra, guardando-a na base de dados e removendo o carrinho.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart");
            if (cart == null || !cart.Any())
                return RedirectToAction("Index");

            // Obter o ID do utilizador autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            // Criar nova compra com os items do carrinho
            var purchase = new Purchase
            {
                ConnectedUserId = userId,
                PurchaseDate = DateTime.Now,
                Status = PurchaseStatus.Completed,
                Items = new List<PurchaseItem>()
            };

            // Criar um objecto item de compra (PurchaseItem) por cada item do carrinho
            foreach (var item in cart)
            {
                purchase.Items.Add(new PurchaseItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            // Remover o carrinho da sessão
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Details", "Purchases", new { id = purchase.Id });
        }
    }
}

