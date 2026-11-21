using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetAgenda.DTOs;
using PawVetAgenda.Models;
using PawVetAgenda.Repositories;
using PawVetAgenda.Services;

namespace PawVetAgenda.Controllers
{
    // Agenda de citas (RF-07, RF-08, RF-09, RF-12, HU-03, HU-04)
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/citas")]
    [ApiController]
    [Authorize]
    public class CitasController : ControllerBase
    {
        private readonly ICitaRepository _citas;
        private readonly IClinicaClient _clinica;
        private readonly IFeriadosClient _feriados;

        public CitasController(ICitaRepository citas, IClinicaClient clinica, IFeriadosClient feriados)
        {
            _citas = citas;
            _clinica = clinica;
            _feriados = feriados;
        }

        /// <summary>Lista las citas con filtros y paginacion (Administrador y Recepcionista).</summary>
        /// <param name="fecha">Opcional: citas de ese dia (aaaa-mm-dd).</param>
        /// <param name="idVeterinario">Opcional.</param>
        /// <param name="idMascota">Opcional.</param>
        /// <param name="estado">Opcional: Agendada, Confirmada, Atendida, Cancelada o NoAsistio.</param>
        /// <param name="pagina">Desde 1.</param>
        /// <param name="tamano">Maximo 100.</param>
        // GET: api/v1/citas?fecha=2026-12-02&idVeterinario=1&estado=Agendada
        [HttpGet]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(typeof(PaginaDto<CitaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaginaDto<CitaDto>>> GetCitas([FromQuery] DateOnly? fecha = null, [FromQuery] int? idVeterinario = null,
            [FromQuery] int? idMascota = null, [FromQuery] EstadoCita? estado = null, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
        {
            if (pagina < 1) pagina = 1;
            if (tamano < 1 || tamano > 100) tamano = 20;

            var (lista, total) = await _citas.GetPaginaAsync(pagina, tamano, fecha, idVeterinario, idMascota, estado);
            return Ok(new PaginaDto<CitaDto> { Items = lista.Select(c => c.ToDto()).ToList(), Pagina = pagina, Tamano = tamano, Total = total });
        }

        /// <summary>Agenda del dia del veterinario logueado, ordenada por hora (RF-09).</summary>
        /// <param name="fecha">Opcional: por defecto, hoy.</param>
        // GET: api/v1/citas/mi-agenda?fecha=2026-12-02
        [HttpGet("mi-agenda")]
        [Authorize(Roles = "Veterinario")]
        [ProducesResponseType(typeof(IEnumerable<CitaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetMiAgenda([FromQuery] DateOnly? fecha = null)
        {
            // el id del veterinario viene en el token (claim id_veterinario)
            var idVeterinario = User.IdVeterinario();
            if (idVeterinario == null)
                return Problem(detail: "Tu usuario no esta vinculado a una ficha de veterinario", statusCode: StatusCodes.Status403Forbidden, title: "Sin permiso");

            var lista = await _citas.GetDelDiaAsync(idVeterinario.Value, fecha ?? DateOnly.FromDateTime(DateTime.Today));
            return Ok(lista.Select(c => c.ToDto()));
        }

        /// <summary>Obtiene una cita.</summary>
        // GET: api/v1/citas/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CitaDto>> GetCita(int id)
        {
            var cita = await _citas.GetByIdAsync(id);
            if (cita == null)
                return Problem(detail: $"No existe la cita con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(cita.ToDto());
        }

        /// <summary>Agenda una cita: verifica mascota y veterinario en la API Clinica, feriados y topes de horario.</summary>
        // POST: api/v1/citas
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(typeof(CitaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<CitaDto>> PostCita(CitaGuardar dto)
        {
            var error = await ValidarAsync(dto, 0);
            if (error != null)
                return error;

            var cita = new Cita
            {
                FechaHora = dto.FechaHora,
                DuracionMinutos = dto.DuracionMinutos,
                Motivo = dto.Motivo.Trim(),
                IdMascota = dto.IdMascota,
                IdVeterinario = dto.IdVeterinario,
                Estado = EstadoCita.Agendada,
                FechaCreacion = DateTime.Now
            };
            await _citas.AddAsync(cita);

            return CreatedAtAction(nameof(GetCita), new { id = cita.IdCita, version = "1" }, cita.ToDto());
        }

        /// <summary>Edita o reprograma una cita Agendada o Confirmada, con las mismas verificaciones.</summary>
        // PUT: api/v1/citas/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> PutCita(int id, CitaGuardar dto)
        {
            var cita = await _citas.GetByIdAsync(id);
            if (cita == null)
                return Problem(detail: $"No existe la cita con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            if (cita.Estado != EstadoCita.Agendada && cita.Estado != EstadoCita.Confirmada)
                return Problem(detail: $"No se puede modificar una cita en estado {cita.Estado}", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            var error = await ValidarAsync(dto, id);
            if (error != null)
                return error;

            cita.FechaHora = dto.FechaHora;
            cita.DuracionMinutos = dto.DuracionMinutos;
            cita.Motivo = dto.Motivo.Trim();
            cita.IdMascota = dto.IdMascota;
            cita.IdVeterinario = dto.IdVeterinario;
            await _citas.UpdateAsync(cita);

            return NoContent();
        }

        /// <summary>Cambia el estado: Confirmada, Cancelada o NoAsistio (diagrama de estados).</summary>
        // PATCH: api/v1/citas/5/estado
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        [ProducesResponseType(typeof(CitaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CitaDto>> CambiarEstado(int id, CambioEstadoCita dto)
        {
            var cita = await _citas.GetByIdAsync(id);
            if (cita == null)
                return Problem(detail: $"No existe la cita con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            try
            {
                // cada metodo de la clase Cita valida si la transicion esta permitida
                if (dto.Estado == "Confirmada") cita.Confirmar();
                else if (dto.Estado == "Cancelada") cita.Cancelar();
                else cita.MarcarNoAsistio();
            }
            catch (TransicionInvalidaException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Transicion de estado no permitida");
            }

            await _citas.UpdateAsync(cita);
            return Ok(cita.ToDto());
        }

        /// <summary>Elimina una cita (solo Administrador). No se puede si ya fue atendida.</summary>
        // DELETE: api/v1/citas/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _citas.GetConAtencionAsync(id);
            if (cita == null)
                return Problem(detail: $"No existe la cita con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            if (cita.Atencion != null || cita.Estado == EstadoCita.Atendida)
                return Problem(detail: "No se puede eliminar una cita que ya tiene una atencion registrada", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            await _citas.DeleteAsync(cita);
            return NoContent();
        }

        // ================== VALIDACIONES DEL NEGOCIO (Secuencia 2) ==================
        // Devuelve null si todo esta correcto, o la respuesta de error.
        // idCitaExcluir: 0 al agendar; el id de la cita al editar (para no toparse consigo misma).
        private async Task<ActionResult?> ValidarAsync(CitaGuardar dto, int idCitaExcluir)
        {
            if (dto.FechaHora <= DateTime.Now)
                return Problem(detail: "La fecha y hora de la cita deben ser futuras", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            // 1. INTEGRACION: verificar en paralelo la mascota y el veterinario en la API Clinica
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var tareaMascota = _clinica.ObtenerMascotaAsync(dto.IdMascota, token);
            var tareaVeterinario = _clinica.ObtenerVeterinarioAsync(dto.IdVeterinario, token);
            await Task.WhenAll(tareaMascota, tareaVeterinario);
            var mascota = tareaMascota.Result;
            var veterinario = tareaVeterinario.Result;

            if (mascota.Estado == EstadoConsulta.NoDisponible || veterinario.Estado == EstadoConsulta.NoDisponible)
                return Problem(detail: "La API Clinica no respondio. Intenta nuevamente en unos minutos.", statusCode: StatusCodes.Status503ServiceUnavailable, title: "Servicio no disponible");

            if (mascota.Estado == EstadoConsulta.NoEncontrado)
                return Problem(detail: $"No existe la mascota con id {dto.IdMascota}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            if (veterinario.Estado == EstadoConsulta.NoEncontrado)
                return Problem(detail: $"No existe el veterinario con id {dto.IdVeterinario}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            // 2. API EXTERNA: no se agenda en feriados (RF-12)
            var feriado = await _feriados.ObtenerFeriadoAsync(DateOnly.FromDateTime(dto.FechaHora));
            if (feriado != null)
                return Problem(detail: $"No se puede agendar: el {dto.FechaHora:dd-MM-yyyy} es feriado ({feriado})", statusCode: StatusCodes.Status400BadRequest, title: "Dia feriado");

            // 3. REGLA DE NEGOCIO: el veterinario no puede tener dos citas que se topen (RF-08)
            var cercanas = await _citas.GetCercanasAsync(dto.IdVeterinario, dto.FechaHora, dto.DuracionMinutos, idCitaExcluir);
            if (cercanas.Any(c => c.SeTopaCon(dto.FechaHora, dto.DuracionMinutos)))
                return Problem(detail: "El veterinario ya tiene una cita en ese horario", statusCode: StatusCodes.Status409Conflict, title: "Tope de horario");

            return null;
        }
    }
}
