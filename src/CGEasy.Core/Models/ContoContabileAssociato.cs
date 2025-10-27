namespace CGEasy.Core.Models;

/// <summary>
/// Modello per conto contabile associato a una voce template
/// Usato per mostrare il dettaglio delle associazioni
/// </summary>
public class ContoContabileAssociato
{
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

