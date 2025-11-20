using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Configurazione SQL Server - salvata in C:\db_CGEASY\sqlconfig.json
    /// Permette di configurare server, istanza, autenticazione per installazioni multi-PC
    /// </summary>
    public class SqlServerConfig
    {
        /// <summary>
        /// Nome o IP del server SQL (es: localhost, 192.168.1.100, SERVER01)
        /// </summary>
        [JsonPropertyName("server")]
        public string Server { get; set; } = "localhost";

        /// <summary>
        /// Istanza SQL Server (es: SQLEXPRESS, MSSQLSERVER, oppure vuoto per istanza default)
        /// </summary>
        [JsonPropertyName("instance")]
        public string? Instance { get; set; } = "SQLEXPRESS";

        /// <summary>
        /// Nome del database
        /// </summary>
        [JsonPropertyName("database")]
        public string Database { get; set; } = "CGEasy";

        /// <summary>
        /// Tipo di autenticazione: "Windows" o "SQL"
        /// </summary>
        [JsonPropertyName("authentication_type")]
        public string AuthenticationType { get; set; } = "Windows";

        /// <summary>
        /// Username SQL Server (solo se AuthenticationType = "SQL")
        /// </summary>
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        /// <summary>
        /// Password SQL Server (solo se AuthenticationType = "SQL")
        /// ATTENZIONE: In produzione meglio criptarla!
        /// </summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        /// <summary>
        /// Timeout connessione in secondi
        /// </summary>
        [JsonPropertyName("connection_timeout")]
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// Abilita Multiple Active Result Sets
        /// </summary>
        [JsonPropertyName("multiple_active_result_sets")]
        public bool MultipleActiveResultSets { get; set; } = true;

        /// <summary>
        /// Trust Server Certificate (necessario per SQL Express)
        /// </summary>
        [JsonPropertyName("trust_server_certificate")]
        public bool TrustServerCertificate { get; set; } = true;

        /// <summary>
        /// Percorso del file di configurazione
        /// </summary>
        public static string ConfigFilePath => Path.Combine(@"C:\db_CGEASY", "sqlconfig.json");

        /// <summary>
        /// Carica configurazione da file JSON
        /// Se il file non esiste, crea configurazione di default per localhost\SQLEXPRESS
        /// </summary>
        public static SqlServerConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<SqlServerConfig>(json);
                    if (config != null)
                    {
                        Console.WriteLine($"✅ Configurazione SQL caricata da: {ConfigFilePath}");
                        Console.WriteLine($"   Server: {config.GetServerInstance()}");
                        Console.WriteLine($"   Database: {config.Database}");
                        Console.WriteLine($"   Autenticazione: {config.AuthenticationType}");
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Errore lettura configurazione SQL: {ex.Message}");
            }

            // Se il file non esiste o c'è un errore, crea configurazione di default
            var defaultConfig = CreateDefault();
            defaultConfig.Save();
            return defaultConfig;
        }

        /// <summary>
        /// Crea configurazione di default per localhost\SQLEXPRESS con Windows Authentication
        /// </summary>
        public static SqlServerConfig CreateDefault()
        {
            return new SqlServerConfig
            {
                Server = "localhost",
                Instance = "SQLEXPRESS",
                Database = "CGEasy",
                AuthenticationType = "Windows",
                ConnectionTimeout = 30,
                MultipleActiveResultSets = true,
                TrustServerCertificate = true
            };
        }

        /// <summary>
        /// Salva configurazione su file JSON
        /// </summary>
        public void Save()
        {
            try
            {
                // Crea cartella se non esiste
                var directory = Path.GetDirectoryName(ConfigFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
                Console.WriteLine($"✅ Configurazione SQL salvata in: {ConfigFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore salvataggio configurazione SQL: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ottiene il server completo con istanza (es: localhost\SQLEXPRESS)
        /// </summary>
        public string GetServerInstance()
        {
            if (string.IsNullOrWhiteSpace(Instance))
            {
                return Server; // Istanza default
            }
            return $"{Server}\\{Instance}";
        }

        /// <summary>
        /// Genera connection string completa per SQL Server
        /// </summary>
        public string GetConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = GetServerInstance(),
                InitialCatalog = Database,
                ConnectTimeout = ConnectionTimeout,
                MultipleActiveResultSets = MultipleActiveResultSets,
                TrustServerCertificate = TrustServerCertificate
            };

            if (AuthenticationType.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                builder.IntegratedSecurity = true;
            }
            else if (AuthenticationType.Equals("SQL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    throw new InvalidOperationException("Username è obbligatorio per autenticazione SQL");
                }
                builder.UserID = Username;
                builder.Password = Password ?? string.Empty;
                builder.IntegratedSecurity = false;
            }
            else
            {
                throw new InvalidOperationException($"Tipo autenticazione non valido: {AuthenticationType}. Usare 'Windows' o 'SQL'");
            }

            return builder.ConnectionString;
        }

        /// <summary>
        /// Valida la configurazione
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Server))
            {
                errorMessage = "Server non può essere vuoto";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Database))
            {
                errorMessage = "Database non può essere vuoto";
                return false;
            }

            if (AuthenticationType.Equals("SQL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    errorMessage = "Username obbligatorio per autenticazione SQL";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}

