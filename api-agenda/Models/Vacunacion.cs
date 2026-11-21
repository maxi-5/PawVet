namespace PawVetAgenda.Models;

public partial class Vacunacion
{
    public int IdVacunacion { get; set; }

    public DateOnly FechaAplicacion { get; set; }

    // se calcula una sola vez al registrar (redundancia deliberada)
    public DateOnly? FechaProxima { get; set; }

    public string? Lote { get; set; }

    // referencias logicas a la API Clinica
    public int IdMascota { get; set; }

    public int IdVeterinario { get; set; }

    public int IdVacuna { get; set; }

    // opcional: vacuna aplicada durante una atencion
    public int? IdAtencion { get; set; }

    public virtual Vacuna? Vacuna { get; set; }

    public virtual Atencion? Atencion { get; set; }

    // ================== REGLAS DEL DIAGRAMA DE CLASES ==================

    // RF-11: fecha de aplicacion + intervalo de dias de la vacuna
    public DateOnly CalcularProximaDosis(Vacuna vacuna)
    {
        return FechaAplicacion.AddDays(vacuna.IntervaloDias);
    }

    // RF-13: ¿la proxima dosis cae dentro de los proximos 'dias' dias?
    public bool EstaPorVencer(int dias)
    {
        if (FechaProxima == null) return false;
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return FechaProxima.Value >= hoy && FechaProxima.Value <= hoy.AddDays(dias);
    }
}
