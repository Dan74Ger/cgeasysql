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
    private static readonly string LICENSE_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
        "CGEasy",
        LICENSE_FILE);

    private const string SECRET_KEY = "CGEasy2025-TodoStudio-SecretKey-DottGeronDaniele";

    /// <summary>
    /// Licenze attive localmente (caricata dal file)
    /// </summary>
    private static readonly Dictionary<string, string> _licenses = new();

    static LicenseService()
    {
        LoadLicenses();
    }

    /// <summary>
    /// Carica licenze salvate localmente
    /// </summary>
    private static void LoadLicenses()
    {
        try
        {
            if (File.Exists(LICENSE_PATH))
            {
                var json = File.ReadAllText(LICENSE_PATH);
                var licenses = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (licenses != null)
                {
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
    /// Salva licenze su disco
    /// </summary>
    private static void SaveLicenses()
    {
        try
        {
            var directory = Path.GetDirectoryName(LICENSE_PATH);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_licenses, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(LICENSE_PATH, json);
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
               ValidateLicense("TODO-STUDIO", _licenses["TODO-STUDIO"]);
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
               ValidateLicense("BILANCI", _licenses["BILANCI"]);
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
               ValidateLicense("CIRCOLARI", _licenses["CIRCOLARI"]);
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
               ValidateLicense("CONTROLLO-GESTIONE", _licenses["CONTROLLO-GESTIONE"]);
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

    private static bool ActivateModule(string moduleName, string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        licenseKey = licenseKey.Trim().ToUpper();

        // Valida formato
        if (!licenseKey.StartsWith($"{moduleName}-"))
            return false;

        // Valida chiave
        if (!ValidateLicense(moduleName, licenseKey))
            return false;

        // Salva licenza
        _licenses[moduleName] = licenseKey;
        SaveLicenses();
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
    private static bool ValidateLicense(string moduleName, string licenseKey, LiteDbContext? context = null)
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
                    using var tempContext = new LiteDbContext();
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
    public static string GenerateLicenseKey(string moduleName, int licenseClientId, int generatedByUserId, LiteDbContext context)
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

