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

        /// <summary>
        /// Percorso default database in cartella documenti pubblici condivisi
        /// C:\Users\Public\Documents\CGEasy\ - Accessibile da tutti gli utenti Windows
        /// Facilmente condivisibile in rete locale
        /// </summary>
        public static string DefaultDatabasePath => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), 
                         "CGEasy", "cgeasy.db");

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

            // Connection string per accesso diretto con commit immediato
            var connectionString = new ConnectionString
            {
                Filename = databasePath,
                Connection = ConnectionType.Direct,
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
        /// Collection License Clients (clienti a cui sono state rilasciate licenze)
        /// </summary>
        public ILiteCollection<LicenseClient> LicenseClients =>
            _database.GetCollection<LicenseClient>("license_clients");

        /// <summary>
        /// Collection License Keys (chiavi di licenza generate)
        /// </summary>
        public ILiteCollection<LicenseKey> LicenseKeys =>
            _database.GetCollection<LicenseKey>("license_keys");

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
        /// Crea utente amministratore di default (primo avvio)
        /// </summary>
        public void SeedDefaultAdmin(string username = "admin", string password = "admin123")
        {
            lock (_lock)
            {
                // Controlla se esiste già un admin
                var existingAdmin = Utenti.FindOne(x => x.Ruolo == RuoloUtente.Administrator);
                if (existingAdmin != null)
                    return; // Admin già esistente

                // Crea admin di default
                var admin = new Utente
                {
                    Username = username,
                    Email = "admin@cgeasy.local",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    Nome = "Amministratore",
                    Cognome = "Sistema",
                    Ruolo = RuoloUtente.Administrator,
                    Attivo = true,
                    DataCreazione = DateTime.UtcNow,
                    DataModifica = DateTime.UtcNow
                };

                var adminId = Utenti.Insert(admin);

                // Crea permessi completi per admin
                var permissions = new UserPermissions
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

                UserPermissions.Insert(permissions);
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
                    _database?.Dispose();
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

