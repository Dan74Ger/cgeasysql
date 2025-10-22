using System;

namespace CGEasy.Core.Models;

/// <summary>
/// Cliente a cui sono state rilasciate licenze
/// (DIVERSO dai clienti pratiche - questo è per gestione licenze software)
/// </summary>
public class LicenseClient
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nome del cliente/azienda
    /// </summary>
    public string NomeCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Email di contatto
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Telefono di contatto
    /// </summary>
    public string? Telefono { get; set; }
    
    /// <summary>
    /// Partita IVA o Codice Fiscale
    /// </summary>
    public string? PartitaIva { get; set; }
    
    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Note { get; set; }
    
    /// <summary>
    /// Data registrazione cliente
    /// </summary>
    public DateTime DataRegistrazione { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Cliente attivo o disattivato
    /// </summary>
    public bool IsActive { get; set; } = true;
}


