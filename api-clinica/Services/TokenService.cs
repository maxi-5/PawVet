using Microsoft.IdentityModel.Tokens;
using PawVetClinica.DTOs;
using PawVetClinica.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PawVetClinica.Services
{
    public interface ITokenService
    {
        LoginResponse GenerarToken(Usuario user);
    }

    // Genera el JWT (HS256, 60 minutos). Esta API EMITE el token; la API Agenda lo valida (ADR-002).
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public LoginResponse GenerarToken(Usuario user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("id_user", user.IdUser.ToString()),
                new Claim("nombre", user.Nombre),
                new Claim(ClaimTypes.Role, user.Rol?.Nombre ?? "")
            };

            // para veterinarios: su id de ficha, que usa la API Agenda (mi agenda, atenciones)
            int? idVeterinario = user.Veterinario?.IdVeterinario;
            if (idVeterinario.HasValue)
                claims.Add(new Claim("id_veterinario", idVeterinario.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var expira = DateTime.UtcNow.AddMinutes(60);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expira,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);

            return new LoginResponse
            {
                Token = handler.WriteToken(token),
                Expira = expira,
                Usuario = user.ToDto(),
                IdVeterinario = idVeterinario
            };
        }
    }
}
