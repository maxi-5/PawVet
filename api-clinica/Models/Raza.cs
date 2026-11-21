namespace PawVetClinica.Models;

public partial class Raza
{
    public int IdRaza { get; set; }

    public string Nombre { get; set; } = null!;

    public int IdEspecie { get; set; }

    public virtual Especie? Especie { get; set; }

    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();

    // Regla del diagrama de clases: una raza solo es valida para su especie (RF-05)
    public bool PerteneceA(int idEspecie)
    {
        return IdEspecie == idEspecie;
    }
}
