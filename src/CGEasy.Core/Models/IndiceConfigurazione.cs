using LiteDB;
using System;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta la configurazione di un indice (standard o personalizzato)
/// salvata per un cliente specifico
/// </summary>
public class IndiceConfigurazione
{
    [BsonId]
    public int Id { get; set; }
    
    /// <summary>
    /// Riferimento al cliente (0 = configurazione globale)
    /// </summary>
    public int ClienteId { get; set; }
    
    /// <summary>
    /// Nome dell'indice (es: "Liquidità Corrente")
    /// </summary>
    public string NomeIndice { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria (Liquidità, Solidità, Redditività, Efficienza, Personalizzato)
    /// </summary>
    public string Categoria { get; set; } = string.Empty;
    
    /// <summary>
    /// Formula leggibile (es: "Attivo Corrente / Passivo Corrente")
    /// </summary>
    public string Formula { get; set; } = string.Empty;
    
    /// <summary>
    /// Unità di misura (€, %, giorni, volte, ecc.)
    /// </summary>
    public string UnitaMisura { get; set; } = string.Empty;
    
    /// <summary>
    /// Se true, l'indice è abilitato per il calcolo
    /// </summary>
    public bool IsAbilitato { get; set; } = true;
    
    /// <summary>
    /// Se true, è un indice standard (non modificabile)
    /// </summary>
    public bool IsStandard { get; set; }
    
    /// <summary>
    /// Codice identificativo univoco (es: "LIQ_CORRENTE")
    /// </summary>
    public string CodiceIndice { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON con i dettagli di calcolo (codici voci, operatori, ecc.)
    /// </summary>
    public string DettagliCalcoloJson { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrizione estesa dell'indice
    /// </summary>
    public string? Descrizione { get; set; }
    
    /// <summary>
    /// Utente che ha creato/modificato
    /// </summary>
    public int UtenteId { get; set; }
    
    public DateTime DataCreazione { get; set; }
    public DateTime? UltimaModifica { get; set; }
    
    /// <summary>
    /// Ordinamento per visualizzazione
    /// </summary>
    public int Ordinamento { get; set; }
}




