using Microsoft.AspNetCore.Identity;
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
    public class UserProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserProfiles
        public async Task<IActionResult> Index()
        {
            var profiles = await _context.UserProfiles.Include(u => u.ApplicationUser).ToListAsync();
            return View(profiles);
        }

        // GET: UserProfiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Procurar o UserProfile, incluindo dados do ApplicationUser
            var userProfile = await _context.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (userProfile == null)
                return NotFound();

            return View(userProfile);
        }


        // GET: UserProfiles/Create
        public IActionResult Create()
        {
            return View();
        }

        /// POST: UserProfiles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Password")] UserProfile userProfile)
        {
            // Validar modelo recebido
            if (ModelState.IsValid)
            {
                // Criar ApplicationUser (Identity)
                var applicationUser = new ApplicationUser
                {
                    UserName = userProfile.Email,
                    Email = userProfile.Email,
                    FullName = userProfile.Name,
                    EmailConfirmed = true // para testes, confirmar automaticamente
                };

                // Criar utilizador no Identity
                var result = await _userManager.CreateAsync(applicationUser, userProfile.Password);

                if (result.Succeeded)
                {
                    // Associar IdentityUserId ao UserProfile
                    userProfile.IdentityUserId = applicationUser.Id;

                    try
                    {
                        // Guardar UserProfile na base de dados
                        _context.Add(userProfile);
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        // Se ocorrer erro ao guardar, remover utilizador Identity criado
                        await _userManager.DeleteAsync(applicationUser);
                        ModelState.AddModelError("", "Erro ao guardar o perfil de utilizador: " + ex.Message);
                    }
                }
                else
                {
                    // Adicionar erros de criação do Identity à ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            // Se algo falhar, voltar à view com mensagens de erro
            return View(userProfile);
        }


        // GET: UserProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            // Procurar o UserProfile com o ID indicado
            var userProfile = await _context.UserProfiles.FindAsync(id);

            if (userProfile == null)
                return NotFound();

            return View(userProfile);
        }


        // POST: UserProfiles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,IdentityUserId")] UserProfile userProfile)
        {
            if (id != userProfile.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualizar ApplicationUser associado, se necessário
                    var applicationUser = await _userManager.FindByIdAsync(userProfile.IdentityUserId);

                    if (applicationUser != null)
                    {
                        applicationUser.FullName = userProfile.Name;
                        applicationUser.Email = userProfile.Email;
                        applicationUser.UserName = userProfile.Email;

                        var updateResult = await _userManager.UpdateAsync(applicationUser);

                        if (!updateResult.Succeeded)
                        {
                            foreach (var error in updateResult.Errors)
                                ModelState.AddModelError("", error.Description);

                            return View(userProfile);
                        }

                        // Atualizar password, se fornecida
                        if (!string.IsNullOrEmpty(userProfile.Password))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(applicationUser);
                            var passwordResult = await _userManager.ResetPasswordAsync(applicationUser, token, userProfile.Password);

                            if (!passwordResult.Succeeded)
                            {
                                foreach (var error in passwordResult.Errors)
                                    ModelState.AddModelError("", error.Description);

                                return View(userProfile);
                            }
                        }
                    }

                    // Atualizar UserProfile na base de dados
                    _context.Update(userProfile);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserProfileExists(userProfile.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Se o modelo não for válido, mostrar novamente a view
            return View(userProfile);
        }

        // GET: UserProfiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var userProfile = await _context.UserProfiles
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (userProfile == null)
                return NotFound();

            return View(userProfile);
        }


        // POST: UserProfiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var userProfile = await _context.UserProfiles
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (userProfile != null)
                {
                    if (!string.IsNullOrEmpty(userProfile.IdentityUserId))
                    {
                        var applicationUser = await _userManager.FindByIdAsync(userProfile.IdentityUserId);
                        if (applicationUser != null)
                            await _userManager.DeleteAsync(applicationUser);
                    }

                    _context.UserProfiles.Remove(userProfile);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao eliminar o perfil: " + ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }


        private bool UserProfileExists(int id)
        {
            return _context.UserProfiles.Any(e => e.Id == id);
        }
    }
}
