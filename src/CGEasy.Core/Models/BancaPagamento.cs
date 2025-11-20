using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta un pagamento a fornitore per una banca (EF Core)
/// </summary>
[Table("banca_pagamenti")]
public class BancaPagamento
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("banca_id")]
    public int BancaId { get; set; }
    
    [Column("nome_fornitore")]
    [Required]
    [MaxLength(200)]
    public string NomeFornitore { get; set; } = string.Empty;
    
    [Column("anno")]
    public int Anno { get; set; }
    
    [Column("mese")]
    public int Mese { get; set; }
    
    [Column("importo")]
    [Precision(18, 2)]
    public decimal Importo { get; set; }
    
    [Column("percentuale_anticipo")]
    [Precision(5, 2)]
    public decimal PercentualeAnticipo { get; set; }
    
    [NotMapped]
    public decimal ImportoAnticipato => Importo * (PercentualeAnticipo / 100);
    
    [Column("data_inizio_anticipo")]
    public DateTime? DataInizioAnticipo { get; set; }
    
    [Column("data_scadenza_anticipo")]
    public DateTime? DataScadenzaAnticipo { get; set; }
    
    [NotMapped]
    public decimal ImportoFatturaScadenza => Importo - ImportoAnticipato;
    
    [Column("pagato")]
    public bool Pagato { get; set; }
    
    [Column("data_scadenza")]
    public DateTime DataScadenza { get; set; }
    
    [Column("data_pagamento_effettivo")]
    public DateTime? DataPagamentoEffettivo { get; set; }
    
    [Column("numero_fattura_fornitore")]
    [MaxLength(50)]
    public string? NumeroFatturaFornitore { get; set; }
    
    [Column("data_fattura_fornitore")]
    public DateTime? DataFatturaFornitore { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    [Column("data_ultima_modifica")]
    public DateTime DataUltimaModifica { get; set; } = DateTime.Now;
}
