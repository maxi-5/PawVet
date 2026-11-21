using PawVetAgenda.Models;

namespace PawVetAgenda.DTOs
{
    public static class Mapeos
    {
        public static CitaDto ToDto(this Cita c) => new()
        {
            IdCita = c.IdCita,
            FechaHora = c.FechaHora,
            DuracionMinutos = c.DuracionMinutos,
            Motivo = c.Motivo,
            Estado = c.Estado.ToString(),
            IdMascota = c.IdMascota,
            IdVeterinario = c.IdVeterinario,
            FechaCreacion = c.FechaCreacion
        };

        public static AtencionDto ToDto(this Atencion a) => new()
        {
            IdAtencion = a.IdAtencion,
            IdCita = a.IdCita,
            Fecha = a.Fecha,
            Diagnostico = a.Diagnostico,
            Tratamiento = a.Tratamiento,
            Peso = a.Peso,
            Temperatura = a.Temperatura,
            IdMascota = a.IdMascota,
            IdVeterinario = a.IdVeterinario
        };

        public static VacunaDto ToDto(this Vacuna v) => new()
        {
            IdVacuna = v.IdVacuna,
            Nombre = v.Nombre,
            IdEspecie = v.IdEspecie,
            IntervaloDias = v.IntervaloDias
        };

        public static VacunacionDto ToDto(this Vacunacion v) => new()
        {
            IdVacunacion = v.IdVacunacion,
            FechaAplicacion = v.FechaAplicacion,
            FechaProxima = v.FechaProxima,
            Lote = v.Lote,
            IdMascota = v.IdMascota,
            IdVacuna = v.IdVacuna,
            Vacuna = v.Vacuna?.Nombre,
            IdVeterinario = v.IdVeterinario,
            IdAtencion = v.IdAtencion
        };
    }
}
