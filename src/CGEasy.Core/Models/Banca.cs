using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta una banca gestita nel sistema (EF Core)
/// </summary>
[Table("banche")]
public class Banca
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("nome_banca")]
    [Required]
    [MaxLength(200)]
    public string NomeBanca { get; set; } = string.Empty;
    
    [Column("codice_identificativo")]
    [MaxLength(50)]
    public string? CodiceIdentificativo { get; set; }
    
    [Column("iban")]
    [MaxLength(50)]
    public string? IBAN { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("saldo_del_giorno")]
    [Precision(18, 2)]
    public decimal SaldoDelGiorno { get; set; }
    
    [Column("fido_cc_accordato")]
    [Precision(18, 2)]
    public decimal FidoCCAccordato { get; set; }
    
    [Column("anticipo_fatture_massimo")]
    [Precision(18, 2)]
    public decimal AnticipoFattureMassimo { get; set; }
    
    [Column("interesse_anticipo_fatture")]
    [Precision(5, 2)]
    public decimal InteresseAnticipoFatture { get; set; }
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    [Column("data_ultima_modifica")]
    public DateTime DataUltimaModifica { get; set; } = DateTime.Now;
}
