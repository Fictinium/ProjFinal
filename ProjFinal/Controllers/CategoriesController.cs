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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obter todas as categorias ordenadas por nome
                var categorias = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(categorias);
            }
            catch
            {
                // Lidar com falha ao obter dados
                ModelState.AddModelError("", "Erro ao carregar categorias.");
                return View(new List<Category>());
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (category == null)
                    return NotFound();

                return View(category);
            }
            catch
            {
                ModelState.AddModelError("", "Erro ao obter detalhes da categoria.");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Categories/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category)
        {
            // Verificar se o modelo é válido
            if (ModelState.IsValid)
            {
                try
                {
                    // Adicionar a nova categoria à base de dados
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Erro ao criar categoria.");
                }
            }

            // Se houver erros, retornar à view com os dados e mensagens
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                // Procurar a categoria com o ID indicado
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                    return NotFound();

                return View(category);
            }
            catch
            {
                ModelState.AddModelError("", "Erro ao carregar categoria.");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
        {
            if (id != category.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualizar a categoria
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                        return NotFound();
                    else
                        throw;
                }
                catch
                {
                    ModelState.AddModelError("", "Erro ao atualizar categoria.");
                }
            }

            // Se o modelo não for válido, mostrar o formulário novamente
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Books)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (category == null)
                    return NotFound();

                return View(category);
            }
            catch
            {
                ModelState.AddModelError("", "Erro ao carregar categoria para eliminar.");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Books)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                    return NotFound();

                // Verificar se existem livros associados
                if (category.Books.Any())
                {
                    // Se houver livros, não permitir a eliminação
                    ModelState.AddModelError("", "Não é possível eliminar esta categoria porque ainda existem livros associados.");
                    return View(category);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "Erro ao eliminar categoria.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
