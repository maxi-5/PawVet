using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetClinica.DTOs;
using PawVetClinica.Models;
using PawVetClinica.Repositories;

namespace PawVetClinica.Controllers
{
    // CRUD completo de veterinarios (catalogo, CU-03)
    // GET: todos los roles (se usa para agendar y desde la API Agenda) | POST, PUT, DELETE: Administrador
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/veterinarios")]
    [ApiController]
    [Authorize]
    public class VeterinariosController : ControllerBase
    {
        private readonly IRepository<Veterinario> _veterinarios;
        private readonly IUsuarioRepository _usuarios;

        public VeterinariosController(IRepository<Veterinario> veterinarios, IUsuarioRepository usuarios)
        {
            _veterinarios = veterinarios;
            _usuarios = usuarios;
        }

        /// <summary>Lista los veterinarios.</summary>
        // GET: api/v1/veterinarios
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VeterinarioDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VeterinarioDto>>> GetVeterinarios()
        {
            var lista = await _veterinarios.GetAllAsync();
            return Ok(lista.OrderBy(v => v.Nombre).Select(v => v.ToDto()));
        }

        /// <summary>Obtiene un veterinario. La API Agenda lo usa para verificar que exista.</summary>
        // GET: api/v1/veterinarios/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(VeterinarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VeterinarioDto>> GetVeterinario(int id)
        {
            var veterinario = await _veterinarios.GetByIdAsync(id);
            if (veterinario == null)
                return Problem(detail: $"No existe el veterinario con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(veterinario.ToDto());
        }

        /// <summary>Crea un veterinario, opcionalmente vinculado a un usuario con rol Veterinario.</summary>
        // POST: api/v1/veterinarios
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(VeterinarioDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VeterinarioDto>> PostVeterinario(VeterinarioGuardar dto)
        {
            var error = await ValidarAsync(dto, 0);
            if (error != null)
                return error;

            var veterinario = new Veterinario();
            CopiarDatos(dto, veterinario);
            await _veterinarios.AddAsync(veterinario);

            return CreatedAtAction(nameof(GetVeterinario), new { id = veterinario.IdVeterinario, version = "1" }, veterinario.ToDto());
        }

        /// <summary>Edita un veterinario.</summary>
        // PUT: api/v1/veterinarios/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutVeterinario(int id, VeterinarioGuardar dto)
        {
            var veterinario = await _veterinarios.GetByIdAsync(id);
            if (veterinario == null)
                return Problem(detail: $"No existe el veterinario con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            var error = await ValidarAsync(dto, id);
            if (error != null)
                return error;

            CopiarDatos(dto, veterinario);
            await _veterinarios.UpdateAsync(veterinario);

            return NoContent();
        }

        /// <summary>Elimina un veterinario.</summary>
        // DELETE: api/v1/veterinarios/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVeterinario(int id)
        {
            var veterinario = await _veterinarios.GetByIdAsync(id);
            if (veterinario == null)
                return Problem(detail: $"No existe el veterinario con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            await _veterinarios.DeleteAsync(veterinario);
            return NoContent();
        }

        // ================== REGLAS DE NEGOCIO ==================
        private async Task<ActionResult?> ValidarAsync(VeterinarioGuardar dto, int idActual)
        {
            var rut = dto.Rut.Trim().ToUpper();
            if (await _veterinarios.ExistsAsync(v => v.Rut == rut && v.IdVeterinario != idActual))
                return Problem(detail: "Ya existe un veterinario con ese RUT", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            if (dto.IdUser.HasValue)
            {
                // la cuenta debe existir y tener el rol Veterinario
                var usuario = await _usuarios.GetByIdAsync(dto.IdUser.Value);
                if (usuario == null || usuario.IdRol != Rol.VETERINARIO)
                    return Problem(detail: $"El usuario {dto.IdUser} no existe o no tiene el rol Veterinario", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

                // y no puede ser la cuenta de otro veterinario
                if (await _veterinarios.ExistsAsync(v => v.IdUser == dto.IdUser && v.IdVeterinario != idActual))
                    return Problem(detail: "Ese usuario ya es la cuenta de otro veterinario", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");
            }

            return null;
        }

        private static void CopiarDatos(VeterinarioGuardar dto, Veterinario veterinario)
        {
            veterinario.Rut = dto.Rut.Trim().ToUpper();
            veterinario.Nombre = dto.Nombre.Trim();
            veterinario.Especialidad = dto.Especialidad;
            veterinario.Telefono = dto.Telefono;
            veterinario.Email = dto.Email;
            veterinario.IdUser = dto.IdUser;
        }
    }
}
