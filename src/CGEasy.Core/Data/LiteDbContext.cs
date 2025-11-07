using LiteDB;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using System;
using System.IO;

namespace CGEasy.Core.Data
{
    /// <summary>
    /// LiteDB Context per gestione database condiviso multi-utente
    /// </summary>
    public class LiteDbContext : IDisposable
    {
        private readonly LiteDatabase _database;
        private readonly string _databasePath;
        private static readonly object _lock = new object();
        private bool _disposed = false;
        private bool _isSingleton = false; // Flag per indicare se questo è il Singleton globale

        /// <summary>
        /// Marca questo context come Singleton (non verrà mai chiuso)
        /// </summary>
        public void MarkAsSingleton()
        {
            _isSingleton = true;
        }

        /// <summary>
        /// Percorso default database
        /// C:\db_CGEASY\ - Cartella fissa per database, licenze e chiave
        /// Se esiste una configurazione personalizzata, usa quella invece del default
        /// </summary>
        public static string DefaultDatabasePath
        {
            get
            {
                // 1. Prova a leggere da configurazione personalizzata
                var configuredPath = DatabaseConfigService.GetConfiguredPath();
                if (!string.IsNullOrEmpty(configuredPath))
                {
                    return configuredPath;
                }

                // 2. Altrimenti usa percorso FISSO di default
                return Path.Combine(@"C:\db_CGEASY", "cgeasy.db");
            }
        }

        /// <summary>
        /// Costruttore - Usa percorso database di default
        /// Controlla se esiste password salvata e la usa automaticamente
        /// </summary>
        public LiteDbContext() : this(DefaultDatabasePath, GetPasswordForDatabase())
        {
        }

