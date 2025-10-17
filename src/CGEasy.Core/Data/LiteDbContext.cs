using LiteDB;
using CGEasy.Core.Models;
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
        /// Percorso default database in cartella condivisa rete
        /// Server: File sarà in cartella condivisa (es: C:\ProgramData\CGEasy\)
        /// Client: Punterà al server tramite UNC path (es: \\SERVER\CGEasy\)
        /// </summary>
        public static string DefaultDatabasePath => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                         "CGEasy", "cgeasy.db");

        /// <summary>
        /// Costruttore - Usa percorso database di default
        /// </summary>
        public LiteDbContext() : this(DefaultDatabasePath)
        {
        }

        /// <summary>
        /// Costruttore con percorso custom
        /// </summary>
        /// <param name="databasePath">Percorso file database (locale o UNC)</param>
        public LiteDbContext(string databasePath)
        {
            _databasePath = databasePath;

            // Crea cartella se non esiste
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Connection string per accesso condiviso multi-utente
            var connectionString = new ConnectionString
            {
                Filename = databasePath,
                Connection = ConnectionType.Shared, // Shared = multi-reader + single writer
                Upgrade = true,
                ReadOnly = false
            };

            _database = new LiteDatabase(connectionString);

            // Configura mapper per auto-incremento ID
            ConfigureMapper();

            // Assicura esistenza collections
            EnsureCollections();
        }

        /// <summary>
        /// Configura BsonMapper per convenzioni ID
        /// </summary>
        private void ConfigureMapper()
        {
            var mapper = BsonMapper.Global;

            // Configura auto-incremento per tutti i models
            mapper.Entity<Utente>()
                .Id(x => x.Id, autoId: true);

            mapper.Entity<UserPermissions>()
                .Id(x => x.Id, autoId: true);

            mapper.Entity<Cliente>()
                .Id(x => x.Id, autoId: true);

            mapper.Entity<Professionista>()
                .Id(x => x.Id, autoId: true);

            mapper.Entity<TipoPratica>()
                .Id(x => x.Id, autoId: true);

            // Ignora proprietà calcolate (NotMapped)
            mapper.Entity<Utente>()
                .Ignore(x => x.NomeCompleto)
                .Ignore(x => x.RuoloDescrizione);

            mapper.Entity<Cliente>()
                .Ignore(x => x.StatoDescrizione)
                .Ignore(x => x.IndirizzoCompleto)
                .Ignore(x => x.DisplayName);

            mapper.Entity<Professionista>()
                .Ignore(x => x.NomeCompleto)
                .Ignore(x => x.StatoDescrizione)
                .Ignore(x => x.StatoCssClass);

            mapper.Entity<TipoPratica>()
                .Ignore(x => x.CategoriaDescrizione)
                .Ignore(x => x.IconaCategoria);

            mapper.Entity<UserPermissions>()
                .Ignore(x => x.ModuliAttivi);
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

        // ===== UTILITY METHODS =====

        /// <summary>
        /// Verifica se il database esiste
        /// </summary>
        public static bool DatabaseExists(string path = null)
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
                    DatabaseSizeBytes = File.Exists(_databasePath) ? new FileInfo(_databasePath).Length : 0,
                    DatabasePath = _databasePath
                };
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
        public long DatabaseSizeBytes { get; set; }
        public string DatabasePath { get; set; } = string.Empty;

        public string DatabaseSizeFormatted => 
            DatabaseSizeBytes < 1024 * 1024 
                ? $"{DatabaseSizeBytes / 1024:N0} KB" 
                : $"{DatabaseSizeBytes / (1024 * 1024):N2} MB";
    }
}

