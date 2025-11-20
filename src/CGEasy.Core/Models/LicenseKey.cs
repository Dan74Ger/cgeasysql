using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models;

/// <summary>
/// Chiave di licenza per un cliente e modulo (EF Core)
/// </summary>
[Table("license_keys")]
public class LicenseKey
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("license_client_id")]
    public int LicenseClientId { get; set; }
    
    [Column("module_name")]
    [Required]
    [MaxLength(100)]
    public string ModuleName { get; set; } = string.Empty;
    
    [Column("full_key")]
    [Required]
    [MaxLength(200)]
    public string FullKey { get; set; } = string.Empty;
    
    [Column("license_guid")]
    [Required]
    [MaxLength(50)]
    public string LicenseGuid { get; set; } = Guid.NewGuid().ToString();
    
    [Column("data_generazione")]
    public DateTime DataGenerazione { get; set; } = DateTime.Now;
    
    [Column("is_active")]
    public bool IsActive { get; set; } = true;
    
    [Column("note")]
    public string? Note { get; set; }
    
    [Column("data_scadenza")]
    public DateTime? DataScadenza { get; set; }
    
    [Column("generated_by_user_id")]
    public int GeneratedByUserId { get; set; }
    
    [NotMapped]
    public bool IsExpired => DataScadenza.HasValue && DataScadenza.Value.Date <= DateTime.Now.Date;
    
    [NotMapped]
    public string DurataDisplay
    {
        get
        {
            if (!DataScadenza.HasValue)
                return "♾️ Perpetua";
            
            var durata = DataScadenza.Value - DataGenerazione;
            var anni = (int)(durata.TotalDays / 365);
            var mesi = (int)((durata.TotalDays % 365) / 30);
            
            if (anni > 0 && mesi == 0)
                return $"{anni} {(anni == 1 ? "anno" : "anni")}";
            else if (anni > 0)
                return $"{anni} {(anni == 1 ? "anno" : "anni")}, {mesi} {(mesi == 1 ? "mese" : "mesi")}";
            else if (mesi > 0)
                return $"{mesi} {(mesi == 1 ? "mese" : "mesi")}";
            else
                return $"{(int)durata.TotalDays} giorni";
        }
    }
}
