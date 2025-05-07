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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookImagesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // POST: BookImages/Create (used from form or AJAX when uploading preview pages)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PageNumber,BookId")] BookImage bookImage, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.ContentType != "image/jpeg" && imageFile.ContentType != "image/png")
            {
                ModelState.AddModelError("", "Tem de submeter uma imagem em formato JPEG ou PNG.");
                return View(bookImage);
            }

            // Guardar o ficheiro
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var savePath = Path.Combine(_webHostEnvironment.WebRootPath, "bookpages");
            Directory.CreateDirectory(savePath);
            var fullPath = Path.Combine(savePath, uniqueFileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            bookImage.Image = Path.Combine("bookpages", uniqueFileName).Replace("\\", "/");

            _context.Add(bookImage);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Books", new { id = bookImage.BookId });
        }

        // POST: BookImages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bookImage = await _context.BookImages.FindAsync(id);
            if (bookImage != null)
            {
                // Apagar ficheiro
                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, bookImage.Image ?? "");
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                _context.BookImages.Remove(bookImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Books", new { id = bookImage?.BookId });
        }
    }
}
