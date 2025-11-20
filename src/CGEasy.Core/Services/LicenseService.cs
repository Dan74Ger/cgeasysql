using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.Core.Services;

/// <summary>
/// Gestione licenze moduli offline con database clienti e chiavi uniche
/// OGNI CHIAVE È UNICA E TRACCIABILE
/// </summary>
public class LicenseService
{
    private const string LICENSE_FILE = "licenses.json";
    
    /// <summary>
    /// Ottiene il percorso del file licenses.json nella stessa cartella del database
    /// Se il database è su server (\\SERVER\Share\cgeasy.db), anche licenses.json sarà lì
    /// </summary>
    private static string GetLicensePath()
    {
        // Ottiene il percorso del database (configurato o default)
        var dbPath = LiteDbContext.DefaultDatabasePath;
        var dbDirectory = Path.GetDirectoryName(dbPath);
        
        if (string.IsNullOrEmpty(dbDirectory))
        {
            // Fallback al percorso default se qualcosa va storto
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "CGEasy",
                LICENSE_FILE);
        }
        
        return Path.Combine(dbDirectory, LICENSE_FILE);
    }

    private const string SECRET_KEY = "CGEasy2025-TodoStudio-SecretKey-DottGeronDaniele";

    /// <summary>
    /// Licenze attive localmente (caricata dal file)
    /// </summary>
    private static readonly Dictionary<string, string> _licenses = new();

    /// <summary>
    /// Repository globale per validazione (iniettato dall'app principale)
    /// </summary>
    private static LicenseRepository? _globalRepository = null;

    static LicenseService()
    {
        LoadLicenses();
    }

    /// <summary>
    /// Imposta il repository globale (chiamato dall'app all'avvio)
    /// </summary>
    public static void SetGlobalRepository(LicenseRepository repository)
    {
        _globalRepository = repository;
    }

    /// <summary>
    /// Carica licenze salvate localmente
    /// </summary>
    private static void LoadLicenses()
    {
        try
        {
            var licensePath = GetLicensePath();
            if (File.Exists(licensePath))
            {
                var json = File.ReadAllText(licensePath);
                var licenses = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (licenses != null)
                {
                    _licenses.Clear(); // Pulisce la cache prima di ricaricare
                    foreach (var license in licenses)
                    {
                        _licenses[license.Key] = license.Value;
                    }
                }
            }
        }
        catch
        {
            // File corrotto o mancante, ignora
        }
    }

    /// <summary>
    /// Ricarica le licenze dal file e ricontrolla la validità nel database
    /// Utile dopo modifiche alla scadenza delle licenze
    /// </summary>
    public static void ReloadLicenses()
    {
        LoadLicenses();
    }

    /// <summary>
    /// Salva licenze su disco
    /// </summary>
    private static void SaveLicenses()
    {
        try
        {
            var licensePath = GetLicensePath();
            var directory = Path.GetDirectoryName(licensePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_licenses, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(licensePath, json);
        }
        catch
        {
            // Errore salvataggio, ignora
        }
    }

    // ===== TODO-STUDIO =====
    
    public static bool IsTodoStudioActive()
    {
        return _licenses.ContainsKey("TODO-STUDIO") && 
               ValidateLicenseWithNewContext("TODO-STUDIO", _licenses["TODO-STUDIO"]);
    }

    public static bool ActivateTodoStudio(string licenseKey)
    {
        return ActivateModule("TODO-STUDIO", licenseKey);
    }

    public static void DeactivateTodoStudio()
    {
        DeactivateModule("TODO-STUDIO");
    }

    public static string? GetTodoStudioKey()
    {
        return GetModuleKey("TODO-STUDIO");
    }

    // ===== BILANCI =====
    
    public static bool IsBilanciActive()
    {
        return _licenses.ContainsKey("BILANCI") && 
               ValidateLicenseWithNewContext("BILANCI", _licenses["BILANCI"]);
    }

    public static bool ActivateBilanci(string licenseKey)
    {
        return ActivateModule("BILANCI", licenseKey);
    }

    public static void DeactivateBilanci()
    {
        DeactivateModule("BILANCI");
    }

    public static string? GetBilanciKey()
    {
        return GetModuleKey("BILANCI");
    }

    // ===== CIRCOLARI =====
    
    public static bool IsCircolariActive()
    {
        return _licenses.ContainsKey("CIRCOLARI") && 
               ValidateLicenseWithNewContext("CIRCOLARI", _licenses["CIRCOLARI"]);
    }

    public static bool ActivateCircolari(string licenseKey)
    {
        return ActivateModule("CIRCOLARI", licenseKey);
    }

    public static void DeactivateCircolari()
    {
        DeactivateModule("CIRCOLARI");
    }

    public static string? GetCircolariKey()
    {
        return GetModuleKey("CIRCOLARI");
    }

    // ===== CONTROLLO GESTIONE =====
    
    public static bool IsControlloGestioneActive()
    {
        return _licenses.ContainsKey("CONTROLLO-GESTIONE") && 
               ValidateLicenseWithNewContext("CONTROLLO-GESTIONE", _licenses["CONTROLLO-GESTIONE"]);
    }

    public static bool ActivateControlloGestione(string licenseKey)
    {
        return ActivateModule("CONTROLLO-GESTIONE", licenseKey);
    }

    public static void DeactivateControlloGestione()
    {
        DeactivateModule("CONTROLLO-GESTIONE");
    }

    public static string? GetControlloGestioneKey()
    {
        return GetModuleKey("CONTROLLO-GESTIONE");
    }

    // ===== METODI GENERICI =====

    private static void LogDebug(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);
        try
        {
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "CGEasy", "license_debug.log");
            var logDir = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}\n");
        }
        catch { /* Ignora errori di log */ }
    }

    private static bool ActivateModule(string moduleName, string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            LogDebug($"❌ Attivazione {moduleName} fallita: chiave vuota");
            return false;
        }

        var originalKey = licenseKey;
        licenseKey = licenseKey.Trim().ToUpper();

        LogDebug($"🔍 Tentativo attivazione {moduleName}");
        LogDebug($"🔍 Chiave originale: [{originalKey}]");
        LogDebug($"🔍 Chiave dopo trim/upper: [{licenseKey}]");

        // Valida formato
        if (!licenseKey.StartsWith($"{moduleName}-"))
        {
            LogDebug($"❌ Attivazione {moduleName} fallita: formato non valido. Chiave: {licenseKey}");
            return false;
        }

        // 🔥 IMPORTANTE: Non possiamo aprire il database qui perché è già aperto dall'app
        // Salviamo nel file JSON e la validazione avverrà al prossimo avvio o quando si accede al modulo
        
        LogDebug($"✅ Formato chiave {moduleName} valido, salvando nel file locale");
        
        // Salva licenza nel file JSON locale
        _licenses[moduleName] = licenseKey;
        SaveLicenses();
        
        LogDebug($"✅ Modulo {moduleName} salvato nel file licenze");
        LogDebug($"⚠️  La validazione della chiave avverrà quando si accede al modulo");
        
        return true;
    }

    private static void DeactivateModule(string moduleName)
    {
        if (_licenses.ContainsKey(moduleName))
        {
            _licenses.Remove(moduleName);
            SaveLicenses();
        }
    }

    private static string? GetModuleKey(string moduleName)
    {
        return _licenses.ContainsKey(moduleName) ? _licenses[moduleName] : null;
    }

    /// <summary>
    /// Valida una chiave di licenza (controlla se esiste nel database)
    /// </summary>
    private static bool ValidateLicense(string moduleName, string licenseKey, CGEasyDbContext? context = null)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        try
        {
            // Se context non è fornito, ne crea uno nuovo (potrebbe causare problemi di lock)
            if (context != null)
            {
                var repo = new LicenseRepository(context);
                return repo.IsKeyValid(licenseKey);
            }
            else
            {
                // Fallback: crea context temporaneo (può fallire se DB già aperto)
                try
                {
                    using var tempContext = new CGEasyDbContext();
                    var repo = new LicenseRepository(tempContext);
                    return repo.IsKeyValid(licenseKey);
                }
                catch
                {
                    // Se il database è bloccato, la chiave potrebbe essere valida ma non possiamo verificarla
                    // In questo caso, confidiamo che se è salvata localmente, è valida
                    return true; // Assume valida se già salvata in _licenses
                }
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida una chiave di licenza creando un nuovo context
    /// 🔥 CONTROLLO RIGOROSO MASSIMO: 
    ///   - Licenza NON esiste → BLOCCA
    ///   - Licenza scaduta → BLOCCA (anche per SISTEMA)
    ///   - Errore DB → BLOCCA (sicurezza massima)
    /// </summary>
    private static bool ValidateLicenseWithNewContext(string moduleName, string licenseKey)
    {
        LogDebug($"🔍 Validazione licenza {moduleName}: chiave=[{licenseKey}]");
        
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            LogDebug($"❌ Validazione {moduleName} fallita: chiave vuota");
            return false;
        }

        try
        {
            // 🔥 USA IL REPOSITORY GLOBALE SE DISPONIBILE (evita conflitti di accesso al DB)
            if (_globalRepository != null)
            {
                LogDebug($"🔍 Uso repository globale per validazione {moduleName}");
                
                // Cerca la chiave nel database usando il repository condiviso
                var key = _globalRepository.GetKeyByFullKey(licenseKey);
                
                if (key == null)
                {
                    LogDebug($"❌ Licenza {moduleName} non trovata nel database: {licenseKey}");
                    return false;
                }
                
                // Verifica scadenza
                bool isValid = key.IsActive && !key.IsExpired;
                
                if (!isValid)
                {
                    LogDebug($"❌ Licenza {moduleName} NON VALIDA - IsActive: {key.IsActive}, IsExpired: {key.IsExpired}, Scadenza: {key.DataScadenza}");
                }
                else
                {
                    LogDebug($"✅ Licenza {moduleName} VALIDA - Scadenza: {key.DataScadenza?.ToString("dd/MM/yyyy") ?? "♾️ Perpetua"}");
                }
                
                return isValid;
            }
            
            // Fallback: apri nuovo contesto (solo se repository non disponibile)
            LogDebug($"🔍 Tentativo apertura database per validazione {moduleName}");
            
            // Usa CGEasyDbContext
            using var context = new CGEasyDbContext();
            var repo = new LicenseRepository(context);
            
            LogDebug($"🔍 Database aperto, cerco chiave nel DB");
            
            // Cerca la chiave nel database
            var fallbackKey = repo.GetKeyByFullKey(licenseKey);
            
            if (fallbackKey == null)
            {
                LogDebug($"❌ Licenza {moduleName} non trovata nel database: {licenseKey}");
                // 🔥 Licenza non trovata = ACCESSO NEGATO
                return false;
            }
            
            // 🔥 CONTROLLO SCADENZA RIGOROSO
            // Verifica che sia attiva e non scaduta (per TUTTI i clienti, anche SISTEMA)
            bool fallbackIsValid = fallbackKey.IsActive && !fallbackKey.IsExpired;
            
            if (!fallbackIsValid)
            {
                LogDebug($"❌ Licenza {moduleName} NON VALIDA - IsActive: {fallbackKey.IsActive}, IsExpired: {fallbackKey.IsExpired}, Scadenza: {fallbackKey.DataScadenza}");
            }
            else
            {
                LogDebug($"✅ Licenza {moduleName} VALIDA - Scadenza: {fallbackKey.DataScadenza?.ToString("dd/MM/yyyy") ?? "♾️ Perpetua"}");
            }
            
            return fallbackIsValid;
        }
        catch (Exception ex)
        {
            // Log per debug
            LogDebug($"❌ ERRORE CRITICO validazione licenza {moduleName}: {ex.Message}");
            LogDebug($"❌ Stack trace: {ex.StackTrace}");
            
            // 🔥 NESSUN FALLBACK - SICUREZZA MASSIMA
            // Se c'è un errore, la licenza NON è valida = ACCESSO NEGATO
            return false;
        }
    }

    /// <summary>
    /// Genera hash unico per licenza (con GUID per unicità)
    /// </summary>
    public static string GenerateLicenseHash(string moduleName, string guid)
    {
        var data = $"{moduleName}-{guid}-{SECRET_KEY}-{DateTime.Now.Ticks}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        
        // Prende primi 16 caratteri dell'hash in base64 pulito
        var hash = Convert.ToBase64String(hashBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .ToUpper();
        
        return hash.Substring(0, Math.Min(16, hash.Length));
    }

    /// <summary>
    /// Genera chiave completa per modulo con cliente (DA USARE NELLA VISTA GESTIONE LICENZE)
    /// OGNI CHIAVE È UNICA E VIENE SALVATA NEL DATABASE
    /// </summary>
    public static string GenerateLicenseKey(string moduleName, int licenseClientId, int generatedByUserId, CGEasyDbContext context)
    {
        var guid = Guid.NewGuid().ToString();
        var hash = GenerateLicenseHash(moduleName, guid);
        var fullKey = $"{moduleName.ToUpper()}-{hash}";
        
        // Salva chiave nel database
        var repo = new LicenseRepository(context);
        var licenseKey = new LicenseKey
        {
            LicenseClientId = licenseClientId,
            ModuleName = moduleName.ToUpper(),
            FullKey = fullKey,
            LicenseGuid = guid,
            DataGenerazione = DateTime.Now,
            IsActive = true,
            GeneratedByUserId = generatedByUserId
        };
        
        repo.InsertKey(licenseKey);
        
        return fullKey;
    }

    /// <summary>
    /// Genera chiave completa SEMPLICE (per uso veloce, senza cliente specifico)
    /// DEPRECATED - Usa GenerateLicenseKey con cliente invece
    /// </summary>
    [Obsolete("Usa GenerateLicenseKey(moduleName, clientId, userId, context) per tracciabilità")]
    public static string GenerateLicenseKey(string moduleName)
    {
        var guid = Guid.NewGuid().ToString();
        var hash = GenerateLicenseHash(moduleName, guid);
        return $"{moduleName.ToUpper()}-{hash}";
    }

    /// <summary>
    /// Ottiene info su tutte le licenze attive
    /// </summary>
    public static Dictionary<string, bool> GetAllModuleStatus()
    {
        return new Dictionary<string, bool>
        {
            { "TODO-STUDIO", IsTodoStudioActive() },
            { "BILANCI", IsBilanciActive() },
            { "CIRCOLARI", IsCircolariActive() },
            { "CONTROLLO-GESTIONE", IsControlloGestioneActive() }
        };
    }

    /// <summary>
    /// Conta moduli attivati
    /// </summary>
    public static int GetActiveModulesCount()
    {
        return GetAllModuleStatus().Count(m => m.Value);
    }
}

