using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CGEasy.Core.Models;

/// <summary>
/// Modello per riga di statistica bilancio
/// Rappresenta una voce del template con i dati calcolati dal bilancio contabile
/// </summary>
public class BilancioStatistica : INotifyPropertyChanged
{
    private decimal _importo;
    private decimal _percentualeFatturato;

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
    /// Importo calcolato
    /// </summary>
    public decimal Importo
    {
        get => _importo;
        set
        {
            _importo = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ImportoFormatted));
            OnPropertyChanged(nameof(IsNegativo));
        }
    }

    /// <summary>
    /// Segno della voce (+ o -)
    /// </summary>
    public string Segno { get; set; } = string.Empty;

    /// <summary>
    /// Percentuale sul fatturato totale (sempre positiva)
    /// </summary>
    public decimal PercentualeFatturato
    {
        get => _percentualeFatturato;
        set
        {
            _percentualeFatturato = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PercentualeDisplay));
        }
    }

    /// <summary>
    /// Formula se presente
    /// </summary>
    public string? Formula { get; set; }

    /// <summary>
    /// Numero di conti associati a questa voce
    /// </summary>
    public int NumeroContiAssociati { get; set; }

    /// <summary>
    /// Lista dei conti associati (per dialog dettaglio)
    /// </summary>
    public List<ContoContabileAssociato> ContiAssociati { get; set; } = new();

    // Proprietà calcolate per UI
    public string ImportoFormatted => Importo.ToString("N2") + " €";
    
    public string PercentualeDisplay => PercentualeFatturato > 0 
        ? $"{PercentualeFatturato:F2}%" 
        : "N/A";

    public bool IsNegativo => Importo < 0;

    public bool HasFormula => !string.IsNullOrWhiteSpace(Formula);

    public bool HasContiAssociati => NumeroContiAssociati > 0;

    public string AssociazioniDisplay => HasContiAssociati 
        ? $"🔍 Dettagli ({NumeroContiAssociati})" 
        : "-";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

