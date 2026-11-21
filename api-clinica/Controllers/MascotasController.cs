using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetClinica.DTOs;
using PawVetClinica.Models;
using PawVetClinica.Repositories;

namespace PawVetClinica.Controllers
{
    // CRUD completo de mascotas (RF-04, RF-05, RF-06, HU-02)
    // GET: todos los roles | POST, PUT: Administrador y Recepcionista | DELETE: Administrador
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/mascotas")]
    [ApiController]
    [Authorize]
    public class MascotasController : ControllerBase
    {
        private readonly IMascotaRepository _mascotas;
        private readonly ITutorRepository _tutores;
        private readonly IRepository<Especie> _especies;
        private readonly ICatalogoRepository _catalogo;

        public MascotasController(IMascotaRepository mascotas, ITutorRepository tutores, IRepository<Especie> especies, ICatalogoRepository catalogo)
        {
            _mascotas = mascotas;
            _tutores = tutores;
            _especies = especies;
            _catalogo = catalogo;
        }

        /// <summary>Lista las mascotas con paginacion. Busca por nombre o microchip y filtra por tutor.</summary>
        // GET: api/v1/mascotas?q=roc&idTutor=1&pagina=1&tamano=20
        [HttpGet]
        [ProducesResponseType(typeof(PaginaDto<MascotaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginaDto<MascotaDto>>> GetMascotas([FromQuery] string? q = null, [FromQuery] int? idTutor = null, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 100) tamano = 20;

            var (lista, total) = await _mascotas.GetPaginaAsync(pagina, tamano, q, idTutor);
            return Ok(new PaginaDto<MascotaDto> { Items = lista.Select(m => m.ToDto()).ToList(), Pagina = pagina, Tamano = tamano, Total = total });
        }

        /// <summary>Obtiene una mascota. La API Agenda lo usa para verificar que exista.</summary>
        // GET: api/v1/mascotas/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MascotaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MascotaDto>> GetMascota(int id)
        {
            var mascota = await _mascotas.GetConDetalleAsync(id);
            if (mascota == null)
                return Problem(detail: $"No existe la mascota con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(mascota.ToDto());
        }

        /// <summary>Registra una mascota asociada a su tutor, especie y raza.</summary>
        // POST: api/v1/mascotas
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(typeof(MascotaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MascotaDto>> PostMascota(MascotaGuardar dto)
        {
            var error = await ValidarAsync(dto, 0);
            if (error != null)
                return error;

            var mascota = new Mascota();
            CopiarDatos(dto, mascota);
            await _mascotas.AddAsync(mascota);

            var creada = await _mascotas.GetConDetalleAsync(mascota.IdMascota);
            return CreatedAtAction(nameof(GetMascota), new { id = mascota.IdMascota, version = "1" }, creada!.ToDto());
        }

        /// <summary>Edita una mascota.</summary>
        // PUT: api/v1/mascotas/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutMascota(int id, MascotaGuardar dto)
        {
            var mascota = await _mascotas.GetByIdAsync(id);
            if (mascota == null)
                return Problem(detail: $"No existe la mascota con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            var error = await ValidarAsync(dto, id);
            if (error != null)
                return error;

            CopiarDatos(dto, mascota);
            await _mascotas.UpdateAsync(mascota);

            return NoContent();
        }

        /// <summary>Elimina una mascota (solo Administrador).</summary>
        // DELETE: api/v1/mascotas/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMascota(int id)
        {
            var mascota = await _mascotas.GetByIdAsync(id);
            if (mascota == null)
                return Problem(detail: $"No existe la mascota con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            await _mascotas.DeleteAsync(mascota);
            return NoContent();
        }

        // ================== REGLAS DE NEGOCIO ==================
        // Devuelve null si todo esta correcto, o la respuesta de error.
        private async Task<ActionResult?> ValidarAsync(MascotaGuardar dto, int idActual)
        {
            if (dto.FechaNacimiento.HasValue && dto.FechaNacimiento.Value > DateOnly.FromDateTime(DateTime.Today))
                return Problem(detail: "La fecha de nacimiento no puede ser futura", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            if (!await _tutores.ExistsAsync(t => t.IdTutor == dto.IdTutor))
                return Problem(detail: $"No existe el tutor con id {dto.IdTutor}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            if (!await _especies.ExistsAsync(e => e.IdEspecie == dto.IdEspecie))
                return Problem(detail: $"No existe la especie con id {dto.IdEspecie}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            // RF-05: la raza debe pertenecer a la especie
            if (dto.IdRaza.HasValue)
            {
                var raza = await _catalogo.GetRazaAsync(dto.IdRaza.Value);
                if (raza == null)
                    return Problem(detail: $"No existe la raza con id {dto.IdRaza}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

                if (!raza.PerteneceA(dto.IdEspecie))
                    return Problem(detail: $"La raza {raza.Nombre} no pertenece a la especie indicada", statusCode: StatusCodes.Status400BadRequest, title: "Raza de otra especie");
            }

            // RF-06: el microchip no se puede repetir
            if (!string.IsNullOrWhiteSpace(dto.Microchip) &&
                await _mascotas.ExistsAsync(m => m.Microchip == dto.Microchip && m.IdMascota != idActual))
                return Problem(detail: "Ya existe una mascota con ese microchip", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            return null;
        }

        private static void CopiarDatos(MascotaGuardar dto, Mascota mascota)
        {
            mascota.Nombre = dto.Nombre.Trim();
            mascota.Sexo = dto.Sexo;
            mascota.FechaNacimiento = dto.FechaNacimiento;
            mascota.Peso = dto.Peso;
            mascota.Microchip = string.IsNullOrWhiteSpace(dto.Microchip) ? null : dto.Microchip;
            mascota.IdTutor = dto.IdTutor;
            mascota.IdEspecie = dto.IdEspecie;
            mascota.IdRaza = dto.IdRaza;
        }
    }
}
