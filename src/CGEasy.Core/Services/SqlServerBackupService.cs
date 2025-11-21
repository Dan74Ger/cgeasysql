using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CGEasy.Core.Data;

namespace CGEasy.Core.Services;

public class SqlServerBackupService
{
    private readonly CGEasyDbContext _context;
    private readonly string _backupDirectory;

    public SqlServerBackupService(CGEasyDbContext context)
    {
        _context = context;
        _backupDirectory = Path.Combine(@"C:\db_CGEASY", "Backups");
        
        // Crea la cartella backup se non esiste
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
        }
    }

    /// <summary>
    /// Crea un backup del database SQL Server
    /// </summary>
    public async Task<(bool Success, string Message, string FilePath)> CreaBackupAsync()
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"CGEasy_backup_{timestamp}.bak";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            // Ottieni il nome del database dalla connection string
            var connectionString = _context.Database.GetConnectionString();
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            // Esegui il backup usando T-SQL
            var sql = $"BACKUP DATABASE [{databaseName}] TO DISK = @backupPath WITH INIT, FORMAT, NAME = 'CGEasy Full Backup'";

            await _context.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@backupPath", backupPath));

            // Verifica che il file sia stato creato
            if (File.Exists(backupPath))
            {
                var fileSize = new FileInfo(backupPath).Length / 1024.0 / 1024.0; // MB
                return (true, $"✅ Backup creato con successo!\n\nFile: {backupFileName}\nDimensione: {fileSize:F2} MB\nPercorso: {backupPath}", backupPath);
            }
            else
            {
                return (false, "❌ Errore: Il file di backup non è stato creato.", string.Empty);
            }
        }
        catch (Exception ex)
        {
            return (false, $"❌ Errore durante il backup:\n{ex.Message}", string.Empty);
        }
    }

    /// <summary>
    /// Ripristina il database da un file di backup
    /// </summary>
    public async Task<(bool Success, string Message)> RipristinaBackupAsync(string backupPath)
    {
        SqlConnection? masterConnection = null;
        
        try
        {
            if (!File.Exists(backupPath))
            {
                return (false, "❌ File di backup non trovato.");
            }

            // Ottieni il nome del database e il server
            var connectionString = _context.Database.GetConnectionString();
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            var serverName = builder.DataSource;

            // Crea una connessione al database master (non al database da ripristinare)
            builder.InitialCatalog = "master";
            var masterConnectionString = builder.ToString();
            
            masterConnection = new SqlConnection(masterConnectionString);
            await masterConnection.OpenAsync();

            // 1. Imposta il database in modalità single user e forza la chiusura di tutte le connessioni
            var setSingleUserSql = $@"
                IF EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
                BEGIN
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                END
            ";
            
            using (var cmd = new SqlCommand(setSingleUserSql, masterConnection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // 2. Ripristina il database
            var restoreSql = $@"
                RESTORE DATABASE [{databaseName}] 
                FROM DISK = @backupPath 
                WITH REPLACE, RECOVERY;
            ";
            
            using (var cmd = new SqlCommand(restoreSql, masterConnection))
            {
                cmd.Parameters.AddWithValue("@backupPath", backupPath);
                cmd.CommandTimeout = 300; // 5 minuti timeout
                await cmd.ExecuteNonQueryAsync();
            }

            // 3. Riporta il database in modalità multi user
            var setMultiUserSql = $@"
                ALTER DATABASE [{databaseName}] SET MULTI_USER;
            ";
            
            using (var cmd = new SqlCommand(setMultiUserSql, masterConnection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            return (true, $"✅ Database ripristinato con successo!\n\nL'applicazione verrà chiusa.\nRiaprila per continuare.");
        }
        catch (Exception ex)
        {
            return (false, $"❌ Errore durante il ripristino:\n\n{ex.Message}\n\nSuggerimenti:\n• Assicurati che SQL Server sia in esecuzione\n• Verifica che il file di backup sia valido\n• L'app verrà riavviata automaticamente");
        }
        finally
        {
            if (masterConnection != null)
            {
                await masterConnection.CloseAsync();
                await masterConnection.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Ottiene la lista dei file di backup disponibili
    /// </summary>
    public (bool Success, string[] Files, string Message) GetBackupFiles()
    {
        try
        {
            if (!Directory.Exists(_backupDirectory))
            {
                return (false, Array.Empty<string>(), "Cartella backup non trovata.");
            }

            var files = Directory.GetFiles(_backupDirectory, "CGEasy_backup_*.bak")
                                 .OrderByDescending(f => File.GetCreationTime(f))
                                 .ToArray();

            if (files.Length == 0)
            {
                return (false, Array.Empty<string>(), "Nessun backup trovato.");
            }

            return (true, files, $"{files.Length} backup disponibili.");
        }
        catch (Exception ex)
        {
            return (false, Array.Empty<string>(), $"Errore: {ex.Message}");
        }
    }

    /// <summary>
    /// Ottiene informazioni sull'ultimo backup
    /// </summary>
    public (string UltimoBackup, int NumeroBackups) GetBackupInfo()
    {
        try
        {
            var result = GetBackupFiles();
            if (result.Success && result.Files.Length > 0)
            {
                var ultimoFile = result.Files[0];
                var dataCreazione = File.GetCreationTime(ultimoFile);
                return ($"{dataCreazione:dd/MM/yyyy HH:mm}", result.Files.Length);
            }
            else
            {
                return ("Nessun backup", 0);
            }
        }
        catch
        {
            return ("Errore lettura", 0);
        }
    }

    /// <summary>
    /// Elimina i backup più vecchi, mantenendo solo gli ultimi N
    /// </summary>
    public async Task<(bool Success, string Message)> PulisciBackupVecchiAsync(int mantieni = 10)
    {
        try
        {
            var result = GetBackupFiles();
            if (!result.Success)
            {
                return (false, result.Message);
            }

            var fileDaEliminare = result.Files.Skip(mantieni).ToList();
            
            if (fileDaEliminare.Count == 0)
            {
                return (true, "Nessun backup da eliminare.");
            }

            foreach (var file in fileDaEliminare)
            {
                File.Delete(file);
            }

            return (true, $"✅ Eliminati {fileDaEliminare.Count} backup vecchi.");
        }
        catch (Exception ex)
        {
            return (false, $"Errore: {ex.Message}");
        }
    }
}

