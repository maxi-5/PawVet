namespace PawVetClinica.Models;

public partial class Veterinario
{
    public int IdVeterinario { get; set; }

    public string Rut { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Especialidad { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    // cuenta de acceso del veterinario (nuevo en el Trabajo 3): permite el claim id_veterinario
    public int? IdUser { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
