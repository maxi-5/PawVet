using System.ComponentModel.DataAnnotations;

namespace PawVetAgenda.DTOs
{
    public class CitaDto
    {
        public int IdCita { get; set; }
        public DateTime FechaHora { get; set; }
        public int DuracionMinutos { get; set; }
        public string Motivo { get; set; } = null!;
        /// <summary>Agendada, Confirmada, Atendida, Cancelada o NoAsistio.</summary>
        public string Estado { get; set; } = null!;
        public int IdMascota { get; set; }
        public int IdVeterinario { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    // Datos para agendar o editar una cita
    public class CitaGuardar
    {
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la mascota")]
        public int IdMascota { get; set; }

        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el veterinario")]
        public int IdVeterinario { get; set; }

        /// <summary>Fecha y hora de inicio (debe ser futura y no feriado).</summary>
        /// <example>2026-12-02T10:30:00</example>
        [Required(ErrorMessage = "La fecha y hora son obligatorias")]
        public DateTime FechaHora { get; set; }

        /// <example>30</example>
        [Range(15, 120, ErrorMessage = "La duracion debe estar entre 15 y 120 minutos")]
        public int DuracionMinutos { get; set; } = 30;

        /// <example>Control anual y vacuna antirrábica</example>
        [Required(ErrorMessage = "El motivo es obligatorio")]
        [StringLength(300, MinimumLength = 3, ErrorMessage = "El motivo debe tener entre 3 y 300 caracteres")]
        public string Motivo { get; set; } = null!;
    }

    // Cambio de estado: PATCH /citas/{id}/estado
    public class CambioEstadoCita
    {
        /// <summary>Confirmada, Cancelada o NoAsistio. Atendida se registra con POST /atenciones.</summary>
        /// <example>Confirmada</example>
        [Required(ErrorMessage = "El estado es obligatorio")]
        [RegularExpression("^(Confirmada|Cancelada|NoAsistio)$", ErrorMessage = "El estado debe ser Confirmada, Cancelada o NoAsistio")]
        public string Estado { get; set; } = null!;
    }

    public class AtencionDto
    {
        public int IdAtencion { get; set; }
        public int IdCita { get; set; }
        public DateTime Fecha { get; set; }
        public string Diagnostico { get; set; } = null!;
        public string? Tratamiento { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Temperatura { get; set; }
        public int IdMascota { get; set; }
        public int IdVeterinario { get; set; }
    }

    // Datos de la atencion. La mascota y el veterinario se copian desde la cita (el cliente no los envia).
    public class AtencionCrear
    {
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la cita")]
        public int IdCita { get; set; }

        /// <example>Otitis externa leve</example>
        [Required(ErrorMessage = "El diagnostico es obligatorio")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "El diagnostico debe tener entre 3 y 500 caracteres")]
        public string Diagnostico { get; set; } = null!;

        /// <example>Limpieza de oídos y gotas cada 12 horas por 7 días</example>
        [StringLength(500)]
        public string? Tratamiento { get; set; }

        /// <summary>Kilogramos.</summary>
        /// <example>12.5</example>
        [Range(0.01, 999.99, ErrorMessage = "El peso debe estar entre 0,01 y 999,99 kg")]
        public decimal? Peso { get; set; }

        /// <summary>Grados Celsius.</summary>
        /// <example>38.5</example>
        [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30 y 45 °C")]
        public decimal? Temperatura { get; set; }
    }

    public class VacunaDto
    {
        public int IdVacuna { get; set; }
        public string Nombre { get; set; } = null!;
        public int IdEspecie { get; set; }
        public int IntervaloDias { get; set; }
    }

    public class VacunacionDto
    {
        public int IdVacunacion { get; set; }
        public DateOnly FechaAplicacion { get; set; }
        public DateOnly? FechaProxima { get; set; }
        public string? Lote { get; set; }
        public int IdMascota { get; set; }
        public int IdVacuna { get; set; }
        public string? Vacuna { get; set; }
        public int IdVeterinario { get; set; }
        public int? IdAtencion { get; set; }
    }

    // Datos de la vacunacion. El veterinario se toma del token y la proxima dosis la calcula la API.
    public class VacunacionCrear
    {
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la mascota")]
        public int IdMascota { get; set; }

        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar la vacuna")]
        public int IdVacuna { get; set; }

        /// <summary>No puede ser futura.</summary>
        /// <example>2026-11-16</example>
        [Required(ErrorMessage = "La fecha de aplicacion es obligatoria")]
        public DateOnly FechaAplicacion { get; set; }

        /// <example>LT-2026-0457</example>
        [StringLength(50)]
        public string? Lote { get; set; }

        /// <summary>Opcional: atencion en la que se aplico.</summary>
        public int? IdAtencion { get; set; }
    }

    // Correccion de una vacunacion ya registrada (fecha o lote).
    // La proxima dosis se vuelve a calcular con el intervalo de la vacuna.
    public class VacunacionEditar
    {
        /// <summary>No puede ser futura.</summary>
        /// <example>2025-12-05</example>
        [Required(ErrorMessage = "La fecha de aplicacion es obligatoria")]
        public DateOnly FechaAplicacion { get; set; }

        /// <example>LT-2025-0102</example>
        [StringLength(50)]
        public string? Lote { get; set; }
    }

    // Lo que responde la API Clinica (solo los campos que se usan aqui)
    public class MascotaInfo
    {
        public int IdMascota { get; set; }
        public string Nombre { get; set; } = null!;
        public int IdEspecie { get; set; }
    }

    public class VeterinarioInfo
    {
        public int IdVeterinario { get; set; }
        public string Nombre { get; set; } = null!;
    }
}
