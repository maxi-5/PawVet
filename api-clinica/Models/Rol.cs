namespace PawVetClinica.Models;

public partial class Rol
{
    // ids fijos de los roles (se crean como datos iniciales)
    public const int ADMINISTRADOR = 1;
    public const int RECEPCIONISTA = 2;
    public const int VETERINARIO = 3;

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
