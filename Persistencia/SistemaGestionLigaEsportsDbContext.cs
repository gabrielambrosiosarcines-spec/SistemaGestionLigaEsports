using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SistemaGestionLigaEsports.Modelos;

namespace SistemaGestionLigaEsports.Persistencia;

public sealed class SistemaGestionLigaEsportsDbContext
    : IdentityDbContext<IdentityUser>
{
    public SistemaGestionLigaEsportsDbContext(
        DbContextOptions<SistemaGestionLigaEsportsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Jugador> Jugadores => Set<Jugador>();

    public DbSet<Equipo> Equipos => Set<Equipo>();

    public DbSet<Torneo> Torneos => Set<Torneo>();

    public DbSet<Inscripcion> Inscripciones => Set<Inscripcion>();

    public DbSet<Grupo> Grupos => Set<Grupo>();

    public DbSet<Partida> Partidas => Set<Partida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jugador = modelBuilder.Entity<Jugador>();
        jugador.HasKey(entidad => entidad.Id);

        var equipo = modelBuilder.Entity<Equipo>();
        equipo.HasKey(entidad => entidad.Id);
        equipo.Ignore(entidad => entidad.JugadorIds);

        var torneo = modelBuilder.Entity<Torneo>();
        torneo.HasKey(entidad => entidad.Id);

        var inscripcion = modelBuilder.Entity<Inscripcion>();
        inscripcion.HasKey(entidad => entidad.Id);
        inscripcion.HasIndex(entidad => new
        {
            entidad.EquipoId,
            entidad.TorneoId
        }).IsUnique();
        inscripcion
            .HasOne<Equipo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.EquipoId)
            .OnDelete(DeleteBehavior.Restrict);
        inscripcion
            .HasOne<Torneo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.TorneoId)
            .OnDelete(DeleteBehavior.Restrict);

        var grupo = modelBuilder.Entity<Grupo>();
        grupo.HasKey(entidad => entidad.Id);
        grupo.Ignore(entidad => entidad.EquipoIds);
        grupo
            .HasOne<Torneo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.TorneoId)
            .OnDelete(DeleteBehavior.Restrict);

        var partida = modelBuilder.Entity<Partida>();
        partida.HasKey(entidad => entidad.Id);
        partida
            .HasOne<Torneo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.TorneoId)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Grupo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.GrupoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Equipo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.EquipoAId)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Equipo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.EquipoBId)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Equipo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.GanadorId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Grupo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.GrupoOrigenEquipoAId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Grupo>()
            .WithMany()
            .HasForeignKey(entidad => entidad.GrupoOrigenEquipoBId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Partida>()
            .WithMany()
            .HasForeignKey(entidad => entidad.PartidaOrigenEquipoAId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        partida
            .HasOne<Partida>()
            .WithMany()
            .HasForeignKey(entidad => entidad.PartidaOrigenEquipoBId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        equipo
            .HasMany<Jugador>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "EquipoJugador",
                configuracion => configuracion
                    .HasOne<Jugador>()
                    .WithMany()
                    .HasForeignKey("JugadorId")
                    .OnDelete(DeleteBehavior.Restrict),
                configuracion => configuracion
                    .HasOne<Equipo>()
                    .WithMany()
                    .HasForeignKey("EquipoId")
                    .OnDelete(DeleteBehavior.Restrict),
                configuracion =>
                {
                    configuracion
                        .HasKey("EquipoId", "JugadorId");
                });

        grupo
            .HasMany<Equipo>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "GrupoEquipo",
                configuracion => configuracion
                    .HasOne<Equipo>()
                    .WithMany()
                    .HasForeignKey("EquipoId")
                    .OnDelete(DeleteBehavior.Restrict),
                configuracion => configuracion
                    .HasOne<Grupo>()
                    .WithMany()
                    .HasForeignKey("GrupoId")
                    .OnDelete(DeleteBehavior.Restrict),
                configuracion =>
                {
                    configuracion
                        .HasKey("GrupoId", "EquipoId");
                });
    }
}
