namespace CGEasy.Core.Models;

/// <summary>
/// Modello per conto contabile associato con informazioni sul periodo
/// Usato per mostrare il dettaglio delle associazioni multi-periodo
/// </summary>
public class ContoContabileAssociatoConPeriodo
{
    /// <summary>
    /// Periodo di riferimento (es: "Gen 2024")
    /// </summary>
    public string Periodo { get; set; } = string.Empty;

    /// <summary>
    /// Mese (1-12)
    /// </summary>
    public int Mese { get; set; }

    /// <summary>
    /// Anno
    /// </summary>
    public int Anno { get; set; }

    /// <summary>
    /// Codice del mastrino/conto contabile
    /// </summary>
    public string CodiceConto { get; set; } = string.Empty;

    /// <summary>
    /// Descrizione del mastrino/conto contabile
    /// </summary>
    public string DescrizioneConto { get; set; } = string.Empty;

    /// <summary>
    /// Importo del conto
    /// </summary>
    public decimal Importo { get; set; }

    /// <summary>
    /// Importo formattato per visualizzazione
    /// </summary>
    public string ImportoFormatted => Importo.ToString("N2") + " €";

    /// <summary>
    /// Indica se l'importo è negativo
    /// </summary>
    public bool IsNegativo => Importo < 0;
}










