﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjFinal.Controllers
{
    public class PurchaseItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PurchaseItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PurchaseItems.Include(p => p.Book).Include(p => p.Purchase);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PurchaseItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseItem = await _context.PurchaseItems
                .Include(p => p.Book)
                .Include(p => p.Purchase)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseItem == null)
            {
                return NotFound();
            }

            return View(purchaseItem);
        }

        // GET: PurchaseItems/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author");
            ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id");
            return View();
        }

        // POST: PurchaseItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,Quantity,PurchaseId,BookId")] PurchaseItem purchaseItem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(purchaseItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Ocorreu um erro ao adicionar o item à compra.");
                }
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", purchaseItem.BookId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id", purchaseItem.PurchaseId);
            return View(purchaseItem);
        }

        // GET: PurchaseItems/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseItem = await _context.PurchaseItems.FindAsync(id);
            if (purchaseItem == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", purchaseItem.BookId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id", purchaseItem.PurchaseId);
            return View(purchaseItem);
        }

        // POST: PurchaseItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Quantity,PurchaseId,BookId")] PurchaseItem purchaseItem)
        {
            if (id != purchaseItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchaseItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseItemExists(purchaseItem.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Erro ao atualizar o item da compra.");
                }
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", purchaseItem.BookId);
            ViewData["PurchaseId"] = new SelectList(_context.Purchases, "Id", "Id", purchaseItem.PurchaseId);
            return View(purchaseItem);
        }

        // GET: PurchaseItems/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchaseItem = await _context.PurchaseItems
                .Include(p => p.Book)
                .Include(p => p.Purchase)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (purchaseItem == null)
            {
                return NotFound();
            }

            return View(purchaseItem);
        }

        // POST: PurchaseItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var purchaseItem = await _context.PurchaseItems.FindAsync(id);
                if (purchaseItem != null)
                {
                    _context.PurchaseItems.Remove(purchaseItem);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Erro ao eliminar o item da compra.");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseItemExists(int id)
        {
            return _context.PurchaseItems.Any(e => e.Id == id);
        }
    }
}
