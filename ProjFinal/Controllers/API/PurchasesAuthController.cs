using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;

namespace ProjFinal.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PurchasesAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchasesAuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devolve todas as compras (admin)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Purchase>>> GetPurchases()
        {
            return await _context.Purchases
                .Include(p => p.Items)
                    .ThenInclude(i => i.Book)
                .Include(p => p.ConnectedUserId)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza o estado de uma compra
        /// </summary>
        /// <param name="id">Id da compra</param>
        /// <param name="purchase">Dados atualizados (principalmente Status)</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPurchase(int id, Purchase purchase)
        {
            if (id != purchase.Id)
            {
                return BadRequest();
            }

            _context.Entry(purchase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseExists(id))
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

        private bool PurchaseExists(int id)
        {
            return _context.Purchases.Any(e => e.Id == id);
        }
    }
}

