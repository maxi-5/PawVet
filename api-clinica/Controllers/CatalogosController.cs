using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawVetClinica.DTOs;
using PawVetClinica.Models;
using PawVetClinica.Repositories;

namespace PawVetClinica.Controllers
{
    // Catalogo de especies (CU-03): GET todos los roles | POST, DELETE: Administrador
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/especies")]
    [ApiController]
    [Authorize]
    public class EspeciesController : ControllerBase
    {
        private readonly IRepository<Especie> _especies;
        private readonly ICatalogoRepository _catalogo;

        public EspeciesController(IRepository<Especie> especies, ICatalogoRepository catalogo)
        {
            _especies = especies;
            _catalogo = catalogo;
        }

        /// <summary>Lista las especies.</summary>
        // GET: api/v1/especies
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EspecieDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EspecieDto>>> GetEspecies()
        {
            var lista = await _especies.GetAllAsync();
            return Ok(lista.OrderBy(e => e.Nombre).Select(e => e.ToDto()));
        }

        /// <summary>Obtiene una especie.</summary>
        // GET: api/v1/especies/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EspecieDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EspecieDto>> GetEspecie(int id)
        {
            var especie = await _especies.GetByIdAsync(id);
            if (especie == null)
                return Problem(detail: $"No existe la especie con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(especie.ToDto());
        }

        /// <summary>Crea una especie (solo Administrador).</summary>
        // POST: api/v1/especies
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(EspecieDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EspecieDto>> PostEspecie(EspecieCrear dto)
        {
            var nombre = dto.Nombre.Trim();
            if (await _especies.ExistsAsync(e => e.Nombre == nombre))
                return Problem(detail: "Ya existe una especie con ese nombre", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            var especie = new Especie { Nombre = nombre };
            await _especies.AddAsync(especie);

            return CreatedAtAction(nameof(GetEspecie), new { id = especie.IdEspecie, version = "1" }, especie.ToDto());
        }

        /// <summary>Elimina una especie (solo Administrador). No se puede si tiene razas o mascotas.</summary>
        // DELETE: api/v1/especies/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteEspecie(int id)
        {
            var especie = await _especies.GetByIdAsync(id);
            if (especie == null)
                return Problem(detail: $"No existe la especie con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            if (await _catalogo.EspecieEnUsoAsync(id))
                return Problem(detail: "No se puede eliminar la especie porque tiene razas o mascotas", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            await _especies.DeleteAsync(especie);
            return NoContent();
        }
    }

    // Catalogo de razas (CU-03): GET todos los roles | POST, DELETE: Administrador
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/razas")]
    [ApiController]
    [Authorize]
    public class RazasController : ControllerBase
    {
        private readonly IRepository<Raza> _razas;
        private readonly IRepository<Especie> _especies;
        private readonly ICatalogoRepository _catalogo;

        public RazasController(IRepository<Raza> razas, IRepository<Especie> especies, ICatalogoRepository catalogo)
        {
            _razas = razas;
            _especies = especies;
            _catalogo = catalogo;
        }

        /// <summary>Lista las razas. Se puede filtrar por especie.</summary>
        // GET: api/v1/razas?idEspecie=1
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RazaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RazaDto>>> GetRazas([FromQuery] int? idEspecie = null)
        {
            var lista = await _catalogo.GetRazasAsync(idEspecie);
            return Ok(lista.Select(r => r.ToDto()));
        }

        /// <summary>Obtiene una raza.</summary>
        // GET: api/v1/razas/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(RazaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RazaDto>> GetRaza(int id)
        {
            var raza = await _catalogo.GetRazaAsync(id);
            if (raza == null)
                return Problem(detail: $"No existe la raza con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            return Ok(raza.ToDto());
        }

        /// <summary>Crea una raza de una especie (solo Administrador).</summary>
        // POST: api/v1/razas
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(typeof(RazaDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RazaDto>> PostRaza(RazaCrear dto)
        {
            if (!await _especies.ExistsAsync(e => e.IdEspecie == dto.IdEspecie))
                return Problem(detail: $"No existe la especie con id {dto.IdEspecie}", statusCode: StatusCodes.Status400BadRequest, title: "Solicitud invalida");

            var raza = new Raza { Nombre = dto.Nombre.Trim(), IdEspecie = dto.IdEspecie };
            await _razas.AddAsync(raza);

            var creada = await _catalogo.GetRazaAsync(raza.IdRaza);
            return CreatedAtAction(nameof(GetRaza), new { id = raza.IdRaza, version = "1" }, creada!.ToDto());
        }

        /// <summary>Elimina una raza (solo Administrador). No se puede si hay mascotas de esa raza.</summary>
        // DELETE: api/v1/razas/1
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteRaza(int id)
        {
            var raza = await _razas.GetByIdAsync(id);
            if (raza == null)
                return Problem(detail: $"No existe la raza con id {id}", statusCode: StatusCodes.Status404NotFound, title: "No encontrado");

            if (await _catalogo.RazaEnUsoAsync(id))
                return Problem(detail: "No se puede eliminar la raza porque hay mascotas de esa raza", statusCode: StatusCodes.Status409Conflict, title: "Conflicto");

            await _razas.DeleteAsync(raza);
            return NoContent();
        }
    }
}
