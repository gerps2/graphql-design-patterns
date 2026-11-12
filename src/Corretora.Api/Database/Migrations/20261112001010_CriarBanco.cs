using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Corretora.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class CriarBanco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'6', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    endereco_rua = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    endereco_numero = table.Column<int>(type: "integer", nullable: false),
                    endereco_cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "posicoes",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    saldo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posicoes", x => new { x.usuario_id, x.ativo });
                    table.ForeignKey(
                        name: "fk_posicoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rentabilidades",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    percentual_mes = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rentabilidades", x => new { x.usuario_id, x.ativo });
                    table.ForeignKey(
                        name: "fk_rentabilidades_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'5', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transacoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_transacoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "id", "endereco_cidade", "endereco_numero", "endereco_rua", "cpf", "email", "nome", "telefone" },
                values: new object[,]
                {
                    { 1, "São Paulo", 10, "Rua das Flores", "123.456.789-00", "ana@corretora.com", "Ana Souza", "11 99999-0001" },
                    { 2, "São Paulo", 1000, "Av. Paulista", "987.654.321-00", "bruno@corretora.com", "Bruno Lima", "11 99999-0002" },
                    { 3, "São Paulo", 200, "Rua Augusta", "111.222.333-44", "carla@corretora.com", "Carla Dias", "11 99999-0003" },
                    { 4, "Campinas", 30, "Rua da Paz", "555.666.777-88", "diego@corretora.com", "Diego Rocha", "11 99999-0004" },
                    { 5, "Santos", 500, "Av. Brasil", "999.888.777-66", "elisa@corretora.com", "Elisa Melo", "11 99999-0005" }
                });

            migrationBuilder.InsertData(
                table: "posicoes",
                columns: new[] { "ativo", "usuario_id", "quantidade", "saldo" },
                values: new object[,]
                {
                    { "HGLG11", 1, 10m, 1590.00m },
                    { "ITUB4", 1, 50m, 1620.00m },
                    { "PETR4", 1, 100m, 3850.00m },
                    { "VALE3", 2, 30m, 1830.00m },
                    { "PETR4", 3, 20m, 770.00m },
                    { "ITUB4", 4, 40m, 1296.00m },
                    { "HGLG11", 5, 5m, 795.00m }
                });

            migrationBuilder.InsertData(
                table: "rentabilidades",
                columns: new[] { "ativo", "usuario_id", "percentual_mes" },
                values: new object[,]
                {
                    { "HGLG11", 1, 0.7m },
                    { "ITUB4", 1, 1.1m },
                    { "PETR4", 1, 2.4m },
                    { "VALE3", 2, -0.8m }
                });

            migrationBuilder.InsertData(
                table: "transacoes",
                columns: new[] { "id", "ativo", "data", "tipo", "usuario_id", "valor" },
                values: new object[,]
                {
                    { 1, "PETR4", new DateOnly(2026, 10, 20), "Compra", 1, 1925.00m },
                    { 2, "ITUB4", new DateOnly(2026, 10, 22), "Compra", 1, 810.00m },
                    { 3, "HGLG11", new DateOnly(2026, 10, 28), "Venda", 1, 318.00m },
                    { 4, "VALE3", new DateOnly(2026, 10, 25), "Compra", 2, 1830.00m }
                });

            migrationBuilder.CreateIndex(
                name: "ix_transacoes_usuario_id",
                table: "transacoes",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "posicoes");

            migrationBuilder.DropTable(
                name: "rentabilidades");

            migrationBuilder.DropTable(
                name: "transacoes");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
