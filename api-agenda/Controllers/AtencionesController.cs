using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetAgenda.DTOs;
using PawVetAgenda.Models;
using PawVetAgenda.Repositories;
using PawVetAgenda.Services;

namespace PawVetAgenda.Controllers
{
    // Ficha clinica: atenciones (RF-10, HU-05)
    // GET: Administrador y Veterinario | POST: solo el Veterinario de la cita
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/atenciones")]
    [ApiController]
    [Authorize(Roles = "Administrador,Veterinario")]
    public class AtencionesController : ControllerBase
    {
        private readonly IAtencionRepository _atenciones;
        private readonly ICitaRepository _citas;

        public AtencionesController(IAtencionRepository atenciones, ICitaRepository citas)
        {
            _atenciones = atenciones;
            _citas = citas;
        }

        /// <summary>Lista atenciones, de la mas reciente a la mas antigua. Filtra por mascota (historial) o por cita.</summary>
        // GET: api/v1/atenciones?idMascota=1
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AtencionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<AtencionDto>>> GetAtenciones([FromQuery] int? idMascota = null, [FromQuery] int? idCita = null)
        {
            var lista = await _atenciones.BuscarAsync(idMascota, idCita);
            return Ok(lista.Select(a => a.ToDto()));
        }

        /// <summary>Obtiene una atencion.</summary>
        // GET: api/v1/atenciones/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AtencionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AtencionDto>> GetAtencion(int id)
        {
            var atencion = await _atenciones.GetByIdAsync(id);
            if (atencion == null)
                return Problem(detail: $"No existe la atencion con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(atencion.ToDto());
        }

        /// <summary>Registra la atencion de una cita Confirmada del veterinario logueado. La cita pasa a Atendida.</summary>
        // POST: api/v1/atenciones
        [HttpPost]
        [Authorize(Roles = "Veterinario")]
        [ProducesResponseType(typeof(AtencionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AtencionDto>> PostAtencion(AtencionCrear dto)
        {
            var cita = await _citas.GetConAtencionAsync(dto.IdCita);
            if (cita == null)
                return Problem(detail: $"No existe la cita con id {dto.IdCita}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            // solo el veterinario asignado a la cita puede atenderla
            if (User.IdVeterinario() != cita.IdVeterinario)
                return Problem(detail: "Esta cita pertenece a otro veterinario", statusCode: StatusCodes.Status403Forbidden, title: "Sin permiso");

            if (cita.Atencion != null)
                return Problem(detail: "La cita ya tiene una atencion registrada", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            try
            {
                // Confirmada -> Atendida (regla de la clase Cita)
                cita.MarcarAtendida();
            }
            catch (TransicionInvalidaException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Transicion de estado no permitida");
            }

            var atencion = new Atencion
            {
                IdCita = cita.IdCita,
                Fecha = DateTime.Now,
                Diagnostico = dto.Diagnostico.Trim(),
                Tratamiento = dto.Tratamiento,
                Peso = dto.Peso,
                Temperatura = dto.Temperatura,
                // se copian desde la cita: el cliente no los envia
                IdMascota = cita.IdMascota,
                IdVeterinario = cita.IdVeterinario
            };
            await _atenciones.RegistrarAsync(cita, atencion);

            return CreatedAtAction(nameof(GetAtencion), new { id = atencion.IdAtencion, version = "1" }, atencion.ToDto());
        }
    }
}
