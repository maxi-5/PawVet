using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetClinica.DTOs;
using PawVetClinica.Models;
using PawVetClinica.Repositories;

namespace PawVetClinica.Controllers
{
    // CRUD completo de tutores (RF-03)
    // GET: todos los roles | POST, PUT: Administrador y Recepcionista | DELETE: Administrador
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/tutores")]
    [ApiController]
    [Authorize]
    public class TutoresController : ControllerBase
    {
        private readonly ITutorRepository _tutores;

        public TutoresController(ITutorRepository tutores)
        {
            _tutores = tutores;
        }

        /// <summary>Lista los tutores con paginacion. Busca por nombre o RUT.</summary>
        // GET: api/v1/tutores?q=gonz&pagina=1&tamano=20
        [HttpGet]
        [ProducesResponseType(typeof(PaginaDto<TutorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginaDto<TutorDto>>> GetTutores([FromQuery] string? q = null, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 100) tamano = 20;

            var (lista, total) = await _tutores.GetPaginaAsync(pagina, tamano, q);
            return Ok(new PaginaDto<TutorDto> { Items = lista.Select(t => t.ToDto()).ToList(), Pagina = pagina, Tamano = tamano, Total = total });
        }

        /// <summary>Obtiene un tutor.</summary>
        // GET: api/v1/tutores/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TutorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TutorDto>> GetTutor(int id)
        {
            var tutor = await _tutores.GetConMascotasAsync(id);
            if (tutor == null)
                return Problem(detail: $"No existe el tutor con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(tutor.ToDto());
        }

        /// <summary>Registra un tutor. El RUT no puede repetirse.</summary>
        // POST: api/v1/tutores
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(typeof(TutorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<TutorDto>> PostTutor(TutorGuardar dto)
        {
            var rut = dto.Rut.Trim().ToUpper();
            if (await _tutores.ExistsAsync(t => t.Rut == rut))
                return Problem(detail: "Ya existe un tutor con ese RUT", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            var tutor = new Tutor
            {
                Rut = rut,
                Nombre = dto.Nombre.Trim(),
                Telefono = dto.Telefono,
                Email = dto.Email,
                Direccion = dto.Direccion
            };
            await _tutores.AddAsync(tutor);

            return CreatedAtAction(nameof(GetTutor), new { id = tutor.IdTutor, version = "1" }, tutor.ToDto());
        }

        /// <summary>Edita un tutor.</summary>
        // PUT: api/v1/tutores/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutTutor(int id, TutorGuardar dto)
        {
            var tutor = await _tutores.GetByIdAsync(id);
            if (tutor == null)
                return Problem(detail: $"No existe el tutor con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            var rut = dto.Rut.Trim().ToUpper();
            if (await _tutores.ExistsAsync(t => t.Rut == rut && t.IdTutor != id))
                return Problem(detail: "Ya existe otro tutor con ese RUT", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            tutor.Rut = rut;
            tutor.Nombre = dto.Nombre.Trim();
            tutor.Telefono = dto.Telefono;
            tutor.Email = dto.Email;
            tutor.Direccion = dto.Direccion;
            await _tutores.UpdateAsync(tutor);

            return NoContent();
        }

        /// <summary>Elimina un tutor (solo Administrador). No se puede si tiene mascotas.</summary>
        // DELETE: api/v1/tutores/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteTutor(int id)
        {
            var tutor = await _tutores.GetByIdAsync(id);
            if (tutor == null)
                return Problem(detail: $"No existe el tutor con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            if (await _tutores.TieneMascotasAsync(id))
                return Problem(detail: "No se puede eliminar el tutor porque tiene mascotas registradas", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            await _tutores.DeleteAsync(tutor);
            return NoContent();
        }
    }
}
