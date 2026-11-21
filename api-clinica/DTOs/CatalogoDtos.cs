using System.ComponentModel.DataAnnotations;

namespace PawVetClinica.DTOs
{
    public class VeterinarioDto
    {
        public int IdVeterinario { get; set; }
        public string Rut { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Especialidad { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public int? IdUser { get; set; }
    }

    // Datos para crear o editar un veterinario
    public class VeterinarioGuardar
    {
        /// <example>15678234-3</example>
        [Required(ErrorMessage = "El RUT es obligatorio")]
        [RutChileno]
        public string Rut { get; set; } = null!;

        /// <example>Camila Soto</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        public string Nombre { get; set; } = null!;

        /// <example>Medicina general</example>
        [StringLength(100)]
        public string? Especialidad { get; set; }

        /// <example>+56987654321</example>
        [RegularExpression(@"^\+56[0-9]{9}$", ErrorMessage = "El telefono debe tener el formato +56XXXXXXXXX")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no tiene un formato valido")]
        [StringLength(150)]
        public string? Email { get; set; }

        /// <summary>Opcional: id del usuario con rol Veterinario que es su cuenta de acceso.</summary>
        public int? IdUser { get; set; }
    }

    public class EspecieDto
    {
        public int IdEspecie { get; set; }
        public string Nombre { get; set; } = null!;
    }

    public class EspecieCrear
    {
        /// <example>Hurón</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        public string Nombre { get; set; } = null!;
    }

    public class RazaDto
    {
        public int IdRaza { get; set; }
        public string Nombre { get; set; } = null!;
        public int IdEspecie { get; set; }
        public string? Especie { get; set; }
    }

    public class RazaCrear
    {
        /// <example>Golden Retriever</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres")]
        public string Nombre { get; set; } = null!;

        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la especie")]
        public int IdEspecie { get; set; }
    }
}
