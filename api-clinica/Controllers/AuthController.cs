using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetClinica.DTOs;
using PawVetClinica.Repositories;
using PawVetClinica.Services;

namespace PawVetClinica.Controllers
{
    // Inicio de sesion: esta API EMITE el JWT (RF-01, HU-01)
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarios;
        private readonly ITokenService _tokenService;

        public AuthController(IUsuarioRepository usuarios, ITokenService tokenService)
        {
            _usuarios = usuarios;
            _tokenService = tokenService;
        }

        /// <summary>Inicia sesion con email y contraseña en el cuerpo, y devuelve el JWT (publico).</summary>
        // POST: api/v1/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest dto)
        {
            var user = await _usuarios.GetByEmailAsync(dto.Email.Trim().ToLower());

            // el mismo 401 si falla el email o la contraseña: no se da pista de cual fallo
            if (user == null || !user.VerificarPassword(dto.Password))
                return Problem(detail: "Credenciales incorrectas", statusCode: StatusCodes.Status401Unauthorized, title: "No autorizado");

            return Ok(_tokenService.GenerarToken(user));
        }
    }
}
