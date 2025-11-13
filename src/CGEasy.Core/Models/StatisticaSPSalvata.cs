using System;
using System.Collections.Generic;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta una statistica SP salvata per poterla ricaricare in seguito
/// </summary>
public class StatisticaSPSalvata
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nome descrittivo dato dall'utente
    /// </summary>
    public string NomeStatistica { get; set; } = string.Empty;
    
    /// <summary>
    /// Cliente di riferimento
    /// </summary>
    public int ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Template usato
    /// </summary>
    public int TemplateMese { get; set; }
    public int TemplateAnno { get; set; }
    public string TemplateDescrizione { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista dei periodi selezionati (JSON serializzato)
    /// Formato: [{"Mese":9,"Anno":2025},{"Mese":10,"Anno":2025}]
    /// </summary>
    public string PeriodiJson { get; set; } = string.Empty;
    
    /// <summary>
    /// Dati statistiche (JSON serializzato)
    /// Contiene l'intera lista di BilancioStatisticaMultiPeriodo
    /// </summary>
    public string DatiStatisticheJson { get; set; } = string.Empty;
    
    /// <summary>
    /// Data creazione
    /// </summary>
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Utente che ha creato
    /// </summary>
    public int UtenteId { get; set; }
    public string NomeUtente { get; set; } = string.Empty;
    
    /// <summary>
    /// Note opzionali
    /// </summary>
    public string? Note { get; set; }
}

