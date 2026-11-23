using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionLigaEsports.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jugadores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jugadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Torneos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Capacidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torneos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipoJugador",
                columns: table => new
                {
                    EquipoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JugadorId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoJugador", x => new { x.EquipoId, x.JugadorId });
                    table.ForeignKey(
                        name: "FK_EquipoJugador_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipoJugador_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TorneoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grupos_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TorneoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GrupoEquipo",
                columns: table => new
                {
                    GrupoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoEquipo", x => new { x.GrupoId, x.EquipoId });
                    table.ForeignKey(
                        name: "FK_GrupoEquipo_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GrupoEquipo_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TorneoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GrupoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EquipoAId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EquipoBId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Fase = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    RondasEquipoA = table.Column<int>(type: "INTEGER", nullable: true),
                    RondasEquipoB = table.Column<int>(type: "INTEGER", nullable: true),
                    GanadorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaResultado = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GrupoOrigenEquipoAId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PosicionGrupoOrigenEquipoA = table.Column<int>(type: "INTEGER", nullable: true),
                    GrupoOrigenEquipoBId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PosicionGrupoOrigenEquipoB = table.Column<int>(type: "INTEGER", nullable: true),
                    PartidaOrigenEquipoAId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PartidaOrigenEquipoBId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partidas_Equipos_EquipoAId",
                        column: x => x.EquipoAId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Equipos_EquipoBId",
                        column: x => x.EquipoBId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Equipos_GanadorId",
                        column: x => x.GanadorId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Grupos_GrupoOrigenEquipoAId",
                        column: x => x.GrupoOrigenEquipoAId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Grupos_GrupoOrigenEquipoBId",
                        column: x => x.GrupoOrigenEquipoBId,
                        principalTable: "Grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Partidas_PartidaOrigenEquipoAId",
                        column: x => x.PartidaOrigenEquipoAId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Partidas_PartidaOrigenEquipoBId",
                        column: x => x.PartidaOrigenEquipoBId,
                        principalTable: "Partidas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Torneos_TorneoId",
                        column: x => x.TorneoId,
                        principalTable: "Torneos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipoJugador_JugadorId",
                table: "EquipoJugador",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoEquipo_EquipoId",
                table: "GrupoEquipo",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Grupos_TorneoId",
                table: "Grupos",
                column: "TorneoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_EquipoId_TorneoId",
                table: "Inscripciones",
                columns: new[] { "EquipoId", "TorneoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_TorneoId",
                table: "Inscripciones",
                column: "TorneoId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_EquipoAId",
                table: "Partidas",
                column: "EquipoAId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_EquipoBId",
                table: "Partidas",
                column: "EquipoBId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_GanadorId",
                table: "Partidas",
                column: "GanadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_GrupoId",
                table: "Partidas",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_GrupoOrigenEquipoAId",
                table: "Partidas",
                column: "GrupoOrigenEquipoAId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_GrupoOrigenEquipoBId",
                table: "Partidas",
                column: "GrupoOrigenEquipoBId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_PartidaOrigenEquipoAId",
                table: "Partidas",
                column: "PartidaOrigenEquipoAId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_PartidaOrigenEquipoBId",
                table: "Partidas",
                column: "PartidaOrigenEquipoBId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_TorneoId",
                table: "Partidas",
                column: "TorneoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipoJugador");

            migrationBuilder.DropTable(
                name: "GrupoEquipo");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "Partidas");

            migrationBuilder.DropTable(
                name: "Jugadores");

            migrationBuilder.DropTable(
                name: "Equipos");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Torneos");
        }
    }
}
