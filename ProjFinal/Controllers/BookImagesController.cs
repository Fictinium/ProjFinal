using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "admin")]
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

            try
            {
                // Guardar o ficheiro
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var savePath = Path.Combine(_webHostEnvironment.WebRootPath, "bookpages");
                Directory.CreateDirectory(savePath); // Safe even if it exists

                var fullPath = Path.Combine(savePath, uniqueFileName);
                using var stream = new FileStream(fullPath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                // Salvar no banco de dados
                bookImage.Image = Path.Combine("bookpages", uniqueFileName).Replace("\\", "/");
                _context.Add(bookImage);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Books", new { id = bookImage.BookId });
            }
            catch (IOException ex)
            {
                ModelState.AddModelError("", "Erro ao guardar o ficheiro de imagem. Verifique permissões e espaço em disco.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro inesperado ao criar imagem.");
            }

            return View(bookImage);
        }

        // POST: BookImages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var bookImage = await _context.BookImages.FindAsync(id);
            if (bookImage != null)
            {
                try
                {
                    // Apagar ficheiro
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, bookImage.Image ?? "");
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);

                    // Remover da base de dados
                    _context.BookImages.Remove(bookImage);
                    await _context.SaveChangesAsync();
                }
                catch (IOException ex)
                {
                    ModelState.AddModelError("", "Erro ao apagar o ficheiro do disco.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Erro inesperado ao apagar imagem.");
                }
            }

            return RedirectToAction("Details", "Books", new { id = bookImage?.BookId });
        }
    }
}
