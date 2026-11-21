namespace PawVetAgenda.Models;

public partial class Vacuna
{
    public int IdVacuna { get; set; }

    public string Nombre { get; set; } = null!;

    // referencia logica a ESPECIE de la API Clinica
    public int IdEspecie { get; set; }

    // dias hasta la proxima dosis
    public int IntervaloDias { get; set; }

    public virtual ICollection<Vacunacion> Vacunaciones { get; set; } = new List<Vacunacion>();
}
