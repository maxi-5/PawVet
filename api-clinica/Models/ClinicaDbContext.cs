using Microsoft.EntityFrameworkCore;

namespace PawVetClinica.Models;

// Base de datos PawVetClinicaDB: la usa SOLO esta API (ADR-001)
public partial class ClinicaDbContext : DbContext
{
    public ClinicaDbContext()
    {
    }

    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Rol> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Veterinario> Veterinarios { get; set; }

    public virtual DbSet<Tutor> Tutores { get; set; }

    public virtual DbSet<Especie> Especies { get; set; }

    public virtual DbSet<Raza> Razas { get; set; }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("ROL");
            entity.HasKey(e => e.IdRol);
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50);
            entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_Rol_Nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");
            entity.HasKey(e => e.IdUser);
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(150);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasColumnName("password").HasMaxLength(100);
            entity.Property(e => e.IdRol).HasColumnName("id_rol");

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Usuario_Email");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Usuario_Rol");
        });

        modelBuilder.Entity<Veterinario>(entity =>
        {
            entity.ToTable("VETERINARIO");
            entity.HasKey(e => e.IdVeterinario);
            entity.Property(e => e.IdVeterinario).HasColumnName("id_veterinario");
            entity.Property(e => e.Rut).HasColumnName("rut").HasMaxLength(12);
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(150);
            entity.Property(e => e.Especialidad).HasColumnName("especialidad").HasMaxLength(100);
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasIndex(e => e.Rut).IsUnique().HasDatabaseName("UQ_Veterinario_Rut");
            // un usuario es la cuenta de como maximo un veterinario (relacion 0..1 - 0..1)
            entity.HasIndex(e => e.IdUser).IsUnique().HasFilter("[id_user] IS NOT NULL").HasDatabaseName("UQ_Veterinario_Usuario");

            // si se elimina el usuario, el veterinario queda sin cuenta
            entity.HasOne(d => d.Usuario).WithOne(p => p.Veterinario)
                .HasForeignKey<Veterinario>(d => d.IdUser)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Veterinario_Usuario");
        });

        modelBuilder.Entity<Tutor>(entity =>
        {
            entity.ToTable("TUTOR");
            entity.HasKey(e => e.IdTutor);
            entity.Property(e => e.IdTutor).HasColumnName("id_tutor");
            entity.Property(e => e.Rut).HasColumnName("rut").HasMaxLength(12);
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(150);
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(e => e.Direccion).HasColumnName("direccion").HasMaxLength(200);

            entity.HasIndex(e => e.Rut).IsUnique().HasDatabaseName("UQ_Tutor_Rut");
        });

        modelBuilder.Entity<Especie>(entity =>
        {
            entity.ToTable("ESPECIE");
            entity.HasKey(e => e.IdEspecie);
            entity.Property(e => e.IdEspecie).HasColumnName("id_especie");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50);
            entity.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("UQ_Especie_Nombre");
        });

        modelBuilder.Entity<Raza>(entity =>
        {
            entity.ToTable("RAZA");
            entity.HasKey(e => e.IdRaza);
            entity.Property(e => e.IdRaza).HasColumnName("id_raza");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(80);
            entity.Property(e => e.IdEspecie).HasColumnName("id_especie");

            entity.HasOne(d => d.Especie).WithMany(p => p.Razas)
                .HasForeignKey(d => d.IdEspecie)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Raza_Especie");
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.ToTable("MASCOTA", t => t.HasCheckConstraint("CK_Mascota_Sexo", "[sexo] IN ('Macho', 'Hembra')"));
            entity.HasKey(e => e.IdMascota);
            entity.Property(e => e.IdMascota).HasColumnName("id_mascota");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(60);
            entity.Property(e => e.Sexo).HasColumnName("sexo").HasMaxLength(10);
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
            entity.Property(e => e.Peso).HasColumnName("peso").HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Microchip).HasColumnName("microchip").HasMaxLength(15);
            entity.Property(e => e.IdTutor).HasColumnName("id_tutor");
            entity.Property(e => e.IdEspecie).HasColumnName("id_especie");
            entity.Property(e => e.IdRaza).HasColumnName("id_raza");

            // unico solo cuando tiene valor (el microchip es opcional) (RF-06)
            entity.HasIndex(e => e.Microchip).IsUnique().HasFilter("[microchip] IS NOT NULL").HasDatabaseName("UQ_Mascota_Microchip");

            // no se elimina un tutor con mascotas: la API responde 409
            entity.HasOne(d => d.Tutor).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.IdTutor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascota_Tutor");

            entity.HasOne(d => d.Especie).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.IdEspecie)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Mascota_Especie");

            entity.HasOne(d => d.Raza).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.IdRaza)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Mascota_Raza");
        });

        // ================== DATOS INICIALES ==================
        modelBuilder.Entity<Rol>().HasData(
            new Rol { IdRol = Rol.ADMINISTRADOR, Nombre = "Administrador" },
            new Rol { IdRol = Rol.RECEPCIONISTA, Nombre = "Recepcionista" },
            new Rol { IdRol = Rol.VETERINARIO, Nombre = "Veterinario" });

        modelBuilder.Entity<Especie>().HasData(
            new Especie { IdEspecie = 1, Nombre = "Perro" },
            new Especie { IdEspecie = 2, Nombre = "Gato" });

        modelBuilder.Entity<Raza>().HasData(
            new Raza { IdRaza = 1, Nombre = "Labrador Retriever", IdEspecie = 1 },
            new Raza { IdRaza = 2, Nombre = "Pastor Alemán", IdEspecie = 1 },
            new Raza { IdRaza = 3, Nombre = "Poodle", IdEspecie = 1 },
            new Raza { IdRaza = 4, Nombre = "Quiltro", IdEspecie = 1 },
            new Raza { IdRaza = 5, Nombre = "Siamés", IdEspecie = 2 },
            new Raza { IdRaza = 6, Nombre = "Persa", IdEspecie = 2 },
            new Raza { IdRaza = 7, Nombre = "Doméstico de pelo corto", IdEspecie = 2 });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
