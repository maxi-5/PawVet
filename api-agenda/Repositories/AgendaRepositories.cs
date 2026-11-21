using Microsoft.EntityFrameworkCore;
using PawVetAgenda.Models;

namespace PawVetAgenda.Repositories
{
    public class CitaRepository : Repository<Cita>, ICitaRepository
    {
        public CitaRepository(AgendaDbContext context) : base(context)
        {
        }

        public async Task<(List<Cita> lista, int total)> GetPaginaAsync(int pagina, int tamano, DateOnly? fecha, int? idVeterinario, int? idMascota, EstadoCita? estado)
        {
            var query = _context.Citas.AsQueryable();

            if (fecha.HasValue)
            {
                var desde = fecha.Value.ToDateTime(TimeOnly.MinValue);
                var hasta = desde.AddDays(1);
                query = query.Where(c => c.FechaHora >= desde && c.FechaHora < hasta);
            }
            if (idVeterinario.HasValue)
                query = query.Where(c => c.IdVeterinario == idVeterinario.Value);
            if (idMascota.HasValue)
                query = query.Where(c => c.IdMascota == idMascota.Value);
            if (estado.HasValue)
                query = query.Where(c => c.Estado == estado.Value);

            var total = await query.CountAsync();
            var lista = await query
                .OrderBy(c => c.FechaHora)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<List<Cita>> GetDelDiaAsync(int idVeterinario, DateOnly fecha)
        {
            var desde = fecha.ToDateTime(TimeOnly.MinValue);
            var hasta = desde.AddDays(1);
            return await _context.Citas
                .Where(c => c.IdVeterinario == idVeterinario && c.FechaHora >= desde && c.FechaHora < hasta)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetCercanasAsync(int idVeterinario, DateTime inicio, int minutos, int idCitaExcluir)
        {
            // una cita dura como maximo 120 minutos: solo pueden topar las que empiezan en esta ventana
            var desde = inicio.AddMinutes(-120);
            var hasta = inicio.AddMinutes(minutos);
            return await _context.Citas
                .Where(c => c.IdVeterinario == idVeterinario
                         && c.IdCita != idCitaExcluir
                         && c.FechaHora > desde
                         && c.FechaHora < hasta)
                .ToListAsync();
        }

        public async Task<Cita?> GetConAtencionAsync(int id)
        {
            return await _context.Citas
                .Include(c => c.Atencion)
                .FirstOrDefaultAsync(c => c.IdCita == id);
        }
    }

    public class AtencionRepository : Repository<Atencion>, IAtencionRepository
    {
        public AtencionRepository(AgendaDbContext context) : base(context)
        {
        }

        public async Task<List<Atencion>> BuscarAsync(int? idMascota, int? idCita)
        {
            var query = _context.Atenciones.AsQueryable();
            if (idMascota.HasValue)
                query = query.Where(a => a.IdMascota == idMascota.Value);
            if (idCita.HasValue)
                query = query.Where(a => a.IdCita == idCita.Value);

            // historial: de la mas reciente a la mas antigua
            return await query.OrderByDescending(a => a.Fecha).ToListAsync();
        }

        public async Task RegistrarAsync(Cita cita, Atencion atencion)
        {
            _context.Atenciones.Add(atencion);
            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();
        }
    }

    public class VacunacionRepository : Repository<Vacunacion>, IVacunacionRepository
    {
        public VacunacionRepository(AgendaDbContext context) : base(context)
        {
        }

        public async Task<List<Vacunacion>> GetPorMascotaAsync(int idMascota)
        {
            return await _context.Vacunaciones
                .Include(v => v.Vacuna)
                .Where(v => v.IdMascota == idMascota)
                .OrderByDescending(v => v.FechaAplicacion)
                .ToListAsync();
        }

        public async Task<List<Vacunacion>> GetPorVencerAsync(int dias)
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var candidatas = await _context.Vacunaciones
                .Include(v => v.Vacuna)
                .Where(v => v.FechaProxima != null && v.FechaProxima >= hoy)
                .OrderBy(v => v.FechaProxima)
                .ToListAsync();

            // regla del diagrama de clases: Vacunacion.EstaPorVencer(dias)
            return candidatas.Where(v => v.EstaPorVencer(dias)).ToList();
        }

        public async Task<Vacunacion?> GetConVacunaAsync(int id)
        {
            return await _context.Vacunaciones
                .Include(v => v.Vacuna)
                .FirstOrDefaultAsync(v => v.IdVacunacion == id);
        }
    }
}
