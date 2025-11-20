using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta un utilizzo di anticipo fatture/SBF per una banca (EF Core)
/// </summary>
[Table("banca_utilizzo_anticipo")]
public class BancaUtilizzoAnticipo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("banca_id")]
    public int BancaId { get; set; }
    
    [Column("fatturato")]
    [Precision(18, 2)]
    public decimal Fatturato { get; set; }
    
    [Column("percentuale_anticipo")]
    [Precision(5, 2)]
    public decimal PercentualeAnticipo { get; set; }
    
    [Column("importo_utilizzo")]
    [Precision(18, 2)]
    public decimal ImportoUtilizzo { get; set; }
    
    [Column("data_inizio_utilizzo")]
    public DateTime DataInizioUtilizzo { get; set; }
    
    [Column("data_scadenza_utilizzo")]
    public DateTime DataScadenzaUtilizzo { get; set; }
    
    [Column("rimborsato")]
    public bool Rimborsato { get; set; }
    
    [Column("data_rimborso_effettivo")]
    public DateTime? DataRimborsoEffettivo { get; set; }
    
    [Column("interessi_maturati")]
    [Precision(18, 2)]
    public decimal InteressiMaturati { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    [Column("data_ultima_modifica")]
    public DateTime DataUltimaModifica { get; set; } = DateTime.Now;
}