        /// <summary>
        /// Determina quale password usare per aprire il database
        /// SEMPRE la password master se il database è criptato
        /// </summary>
        private static string? GetPasswordForDatabase()
        {
            try
            {
                // Se esiste file password, il DB è criptato con MASTER PASSWORD
                if (DatabaseEncryptionService.IsDatabaseEncrypted())
                {
                    return DatabaseEncryptionService.GetMasterPassword();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Costruttore con percorso custom
        /// </summary>
        /// <param name="databasePath">Percorso file database (locale o UNC)</param>
        /// <param name="password">Password per crittografia database (opzionale)</param>
        public LiteDbContext(string databasePath, string? password = null)
        {
            _databasePath = databasePath;

            // Crea cartella se non esiste
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Crea mapper dedicato (non usare Global per evitare conflitti)
            var mapper = new BsonMapper();
            
            // Configura ID auto-increment
            mapper.Entity<Utente>().Id(x => x.Id, autoId: true);
            mapper.Entity<UserPermissions>().Id(x => x.Id, autoId: true);
            mapper.Entity<Cliente>().Id(x => x.Id, autoId: true);
            mapper.Entity<Professionista>().Id(x => x.Id, autoId: true);
            mapper.Entity<TipoPratica>().Id(x => x.Id, autoId: true);
            mapper.Entity<AuditLog>().Id(x => x.Id, autoId: true);
            mapper.Entity<TodoStudio>().Id(x => x.Id, autoId: true);
            mapper.Entity<LicenseClient>().Id(x => x.Id, autoId: true);
            mapper.Entity<LicenseKey>().Id(x => x.Id, autoId: true);
            mapper.Entity<BilancioContabile>().Id(x => x.Id, autoId: true);
            mapper.Entity<BilancioTemplate>().Id(x => x.Id, autoId: true);
            mapper.Entity<AssociazioneMastrino>().Id(x => x.Id, autoId: true);
            mapper.Entity<AssociazioneMastrinoDettaglio>().Id(x => x.Id, autoId: true);
            mapper.Entity<Argomento>().Id(x => x.Id, autoId: true);
            mapper.Entity<Circolare>().Id(x => x.Id, autoId: true);
            
            // Banche
            mapper.Entity<Banca>().Id(x => x.Id, autoId: true);
            mapper.Entity<BancaIncasso>().Id(x => x.Id, autoId: true);
            mapper.Entity<BancaPagamento>().Id(x => x.Id, autoId: true);
            mapper.Entity<BancaUtilizzoAnticipo>().Id(x => x.Id, autoId: true);
            mapper.Entity<BancaSaldoGiornaliero>().Id(x => x.Id, autoId: true);

            // Connection string per accesso condiviso (multi-utente)
            var connectionString = new ConnectionString
            {
                Filename = databasePath,
                Connection = ConnectionType.Shared, // Permette accesso concorrente da più PC
                Upgrade = true,
                ReadOnly = false,
                Password = password // Aggiunge password se presente
            };

            // Crea database con mapper dedicato
            _database = new LiteDatabase(connectionString, mapper);

            // Assicura esistenza collections
            EnsureCollections();
        }

        /// <summary>
        /// Assicura che tutte le collections esistano
        /// </summary>
        private void EnsureCollections()
        {
            // Crea indici per performance
            lock (_lock)
            {
                // Utenti
                var utentiCol = _database.GetCollection<Utente>("utenti");
                utentiCol.EnsureIndex(x => x.Username, unique: true);
                utentiCol.EnsureIndex(x => x.Email);

                // Clienti
                var clientiCol = _database.GetCollection<Cliente>("clienti");
                clientiCol.EnsureIndex(x => x.NomeCliente);
                clientiCol.EnsureIndex(x => x.Attivo);
                clientiCol.EnsureIndex(x => x.IdProfessionista);

                // Professionisti
                var professionistiCol = _database.GetCollection<Professionista>("professionisti");
                professionistiCol.EnsureIndex(x => x.Cognome);
                professionistiCol.EnsureIndex(x => x.Attivo);

                // Tipo Pratica
                var tipoPraticaCol = _database.GetCollection<TipoPratica>("tipo_pratiche");
                tipoPraticaCol.EnsureIndex(x => x.Categoria);
                tipoPraticaCol.EnsureIndex(x => x.Attivo);
                tipoPraticaCol.EnsureIndex(x => x.Ordine);

                // User Permissions
                var permissionsCol = _database.GetCollection<UserPermissions>("user_permissions");
                permissionsCol.EnsureIndex(x => x.IdUtente, unique: true);

                // Audit Logs
                var auditCol = _database.GetCollection<AuditLog>("audit_logs");
                auditCol.EnsureIndex(x => x.IdUtente);
                auditCol.EnsureIndex(x => x.Entita);
                auditCol.EnsureIndex(x => x.Timestamp);

                // TODO Studio
                var todoStudioCol = _database.GetCollection<TodoStudio>("todoStudio");
                todoStudioCol.EnsureIndex(x => x.Stato);
                todoStudioCol.EnsureIndex(x => x.Priorita);
                todoStudioCol.EnsureIndex(x => x.Categoria);
                todoStudioCol.EnsureIndex(x => x.DataScadenza);
                todoStudioCol.EnsureIndex(x => x.CreatoreId);
                todoStudioCol.EnsureIndex(x => x.ClienteId);
                todoStudioCol.EnsureIndex(x => x.TipoPraticaId);

                // License Clients
                var licenseClientsCol = _database.GetCollection<LicenseClient>("license_clients");
                licenseClientsCol.EnsureIndex(x => x.NomeCliente);
                licenseClientsCol.EnsureIndex(x => x.IsActive);

                // License Keys
                var licenseKeysCol = _database.GetCollection<LicenseKey>("license_keys");
                licenseKeysCol.EnsureIndex(x => x.LicenseClientId);
                licenseKeysCol.EnsureIndex(x => x.ModuleName);
                licenseKeysCol.EnsureIndex(x => x.FullKey, unique: true);
                licenseKeysCol.EnsureIndex(x => x.IsActive);
                licenseKeysCol.EnsureIndex(x => x.DataGenerazione);

                // Associazioni Mastrini
                var associazioniCol = _database.GetCollection<AssociazioneMastrino>("associazioni_mastrini");
                associazioniCol.EnsureIndex(x => x.ClienteId);
                associazioniCol.EnsureIndex(x => x.Mese);
                associazioniCol.EnsureIndex(x => x.Anno);
                associazioniCol.EnsureIndex(x => x.DataCreazione);

                // Associazioni Mastrini Dettagli
                var associazioniDettagliCol = _database.GetCollection<AssociazioneMastrinoDettaglio>("associazioni_mastrini_dettagli");
                associazioniDettagliCol.EnsureIndex(x => x.AssociazioneId);
                associazioniDettagliCol.EnsureIndex(x => x.CodiceMastrino);

                // Argomenti
                var argomentiCol = _database.GetCollection<Argomento>("argomenti");
                argomentiCol.EnsureIndex(x => x.Nome);
                argomentiCol.EnsureIndex(x => x.DataCreazione);

                // Circolari
                var circolariCol = _database.GetCollection<Circolare>("circolari");
                circolariCol.EnsureIndex(x => x.ArgomentoId);
                circolariCol.EnsureIndex(x => x.Anno);
                circolariCol.EnsureIndex(x => x.Descrizione);
                circolariCol.EnsureIndex(x => x.DataImportazione);

                // Banche
                var bancheCol = _database.GetCollection<Banca>("banche");
                bancheCol.EnsureIndex(x => x.NomeBanca);
                bancheCol.EnsureIndex(x => x.DataCreazione);

                // Banca Incassi
                var bancaIncassiCol = _database.GetCollection<BancaIncasso>("banca_incassi");
                bancaIncassiCol.EnsureIndex(x => x.BancaId);
                bancaIncassiCol.EnsureIndex(x => x.Anno);
                bancaIncassiCol.EnsureIndex(x => x.Mese);
                bancaIncassiCol.EnsureIndex(x => x.DataScadenza);
                bancaIncassiCol.EnsureIndex(x => x.Incassato);

                // Banca Pagamenti
                var bancaPagamentiCol = _database.GetCollection<BancaPagamento>("banca_pagamenti");
                bancaPagamentiCol.EnsureIndex(x => x.BancaId);
                bancaPagamentiCol.EnsureIndex(x => x.Anno);
                bancaPagamentiCol.EnsureIndex(x => x.Mese);
                bancaPagamentiCol.EnsureIndex(x => x.DataScadenza);
                bancaPagamentiCol.EnsureIndex(x => x.Pagato);

                // Banca Utilizzo Anticipo
                var bancaUtilizzoAnticipoCol = _database.GetCollection<BancaUtilizzoAnticipo>("banca_utilizzo_anticipo");
                bancaUtilizzoAnticipoCol.EnsureIndex(x => x.BancaId);
                bancaUtilizzoAnticipoCol.EnsureIndex(x => x.DataInizioUtilizzo);
                bancaUtilizzoAnticipoCol.EnsureIndex(x => x.DataScadenzaUtilizzo);
                bancaUtilizzoAnticipoCol.EnsureIndex(x => x.Rimborsato);

                // Banca Saldo Giornaliero
                var bancaSaldoGiornalieroCol = _database.GetCollection<BancaSaldoGiornaliero>("banca_saldo_giornaliero");
                bancaSaldoGiornalieroCol.EnsureIndex(x => x.BancaId);
                bancaSaldoGiornalieroCol.EnsureIndex(x => x.Data);
            }
        }

        // ===== COLLECTIONS =====

        /// <summary>
        /// Collection Utenti
        /// </summary>
        public ILiteCollection<Utente> Utenti => _database.GetCollection<Utente>("utenti");

        /// <summary>
        /// Collection User Permissions
        /// </summary>
        public ILiteCollection<UserPermissions> UserPermissions => 
            _database.GetCollection<UserPermissions>("user_permissions");

        /// <summary>
        /// Collection Clienti
        /// </summary>
        public ILiteCollection<Cliente> Clienti => _database.GetCollection<Cliente>("clienti");

        /// <summary>
        /// Collection Professionisti
        /// </summary>
        public ILiteCollection<Professionista> Professionisti => 
            _database.GetCollection<Professionista>("professionisti");

        /// <summary>
        /// Collection Tipo Pratiche
        /// </summary>
        public ILiteCollection<TipoPratica> TipoPratiche => 
            _database.GetCollection<TipoPratica>("tipo_pratiche");

        /// <summary>
        /// Collection Audit Logs
        /// </summary>
        public ILiteCollection<AuditLog> AuditLogs =>
            _database.GetCollection<AuditLog>("audit_logs");

        /// <summary>
        /// Collection TODO Studio
        /// </summary>
        public ILiteCollection<TodoStudio> TodoStudio =>
            _database.GetCollection<TodoStudio>("todoStudio");

        /// <summary>
        /// Collection Bilancio Contabile (mastrini importati da Excel)
        /// </summary>
        public ILiteCollection<BilancioContabile> BilancioContabile =>
            _database.GetCollection<BilancioContabile>("bilancio_contabile");

        /// <summary>
        /// Collection Bilancio Template (template riclassificazione)
        /// </summary>
        public ILiteCollection<BilancioTemplate> BilancioTemplate =>
            _database.GetCollection<BilancioTemplate>("bilancio_template");

        /// <summary>
        /// Collection License Clients (clienti a cui sono state rilasciate licenze)
        /// </summary>
        public ILiteCollection<LicenseClient> LicenseClients =>
            _database.GetCollection<LicenseClient>("license_clients");

        /// <summary>
        /// Collection License Keys (chiavi di licenza generate)
        /// </summary>
        public ILiteCollection<LicenseKey> LicenseKeys =>
            _database.GetCollection<LicenseKey>("license_keys");

        /// <summary>
        /// Collection Associazioni Mastrini (testata associazioni)
        /// </summary>
        public ILiteCollection<AssociazioneMastrino> AssociazioniMastrini =>
            _database.GetCollection<AssociazioneMastrino>("associazioni_mastrini");

        /// <summary>
        /// Collection Associazioni Mastrini Dettagli (dettagli mappature)
        /// </summary>
        public ILiteCollection<AssociazioneMastrinoDettaglio> AssociazioniMastriniDettagli =>
            _database.GetCollection<AssociazioneMastrinoDettaglio>("associazioni_mastrini_dettagli");

        /// <summary>
        /// Collection Argomenti (categorie circolari)
        /// </summary>
        public ILiteCollection<Argomento> Argomenti =>
            _database.GetCollection<Argomento>("argomenti");

        /// <summary>
        /// Collection Circolari (documenti importati)
        /// </summary>
        public ILiteCollection<Circolare> Circolari =>
            _database.GetCollection<Circolare>("circolari");

        /// <summary>
        /// Collection Banche
        /// </summary>
        public ILiteCollection<Banca> Banche =>
            _database.GetCollection<Banca>("banche");

        /// <summary>
        /// Collection Banca Incassi
        /// </summary>
        public ILiteCollection<BancaIncasso> BancaIncassi =>
            _database.GetCollection<BancaIncasso>("banca_incassi");

        /// <summary>
        /// Collection Banca Pagamenti
        /// </summary>
        public ILiteCollection<BancaPagamento> BancaPagamenti =>
            _database.GetCollection<BancaPagamento>("banca_pagamenti");

        /// <summary>
        /// Collection Banca Utilizzo Anticipo
        /// </summary>
        public ILiteCollection<BancaUtilizzoAnticipo> BancaUtilizzoAnticipo =>
            _database.GetCollection<BancaUtilizzoAnticipo>("banca_utilizzo_anticipo");

        /// <summary>
        /// Collection Banca Saldo Giornaliero
        /// </summary>
        public ILiteCollection<BancaSaldoGiornaliero> BancaSaldoGiornaliero =>
            _database.GetCollection<BancaSaldoGiornaliero>("banca_saldo_giornaliero");

        // ===== UTILITY METHODS =====

        /// <summary>
        /// Verifica se il database esiste
        /// </summary>
        public static bool DatabaseExists(string? path = null)
        {
            path ??= DefaultDatabasePath;
            return File.Exists(path);
        }

        /// <summary>
        /// Crea utenti amministratori di default (primo avvio)
        /// - admin1/123123: Utente visibile per il cliente
        /// - admin/123456: Utente nascosto per sviluppatore (backdoor)
        /// </summary>
        public void SeedDefaultAdmin(string username = "admin", string password = "123456")
        {
            lock (_lock)
            {
                // Controlla se esistono già admin
                var existingAdmins = Utenti.Find(x => x.Ruolo == RuoloUtente.Administrator).ToList();
                if (existingAdmins.Any())
                    return; // Admin già esistenti

                // 1. Crea admin PRINCIPALE (nascosto) - per sviluppatore
                var adminPrincipale = new Utente
                {
                    Username = "admin",
                    Email = "admin@cgeasy.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Nome = "Amministratore",
                    Cognome = "Sistema",
                    Ruolo = RuoloUtente.Administrator,
                    Attivo = true,
                    DataCreazione = DateTime.UtcNow,
                    DataModifica = DateTime.UtcNow
                };

                var adminId = Utenti.Insert(adminPrincipale);

                // Permessi completi per admin principale
                var permissionsAdmin = new UserPermissions
                {
                    IdUtente = adminId,
                    ModuloTodo = true,
                    ModuloBilanci = true,
                    ModuloCircolari = true,
                    ModuloControlloGestione = true,
                    ClientiCreate = true,
                    ClientiRead = true,
                    ClientiUpdate = true,
                    ClientiDelete = true,
                    ProfessionistiCreate = true,
                    ProfessionistiRead = true,
                    ProfessionistiUpdate = true,
                    ProfessionistiDelete = true,
                    UtentiManage = true,
                    DataCreazione = DateTime.UtcNow,
                    DataModifica = DateTime.UtcNow
                };

                UserPermissions.Insert(permissionsAdmin);

                // 2. Crea admin1 (visibile) - per cliente
                var admin1 = new Utente
                {
                    Username = "admin1",
                    Email = "admin1@cgeasy.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123123"),
                    Nome = "Amministratore",
                    Cognome = "Cliente",
                    Ruolo = RuoloUtente.Administrator,
                    Attivo = true,
                    DataCreazione = DateTime.UtcNow,
                    DataModifica = DateTime.UtcNow
                };

                var admin1Id = Utenti.Insert(admin1);

                // Permessi completi anche per admin1
                var permissionsAdmin1 = new UserPermissions
                {
                    IdUtente = admin1Id,
                    ModuloTodo = true,
                    ModuloBilanci = true,
                    ModuloCircolari = true,
                    ModuloControlloGestione = true,
                    ClientiCreate = true,
                    ClientiRead = true,
                    ClientiUpdate = true,
                    ClientiDelete = true,
                    ProfessionistiCreate = true,
                    ProfessionistiRead = true,
                    ProfessionistiUpdate = true,
                    ProfessionistiDelete = true,
                    UtentiManage = true,
                    DataCreazione = DateTime.UtcNow,
                    DataModifica = DateTime.UtcNow
                };

                UserPermissions.Insert(permissionsAdmin1);

                // Crea file db.key per indicare che il database è criptato
                DatabaseEncryptionService.SavePassword(DatabaseEncryptionService.GetMasterPassword());
            }
        }

        /// <summary>
        /// Ottiene statistiche database
        /// </summary>
        public DatabaseStats GetStats()
        {
            lock (_lock)
            {
                return new DatabaseStats
                {
                    TotalUtenti = Utenti.Count(),
                    TotalClienti = Clienti.Count(),
                    TotalProfessionisti = Professionisti.Count(),
                    TotalTipoPratiche = TipoPratiche.Count(),
                    TotalTodoStudio = TodoStudio.Count(),
                    TotalLicenseClients = LicenseClients.Count(),
                    TotalLicenseKeys = LicenseKeys.Count(),
                    DatabaseSizeBytes = File.Exists(_databasePath) ? new FileInfo(_databasePath).Length : 0,
                    DatabasePath = _databasePath
                };
            }
        }

        /// <summary>
        /// Forza il checkpoint (salvataggio su disco)
        /// </summary>
        public void Checkpoint()
        {
            lock (_lock)
            {
                _database.Checkpoint();
            }
        }

        // ===== DISPOSE PATTERN =====

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // NON chiudere il database se è Singleton (per modalità Shared multi-utente)
                    // Il Singleton rimane aperto per tutta la durata dell'applicazione
                    if (!_isSingleton)
                {
                    _database?.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        ~LiteDbContext()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Statistiche database
    /// </summary>
    public class DatabaseStats
    {
        public int TotalUtenti { get; set; }
        public int TotalClienti { get; set; }
        public int TotalProfessionisti { get; set; }
        public int TotalTipoPratiche { get; set; }
        public int TotalTodoStudio { get; set; }
        public int TotalLicenseClients { get; set; }
        public int TotalLicenseKeys { get; set; }
        public long DatabaseSizeBytes { get; set; }
        public string DatabasePath { get; set; } = string.Empty;

        public string DatabaseSizeFormatted => 
            DatabaseSizeBytes < 1024 * 1024 
                ? $"{DatabaseSizeBytes / 1024:N0} KB" 
                : $"{DatabaseSizeBytes / (1024 * 1024):N2} MB";
    }
}

