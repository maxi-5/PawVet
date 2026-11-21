using PawVetAgenda.Models;

namespace PawVetAgenda.Repositories
{
    public interface ICitaRepository : IRepository<Cita>
    {
        Task<(List<Cita> lista, int total)> GetPaginaAsync(int pagina, int tamano, DateOnly? fecha, int? idVeterinario, int? idMascota, EstadoCita? estado);
        Task<List<Cita>> GetDelDiaAsync(int idVeterinario, DateOnly fecha);
        // citas del veterinario que podrian topar con el horario indicado
        Task<List<Cita>> GetCercanasAsync(int idVeterinario, DateTime inicio, int minutos, int idCitaExcluir);
        Task<Cita?> GetConAtencionAsync(int id);
    }

    public interface IAtencionRepository : IRepository<Atencion>
    {
        Task<List<Atencion>> BuscarAsync(int? idMascota, int? idCita);
        // guarda la atencion y el cambio de estado de la cita en una sola operacion
        Task RegistrarAsync(Cita cita, Atencion atencion);
    }

    public interface IVacunacionRepository : IRepository<Vacunacion>
    {
        Task<List<Vacunacion>> GetPorMascotaAsync(int idMascota);
        Task<List<Vacunacion>> GetPorVencerAsync(int dias);
        Task<Vacunacion?> GetConVacunaAsync(int id);
    }
}
