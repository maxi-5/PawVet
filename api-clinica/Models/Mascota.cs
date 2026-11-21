namespace PawVetClinica.Models;

public partial class Mascota
{
    public int IdMascota { get; set; }

    public string Nombre { get; set; } = null!;

    public string Sexo { get; set; } = null!;

    public DateOnly? FechaNacimiento { get; set; }

    public decimal? Peso { get; set; }

    // 15 digitos, opcional y unico (Ley 21.020) (RF-06)
    public string? Microchip { get; set; }

    public int IdTutor { get; set; }

    public int IdEspecie { get; set; }

    // opcional: mestizo o raza desconocida
    public int? IdRaza { get; set; }

    public virtual Tutor? Tutor { get; set; }

    public virtual Especie? Especie { get; set; }

    public virtual Raza? Raza { get; set; }

    // Regla del diagrama de clases: edad en años a partir de la fecha de nacimiento (RF-04)
    public int? CalcularEdad()
    {
        if (FechaNacimiento == null) return null;

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var edad = hoy.Year - FechaNacimiento.Value.Year;
        if (FechaNacimiento.Value > hoy.AddYears(-edad)) edad--;
        return edad < 0 ? 0 : edad;
    }
}
