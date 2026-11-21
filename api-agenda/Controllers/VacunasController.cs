using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetAgenda.DTOs;
using PawVetAgenda.Models;
using PawVetAgenda.Repositories;
using PawVetAgenda.Services;

namespace PawVetAgenda.Controllers
{
    // Catalogo de vacunas: todos los roles
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/vacunas")]
    [ApiController]
    [Authorize]
    public class VacunasController : ControllerBase
    {
        private readonly IRepository<Vacuna> _vacunas;

        public VacunasController(IRepository<Vacuna> vacunas)
        {
            _vacunas = vacunas;
        }

        /// <summary>Lista las vacunas. Se puede filtrar por especie.</summary>
        // GET: api/v1/vacunas?idEspecie=1
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VacunaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VacunaDto>>> GetVacunas([FromQuery] int? idEspecie = null)
        {
            var lista = await _vacunas.GetAllAsync();
            if (idEspecie.HasValue)
                lista = lista.Where(v => v.IdEspecie == idEspecie.Value);

            return Ok(lista.OrderBy(v => v.Nombre).Select(v => v.ToDto()));
        }
    }

    // Vacunaciones (RF-11, RF-13, CU-10)
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/vacunaciones")]
    [ApiController]
    [Authorize]
    public class VacunacionesController : ControllerBase
    {
        private readonly IVacunacionRepository _vacunaciones;
        private readonly IRepository<Vacuna> _vacunas;
        private readonly IAtencionRepository _atenciones;
        private readonly IClinicaClient _clinica;

        public VacunacionesController(IVacunacionRepository vacunaciones, IRepository<Vacuna> vacunas, IAtencionRepository atenciones, IClinicaClient clinica)
        {
            _vacunaciones = vacunaciones;
            _vacunas = vacunas;
            _atenciones = atenciones;
            _clinica = clinica;
        }

        /// <summary>Vacunaciones de una mascota, de la mas reciente a la mas antigua.</summary>
        /// <param name="idMascota">Obligatorio.</param>
        // GET: api/v1/vacunaciones?idMascota=1
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VacunacionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<VacunacionDto>>> GetVacunaciones([FromQuery] int? idMascota = null)
        {
            if (idMascota == null || idMascota < 1)
                return Problem(detail: "Debe indicar idMascota", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            var lista = await _vacunaciones.GetPorMascotaAsync(idMascota.Value);
            return Ok(lista.Select(v => v.ToDto()));
        }

        /// <summary>Vacunas cuya proxima dosis vence en los proximos dias (por defecto 30).</summary>
        // GET: api/v1/vacunaciones/por-vencer?dias=30
        [HttpGet("por-vencer")]
        [ProducesResponseType(typeof(IEnumerable<VacunacionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VacunacionDto>>> GetPorVencer([FromQuery] int dias = 30)
        {
            if (dias < 1 || dias > 365) dias = 30;

            var lista = await _vacunaciones.GetPorVencerAsync(dias);
            return Ok(lista.Select(v => v.ToDto()));
        }

        /// <summary>Corrige la fecha de aplicacion o el lote de una vacunacion (solo Veterinario). Recalcula la proxima dosis.</summary>
        // PUT: api/v1/vacunaciones/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Veterinario")]
        [ProducesResponseType(typeof(VacunacionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VacunacionDto>> PutVacunacion(int id, VacunacionEditar dto)
        {
            var vacunacion = await _vacunaciones.GetConVacunaAsync(id);
            if (vacunacion == null)
                return Problem(detail: $"No existe la vacunacion con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            if (dto.FechaAplicacion > DateOnly.FromDateTime(DateTime.Today))
                return Problem(detail: "La fecha de aplicacion no puede ser futura", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            vacunacion.FechaAplicacion = dto.FechaAplicacion;
            vacunacion.Lote = dto.Lote;
            // RF-11: se vuelve a calcular con el intervalo de la vacuna (regla de la clase Vacunacion)
            vacunacion.FechaProxima = vacunacion.CalcularProximaDosis(vacunacion.Vacuna!);
            await _vacunaciones.UpdateAsync(vacunacion);

            return Ok(vacunacion.ToDto());
        }

        /// <summary>Registra una vacunacion (solo Veterinario). Calcula la fecha de la proxima dosis.</summary>
        // POST: api/v1/vacunaciones
        [HttpPost]
        [Authorize(Roles = "Veterinario")]
        [ProducesResponseType(typeof(VacunacionDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<VacunacionDto>> PostVacunacion(VacunacionCrear dto)
        {
            var idVeterinario = User.IdVeterinario();
            if (idVeterinario == null)
                return Problem(detail: "Tu usuario no esta vinculado a una ficha de veterinario", statusCode: StatusCodes.Status403Forbidden, title: "Sin permiso");

            if (dto.FechaAplicacion > DateOnly.FromDateTime(DateTime.Today))
                return Problem(detail: "La fecha de aplicacion no puede ser futura", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            var vacuna = await _vacunas.GetByIdAsync(dto.IdVacuna);
            if (vacuna == null)
                return Problem(detail: $"No existe la vacuna con id {dto.IdVacuna}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            // INTEGRACION: la mascota vive en la API Clinica
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var mascota = await _clinica.ObtenerMascotaAsync(dto.IdMascota, token);
            if (mascota.Estado == EstadoConsulta.NoDisponible)
                return Problem(detail: "La API Clinica no respondio. Intenta nuevamente en unos minutos.", statusCode: StatusCodes.Status503ServiceUnavailable, title: "Servicio no disponible");
            if (mascota.Estado == EstadoConsulta.NoEncontrado)
                return Problem(detail: $"No existe la mascota con id {dto.IdMascota}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            // la vacuna debe ser para la especie de la mascota
            if (vacuna.IdEspecie != mascota.Dato!.IdEspecie)
                return Problem(detail: $"La vacuna {vacuna.Nombre} no corresponde a la especie de la mascota", statusCode: StatusCodes.Status400BadRequest, title: "Vacuna de otra especie");

            if (dto.IdAtencion.HasValue)
            {
                var atencion = await _atenciones.GetByIdAsync(dto.IdAtencion.Value);
                if (atencion == null || atencion.IdMascota != dto.IdMascota)
                    return Problem(detail: "La atencion indicada no existe o es de otra mascota", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");
            }

            var vacunacion = new Vacunacion
            {
                FechaAplicacion = dto.FechaAplicacion,
                Lote = dto.Lote,
                IdMascota = dto.IdMascota,
                IdVacuna = dto.IdVacuna,
                IdVeterinario = idVeterinario.Value,
                IdAtencion = dto.IdAtencion
            };
            // RF-11: fecha de aplicacion + intervalo de la vacuna (regla de la clase Vacunacion)
            vacunacion.FechaProxima = vacunacion.CalcularProximaDosis(vacuna);
            await _vacunaciones.AddAsync(vacunacion);

            var creada = await _vacunaciones.GetConVacunaAsync(vacunacion.IdVacunacion);
            return StatusCode(StatusCodes.Status201Created, creada!.ToDto());
        }
    }
}
