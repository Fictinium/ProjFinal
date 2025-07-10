using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjFinal.Models;
using ProjFinal.Services;
using ProjFinal.Models.ViewModels;


namespace ProjFinal.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // procura pelo 'username' na base de dados, 
            // para determinar se o utilizador existe
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized("Credenciais inválidas.");

            // se chego aqui, é pq o 'username' existe
            // mas, a password é válida?
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Credenciais inválidas.");

            // houve sucesso na autenticação
            // vou gerar o 'token', associado ao utilizador
            var token = _tokenService.GenerateToken(user);

            // devolvo o 'token'
            return Ok(new { token });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validar dados de entrada
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar se o email já está em uso
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Este email já está registado." });

            // Criar novo ApplicationUser
            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                EmailConfirmed = true // para testes; em produção deverá enviar email de confirmação
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (!result.Succeeded)
            {
                // Retornar erros
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            // Gerar token JWT para o novo utilizador
            var token = _tokenService.GenerateToken(newUser);

            // Devolver token e info do utilizador
            return Ok(new
            {
                token,
                user = new
                {
                    newUser.Id,
                    newUser.FullName,
                    newUser.Email
                }
            });
        }

    }
}

