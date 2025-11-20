using Microsoft.EntityFrameworkCore;
using CGEasy.Core.Models;
using System;
using System.IO;

namespace CGEasy.Core.Data
{
    /// <summary>
    /// Entity Framework Core DbContext per SQL Server
    /// Sostituisce gradualmente LiteDbContext
    /// </summary>
    public class CGEasyDbContext : DbContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Costruttore default - usa connection string da file
        /// </summary>
        public CGEasyDbContext() : this(GetDefaultConnectionString())
        {
        }

        /// <summary>
        /// Costruttore con connection string custom
        /// </summary>
        public CGEasyDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Costruttore per DbContextOptions (usato da DI)
        /// </summary>
        public CGEasyDbContext(DbContextOptions<CGEasyDbContext> options) : base(options)
        {
            _connectionString = string.Empty;
        }

        // ===== DBSETS - TABELLA PER TABELLA =====
        
        /// <summary>
        /// TABELLA 1: Professionisti
        /// </summary>
        public DbSet<Professionista> Professionisti { get; set; }

        /// <summary>
        /// TABELLA 2: Utenti (per login/autenticazione)
        /// </summary>
        public DbSet<Utente> Utenti { get; set; }

        /// <summary>
        /// TABELLA 3: User Permissions (permessi utenti)
        /// </summary>
        public DbSet<UserPermissions> UserPermissions { get; set; }

        /// <summary>
        /// TABELLA 4: Clienti
        /// </summary>
        public DbSet<Cliente> Clienti { get; set; }

        /// <summary>
        /// TABELLA 5: Tipo Pratiche
        /// </summary>
        public DbSet<TipoPratica> TipoPratiche { get; set; }

        /// <summary>
        /// TABELLA 6: Audit Logs
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// TABELLA 7: TODO Studio
        /// </summary>
        public DbSet<TodoStudio> TodoStudio { get; set; }

        /// <summary>
        /// TABELLA 8: License Clients
        /// </summary>
        public DbSet<LicenseClient> LicenseClients { get; set; }

        /// <summary>
        /// TABELLA 9: License Keys
        /// </summary>
        public DbSet<LicenseKey> LicenseKeys { get; set; }

        /// <summary>
        /// TABELLA 10: Bilancio Contabile
        /// </summary>
        public DbSet<BilancioContabile> BilancioContabile { get; set; }

        /// <summary>
        /// TABELLA 11: Bilancio Template
        /// </summary>
        public DbSet<BilancioTemplate> BilancioTemplate { get; set; }

        /// <summary>
        /// TABELLA 12: Associazioni Mastrini
        /// </summary>
        public DbSet<AssociazioneMastrino> AssociazioniMastrini { get; set; }

        /// <summary>
        /// TABELLA 13: Associazioni Mastrini Dettagli
        /// </summary>
        public DbSet<AssociazioneMastrinoDettaglio> AssociazioniMastriniDettagli { get; set; }

        /// <summary>
        /// TABELLA 14: Argomenti
        /// </summary>
        public DbSet<Argomento> Argomenti { get; set; }

        /// <summary>
        /// TABELLA 15: Circolari
        /// </summary>
        public DbSet<Circolare> Circolari { get; set; }

        /// <summary>
        /// TABELLA 16: Banche
        /// </summary>
        public DbSet<Banca> Banche { get; set; }

        /// <summary>
        /// TABELLA 17: Banca Incassi
        /// </summary>
        public DbSet<BancaIncasso> BancaIncassi { get; set; }

        /// <summary>
        /// TABELLA 18: Banca Pagamenti
        /// </summary>
        public DbSet<BancaPagamento> BancaPagamenti { get; set; }

        /// <summary>
        /// TABELLA 19: Banca Utilizzo Anticipo
        /// </summary>
        public DbSet<BancaUtilizzoAnticipo> BancaUtilizzoAnticipo { get; set; }

        /// <summary>
        /// TABELLA 20: Banca Saldo Giornaliero
        /// </summary>
        public DbSet<BancaSaldoGiornaliero> BancaSaldoGiornaliero { get; set; }

        /// <summary>
        /// TABELLA 21: Finanziamento Import
        /// </summary>
        public DbSet<FinanziamentoImport> FinanziamentoImport { get; set; }

