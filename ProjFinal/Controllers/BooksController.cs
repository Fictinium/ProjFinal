using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjFinal.Data;
using ProjFinal.Helpers;
using ProjFinal.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
        // Anotação não recomendada para produção!!
        // [DisableRequestSizeLimit]
        public async Task<IActionResult> Create([Bind("Title,Author,Description,AuxPrice,PublishedDate")] Book book,
                                        IFormFile BookFile,
                                        List<IFormFile> pageImages,
                                        List<int> selectedCategoryIds)
        {
            bool hasError = false;

            // Validar se o ficheiro foi fornecido
            if (BookFile == null || BookFile.ContentType != "application/pdf")
            {
                hasError = true;
                ModelState.AddModelError("", "Tem de submeter um ficheiro PDF do livro.");
            }

            // Remover validação de propriedades calculadas
            ModelState.Remove("Price");
            ModelState.Remove("FileUrl");

            // Verificar se o modelo é válido e não houve erro manual
            if (ModelState.IsValid && !hasError)
            {
                try
                {
                    // Converter AuxPrice para decimal com cultura PT
                    book.Price = Convert.ToDecimal(book.AuxPrice.Replace('.', ','), new CultureInfo("pt-PT"));
                }
                catch
                {
                    hasError = true;
                    ModelState.AddModelError("AuxPrice", "Preço inválido.");
                }
            }

            if (ModelState.IsValid && !hasError)
            {
                if (selectedCategoryIds == null || !selectedCategoryIds.Any())
                {
                    hasError = true;
                    ModelState.AddModelError("", "Tem de escolher pelo menos uma categoria.");
                }
                else
                {
                    book.Categories = await _context.Categories
                        .Where(c => selectedCategoryIds.Contains(c.Id))
                        .ToListAsync();
                }
                if (hasError)
                {
                    // Em caso de erro, voltar a recarregar a lista de categorias
                    ViewBag.CategoryList = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
                    return View(book);
                }

                try
                {
                    // Guardar o livro na base de dados (ainda sem caminho do ficheiro)
                    _context.Add(book);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    ModelState.AddModelError("", "Erro ao guardar o livro na base de dados.");
                    return View(book);
                }

                // Criar pasta única para o livro com base no título + GUID
                string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                string sanitizedTitle = string.Concat(book.Title
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
                    .Replace(" ", "-")
                    .ToLowerInvariant();

                string folderName = $"{sanitizedTitle}-{uniqueId}";
                string bookFolder = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Books", folderName);
                string imageFolder = Path.Combine(bookFolder, "BookImages");

                try
                {
                    Directory.CreateDirectory(bookFolder);
                    Directory.CreateDirectory(imageFolder);
                }
                catch
                {
                    ModelState.AddModelError("", "Erro ao criar diretórios no servidor.");
                    return View(book);
                }

                // Guardar PDF com nome original (normalizado)
                try
                {
                    string extension = Path.GetExtension(BookFile.FileName);
                    string safePdfName = $"book_{Guid.NewGuid()}{extension}";
                    string pdfPath = Path.Combine(bookFolder, safePdfName);

                    using var pdfStream = new FileStream(pdfPath, FileMode.Create);
                    await BookFile.CopyToAsync(pdfStream);

                    book.BookFile = Path.Combine("Books", folderName, safePdfName).Replace("\\", "/");

                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    ModelState.AddModelError("", "Erro ao guardar o ficheiro PDF.");
                    return View(book);
                }

                // Guardar imagens (se existirem)
                if (pageImages != null && pageImages.Any())
                {
                    int pageNumber = 1;

                    foreach (var image in pageImages)
                    {
                        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                            continue;

                        string imageName = $"page_{pageNumber}{ext}";
                        string imagePath = Path.Combine(imageFolder, imageName);

                        try
                        {
                            using var imgStream = new FileStream(imagePath, FileMode.Create);
                            await image.CopyToAsync(imgStream);
                        }
                        catch
                        {
                            continue;
                        }

                        var bookImage = new BookImage
                        {
                            BookId = book.Id,
                            PageNumber = pageNumber++,
                            Image = Path.Combine("Books", sanitizedTitle, "BookImages", imageName).Replace("\\", "/")
                        };

                        _context.BookImages.Add(bookImage);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Erro ao guardar imagens na base de dados.");
                        return View(book);
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Description,AuxPrice,PublishedDate,FileUrl")] Book book, IFormFile ficheiroLivro, List<int> selectedCategoryIds, List<IFormFile> pageImages)
        {
            if (id != book.Id)
                return NotFound();

            bool hasError = false;

            // Remover validação automática para preço
            ModelState.Remove("Price");

            // Tentar converter o preço auxiliar
            try
            {
                book.Price = Convert.ToDecimal(book.AuxPrice.Replace('.', ','), new CultureInfo("pt-PT"));
            }
            catch
            {
                hasError = true;
                ModelState.AddModelError("AuxPrice", "Formato de preço inválido.");
            }

            // Verificar se foram selecionadas categorias
            if (selectedCategoryIds == null || !selectedCategoryIds.Any())
            {
                hasError = true;
                ModelState.AddModelError("", "Tem de escolher pelo menos uma categoria.");
            }

            // Obter o livro atual da base de dados com imagens e categorias
            var existingBook = await _context.Books
                .Include(b => b.Categories)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBook == null)
                return NotFound();

            // Criar estrutura de pastas com base no nome do livro
            string bookFolder = Path.Combine("PrivateFiles", "Books", FileHelpers.SanitizeFolderName(book.Title));
            string fullBookFolderPath = Path.Combine(Directory.GetCurrentDirectory(), bookFolder);
            string imageFolderPath = Path.Combine(fullBookFolderPath, "BookImages");

            if (!Directory.Exists(fullBookFolderPath))
                Directory.CreateDirectory(fullBookFolderPath);

            if (!Directory.Exists(imageFolderPath))
                Directory.CreateDirectory(imageFolderPath);

            // Se for enviado novo ficheiro PDF
            if (ficheiroLivro != null)
            {
                if (ficheiroLivro.ContentType != "application/pdf")
                {
                    hasError = true;
                    ModelState.AddModelError("", "O ficheiro submetido tem de ser um PDF.");
                }
                else
                {
                    try
                    {
                        // Caminho para o novo ficheiro com nome original
                        string pdfPath = Path.Combine(fullBookFolderPath, Path.GetFileName(ficheiroLivro.FileName));

                        // Apagar ficheiro anterior (se existir)
                        if (!string.IsNullOrEmpty(existingBook.BookFile))
                        {
                            string oldPath = Path.Combine(fullBookFolderPath, existingBook.BookFile);
                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        // Guardar novo ficheiro no disco
                        using var stream = new FileStream(pdfPath, FileMode.Create);
                        await ficheiroLivro.CopyToAsync(stream);

                        // Atualizar nome do ficheiro guardado
                        existingBook.BookFile = Path.GetFileName(ficheiroLivro.FileName);
                    }
                    catch
                    {
                        hasError = true;
                        ModelState.AddModelError("", "Erro ao guardar o novo ficheiro do livro.");
                    }
                }
            }

            // Se tudo estiver válido, proceder com atualização
            if (ModelState.IsValid && !hasError)
            {
                try
                {
                    // Atualizar campos básicos
                    existingBook.Title = book.Title;
                    existingBook.Author = book.Author;
                    existingBook.Description = book.Description;
                    existingBook.PublishedDate = book.PublishedDate;
                    existingBook.Price = book.Price;

                    // Atualizar categorias selecionadas
                    existingBook.Categories.Clear();
                    existingBook.Categories = _context.Categories
                        .Where(c => selectedCategoryIds.Contains(c.Id))
                        .ToList();

                    // Substituir imagens se forem fornecidas novas
                    if (pageImages != null && pageImages.Any())
                    {
                        // Apagar imagens antigas do disco
                        foreach (var img in existingBook.Images)
                        {
                            string imgPath = Path.Combine(_webHostEnvironment.WebRootPath, img.Image ?? "");
                            if (System.IO.File.Exists(imgPath))
                                System.IO.File.Delete(imgPath);
                        }

                        // Remover imagens da base de dados
                        _context.BookImages.RemoveRange(existingBook.Images);

                        int pageNumber = 1;
                        foreach (var image in pageImages)
                        {
                            // Verificar tipo da imagem
                            if (image.ContentType != "image/jpeg" && image.ContentType != "image/png")
                                continue;

                            // Gerar nome e caminho do ficheiro
                            string imgName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName).ToLowerInvariant();
                            string imgSavePath = Path.Combine(imageFolderPath, imgName);

                            // Guardar imagem no disco
                            using var imageStream = new FileStream(imgSavePath, FileMode.Create);
                            await image.CopyToAsync(imageStream);

                            // Criar entrada para a base de dados
                            var newImage = new BookImage
                            {
                                BookId = existingBook.Id,
                                PageNumber = pageNumber++,
                                Image = Path.Combine(bookFolder, "BookImages", imgName).Replace("\\", "/")
                            };

                            _context.BookImages.Add(newImage);
                        }
                    }

                    // Guardar todas as alterações
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Books.Any(e => e.Id == book.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Recarregar categorias selecionadas em caso de erro
            ViewBag.CategoryList = new MultiSelectList(
                _context.Categories.OrderBy(c => c.Name),
                "Id",
                "Name",
                selectedCategoryIds
            );

            return View(book);
        }


        // GET: Books/Delete/5
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Procurar o livro na base de dados com categorias e imagens incluídas
            var book = await _context.Books
                .Include(b => b.Categories)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
                return NotFound();

            // Gerar o caminho da pasta do livro
            string sanitizedFolder = FileHelpers.SanitizeFolderName(book.Title);
            string bookFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "PrivateFiles", "Books", sanitizedFolder);

            try
            {
                // Eliminar ficheiro PDF, se existir
                if (!string.IsNullOrEmpty(book.BookFile))
                {
                    string pdfPath = Path.Combine(bookFolderPath, book.BookFile);
                    if (System.IO.File.Exists(pdfPath))
                        System.IO.File.Delete(pdfPath);
                }

                // Eliminar imagens de páginas, se existirem
                string imagesFolder = Path.Combine(bookFolderPath, "BookImages");
                if (Directory.Exists(imagesFolder))
                {
                    foreach (var image in Directory.GetFiles(imagesFolder))
                    {
                        System.IO.File.Delete(image);
                    }
                }

                // Eliminar a pasta inteira do livro, se existir
                if (Directory.Exists(bookFolderPath))
                {
                    Directory.Delete(bookFolderPath, true); // true = recursive
                }
            }
            catch
            {
                // Se algo falhar ao eliminar os ficheiros, não parar o processo
            }

            // Remover ligações com categorias
            book.Categories.Clear();

            try
            {
                // Remover da base de dados (livro e imagens associadas via cascade)
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "Erro ao eliminar o livro da base de dados.");
                return View(book);
            }

            return RedirectToAction(nameof(Index));
        }


        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
