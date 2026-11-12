using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Corretora.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarContasEOrdens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contas",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contas", x => x.usuario_id);
                    table.ForeignKey(
                        name: "fk_contas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ordens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    criada_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ordens", x => x.id);
                    table.ForeignKey(
                        name: "fk_ordens_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "contas",
                columns: new[] { "usuario_id", "saldo" },
                values: new object[,]
                {
                    { 1, 10000.00m },
                    { 2, 5000.00m },
                    { 3, 2000.00m },
                    { 4, 1000.00m },
                    { 5, 1000.00m }
                });

            migrationBuilder.CreateIndex(
                name: "ix_ordens_usuario_id",
                table: "ordens",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contas");

            migrationBuilder.DropTable(
                name: "ordens");
        }
    }
}
