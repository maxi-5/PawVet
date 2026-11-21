using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetClinica.DTOs;
using PawVetClinica.Models;
using PawVetClinica.Repositories;

namespace PawVetClinica.Controllers
{
    // Usuarios del sistema: solo el Administrador (RF-02, HU-06)
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/usuarios")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarios;
        private readonly IRepository<Rol> _roles;

        public UsuariosController(IUsuarioRepository usuarios, IRepository<Rol> roles)
        {
            _usuarios = usuarios;
            _roles = roles;
        }

        /// <summary>Lista los usuarios con paginacion.</summary>
        // GET: api/v1/usuarios?pagina=1&tamano=20
        [HttpGet]
        [ProducesResponseType(typeof(PaginaDto<UsuarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginaDto<UsuarioDto>>> GetUsuarios([FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 100) tamano = 20;

            var (lista, total) = await _usuarios.GetPaginaAsync(pagina, tamano);
            return Ok(new PaginaDto<UsuarioDto> { Items = lista.Select(u => u.ToDto()).ToList(), Pagina = pagina, Tamano = tamano, Total = total });
        }

        /// <summary>Obtiene un usuario.</summary>
        // GET: api/v1/usuarios/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _usuarios.GetConRolAsync(id);
            if (usuario == null)
                return Problem(detail: $"No existe el usuario con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(usuario.ToDto());
        }

        /// <summary>Crea un usuario con su rol. La contraseña se guarda con BCrypt.</summary>
        // POST: api/v1/usuarios
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UsuarioDto>> PostUsuario(UsuarioCrear dto)
        {
            var email = dto.Email.Trim().ToLower();
            if (await _usuarios.ExistsAsync(u => u.Email == email))
                return Problem(detail: "Ya existe un usuario con ese email", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            if (!await _roles.ExistsAsync(r => r.IdRol == dto.IdRol))
                return Problem(detail: $"No existe el rol con id {dto.IdRol}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Email = email,
                //Encriptamos el password a utilizar
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdRol = dto.IdRol
            };
            await _usuarios.AddAsync(usuario);

            var creado = await _usuarios.GetConRolAsync(usuario.IdUser);
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUser, version = "1" }, creado!.ToDto());
        }

        /// <summary>Elimina un usuario. Si era la cuenta de un veterinario, la ficha queda sin cuenta.</summary>
        // DELETE: api/v1/usuarios/5
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _usuarios.GetByIdAsync(id);
            if (usuario == null)
                return Problem(detail: $"No existe el usuario con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            // cambio registrado en el CHANGELOG (v1.0.1): el administrador no puede eliminarse a si mismo
            if (id == int.Parse(User.FindFirst("id_user")!.Value))
                return Problem(detail: "No puedes eliminar tu propio usuario", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            await _usuarios.DeleteAsync(usuario);
            return NoContent();
        }
    }

    // Catalogo de roles: solo el Administrador
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/roles")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class RolesController : ControllerBase
    {
        private readonly IRepository<Rol> _roles;

        public RolesController(IRepository<Rol> roles)
        {
            _roles = roles;
        }

        /// <summary>Lista los roles del sistema.</summary>
        // GET: api/v1/roles
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RolDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
        {
            var roles = await _roles.GetAllAsync();
            return Ok(roles.OrderBy(r => r.IdRol).Select(r => new RolDto { IdRol = r.IdRol, Nombre = r.Nombre }));
        }
    }
}
