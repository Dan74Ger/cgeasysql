using System;

namespace CGEasy.Core.Models;

/// <summary>
/// Chiave di licenza generata per un cliente e un modulo specifico
/// </summary>
public class LicenseKey
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID del cliente a cui è stata rilasciata la licenza
    /// </summary>
    public int LicenseClientId { get; set; }
    
    /// <summary>
    /// Nome del modulo (TODO-STUDIO, BILANCI, CIRCOLARI, CONTROLLO-GESTIONE)
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Chiave completa generata (es: TODO-STUDIO-A1B2C3D4E5F6)
    /// </summary>
    public string FullKey { get; set; } = string.Empty;
    
    /// <summary>
    /// GUID unico per questa licenza
    /// </summary>
    public string LicenseGuid { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Data e ora di generazione
    /// </summary>
    public DateTime DataGenerazione { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Licenza attiva o revocata
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Note sulla licenza
    /// </summary>
    public string? Note { get; set; }
    
    /// <summary>
    /// Data scadenza (null = licenza perpetua)
    /// </summary>
    public DateTime? DataScadenza { get; set; }
    
    /// <summary>
    /// Utente che ha generato la chiave
    /// </summary>
    public int GeneratedByUserId { get; set; }
    
    /// <summary>
    /// Verifica se la licenza è scaduta
    /// </summary>
    public bool IsExpired => DataScadenza.HasValue && DataScadenza.Value < DateTime.Now;
}


