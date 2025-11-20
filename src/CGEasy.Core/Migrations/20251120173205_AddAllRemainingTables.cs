using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CGEasy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAllRemainingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "argomenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    utente_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_argomenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "associazione_mastrini",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    cliente_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    mese = table.Column<int>(type: "int", nullable: false),
                    anno = table.Column<int>(type: "int", nullable: false),
                    bilancio_descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    template_id = table.Column<int>(type: "int", nullable: true),
                    template_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    descrizione = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    creato_by = table.Column<int>(type: "int", nullable: false),
                    creato_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    data_modifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    numero_mappature = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_associazione_mastrini", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "associazione_mastrini_dettagli",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    associazione_id = table.Column<int>(type: "int", nullable: false),
                    codice_mastrino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descrizione_mastrino = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    importo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    template_voce_id = table.Column<int>(type: "int", nullable: true),
                    template_codice = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    template_descrizione = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    template_segno = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_associazione_mastrini_dettagli", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_utente = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    azione = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entita = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    id_entita = table.Column<int>(type: "int", nullable: true),
                    descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    valori_precedenti = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    valori_nuovi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "banca_incassi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    banca_id = table.Column<int>(type: "int", nullable: false),
                    nome_cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    anno = table.Column<int>(type: "int", nullable: false),
                    mese = table.Column<int>(type: "int", nullable: false),
                    importo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    percentuale_anticipo = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    data_inizio_anticipo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    anticipo_gestito_cc = table.Column<bool>(type: "bit", nullable: false),
                    data_scadenza_anticipo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    anticipo_chiuso_cc = table.Column<bool>(type: "bit", nullable: false),
                    incassato = table.Column<bool>(type: "bit", nullable: false),
                    data_scadenza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_incasso_effettivo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    numero_fattura_cliente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    data_fattura_cliente = table.Column<DateTime>(type: "datetime2", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_ultima_modifica = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banca_incassi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "banca_pagamenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    banca_id = table.Column<int>(type: "int", nullable: false),
                    nome_fornitore = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    anno = table.Column<int>(type: "int", nullable: false),
                    mese = table.Column<int>(type: "int", nullable: false),
                    importo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    percentuale_anticipo = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    data_inizio_anticipo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    data_scadenza_anticipo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    pagato = table.Column<bool>(type: "bit", nullable: false),
                    data_scadenza = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_pagamento_effettivo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    numero_fattura_fornitore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    data_fattura_fornitore = table.Column<DateTime>(type: "datetime2", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_ultima_modifica = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banca_pagamenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "banca_saldo_giornaliero",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    banca_id = table.Column<int>(type: "int", nullable: false),
                    data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banca_saldo_giornaliero", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "banca_utilizzo_anticipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    banca_id = table.Column<int>(type: "int", nullable: false),
                    fatturato = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    percentuale_anticipo = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    importo_utilizzo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    data_inizio_utilizzo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_scadenza_utilizzo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    rimborsato = table.Column<bool>(type: "bit", nullable: false),
                    data_rimborso_effettivo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    interessi_maturati = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_ultima_modifica = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banca_utilizzo_anticipo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "banche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_banca = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    codice_identificativo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    iban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    saldo_del_giorno = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fido_cc_accordato = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    anticipo_fatture_massimo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    interesse_anticipo_fatture = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_ultima_modifica = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bilancio_contabile",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    cliente_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    mese = table.Column<int>(type: "int", nullable: false),
                    anno = table.Column<int>(type: "int", nullable: false),
                    descrizione_bilancio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    tipo_bilancio = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    codice_mastrino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descrizione_mastrino = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    importo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_import = table.Column<DateTime>(type: "datetime2", nullable: false),
                    imported_by = table.Column<int>(type: "int", nullable: false),
                    imported_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bilancio_contabile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bilancio_template",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    cliente_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    mese = table.Column<int>(type: "int", nullable: false),
                    anno = table.Column<int>(type: "int", nullable: false),
                    descrizione_bilancio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    tipo_bilancio = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    codice_mastrino = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descrizione_mastrino = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    importo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_import = table.Column<DateTime>(type: "datetime2", nullable: false),
                    imported_by = table.Column<int>(type: "int", nullable: false),
                    imported_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    segno = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    formula = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ordine = table.Column<int>(type: "int", nullable: false),
                    livello = table.Column<int>(type: "int", nullable: false),
                    gruppo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    gruppo_padre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bilancio_template", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "circolari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    argomento_id = table.Column<int>(type: "int", nullable: false),
                    descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    anno = table.Column<int>(type: "int", nullable: false),
                    nome_file = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    percorso_file = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    data_importazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    utente_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_circolari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    id_professionista = table.Column<int>(type: "int", nullable: true),
                    mail_cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cf_cliente = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    piva_cliente = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    codice_ateco = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    indirizzo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    citta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    provincia = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    cap = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    legale_rappresentante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cf_legale_rappresentante = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    attivo = table.Column<bool>(type: "bit", nullable: false),
                    data_attivazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_modifica = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_cessazione = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clienti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "finanziamento_import",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_finanziamento = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    data_inizio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    data_fine = table.Column<DateTime>(type: "datetime2", nullable: false),
                    importo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    banca_id = table.Column<int>(type: "int", nullable: false),
                    nome_fornitore = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    numero_fattura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    data_fattura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    incasso_id = table.Column<int>(type: "int", nullable: false),
                    pagamento_id = table.Column<int>(type: "int", nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    utente_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finanziamento_import", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "indice_configurazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    nome_indice = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    formula = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    unita_misura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_abilitato = table.Column<bool>(type: "bit", nullable: false),
                    is_standard = table.Column<bool>(type: "bit", nullable: false),
                    codice_indice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dettagli_calcolo_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    descrizione = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    utente_id = table.Column<int>(type: "int", nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ultima_modifica = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    ordinamento = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indice_configurazione", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "indici_personalizzati",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    nome_indice = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    operatore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    moltiplicatore = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    unita_misura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    codici_numeratore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    codici_denominatore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    operazione_numeratore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    operazione_denominatore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    creato_by = table.Column<int>(type: "int", nullable: false),
                    data_modifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ordine = table.Column<int>(type: "int", nullable: false),
                    attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indici_personalizzati", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "license_clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    telefono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    partita_iva = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_registrazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "license_keys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    license_client_id = table.Column<int>(type: "int", nullable: false),
                    module_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    full_key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    license_guid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    data_generazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    data_scadenza = table.Column<DateTime>(type: "datetime2", nullable: true),
                    generated_by_user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "statistica_ce_salvata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_statistica = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    nome_cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    template_mese = table.Column<int>(type: "int", nullable: false),
                    template_anno = table.Column<int>(type: "int", nullable: false),
                    template_descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    periodi_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dati_statistiche_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    utente_id = table.Column<int>(type: "int", nullable: false),
                    nome_utente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statistica_ce_salvata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "statistica_sp_salvata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_statistica = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    nome_cliente = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    template_mese = table.Column<int>(type: "int", nullable: false),
                    template_anno = table.Column<int>(type: "int", nullable: false),
                    template_descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    periodi_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dati_statistiche_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    utente_id = table.Column<int>(type: "int", nullable: false),
                    nome_utente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statistica_sp_salvata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_pratiche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nome_pratica = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descrizione = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    categoria = table.Column<int>(type: "int", nullable: false),
                    priorita_default = table.Column<int>(type: "int", nullable: false),
                    durata_stimata_giorni = table.Column<int>(type: "int", nullable: true),
                    attivo = table.Column<bool>(type: "bit", nullable: false),
                    ordine = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_pratiche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "todo_studio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titolo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    descrizione = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    categoria = table.Column<int>(type: "int", nullable: false),
                    priorita = table.Column<int>(type: "int", nullable: false),
                    stato = table.Column<int>(type: "int", nullable: false),
                    tipo_pratica_id = table.Column<int>(type: "int", nullable: true),
                    tipo_pratica_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    cliente_id = table.Column<int>(type: "int", nullable: true),
                    cliente_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    creatore_id = table.Column<int>(type: "int", nullable: false),
                    creatore_nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    professionisti_assegnati_ids_json = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    professionisti_assegnati_nomi_json = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    data_creazione = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    data_inizio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    data_scadenza = table.Column<DateTime>(type: "datetime2", nullable: true),
                    data_completamento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    data_ultima_modifica = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    orario_inizio = table.Column<TimeSpan>(type: "time", nullable: true),
                    orario_fine = table.Column<TimeSpan>(type: "time", nullable: true),
                    allegati_json = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_todo_studio", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Argomenti_Nome",
                table: "argomenti",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssociazioneMastrino_AnnoMese",
                table: "associazione_mastrini",
                columns: new[] { "anno", "mese" });

            migrationBuilder.CreateIndex(
                name: "IX_AssociazioneMastrino_ClienteId",
                table: "associazione_mastrini",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_AssociazioneMastrinoDettaglio_AssociazioneId",
                table: "associazione_mastrini_dettagli",
                column: "associazione_id");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Azione",
                table: "audit_logs",
                column: "azione");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Entita",
                table: "audit_logs",
                column: "entita");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_IdUtente",
                table: "audit_logs",
                column: "id_utente");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_BancaIncassi_Anno",
                table: "banca_incassi",
                column: "anno");

            migrationBuilder.CreateIndex(
                name: "IX_BancaIncassi_BancaId",
                table: "banca_incassi",
                column: "banca_id");

            migrationBuilder.CreateIndex(
                name: "IX_BancaIncassi_DataScadenza",
                table: "banca_incassi",
                column: "data_scadenza");

            migrationBuilder.CreateIndex(
                name: "IX_BancaIncassi_Mese",
                table: "banca_incassi",
                column: "mese");

            migrationBuilder.CreateIndex(
                name: "IX_BancaPagamenti_Anno",
                table: "banca_pagamenti",
                column: "anno");

            migrationBuilder.CreateIndex(
                name: "IX_BancaPagamenti_BancaId",
                table: "banca_pagamenti",
                column: "banca_id");

            migrationBuilder.CreateIndex(
                name: "IX_BancaPagamenti_DataScadenza",
                table: "banca_pagamenti",
                column: "data_scadenza");

            migrationBuilder.CreateIndex(
                name: "IX_BancaPagamenti_Mese",
                table: "banca_pagamenti",
                column: "mese");

            migrationBuilder.CreateIndex(
                name: "IX_BancaSaldoGiornaliero_BancaId",
                table: "banca_saldo_giornaliero",
                column: "banca_id");

            migrationBuilder.CreateIndex(
                name: "IX_BancaSaldoGiornaliero_Data",
                table: "banca_saldo_giornaliero",
                column: "data");

            migrationBuilder.CreateIndex(
                name: "IX_BancaUtilizzoAnticipo_BancaId",
                table: "banca_utilizzo_anticipo",
                column: "banca_id");

            migrationBuilder.CreateIndex(
                name: "IX_BancaUtilizzoAnticipo_DataInizio",
                table: "banca_utilizzo_anticipo",
                column: "data_inizio_utilizzo");

            migrationBuilder.CreateIndex(
                name: "IX_BancaUtilizzoAnticipo_DataScadenza",
                table: "banca_utilizzo_anticipo",
                column: "data_scadenza_utilizzo");

            migrationBuilder.CreateIndex(
                name: "IX_Banche_NomeBanca",
                table: "banche",
                column: "nome_banca");

            migrationBuilder.CreateIndex(
                name: "IX_BilancioContabile_AnnoMese",
                table: "bilancio_contabile",
                columns: new[] { "anno", "mese" });

            migrationBuilder.CreateIndex(
                name: "IX_BilancioContabile_ClienteId",
                table: "bilancio_contabile",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_BilancioTemplate_AnnoMese",
                table: "bilancio_template",
                columns: new[] { "anno", "mese" });

            migrationBuilder.CreateIndex(
                name: "IX_BilancioTemplate_ClienteId",
                table: "bilancio_template",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Circolari_Anno",
                table: "circolari",
                column: "anno");

            migrationBuilder.CreateIndex(
                name: "IX_Circolari_ArgomentoId",
                table: "circolari",
                column: "argomento_id");

            migrationBuilder.CreateIndex(
                name: "IX_Circolari_DataImportazione",
                table: "circolari",
                column: "data_importazione");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_Attivo",
                table: "clienti",
                column: "attivo");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_CfCliente",
                table: "clienti",
                column: "cf_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_IdProfessionista",
                table: "clienti",
                column: "id_professionista");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_NomeCliente",
                table: "clienti",
                column: "nome_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Clienti_PivaCliente",
                table: "clienti",
                column: "piva_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_FinanziamentoImport_BancaId",
                table: "finanziamento_import",
                column: "banca_id");

            migrationBuilder.CreateIndex(
                name: "IX_FinanziamentoImport_DataInizio",
                table: "finanziamento_import",
                column: "data_inizio");

            migrationBuilder.CreateIndex(
                name: "IX_IndiceConfigurazione_Categoria",
                table: "indice_configurazione",
                column: "categoria");

            migrationBuilder.CreateIndex(
                name: "IX_IndiceConfigurazione_ClienteId",
                table: "indice_configurazione",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_IndiceConfigurazione_CodiceIndice",
                table: "indice_configurazione",
                column: "codice_indice");

            migrationBuilder.CreateIndex(
                name: "IX_IndiceConfigurazione_IsAbilitato",
                table: "indice_configurazione",
                column: "is_abilitato");

            migrationBuilder.CreateIndex(
                name: "IX_IndicePersonalizzato_ClienteId",
                table: "indici_personalizzati",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseClients_Email",
                table: "license_clients",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseClients_IsActive",
                table: "license_clients",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseClients_NomeCliente",
                table: "license_clients",
                column: "nome_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeys_ClientId",
                table: "license_keys",
                column: "license_client_id");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeys_Guid",
                table: "license_keys",
                column: "license_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeys_IsActive",
                table: "license_keys",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeys_ModuleName",
                table: "license_keys",
                column: "module_name");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticaCESalvata_ClienteId",
                table: "statistica_ce_salvata",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticaSPSalvata_ClienteId",
                table: "statistica_sp_salvata",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_TipoPratiche_Attivo",
                table: "tipo_pratiche",
                column: "attivo");

            migrationBuilder.CreateIndex(
                name: "IX_TipoPratiche_Categoria",
                table: "tipo_pratiche",
                column: "categoria");

            migrationBuilder.CreateIndex(
                name: "IX_TipoPratiche_NomePratica",
                table: "tipo_pratiche",
                column: "nome_pratica");

            migrationBuilder.CreateIndex(
                name: "IX_TipoPratiche_Ordine",
                table: "tipo_pratiche",
                column: "ordine");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_Categoria",
                table: "todo_studio",
                column: "categoria");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_ClienteId",
                table: "todo_studio",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_CreatoreId",
                table: "todo_studio",
                column: "creatore_id");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_DataCreazione",
                table: "todo_studio",
                column: "data_creazione");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_DataScadenza",
                table: "todo_studio",
                column: "data_scadenza");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_Priorita",
                table: "todo_studio",
                column: "priorita");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_Stato",
                table: "todo_studio",
                column: "stato");

            migrationBuilder.CreateIndex(
                name: "IX_TodoStudio_TipoPraticaId",
                table: "todo_studio",
                column: "tipo_pratica_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "argomenti");

            migrationBuilder.DropTable(
                name: "associazione_mastrini");

            migrationBuilder.DropTable(
                name: "associazione_mastrini_dettagli");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "banca_incassi");

            migrationBuilder.DropTable(
                name: "banca_pagamenti");

            migrationBuilder.DropTable(
                name: "banca_saldo_giornaliero");

            migrationBuilder.DropTable(
                name: "banca_utilizzo_anticipo");

            migrationBuilder.DropTable(
                name: "banche");

            migrationBuilder.DropTable(
                name: "bilancio_contabile");

            migrationBuilder.DropTable(
                name: "bilancio_template");

            migrationBuilder.DropTable(
                name: "circolari");

            migrationBuilder.DropTable(
                name: "clienti");

            migrationBuilder.DropTable(
                name: "finanziamento_import");

            migrationBuilder.DropTable(
                name: "indice_configurazione");

            migrationBuilder.DropTable(
                name: "indici_personalizzati");

            migrationBuilder.DropTable(
                name: "license_clients");

            migrationBuilder.DropTable(
                name: "license_keys");

            migrationBuilder.DropTable(
                name: "statistica_ce_salvata");

            migrationBuilder.DropTable(
                name: "statistica_sp_salvata");

            migrationBuilder.DropTable(
                name: "tipo_pratiche");

            migrationBuilder.DropTable(
                name: "todo_studio");
        }
    }
}
