using System.ComponentModel.DataAnnotations;

namespace PawVetClinica.DTOs
{
    public class TutorDto
    {
        public int IdTutor { get; set; }
        public string Rut { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public int CantidadMascotas { get; set; }
    }

    // Datos para crear o editar un tutor
    public class TutorGuardar
    {
        /// <example>12345678-5</example>
        [Required(ErrorMessage = "El RUT es obligatorio")]
        [RutChileno]
        public string Rut { get; set; } = null!;

        /// <example>María González</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string Nombre { get; set; } = null!;

        /// <example>+56912345678</example>
        [Required(ErrorMessage = "El telefono es obligatorio")]
        [RegularExpression(@"^\+56[0-9]{9}$", ErrorMessage = "El telefono debe tener el formato +56XXXXXXXXX")]
        public string Telefono { get; set; } = null!;

        /// <example>maria.gonzalez@correo.cl</example>
        [EmailAddress(ErrorMessage = "El email no tiene un formato valido")]
        [StringLength(150)]
        public string? Email { get; set; }

        /// <example>Villagrán 455, Mulchén</example>
        [StringLength(200)]
        public string? Direccion { get; set; }
    }
}
