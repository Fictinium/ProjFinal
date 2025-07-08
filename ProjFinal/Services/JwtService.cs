using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProjFinal.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjFinal.Services
{
    /// <summary>
    /// Geração de 'tokens' JWT (Java Web Token)
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(ApplicationUser user)
        {
            // Obter configuração JWT
            var jwtSettings = _config.GetSection("JwtSettings");

            // Criar chave de segurança a partir do secret
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            // Criar credenciais de assinatura
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Definir claims do token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),   // ID do utilizador
                new Claim(JwtRegisteredClaimNames.Email, user.Email),  // Email do utilizador
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token ID único
            };

            // Criar o token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            // Retornar o token em formato string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