        /// <summary>
        /// TABELLA 22: Indici Personalizzati
        /// </summary>
        public DbSet<IndicePersonalizzato> IndiciPersonalizzati { get; set; }

        /// <summary>
        /// TABELLA 23: Statistiche CE Salvate
        /// </summary>
        public DbSet<StatisticaCESalvata> StatisticheCESalvate { get; set; }

        /// <summary>
        /// TABELLA 24: Statistiche SP Salvate
        /// </summary>
        public DbSet<StatisticaSPSalvata> StatisticheSPSalvate { get; set; }

        /// <summary>
        /// TABELLA 25: Indici Configurazione
        /// </summary>
        public DbSet<IndiceConfigurazione> IndiciConfigurazione { get; set; }

        /// <summary>
        /// Configurazione SQL Server
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
                
                // Logging per debug (opzionale)
                #if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
                #endif
            }
        }

        /// <summary>
        /// Configurazione modelli (indici, relazioni, constraints)
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== PROFESSIONISTI =====
            ConfigureProfessionista(modelBuilder);

            // ===== UTENTI =====
            ConfigureUtente(modelBuilder);

            // ===== USER PERMISSIONS =====
            ConfigureUserPermissions(modelBuilder);

            // ===== CLIENTI =====
            ConfigureCliente(modelBuilder);

            // ===== TIPO PRATICHE =====
            ConfigureTipoPratica(modelBuilder);

            // ===== ARGOMENTI =====
            ConfigureArgomento(modelBuilder);

            // ===== CIRCOLARI =====
            ConfigureCircolare(modelBuilder);

            // ===== LICENSES =====
            ConfigureLicenseClient(modelBuilder);
            ConfigureLicenseKey(modelBuilder);

            // ===== AUDIT LOG =====
            ConfigureAuditLog(modelBuilder);

            // ===== BANCHE =====
            ConfigureBanca(modelBuilder);
            ConfigureBancaIncasso(modelBuilder);
            ConfigureBancaPagamento(modelBuilder);
            ConfigureBancaUtilizzoAnticipo(modelBuilder);
            ConfigureBancaSaldoGiornaliero(modelBuilder);
            ConfigureFinanziamentoImport(modelBuilder);

            // ===== BILANCI =====
            ConfigureBilancioContabile(modelBuilder);
            ConfigureBilancioTemplate(modelBuilder);
            ConfigureAssociazioneMastrino(modelBuilder);
            ConfigureAssociazioneMastrinoDettaglio(modelBuilder);
            ConfigureStatisticaSPSalvata(modelBuilder);
            ConfigureStatisticaCESalvata(modelBuilder);
            ConfigureIndicePersonalizzato(modelBuilder);
            ConfigureIndiceConfigurazione(modelBuilder);

            // ===== TODO STUDIO =====
            ConfigureTodoStudio(modelBuilder);

            // TODO: Aggiungere configurazioni altre tabelle man mano
        }

        /// <summary>
        /// Configurazione tabella Professionisti
        /// </summary>
        private void ConfigureProfessionista(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Professionista>(entity =>
            {
                // Nome tabella
                entity.ToTable("professionisti");

                // Chiave primaria (già definita con [Key] nel model)
                entity.HasKey(e => e.Id);

                // Indici per performance
                entity.HasIndex(e => e.Cognome)
                    .HasDatabaseName("IX_Professionisti_Cognome");

                entity.HasIndex(e => e.Attivo)
                    .HasDatabaseName("IX_Professionisti_Attivo");

                // Configurazione proprietà
                entity.Property(e => e.Nome)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Cognome)
                    .HasMaxLength(100)
                    .IsRequired();

                // Date con valori di default
                entity.Property(e => e.DataAttivazione)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.DataModifica)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Configurazione tabella Utenti
        /// </summary>
        private void ConfigureUtente(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Utente>(entity =>
            {
                // Nome tabella
                entity.ToTable("utenti");

                // Chiave primaria
                entity.HasKey(e => e.Id);

                // Indici
                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasDatabaseName("IX_Utenti_Username");

                entity.HasIndex(e => e.Email)
                    .HasDatabaseName("IX_Utenti_Email");

                entity.HasIndex(e => e.Ruolo)
                    .HasDatabaseName("IX_Utenti_Ruolo");

                entity.HasIndex(e => e.Attivo)
                    .HasDatabaseName("IX_Utenti_Attivo");

                // Configurazione proprietà
                entity.Property(e => e.Username)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.Nome)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Cognome)
                    .HasMaxLength(100)
                    .IsRequired();

                // Date con valori di default
                entity.Property(e => e.DataCreazione)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.DataModifica)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Configurazione tabella User Permissions
        /// </summary>
        private void ConfigureUserPermissions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPermissions>(entity =>
            {
                // Nome tabella
                entity.ToTable("user_permissions");

                // Chiave primaria
                entity.HasKey(e => e.Id);

                // Indice UNIQUE su IdUtente (relazione 1:1 con Utente)
                entity.HasIndex(e => e.IdUtente)
                    .IsUnique()
                    .HasDatabaseName("IX_UserPermissions_IdUtente");

                // Relazione con Utente (opzionale - può essere gestita a livello applicativo)
                // Uncomment se vuoi Foreign Key:
                // entity.HasOne<Utente>()
                //     .WithOne()
                //     .HasForeignKey<UserPermissions>(e => e.IdUtente)
                //     .OnDelete(DeleteBehavior.Cascade);

                // Date con valori di default
                entity.Property(e => e.DataCreazione)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.DataModifica)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Ottiene connection string di default da file configurazione
        /// </summary>
        private static string GetDefaultConnectionString()
        {
            try
            {
                // Percorso file connection string
                var configPath = Path.Combine(@"C:\db_CGEASY", "connectionstring.txt");

                if (File.Exists(configPath))
                {
                    var connectionString = File.ReadAllText(configPath).Trim();
                    Console.WriteLine($"✅ Connection string caricata da: {configPath}");
                    return connectionString;
                }

                // Fallback: connection string di default
                var defaultConnectionString = "Server=localhost\\SQLEXPRESS;Database=CGEasy;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
                Console.WriteLine($"⚠️ File {configPath} non trovato, uso connection string di default");
                return defaultConnectionString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore lettura connection string: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Test connessione al database
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                Database.CanConnect();
                Console.WriteLine("✅ Connessione SQL Server OK");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore connessione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Configurazione tabella Clienti
        /// </summary>
        private void ConfigureCliente(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                // Nome tabella
                entity.ToTable("clienti");

                // Chiave primaria
                entity.HasKey(e => e.Id);

                // Indici per performance
                entity.HasIndex(e => e.NomeCliente)
                    .HasDatabaseName("IX_Clienti_NomeCliente");

                entity.HasIndex(e => e.Attivo)
                    .HasDatabaseName("IX_Clienti_Attivo");

                entity.HasIndex(e => e.IdProfessionista)
                    .HasDatabaseName("IX_Clienti_IdProfessionista");

                entity.HasIndex(e => e.PivaCliente)
                    .HasDatabaseName("IX_Clienti_PivaCliente");

                entity.HasIndex(e => e.CfCliente)
                    .HasDatabaseName("IX_Clienti_CfCliente");

                // Configurazione proprietà
                entity.Property(e => e.NomeCliente)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.MailCliente)
                    .HasMaxLength(200);

                entity.Property(e => e.CfCliente)
                    .HasMaxLength(16);

                entity.Property(e => e.PivaCliente)
                    .HasMaxLength(20);

                entity.Property(e => e.CodiceAteco)
                    .HasMaxLength(20);

                entity.Property(e => e.Indirizzo)
                    .HasMaxLength(300);

                entity.Property(e => e.Citta)
                    .HasMaxLength(100);

                entity.Property(e => e.Provincia)
                    .HasMaxLength(5);

                entity.Property(e => e.Cap)
                    .HasMaxLength(10);

                entity.Property(e => e.LegaleRappresentante)
                    .HasMaxLength(200);

                entity.Property(e => e.CfLegaleRappresentante)
                    .HasMaxLength(16);

                // Date con valori di default
                entity.Property(e => e.DataAttivazione)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.DataModifica)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Configurazione tabella Tipo Pratiche
        /// </summary>
        private void ConfigureTipoPratica(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TipoPratica>(entity =>
            {
                // Nome tabella
                entity.ToTable("tipo_pratiche");

                // Chiave primaria
                entity.HasKey(e => e.Id);

                // Indici per performance
                entity.HasIndex(e => e.NomePratica)
                    .HasDatabaseName("IX_TipoPratiche_NomePratica");

                entity.HasIndex(e => e.Attivo)
                    .HasDatabaseName("IX_TipoPratiche_Attivo");

                entity.HasIndex(e => e.Categoria)
                    .HasDatabaseName("IX_TipoPratiche_Categoria");

                entity.HasIndex(e => e.Ordine)
                    .HasDatabaseName("IX_TipoPratiche_Ordine");

                // Configurazione proprietà
                entity.Property(e => e.NomePratica)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Descrizione)
                    .HasMaxLength(500);

                // Enum come int
                entity.Property(e => e.Categoria)
                    .HasConversion<int>();

                // Date con valori di default
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Ottiene statistiche database
        /// </summary>
        public string GetDatabaseInfo()
        {
            var dbName = Database.GetDbConnection().Database;
            var serverVersion = Database.GetDbConnection().ServerVersion;
            return $"Database: {dbName} | Server: {serverVersion}";
        }

        /// <summary>
        /// Configurazione tabella Argomenti
        /// </summary>
        private void ConfigureArgomento(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Argomento>(entity =>
            {
                entity.ToTable("argomenti");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Nome).IsUnique().HasDatabaseName("IX_Argomenti_Nome");

                entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descrizione).HasMaxLength(500);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Configurazione tabella Circolari
        /// </summary>
        private void ConfigureCircolare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Circolare>(entity =>
            {
                entity.ToTable("circolari");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ArgomentoId).HasDatabaseName("IX_Circolari_ArgomentoId");
                entity.HasIndex(e => e.Anno).HasDatabaseName("IX_Circolari_Anno");
                entity.HasIndex(e => e.DataImportazione).HasDatabaseName("IX_Circolari_DataImportazione");

                entity.Property(e => e.Descrizione).HasMaxLength(500).IsRequired();
                entity.Property(e => e.NomeFile).HasMaxLength(300).IsRequired();
                entity.Property(e => e.PercorsoFile).HasMaxLength(500).IsRequired();
                entity.Property(e => e.DataImportazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureLicenseClient(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LicenseClient>(entity =>
            {
                entity.ToTable("license_clients");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.NomeCliente).HasDatabaseName("IX_LicenseClients_NomeCliente");
                entity.HasIndex(e => e.Email).HasDatabaseName("IX_LicenseClients_Email");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_LicenseClients_IsActive");

                entity.Property(e => e.NomeCliente).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Telefono).HasMaxLength(50);
                entity.Property(e => e.PartitaIva).HasMaxLength(20);
                entity.Property(e => e.DataRegistrazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureLicenseKey(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LicenseKey>(entity =>
            {
                entity.ToTable("license_keys");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.LicenseClientId).HasDatabaseName("IX_LicenseKeys_ClientId");
                entity.HasIndex(e => e.ModuleName).HasDatabaseName("IX_LicenseKeys_ModuleName");
                entity.HasIndex(e => e.LicenseGuid).IsUnique().HasDatabaseName("IX_LicenseKeys_Guid");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_LicenseKeys_IsActive");

                entity.Property(e => e.ModuleName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FullKey).HasMaxLength(200).IsRequired();
                entity.Property(e => e.LicenseGuid).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DataGenerazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.IdUtente).HasDatabaseName("IX_AuditLogs_IdUtente");
                entity.HasIndex(e => e.Azione).HasDatabaseName("IX_AuditLogs_Azione");
                entity.HasIndex(e => e.Entita).HasDatabaseName("IX_AuditLogs_Entita");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp");

                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Azione).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Entita).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descrizione).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureBanca(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Banca>(entity =>
            {
                entity.ToTable("banche");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NomeBanca).HasDatabaseName("IX_Banche_NomeBanca");
                entity.Property(e => e.NomeBanca).HasMaxLength(200).IsRequired();
                entity.Property(e => e.CodiceIdentificativo).HasMaxLength(50);
                entity.Property(e => e.IBAN).HasMaxLength(50);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.DataUltimaModifica).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureBancaIncasso(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BancaIncasso>(entity =>
            {
                entity.ToTable("banca_incassi");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BancaId).HasDatabaseName("IX_BancaIncassi_BancaId");
                entity.HasIndex(e => e.Anno).HasDatabaseName("IX_BancaIncassi_Anno");
                entity.HasIndex(e => e.Mese).HasDatabaseName("IX_BancaIncassi_Mese");
                entity.HasIndex(e => e.DataScadenza).HasDatabaseName("IX_BancaIncassi_DataScadenza");
                entity.Property(e => e.NomeCliente).HasMaxLength(200).IsRequired();
                entity.Property(e => e.NumeroFatturaCliente).HasMaxLength(50);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureBancaPagamento(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BancaPagamento>(entity =>
            {
                entity.ToTable("banca_pagamenti");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BancaId).HasDatabaseName("IX_BancaPagamenti_BancaId");
                entity.HasIndex(e => e.Anno).HasDatabaseName("IX_BancaPagamenti_Anno");
                entity.HasIndex(e => e.Mese).HasDatabaseName("IX_BancaPagamenti_Mese");
                entity.HasIndex(e => e.DataScadenza).HasDatabaseName("IX_BancaPagamenti_DataScadenza");
                entity.Property(e => e.NomeFornitore).HasMaxLength(200).IsRequired();
                entity.Property(e => e.NumeroFatturaFornitore).HasMaxLength(50);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureBancaUtilizzoAnticipo(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BancaUtilizzoAnticipo>(entity =>
            {
                entity.ToTable("banca_utilizzo_anticipo");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BancaId).HasDatabaseName("IX_BancaUtilizzoAnticipo_BancaId");
                entity.HasIndex(e => e.DataInizioUtilizzo).HasDatabaseName("IX_BancaUtilizzoAnticipo_DataInizio");
                entity.HasIndex(e => e.DataScadenzaUtilizzo).HasDatabaseName("IX_BancaUtilizzoAnticipo_DataScadenza");
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureBancaSaldoGiornaliero(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BancaSaldoGiornaliero>(entity =>
            {
                entity.ToTable("banca_saldo_giornaliero");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BancaId).HasDatabaseName("IX_BancaSaldoGiornaliero_BancaId");
                entity.HasIndex(e => e.Data).HasDatabaseName("IX_BancaSaldoGiornaliero_Data");
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureFinanziamentoImport(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinanziamentoImport>(entity =>
            {
                entity.ToTable("finanziamento_import");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.BancaId).HasDatabaseName("IX_FinanziamentoImport_BancaId");
                entity.HasIndex(e => e.DataInizio).HasDatabaseName("IX_FinanziamentoImport_DataInizio");
                entity.Property(e => e.NomeFinanziamento).HasMaxLength(200).IsRequired();
                entity.Property(e => e.NomeFornitore).HasMaxLength(200);
                entity.Property(e => e.NumeroFattura).HasMaxLength(50);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureBilancioContabile(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BilancioContabile>(entity =>
            {
                entity.ToTable("bilancio_contabile");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_BilancioContabile_ClienteId");
                entity.HasIndex(e => new { e.Anno, e.Mese }).HasDatabaseName("IX_BilancioContabile_AnnoMese");
                entity.Property(e => e.ClienteNome).HasMaxLength(200).IsRequired();
                entity.Property(e => e.TipoBilancio).HasMaxLength(10).IsRequired();
                entity.Property(e => e.CodiceMastrino).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DescrizioneMastrino).HasMaxLength(300).IsRequired();
            });
        }

        private void ConfigureBilancioTemplate(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BilancioTemplate>(entity =>
            {
                entity.ToTable("bilancio_template");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_BilancioTemplate_ClienteId");
                entity.HasIndex(e => new { e.Anno, e.Mese }).HasDatabaseName("IX_BilancioTemplate_AnnoMese");
                entity.Property(e => e.ClienteNome).HasMaxLength(200).IsRequired();
                entity.Property(e => e.TipoBilancio).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Segno).HasMaxLength(1);
                entity.Property(e => e.Formula).HasMaxLength(200);
            });
        }

        private void ConfigureAssociazioneMastrino(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssociazioneMastrino>(entity =>
            {
                entity.ToTable("associazione_mastrini");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_AssociazioneMastrino_ClienteId");
                entity.HasIndex(e => new { e.Anno, e.Mese }).HasDatabaseName("IX_AssociazioneMastrino_AnnoMese");
                entity.Property(e => e.ClienteNome).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureAssociazioneMastrinoDettaglio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssociazioneMastrinoDettaglio>(entity =>
            {
                entity.ToTable("associazione_mastrini_dettagli");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssociazioneId).HasDatabaseName("IX_AssociazioneMastrinoDettaglio_AssociazioneId");
                entity.Property(e => e.CodiceMastrino).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DescrizioneMastrino).HasMaxLength(300).IsRequired();
            });
        }

        private void ConfigureStatisticaSPSalvata(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StatisticaSPSalvata>(entity =>
            {
                entity.ToTable("statistica_sp_salvata");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_StatisticaSPSalvata_ClienteId");
                entity.Property(e => e.NomeStatistica).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureStatisticaCESalvata(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StatisticaCESalvata>(entity =>
            {
                entity.ToTable("statistica_ce_salvata");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_StatisticaCESalvata_ClienteId");
                entity.Property(e => e.NomeStatistica).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureIndicePersonalizzato(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndicePersonalizzato>(entity =>
            {
                entity.ToTable("indici_personalizzati");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_IndicePersonalizzato_ClienteId");
                entity.Property(e => e.NomeIndice).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Operatore).HasMaxLength(50);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureIndiceConfigurazione(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndiceConfigurazione>(entity =>
            {
                entity.ToTable("indice_configurazione");
                entity.HasKey(e => e.Id);
                
                // Indici per performance
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_IndiceConfigurazione_ClienteId");
                entity.HasIndex(e => e.CodiceIndice).HasDatabaseName("IX_IndiceConfigurazione_CodiceIndice");
                entity.HasIndex(e => e.Categoria).HasDatabaseName("IX_IndiceConfigurazione_Categoria");
                entity.HasIndex(e => e.IsAbilitato).HasDatabaseName("IX_IndiceConfigurazione_IsAbilitato");
                
                // Configurazione proprietà
                entity.Property(e => e.NomeIndice).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Formula).HasMaxLength(500).IsRequired();
                entity.Property(e => e.UnitaMisura).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CodiceIndice).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descrizione).HasMaxLength(1000);
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UltimaModifica).HasDefaultValueSql("GETUTCDATE()");
            });
        }

        private void ConfigureTodoStudio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoStudio>(entity =>
            {
                entity.ToTable("todo_studio");
                entity.HasKey(e => e.Id);

                // Indici per performance
                entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_TodoStudio_ClienteId");
                entity.HasIndex(e => e.TipoPraticaId).HasDatabaseName("IX_TodoStudio_TipoPraticaId");
                entity.HasIndex(e => e.CreatoreId).HasDatabaseName("IX_TodoStudio_CreatoreId");
                entity.HasIndex(e => e.Stato).HasDatabaseName("IX_TodoStudio_Stato");
                entity.HasIndex(e => e.Priorita).HasDatabaseName("IX_TodoStudio_Priorita");
                entity.HasIndex(e => e.Categoria).HasDatabaseName("IX_TodoStudio_Categoria");
                entity.HasIndex(e => e.DataScadenza).HasDatabaseName("IX_TodoStudio_DataScadenza");
                entity.HasIndex(e => e.DataCreazione).HasDatabaseName("IX_TodoStudio_DataCreazione");

                // Configurazione proprietà
                entity.Property(e => e.Titolo).HasMaxLength(300).IsRequired();
                entity.Property(e => e.TipoPraticaNome).HasMaxLength(200);
                entity.Property(e => e.ClienteNome).HasMaxLength(200);
                entity.Property(e => e.CreatoreNome).HasMaxLength(200).IsRequired();
                
                // Enum come int
                entity.Property(e => e.Categoria).HasConversion<int>();
                entity.Property(e => e.Priorita).HasConversion<int>();
                entity.Property(e => e.Stato).HasConversion<int>();
                
                // Default values
                entity.Property(e => e.DataCreazione).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.DataUltimaModifica).HasDefaultValueSql("GETUTCDATE()");
                
                // JSON columns con default
                entity.Property(e => e.ProfessionistiAssegnatiIdsJson).HasDefaultValue("[]");
                entity.Property(e => e.ProfessionistiAssegnatiNomiJson).HasDefaultValue("[]");
                entity.Property(e => e.AllegatiJson).HasDefaultValue("[]");
            });
        }
    }
}

