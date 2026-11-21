namespace PawVetAgenda.Models;

// Estados del ciclo de vida de una cita (diagrama de estados)
public enum EstadoCita
{
    Agendada,
    Confirmada,
    Atendida,
    Cancelada,
    NoAsistio
}

// Se lanza cuando se intenta una transicion de estado no permitida (la API responde 409)
public class TransicionInvalidaException : InvalidOperationException
{
    public TransicionInvalidaException(string mensaje) : base(mensaje)
    {
    }
}

public partial class Cita
{
    public int IdCita { get; set; }

    public DateTime FechaHora { get; set; }

    // 15 a 120 minutos, por defecto 30
    public int DuracionMinutos { get; set; } = 30;

    public string Motivo { get; set; } = null!;

    public EstadoCita Estado { get; set; } = EstadoCita.Agendada;

    // referencias logicas a la base de la API Clinica (sin clave foranea, ADR-001)
    public int IdMascota { get; set; }

    public int IdVeterinario { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public virtual Atencion? Atencion { get; set; }

    // ================== REGLAS DEL DIAGRAMA DE CLASES ==================

    // Agendada -> Confirmada, antes de la hora de la cita
    public void Confirmar()
    {
        if (Estado != EstadoCita.Agendada)
            throw new TransicionInvalidaException($"Solo se puede confirmar una cita Agendada (estado actual: {Estado})");
        if (FechaHora <= DateTime.Now)
            throw new TransicionInvalidaException("No se puede confirmar una cita cuya hora ya paso");
        Estado = EstadoCita.Confirmada;
    }

    // Agendada o Confirmada -> Cancelada, antes de la hora de la cita
    public void Cancelar()
    {
        if (Estado != EstadoCita.Agendada && Estado != EstadoCita.Confirmada)
            throw new TransicionInvalidaException($"No se puede cancelar una cita en estado {Estado}");
        if (FechaHora <= DateTime.Now)
            throw new TransicionInvalidaException("No se puede cancelar una cita cuya hora ya paso");
        Estado = EstadoCita.Cancelada;
    }

    // Confirmada -> Atendida, cuando el veterinario registra la atencion
    public void MarcarAtendida()
    {
        if (Estado != EstadoCita.Confirmada)
            throw new TransicionInvalidaException($"Solo se puede atender una cita Confirmada (estado actual: {Estado})");
        Estado = EstadoCita.Atendida;
    }

    // Agendada o Confirmada -> NoAsistio, despues de la hora de la cita
    public void MarcarNoAsistio()
    {
        if (Estado != EstadoCita.Agendada && Estado != EstadoCita.Confirmada)
            throw new TransicionInvalidaException($"No se puede marcar como no asistida una cita en estado {Estado}");
        if (FechaHora > DateTime.Now)
            throw new TransicionInvalidaException("Solo se puede marcar NoAsistio despues de la hora de la cita");
        Estado = EstadoCita.NoAsistio;
    }

    // RF-08: ¿esta cita se topa con otra que empieza en 'inicio' y dura 'minutos'?
    public bool SeTopaCon(DateTime inicio, int minutos)
    {
        // las citas canceladas o no asistidas no ocupan el horario
        if (Estado == EstadoCita.Cancelada || Estado == EstadoCita.NoAsistio)
            return false;

        var fin = FechaHora.AddMinutes(DuracionMinutos);
        var finNueva = inicio.AddMinutes(minutos);
        return inicio < fin && FechaHora < finNueva;
    }
}
