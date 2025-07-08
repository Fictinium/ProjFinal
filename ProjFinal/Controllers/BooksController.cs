using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Models;

namespace ProjFinal.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Books.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Categories)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            // Preparar lista de categorias para o <select multiple>
            ViewBag.CategoryList = new MultiSelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");

            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,Description,AuxPrice,PublishedDate,Categories")] Book book, IFormFile bookFile, List<IFormFile> pageImages)
        {
            bool hasError = false;

            // Verifica se o ficheiro foi fornecido
            if (bookFile == null)
            {
                hasError = true;
                ModelState.AddModelError("", "Tem de submeter um ficheiro PDF do livro.");
            }
            else
            {
                // Verifica se o ficheiro é do tipo PDF
                if (bookFile.ContentType != "application/pdf")
                {
                    hasError = true;
                    ModelState.AddModelError("", "O ficheiro submetido tem de ser em formato PDF.");
                }
                else
                {
                    // Geração de um nome único para o ficheiro
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(bookFile.FileName).ToLowerInvariant();

                    // Armazenar o nome do ficheiro na base de dados
                    book.BookFile = uniqueFileName;

                    // Definir a pasta onde o ficheiro será guardado
                    string storageFolder = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Books");

                    // Criar a pasta se não existir
                    if (!Directory.Exists(storageFolder))
                        Directory.CreateDirectory(storageFolder);

                    // Caminho completo do ficheiro
                    string filePath = Path.Combine(storageFolder, uniqueFileName);

                    // Guardar o ficheiro no disco
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await bookFile.CopyToAsync(stream);
                }
            }

            // Remover validação dos campos calculados
            ModelState.Remove("Price");
            ModelState.Remove("FileUrl");

            // Verificar se o modelo é válido e não houve erros manuais
            if (ModelState.IsValid && !hasError)
            {
                // Converter o valor do preço auxiliar para decimal
                book.Price = Convert.ToDecimal(book.AuxPrice.Replace('.', ','), new CultureInfo("pt-PT"));

                // Adicionar o livro à base de dados
                _context.Add(book);
                await _context.SaveChangesAsync();

                // Guardar imagens de páginas, se fornecidas
                if (pageImages != null && pageImages.Any())
                {
                    int pageNumber = 1;
                    string imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "bookpages");

                    if (!Directory.Exists(imageFolder))
                        Directory.CreateDirectory(imageFolder);

                    foreach (var image in pageImages)
                    {
                        if (image.ContentType != "image/jpeg" && image.ContentType != "image/png")
                            continue; // ignorar ficheiros inválidos

                        string imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName).ToLowerInvariant();
                        string imagePath = Path.Combine(imageFolder, imageName);

                        using var imageStream = new FileStream(imagePath, FileMode.Create);
                        await image.CopyToAsync(imageStream);

                        var bookImage = new BookImage
                        {
                            BookId = book.Id,
                            PageNumber = pageNumber++,
                            Image = Path.Combine("bookpages", imageName).Replace("\\", "/")
                        };

                        _context.BookImages.Add(bookImage);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }



        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            // Preencher AuxPrice com base no valor atual de Price
            book.AuxPrice = book.Price.ToString("F2", new CultureInfo("pt-PT"));

            // Preselecionar categorias já associadas
            ViewBag.CategoryList = new MultiSelectList(
                _context.Categories.OrderBy(c => c.Name),
                "Id",
                "Name",
                book.Categories.Select(c => c.Id)
            );

            return View(book);
        }


        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Description,AuxPrice,PublishedDate,FileUrl")] Book book, IFormFile ficheiroLivro, List<int> selectedCategoryIds, List<IFormFile> pageImages)
        {
            if (id != book.Id)
                return NotFound();

            bool hasError = false;

            // Remover validações automáticas
            ModelState.Remove("Price");

            // Validar e converter AuxPrice
            try
            {
                book.Price = Convert.ToDecimal(book.AuxPrice.Replace('.', ','), new CultureInfo("pt-PT"));
            }
            catch
            {
                hasError = true;
                ModelState.AddModelError("AuxPrice", "Formato de preço inválido.");
            }

            // Validar categorias selecionadas
            if (selectedCategoryIds == null || !selectedCategoryIds.Any())
            {
                hasError = true;
                ModelState.AddModelError("", "Tem de escolher pelo menos uma categoria.");
            }

            // Validar e processar novo ficheiro, se enviado
            if (ficheiroLivro != null)
            {
                if (ficheiroLivro.ContentType != "application/pdf")
                {
                    hasError = true;
                    ModelState.AddModelError("", "O ficheiro submetido tem de ser um PDF.");
                }
                else
                {
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(ficheiroLivro.FileName).ToLowerInvariant();
                    string storagePath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Books");

                    if (!Directory.Exists(storagePath))
                        Directory.CreateDirectory(storagePath);

                    string fullPath = Path.Combine(storagePath, newFileName);

                    // Guardar novo ficheiro
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await ficheiroLivro.CopyToAsync(stream);

                    // Apagar o ficheiro anterior (se existir)
                    string oldFilePath = Path.Combine(storagePath, book.BookFile ?? "");
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    // Atualizar o caminho
                    book.BookFile = newFileName;
                }
            }

            if (ModelState.IsValid && !hasError)
            {
                try
                {
                    // Atualizar categorias
                    var existingBook = await _context.Books
                        .Include(b => b.Categories)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (existingBook == null)
                        return NotFound();

                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.Description = book.Description;
                    existingBook.PublishedDate = book.PublishedDate;
                    existingBook.Price = book.Price;
                    if (!string.IsNullOrEmpty(book.BookFile))
                        existingBook.BookFile = book.BookFile;

                    // Substituir categorias
                    existingBook.Categories.Clear();
                    existingBook.Categories = _context.Categories
                        .Where(c => selectedCategoryIds.Contains(c.Id))
                        .ToList();

                    // Substituir imagens de páginas (se forem fornecidas novas)
                    if (pageImages != null && pageImages.Any())
                    {
                        // Apagar imagens antigas do disco
                        foreach (var img in existingBook.Images)
                        {
                            string imgPath = Path.Combine(_webHostEnvironment.WebRootPath, img.Image ?? "");
                            if (System.IO.File.Exists(imgPath))
                                System.IO.File.Delete(imgPath);
                        }

                        // Remover entradas antigas da BD
                        _context.BookImages.RemoveRange(existingBook.Images);

                        // Adicionar novas imagens
                        string imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "bookpages");
                        if (!Directory.Exists(imageFolder))
                            Directory.CreateDirectory(imageFolder);

                        int pageNumber = 1;
                        foreach (var image in pageImages)
                        {
                            if (image.ContentType != "image/jpeg" && image.ContentType != "image/png")
                                continue;

                            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName).ToLowerInvariant();
                            string imagePath = Path.Combine(imageFolder, imageName);

                            using var imageStream = new FileStream(imagePath, FileMode.Create);
                            await image.CopyToAsync(imageStream);

                            var newImage = new BookImage
                            {
                                BookId = existingBook.Id,
                                PageNumber = pageNumber++,
                                Image = Path.Combine("bookpages", imageName).Replace("\\", "/")
                            };

                            _context.BookImages.Add(newImage);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.Id == book.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new MultiSelectList(
                _context.Categories.OrderBy(c => c.Name),
                "Id",
                "Name",
                selectedCategoryIds
            );

            return View(book);
        }


        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Procurar o livro na base de dados
            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            // Eliminar o ficheiro do disco, se existir
            if (!string.IsNullOrEmpty(book.BookFile))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Books", book.BookFile);

                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            // Remover relações com categorias
            book.Categories.Clear();

            // Remover o livro da base de dados
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
