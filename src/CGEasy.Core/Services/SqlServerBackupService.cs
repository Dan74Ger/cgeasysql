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
        try
        {
            if (!File.Exists(backupPath))
            {
                return (false, "❌ File di backup non trovato.");
            }

            // Ottieni il nome del database
            var connectionString = _context.Database.GetConnectionString();
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;

            // Chiudi tutte le connessioni attive al database
            var killConnectionsSql = $@"
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            ";

            // Ripristina il database
            var restoreSql = $@"
                RESTORE DATABASE [{databaseName}] 
                FROM DISK = @backupPath 
                WITH REPLACE;
                
                ALTER DATABASE [{databaseName}] SET MULTI_USER;
            ";

            await _context.Database.ExecuteSqlRawAsync(killConnectionsSql);
            await _context.Database.ExecuteSqlRawAsync(restoreSql, new SqlParameter("@backupPath", backupPath));

            return (true, $"✅ Database ripristinato con successo!\n\nL'applicazione verrà riavviata.");
        }
        catch (Exception ex)
        {
            return (false, $"❌ Errore durante il ripristino:\n{ex.Message}\n\nAssicurati che SQL Server sia in esecuzione e che non ci siano connessioni attive al database.");
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

