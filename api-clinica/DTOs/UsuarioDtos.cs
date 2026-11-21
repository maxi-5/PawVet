using System.ComponentModel.DataAnnotations;

namespace PawVetClinica.DTOs
{
    // Credenciales en el CUERPO de la peticion (en el Trabajo 2 iban en la URL)
    public class LoginRequest
    {
        /// <example>admin@pawvet.cl</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato valido")]
        public string Email { get; set; } = null!;

        /// <example>Admin123!</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = null!;
    }

    // Respuesta del login
    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expira { get; set; }
        public UsuarioDto Usuario { get; set; } = null!;
        /// <summary>Solo para usuarios con rol Veterinario vinculados a una ficha de veterinario.</summary>
        public int? IdVeterinario { get; set; }
    }

    // Datos de un usuario que se devuelven (nunca la contraseña)
    public class UsuarioDto
    {
        public int IdUser { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int IdRol { get; set; }
        public string Rol { get; set; } = null!;
    }

    // Datos para crear un usuario
    public class UsuarioCrear
    {
        /// <example>Camila Soto</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string Nombre { get; set; } = null!;

        /// <example>camila.soto@pawvet.cl</example>
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato valido")]
        [StringLength(150)]
        public string Email { get; set; } = null!;

        /// <example>Secreta123</example>
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = null!;

        /// <summary>1 Administrador, 2 Recepcionista, 3 Veterinario.</summary>
        /// <example>3</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un rol")]
        public int IdRol { get; set; }
    }

    public class RolDto
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = null!;
    }
}
