using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CGEasy.Core.Models;

/// <summary>
/// Modello per riga di statistica bilancio con supporto multi-periodo
/// Rappresenta una voce del template con i dati calcolati da più bilanci contabili
/// </summary>
public class BilancioStatisticaMultiPeriodo : INotifyPropertyChanged
{
    /// <summary>
    /// ID della voce template di riferimento
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// Codice della voce (es: A.1, B.2, etc.)
    /// </summary>
    public string Codice { get; set; } = string.Empty;

    /// <summary>
    /// Descrizione della voce
    /// </summary>
    public string Descrizione { get; set; } = string.Empty;

    /// <summary>
    /// Segno della voce (+ o -)
    /// </summary>
    public string Segno { get; set; } = string.Empty;

    /// <summary>
    /// Formula se presente
    /// </summary>
    public string? Formula { get; set; }

    /// <summary>
    /// Dati per ogni periodo (chiave: identificativo periodo, valore: dati periodo)
    /// </summary>
    public Dictionary<string, DatiPeriodo> DatiPerPeriodo { get; set; } = new();

    /// <summary>
    /// Importo totale (somma di tutti i periodi)
    /// </summary>
    public decimal ImportoTotale { get; set; }

    /// <summary>
    /// Percentuale totale sul fatturato complessivo
    /// </summary>
    public decimal PercentualeTotale { get; set; }

    /// <summary>
    /// Numero totale di conti associati (tutti i periodi)
    /// </summary>
    public int NumeroContiAssociatiTotali { get; set; }

    /// <summary>
    /// Lista completa dei conti associati di tutti i periodi (per dialog dettaglio)
    /// </summary>
    public List<ContoContabileAssociatoConPeriodo> ContiAssociatiTutti { get; set; } = new();

    // Proprietà calcolate per UI
    public string ImportoTotaleFormatted => ImportoTotale.ToString("N2") + " €";
    
    public string PercentualeTotaleDisplay => PercentualeTotale > 0 
        ? $"{PercentualeTotale:F2}%" 
        : "N/A";

    public bool IsNegativo => ImportoTotale < 0;

    public bool HasFormula => !string.IsNullOrWhiteSpace(Formula);

    public bool HasContiAssociati => NumeroContiAssociatiTotali > 0;

    public string AssociazioniDisplay => HasContiAssociati 
        ? $"🔍 Dettagli ({NumeroContiAssociatiTotali})" 
        : "-";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Dati statistici per un singolo periodo
/// </summary>
public class DatiPeriodo
{
    /// <summary>
    /// Identificativo del periodo (es: "Gen 2024", "Feb 2024")
    /// </summary>
    public string PeriodoDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Mese (1-12)
    /// </summary>
    public int Mese { get; set; }

    /// <summary>
    /// Anno
    /// </summary>
    public int Anno { get; set; }

    /// <summary>
    /// Importo per questo periodo
    /// </summary>
    public decimal Importo { get; set; }

    /// <summary>
    /// Percentuale sul fatturato di questo periodo
    /// </summary>
    public decimal Percentuale { get; set; }

    /// <summary>
    /// Numero di conti associati per questo periodo
    /// </summary>
    public int NumeroConti { get; set; }

    /// <summary>
    /// Lista dei conti associati per questo periodo
    /// </summary>
    public List<ContoContabileAssociato> Conti { get; set; } = new();

    // Proprietà formattate
    public string ImportoFormatted => Importo.ToString("N2") + " €";
    
    public string PercentualeDisplay => Percentuale > 0 
        ? $"{Percentuale:F2}%" 
        : "N/A";
}










