using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;
using System.Security.Claims;

namespace ProjFinal.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PurchasesAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchasesAPIController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Devolve o histórico de compras do utilizador autenticado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Purchase>>> GetUserPurchases()
        {
            // Obter o ID do utilizador autenticado a partir do token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var purchases = await _context.Purchases
                .Where(p => p.ConnectedUserId == userId)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Book)
                .ToListAsync();

            return purchases;
        }

        /// <summary>
        /// Devolve detalhes de uma compra específica do utilizador autenticado
        /// </summary>
        /// <param name="id">Id da compra</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Purchase>> GetPurchase(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var purchase = await _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(i => i.Book)
                .FirstOrDefaultAsync(p => p.Id == id && p.ConnectedUserId == userId);

            if (purchase == null)
            {
                return NotFound();
            }

            return purchase;
        }
    }
}
