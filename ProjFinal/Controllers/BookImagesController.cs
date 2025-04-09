using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data.YourProjectNamespace.Data;
using ProjFinal.Models;

namespace ProjFinal.Controllers
{
    public class BookImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BookImages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BookImages.Include(b => b.Book);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BookImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookImage = await _context.BookImages
                .Include(b => b.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookImage == null)
            {
                return NotFound();
            }

            return View(bookImage);
        }

        // GET: BookImages/Create
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author");
            return View();
        }

        // POST: BookImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Image,PageNumber,BookId")] BookImage bookImage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bookImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", bookImage.BookId);
            return View(bookImage);
        }

        // GET: BookImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookImage = await _context.BookImages.FindAsync(id);
            if (bookImage == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", bookImage.BookId);
            return View(bookImage);
        }

        // POST: BookImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Image,PageNumber,BookId")] BookImage bookImage)
        {
            if (id != bookImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookImageExists(bookImage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "Id", "Author", bookImage.BookId);
            return View(bookImage);
        }

        // GET: BookImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookImage = await _context.BookImages
                .Include(b => b.Book)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bookImage == null)
            {
                return NotFound();
            }

            return View(bookImage);
        }

        // POST: BookImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookImage = await _context.BookImages.FindAsync(id);
            if (bookImage != null)
            {
                _context.BookImages.Remove(bookImage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookImageExists(int id)
        {
            return _context.BookImages.Any(e => e.Id == id);
        }
    }
}
