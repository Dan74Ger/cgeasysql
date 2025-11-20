using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta un incasso da cliente per una banca (EF Core)
/// </summary>
[Table("banca_incassi")]
public class BancaIncasso
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("banca_id")]
    public int BancaId { get; set; }
    
    [Column("nome_cliente")]
    [Required]
    [MaxLength(200)]
    public string NomeCliente { get; set; } = string.Empty;
    
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
    
    [Column("anticipo_gestito_cc")]
    public bool AnticipoGestito_CC { get; set; }
    
    [Column("data_scadenza_anticipo")]
    public DateTime? DataScadenzaAnticipo { get; set; }
    
    [Column("anticipo_chiuso_cc")]
    public bool AnticipoChiuso_CC { get; set; }
    
    [NotMapped]
    public decimal ImportoFatturaScadenza => Importo - ImportoAnticipato;
    
    [Column("incassato")]
    public bool Incassato { get; set; }
    
    [Column("data_scadenza")]
    public DateTime DataScadenza { get; set; }
    
    [Column("data_incasso_effettivo")]
    public DateTime? DataIncassoEffettivo { get; set; }
    
    [Column("numero_fattura_cliente")]
    [MaxLength(50)]
    public string? NumeroFatturaCliente { get; set; }
    
    [Column("data_fattura_cliente")]
    public DateTime? DataFatturaCliente { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    [Column("data_ultima_modifica")]
    public DateTime DataUltimaModifica { get; set; } = DateTime.Now;
}
