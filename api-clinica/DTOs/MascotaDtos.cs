using System.ComponentModel.DataAnnotations;

namespace PawVetClinica.DTOs
{
    public class MascotaDto
    {
        public int IdMascota { get; set; }
        public string Nombre { get; set; } = null!;
        public string Sexo { get; set; } = null!;
        public DateOnly? FechaNacimiento { get; set; }
        /// <summary>Edad en años, calculada por la API.</summary>
        public int? Edad { get; set; }
        public decimal? Peso { get; set; }
        public string? Microchip { get; set; }
        public int IdTutor { get; set; }
        public string? NombreTutor { get; set; }
        public int IdEspecie { get; set; }
        public string? Especie { get; set; }
        public int? IdRaza { get; set; }
        public string? Raza { get; set; }
    }

    // Datos para crear o editar una mascota
    public class MascotaGuardar
    {
        /// <example>Rocky</example>
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 60 caracteres")]
        public string Nombre { get; set; } = null!;

        /// <example>Macho</example>
        [Required(ErrorMessage = "El sexo es obligatorio")]
        [RegularExpression("^(Macho|Hembra)$", ErrorMessage = "El sexo debe ser Macho o Hembra")]
        public string Sexo { get; set; } = null!;

        /// <summary>Opcional, aaaa-mm-dd. No puede ser futura.</summary>
        /// <example>2021-05-10</example>
        public DateOnly? FechaNacimiento { get; set; }

        /// <example>12.5</example>
        [Range(0.01, 999.99, ErrorMessage = "El peso debe estar entre 0,01 y 999,99 kg")]
        public decimal? Peso { get; set; }

        /// <summary>Opcional: exactamente 15 digitos y unico.</summary>
        /// <example>941000012345678</example>
        [RegularExpression(@"^[0-9]{15}$", ErrorMessage = "El microchip debe tener exactamente 15 digitos")]
        public string? Microchip { get; set; }

        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el tutor")]
        public int IdTutor { get; set; }

        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la especie")]
        public int IdEspecie { get; set; }

        /// <summary>Opcional: si se indica, debe pertenecer a la especie.</summary>
        /// <example>1</example>
        public int? IdRaza { get; set; }
    }
}
