using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CGEasy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddUtentiAndPermissionsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_utente = table.Column<int>(type: "int", nullable: false),
                    modulo_todo = table.Column<bool>(type: "bit", nullable: false),
                    modulo_bilanci = table.Column<bool>(type: "bit", nullable: false),
                    modulo_circolari = table.Column<bool>(type: "bit", nullable: false),
                    modulo_controllo_gestione = table.Column<bool>(type: "bit", nullable: false),
                    clienti_create = table.Column<bool>(type: "bit", nullable: false),
                    clienti_read = table.Column<bool>(type: "bit", nullable: false),
                    clienti_update = table.Column<bool>(type: "bit", nullable: false),
                    clienti_delete = table.Column<bool>(type: "bit", nullable: false),
                    professionisti_create = table.Column<bool>(type: "bit", nullable: false),
                    professionisti_read = table.Column<bool>(type: "bit", nullable: false),
                    professionisti_update = table.Column<bool>(type: "bit", nullable: false),
                    professionisti_delete = table.Column<bool>(type: "bit", nullable: false),
                    tipopratiche_create = table.Column<bool>(type: "bit", nullable: false),
                    tipopratiche_read = table.Column<bool>(type: "bit", nullable: false),
                    tipopratiche_update = table.Column<bool>(type: "bit", nullable: false),
                    tipopratiche_delete = table.Column<bool>(type: "bit", nullable: false),
                    utenti_manage = table.Column<bool>(type: "bit", nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_modifica = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "utenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    cognome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ruolo = table.Column<int>(type: "int", nullable: false),
                    attivo = table.Column<bool>(type: "bit", nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_modifica = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ultimo_accesso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    data_cessazione = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utenti", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_IdUtente",
                table: "user_permissions",
                column: "id_utente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Attivo",
                table: "utenti",
                column: "attivo");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Email",
                table: "utenti",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Ruolo",
                table: "utenti",
                column: "ruolo");

            migrationBuilder.CreateIndex(
                name: "IX_Utenti_Username",
                table: "utenti",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_permissions");

            migrationBuilder.DropTable(
                name: "utenti");
        }
    }
}
