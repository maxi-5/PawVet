using PawVetClinica.Models;

namespace PawVetClinica.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        // trae el usuario con su rol y, si es veterinario, su ficha (para el login)
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetConRolAsync(int id);
        Task<(List<Usuario> lista, int total)> GetPaginaAsync(int pagina, int tamano);
    }

    public interface ITutorRepository : IRepository<Tutor>
    {
        Task<(List<Tutor> lista, int total)> GetPaginaAsync(int pagina, int tamano, string? q);
        Task<Tutor?> GetConMascotasAsync(int id);
        Task<bool> TieneMascotasAsync(int id);
    }

    public interface IMascotaRepository : IRepository<Mascota>
    {
        Task<(List<Mascota> lista, int total)> GetPaginaAsync(int pagina, int tamano, string? q, int? idTutor);
        Task<Mascota?> GetConDetalleAsync(int id);
    }

    public interface ICatalogoRepository
    {
        Task<List<Raza>> GetRazasAsync(int? idEspecie);
        Task<Raza?> GetRazaAsync(int id);
        Task<bool> EspecieEnUsoAsync(int idEspecie);
        Task<bool> RazaEnUsoAsync(int idRaza);
    }
}
