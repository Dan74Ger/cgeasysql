using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CGEasy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionistaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "professionisti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    cognome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    attivo = table.Column<bool>(type: "bit", nullable: false),
                    data_attivazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_modifica = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_cessazione = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professionisti", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Professionisti_Attivo",
                table: "professionisti",
                column: "attivo");

            migrationBuilder.CreateIndex(
                name: "IX_Professionisti_Cognome",
                table: "professionisti",
                column: "cognome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "professionisti");
        }
    }
}
