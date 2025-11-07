using System;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta un incasso da cliente per una banca
/// </summary>
public class BancaIncasso
{
    public int Id { get; set; }
    
    /// <summary>
    /// ID della banca di riferimento
    /// </summary>
    public int BancaId { get; set; }
    
    /// <summary>
    /// Nome del cliente
    /// </summary>
    public string NomeCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Anno di riferimento
    /// </summary>
    public int Anno { get; set; }
    
    /// <summary>
    /// Mese di riferimento (1-12)
    /// </summary>
    public int Mese { get; set; }
    
    /// <summary>
    /// Importo della fattura
    /// </summary>
    public decimal Importo { get; set; }
    
    /// <summary>
    /// Percentuale di anticipo richiesta (0-100)
    /// </summary>
    public decimal PercentualeAnticipo { get; set; }
    
    /// <summary>
    /// Importo anticipato calcolato (Importo * PercentualeAnticipo / 100)
    /// </summary>
    public decimal ImportoAnticipato => Importo * (PercentualeAnticipo / 100);
    
    /// <summary>
    /// Data inizio anticipo
    /// </summary>
    public DateTime? DataInizioAnticipo { get; set; }
    
    /// <summary>
    /// Anticipo gestito tramite C/C (non visibile in Saldo Previsto)
    /// </summary>
    public bool AnticipoGestito_CC { get; set; }
    
    /// <summary>
    /// Data scadenza anticipo
    /// </summary>
    public DateTime? DataScadenzaAnticipo { get; set; }
    
    /// <summary>
    /// Anticipo chiuso tramite C/C (storno non visibile in Saldo Previsto)
    /// </summary>
    public bool AnticipoChiuso_CC { get; set; }
    
    /// <summary>
    /// Importo fattura a scadenza (Importo - ImportoAnticipato)
    /// </summary>
    public decimal ImportoFatturaScadenza => Importo - ImportoAnticipato;
    
    /// <summary>
    /// Flag che indica se l'importo è stato incassato
    /// </summary>
    public bool Incassato { get; set; }
    
    /// <summary>
    /// Data di scadenza prevista per l'incasso (fattura)
    /// </summary>
    public DateTime DataScadenza { get; set; }
    
    /// <summary>
    /// Data effettiva di incasso (se avvenuto)
    /// </summary>
    public DateTime? DataIncassoEffettivo { get; set; }
    
    /// <summary>
    /// Numero fattura cliente (opzionale)
    /// </summary>
    public string? NumeroFatturaCliente { get; set; }
    
    /// <summary>
    /// Data fattura cliente (opzionale)
    /// </summary>
    public DateTime? DataFatturaCliente { get; set; }
    
    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Note { get; set; }
    
    /// <summary>
    /// Data creazione record
    /// </summary>
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Data ultima modifica
    /// </summary>
    public DateTime DataUltimaModifica { get; set; } = DateTime.Now;
}

