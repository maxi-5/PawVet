namespace PawVetClinica.Models;

public partial class Especie
{
    public int IdEspecie { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Raza> Razas { get; set; } = new List<Raza>();

    public virtual ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}
