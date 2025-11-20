using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models;

/// <summary>
/// Cliente a cui sono state rilasciate licenze (EF Core)
/// </summary>
[Table("license_clients")]
public class LicenseClient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("nome_cliente")]
    [Required]
    [MaxLength(200)]
    public string NomeCliente { get; set; } = string.Empty;
    
    [Column("email")]
    [MaxLength(150)]
    public string? Email { get; set; }
    
    [Column("telefono")]
    [MaxLength(50)]
    public string? Telefono { get; set; }
    
    [Column("partita_iva")]
    [MaxLength(20)]
    public string? PartitaIva { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_registrazione")]
    public DateTime DataRegistrazione { get; set; } = DateTime.Now;
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
