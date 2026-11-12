using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Corretora.Api.Database.Migrations
{
    /// <inheritdoc />
    public partial class ConcorrenciaNaConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "contas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddCheckConstraint(
                name: "ck_contas_saldo_nao_negativo",
                table: "contas",
                sql: "saldo >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_contas_saldo_nao_negativo",
                table: "contas");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "contas");
        }
    }
}
