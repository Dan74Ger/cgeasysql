using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

[Table("bilancio_template")]
public class BilancioTemplate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("cliente_id")]
    public int ClienteId { get; set; }
    
    [Column("cliente_nome")]
    [Required]
    [MaxLength(200)]
    public string ClienteNome { get; set; } = string.Empty;
    
    [Column("mese")]
    public int Mese { get; set; }
    
    [Column("anno")]
    public int Anno { get; set; }
    
    [Column("descrizione_bilancio")]
    [MaxLength(500)]
    public string? DescrizioneBilancio { get; set; }
    
    [Column("tipo_bilancio")]
    [Required]
    [MaxLength(10)]
    public string TipoBilancio { get; set; } = "CE";
    
    [Column("codice_mastrino")]
    [Required]
    [MaxLength(50)]
    public string CodiceMastrino { get; set; } = string.Empty;
    
    [Column("descrizione_mastrino")]
    [Required]
    [MaxLength(300)]
    public string DescrizioneMastrino { get; set; } = string.Empty;
    
    [Column("importo")]
    [Precision(18, 2)]
    public decimal Importo { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_import")]
    public DateTime DataImport { get; set; } = DateTime.Now;
    
    [Column("imported_by")]
    public int ImportedBy { get; set; }
    
    [Column("imported_by_name")]
    [MaxLength(100)]
    public string ImportedByName { get; set; } = string.Empty;
    
    [Column("segno")]
    [MaxLength(1)]
    public string Segno { get; set; } = "+";
    
    [Column("formula")]
    [MaxLength(200)]
    public string? Formula { get; set; }
    
    [Column("ordine")]
    public int Ordine { get; set; }
    
    [Column("livello")]
    public int Livello { get; set; }
    
    [Column("gruppo")]
    [MaxLength(100)]
    public string? Gruppo { get; set; }
    
    [Column("gruppo_padre")]
    [MaxLength(100)]
    public string? GruppoPadre { get; set; }
    
    [NotMapped]
    public string ImportoFormatted => Importo.ToString("C2");
    
    [NotMapped]
    public bool HasDifferenza { get; set; }
    
    [NotMapped]
    public bool IsTestoRosso => Segno == "-";
}
