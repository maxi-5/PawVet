namespace PawVetClinica.Models;

public partial class Usuario
{
    public int IdUser { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    // hash BCrypt, nunca la contraseña en texto plano (RNF-02)
    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public virtual Rol? Rol { get; set; }

    public virtual Veterinario? Veterinario { get; set; }

    // Regla del diagrama de clases: compara la contraseña con el hash BCrypt (RF-01, RNF-02)
    public bool VerificarPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }
}
