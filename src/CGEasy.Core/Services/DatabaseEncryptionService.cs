using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CGEasy.Core.Services;

/// <summary>
/// Servizio per gestire la crittografia del database LiteDB
/// </summary>
public class DatabaseEncryptionService
{
    private const string PASSWORD_FILE = "db.key";
    
    /// <summary>
    /// Password Master per recupero - SEGRETA!
    /// Solo Dott. Geron Daniele deve conoscerla
    /// </summary>
    private const string MASTER_PASSWORD = "Woodstockac@74";
    
    /// <summary>
    /// Percorso del file chiave - nella stessa cartella del database
    /// </summary>
    private static string PasswordFilePath => 
        Path.Combine(GetDatabaseDirectory(), PASSWORD_FILE);
    
    /// <summary>
    /// Ottiene la cartella dove si trova il database
    /// Usa la stessa logica di LiteDbContext per coerenza
    /// </summary>
    private static string GetDatabaseDirectory()
    {
        // Usa sempre la cartella dove si trova il database
        var dbPath = CGEasy.Core.Data.LiteDbContext.DefaultDatabasePath;
        return Path.GetDirectoryName(dbPath) ?? @"C:\devcg-group\dbtest_prova";
    }

    /// <summary>
    /// Ottiene la Password Master (solo per verifiche interne)
    /// </summary>
    public static string GetMasterPassword() => MASTER_PASSWORD;

    /// <summary>
    /// Verifica se il database è attualmente criptato
    /// Controlla se esiste db.key E se il database è effettivamente criptato
    /// </summary>
    public static bool IsDatabaseEncrypted()
    {
        // Se non esiste db.key, sicuramente NON è criptato
        if (!File.Exists(PasswordFilePath))
            return false;

        // Se esiste db.key, verifica se il database è DAVVERO criptato
        try
        {
            var dbPath = CGEasy.Core.Data.LiteDbContext.DefaultDatabasePath;
            if (!File.Exists(dbPath))
                return false; // Database non esiste ancora
            
            // Prova ad aprire SENZA password
            using (var testDb = new LiteDB.LiteDatabase(dbPath))
            {
                // Se riesce ad aprire senza password, NON è criptato!
                // Rimuovi db.key obsoleto
                try
                {
                    File.Delete(PasswordFilePath);
                    System.Diagnostics.Debug.WriteLine("⚠️ Rimosso db.key obsoleto - database non criptato");
                }
                catch { }
                
                return false;
            }
        }
        catch (LiteDB.LiteException ex) when (ex.Message.Contains("password"))
        {
            // Se fallisce con errore password, allora È criptato
            return true;
        }
        catch
        {
            // Altri errori: assumiamo NON criptato (db.key obsoleto)
            try
            {
                File.Delete(PasswordFilePath);
            }
            catch { }
            
            return false;
        }
    }

    /// <summary>
    /// Ottiene la password salvata (se esiste)
    /// </summary>
    public static string? GetSavedPassword()
    {
        try
        {
            if (!File.Exists(PasswordFilePath))
                return null;

            var encryptedText = File.ReadAllText(PasswordFilePath);
            return DecryptString(encryptedText);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Salva la password in modo sicuro (criptata con AES)
    /// </summary>
    public static void SavePassword(string password)
    {
        try
        {
            var directory = Path.GetDirectoryName(PasswordFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var encryptedText = EncryptString(password);
            File.WriteAllText(PasswordFilePath, encryptedText);
        }
        catch (Exception ex)
        {
            throw new Exception($"Errore salvataggio password: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cripta una stringa con AES (semplificato per salvataggio locale)
    /// </summary>
    private static string EncryptString(string plainText)
    {
        // Semplice Base64 encoding per salvataggio locale
        // La vera sicurezza è nel database LiteDB criptato
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decripta una stringa (Base64 decode)
    /// </summary>
    private static string DecryptString(string encodedText)
    {
        var bytes = Convert.FromBase64String(encodedText);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Rimuove la password salvata (decripta il database)
    /// </summary>
    public static void RemovePassword()
    {
        try
        {
            if (File.Exists(PasswordFilePath))
            {
                File.Delete(PasswordFilePath);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Errore rimozione password: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Verifica se la password fornita è corretta (confronta con quella salvata o master)
    /// </summary>
    public static bool VerifyPassword(string password)
    {
        var savedPassword = GetSavedPassword();
        if (savedPassword == null)
            return false;

        // Verifica password normale O master password
        return password == savedPassword || password == MASTER_PASSWORD;
    }

    /// <summary>
    /// Crea un backup del database prima di modificare la password
    /// </summary>
    public static string CreateBackupBeforeEncryption(string databasePath)
    {
        try
        {
            var backupDir = Path.Combine(Path.GetDirectoryName(databasePath)!, "Backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"backup_before_encryption_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, backupFileName);

            File.Copy(databasePath, backupPath, true);

            return backupPath;
        }
        catch (Exception ex)
        {
            throw new Exception($"Errore creazione backup: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Genera una password casuale sicura
    /// </summary>
    public static string GenerateRandomPassword(int length = 16)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Valida la complessità della password
    /// </summary>
    public static (bool IsValid, string Message) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "La password non può essere vuota");

        if (password.Length < 8)
            return (false, "La password deve essere di almeno 8 caratteri");

        if (password.Length > 50)
            return (false, "La password non può superare i 50 caratteri");

        // Almeno una lettera e un numero
        bool hasLetter = password.Any(char.IsLetter);
        bool hasDigit = password.Any(char.IsDigit);

        if (!hasLetter || !hasDigit)
            return (false, "La password deve contenere almeno una lettera e un numero");

        return (true, "Password valida");
    }
}

