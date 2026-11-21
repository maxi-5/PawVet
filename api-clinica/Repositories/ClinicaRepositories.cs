using Microsoft.EntityFrameworkCore;
using PawVetClinica.Models;

namespace PawVetClinica.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(ClinicaDbContext context) : base(context)
        {
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Veterinario)
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetConRolAsync(int id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUser == id);
        }

        public async Task<(List<Usuario> lista, int total)> GetPaginaAsync(int pagina, int tamano)
        {
            var query = _context.Usuarios.Include(u => u.Rol);
            var total = await query.CountAsync();
            var lista = await query
                .OrderBy(u => u.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
            return (lista, total);
        }
    }

    public class TutorRepository : Repository<Tutor>, ITutorRepository
    {
        public TutorRepository(ClinicaDbContext context) : base(context)
        {
        }

        public async Task<(List<Tutor> lista, int total)> GetPaginaAsync(int pagina, int tamano, string? q)
        {
            var query = _context.Tutores.Include(t => t.Mascotas).AsQueryable();

            // busqueda parcial por nombre o RUT
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.Nombre.Contains(q) || t.Rut.Contains(q));

            var total = await query.CountAsync();
            var lista = await query
                .OrderBy(t => t.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<Tutor?> GetConMascotasAsync(int id)
        {
            return await _context.Tutores
                .Include(t => t.Mascotas)
                .FirstOrDefaultAsync(t => t.IdTutor == id);
        }

        public async Task<bool> TieneMascotasAsync(int id)
        {
            return await _context.Mascotas.AnyAsync(m => m.IdTutor == id);
        }
    }

    public class MascotaRepository : Repository<Mascota>, IMascotaRepository
    {
        public MascotaRepository(ClinicaDbContext context) : base(context)
        {
        }

        public async Task<(List<Mascota> lista, int total)> GetPaginaAsync(int pagina, int tamano, string? q, int? idTutor)
        {
            var query = _context.Mascotas
                .Include(m => m.Tutor)
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(m => m.Nombre.Contains(q) || (m.Microchip != null && m.Microchip.Contains(q)));

            if (idTutor.HasValue)
                query = query.Where(m => m.IdTutor == idTutor.Value);

            var total = await query.CountAsync();
            var lista = await query
                .OrderBy(m => m.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToListAsync();
            return (lista, total);
        }

        public async Task<Mascota?> GetConDetalleAsync(int id)
        {
            return await _context.Mascotas
                .Include(m => m.Tutor)
                .Include(m => m.Especie)
                .Include(m => m.Raza)
                .FirstOrDefaultAsync(m => m.IdMascota == id);
        }
    }

    public class CatalogoRepository : ICatalogoRepository
    {
        private readonly ClinicaDbContext _context;

        public CatalogoRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Raza>> GetRazasAsync(int? idEspecie)
        {
            var query = _context.Razas.Include(r => r.Especie).AsQueryable();
            if (idEspecie.HasValue)
                query = query.Where(r => r.IdEspecie == idEspecie.Value);
            return await query.OrderBy(r => r.Nombre).ToListAsync();
        }

        public async Task<Raza?> GetRazaAsync(int id)
        {
            return await _context.Razas.Include(r => r.Especie).FirstOrDefaultAsync(r => r.IdRaza == id);
        }

        public async Task<bool> EspecieEnUsoAsync(int idEspecie)
        {
            return await _context.Razas.AnyAsync(r => r.IdEspecie == idEspecie)
                || await _context.Mascotas.AnyAsync(m => m.IdEspecie == idEspecie);
        }

        public async Task<bool> RazaEnUsoAsync(int idRaza)
        {
            return await _context.Mascotas.AnyAsync(m => m.IdRaza == idRaza);
        }
    }
}
