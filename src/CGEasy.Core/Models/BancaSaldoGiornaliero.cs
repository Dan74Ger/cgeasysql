using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta lo storico del saldo giornaliero di una banca (EF Core)
/// </summary>
[Table("banca_saldo_giornaliero")]
public class BancaSaldoGiornaliero
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("banca_id")]
    public int BancaId { get; set; }
    
    [Column("data")]
    public DateTime Data { get; set; }
    
    [Column("saldo")]
    [Precision(18, 2)]
    public decimal Saldo { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
}
