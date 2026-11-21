namespace PawVetAgenda.Models;

public partial class Atencion
{
    public int IdAtencion { get; set; }

    public int IdCita { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Now;

    public string Diagnostico { get; set; } = null!;

    public string? Tratamiento { get; set; }

    public decimal? Peso { get; set; }

    public decimal? Temperatura { get; set; }

    // copiados desde la cita (redundancia deliberada para consultar el historial por mascota)
    public int IdMascota { get; set; }

    public int IdVeterinario { get; set; }

    public virtual Cita? Cita { get; set; }

    public virtual ICollection<Vacunacion> Vacunaciones { get; set; } = new List<Vacunacion>();
}
