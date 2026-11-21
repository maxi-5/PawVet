using Microsoft.EntityFrameworkCore;

namespace PawVetAgenda.Models;

// Base de datos PawVetAgendaDB: la usa SOLO esta API (ADR-001)
public partial class AgendaDbContext : DbContext
{
    public AgendaDbContext()
    {
    }

    public AgendaDbContext(DbContextOptions<AgendaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cita> Citas { get; set; }

    public virtual DbSet<Atencion> Atenciones { get; set; }

    public virtual DbSet<Vacuna> Vacunas { get; set; }

    public virtual DbSet<Vacunacion> Vacunaciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cita>(entity =>
        {
            entity.ToTable("CITA", t =>
            {
                t.HasCheckConstraint("CK_Cita_Estado", "[estado] IN ('Agendada', 'Confirmada', 'Atendida', 'Cancelada', 'NoAsistio')");
                t.HasCheckConstraint("CK_Cita_Duracion", "[duracion_minutos] BETWEEN 15 AND 120");
            });
            entity.HasKey(e => e.IdCita);
            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.FechaHora).HasColumnName("fecha_hora");
            entity.Property(e => e.DuracionMinutos).HasColumnName("duracion_minutos");
            entity.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(300);
            // el enum se guarda como texto (Agendada, Confirmada, ...)
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>();
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdVeterinario).HasColumnName("id_veterinario");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");

            // busqueda de topes de horario y agenda del dia
            entity.HasIndex(e => new { e.IdVeterinario, e.FechaHora }).HasDatabaseName("IX_Cita_Veterinario_Fecha");
        });

        modelBuilder.Entity<Atencion>(entity =>
        {
            entity.ToTable("ATENCION");
            entity.HasKey(e => e.IdAtencion);
            entity.Property(e => e.IdAtencion).HasColumnName("id_atencion");
            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Diagnostico).HasColumnName("diagnostico").HasMaxLength(500);
            entity.Property(e => e.Tratamiento).HasColumnName("tratamiento").HasMaxLength(500);
            entity.Property(e => e.Peso).HasColumnName("peso").HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Temperatura).HasColumnName("temperatura").HasColumnType("decimal(4, 1)");
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdVeterinario).HasColumnName("id_veterinario");

            // una cita genera como maximo una atencion (1 a 0..1)
            entity.HasIndex(e => e.IdCita).IsUnique().HasDatabaseName("UQ_Atencion_Cita");
            entity.HasIndex(e => e.IdMascota).HasDatabaseName("IX_Atencion_Mascota");

            entity.HasOne(d => d.Cita).WithOne(p => p.Atencion)
                .HasForeignKey<Atencion>(d => d.IdCita)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Atencion_Cita");
        });

        modelBuilder.Entity<Vacuna>(entity =>
        {
            entity.ToTable("VACUNA");
            entity.HasKey(e => e.IdVacuna);
            entity.Property(e => e.IdVacuna).HasColumnName("id_vacuna");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100);
            entity.Property(e => e.IdEspecie).HasColumnName("id_especie");
            entity.Property(e => e.IntervaloDias).HasColumnName("intervalo_dias");
        });

        modelBuilder.Entity<Vacunacion>(entity =>
        {
            entity.ToTable("VACUNACION");
            entity.HasKey(e => e.IdVacunacion);
            entity.Property(e => e.IdVacunacion).HasColumnName("id_vacunacion");
            entity.Property(e => e.FechaAplicacion).HasColumnName("fecha_aplicacion");
            entity.Property(e => e.FechaProxima).HasColumnName("fecha_proxima");
            entity.Property(e => e.Lote).HasColumnName("lote").HasMaxLength(50);
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.IdVacuna).HasColumnName("id_vacuna");
            entity.Property(e => e.IdVeterinario).HasColumnName("id_veterinario");
            entity.Property(e => e.IdAtencion).HasColumnName("id_atencion");

            entity.HasIndex(e => e.IdMascota).HasDatabaseName("IX_Vacunacion_Mascota");

            entity.HasOne(d => d.Vacuna).WithMany(p => p.Vacunaciones)
                .HasForeignKey(d => d.IdVacuna)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Vacunacion_Vacuna");

            entity.HasOne(d => d.Atencion).WithMany(p => p.Vacunaciones)
                .HasForeignKey(d => d.IdAtencion)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Vacunacion_Atencion");
        });

        // ================== DATOS INICIALES ==================
        // id_especie: 1 = Perro, 2 = Gato (datos iniciales de la API Clinica)
        modelBuilder.Entity<Vacuna>().HasData(
            new Vacuna { IdVacuna = 1, Nombre = "Antirrábica canina", IdEspecie = 1, IntervaloDias = 365 },
            new Vacuna { IdVacuna = 2, Nombre = "Óctuple canina", IdEspecie = 1, IntervaloDias = 365 },
            new Vacuna { IdVacuna = 3, Nombre = "Tos de las perreras", IdEspecie = 1, IntervaloDias = 365 },
            new Vacuna { IdVacuna = 4, Nombre = "Antirrábica felina", IdEspecie = 2, IntervaloDias = 365 },
            new Vacuna { IdVacuna = 5, Nombre = "Triple felina", IdEspecie = 2, IntervaloDias = 365 },
            new Vacuna { IdVacuna = 6, Nombre = "Leucemia felina", IdEspecie = 2, IntervaloDias = 365 });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
