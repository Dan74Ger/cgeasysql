using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models;

[Table("statistica_sp_salvata")]
public class StatisticaSPSalvata
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("nome_statistica")]
    [Required]
    [MaxLength(200)]
    public string NomeStatistica { get; set; } = string.Empty;
    
    [Column("cliente_id")]
    public int ClienteId { get; set; }
    
    [Column("nome_cliente")]
    [Required]
    [MaxLength(200)]
    public string NomeCliente { get; set; } = string.Empty;
    
    [Column("template_mese")]
    public int TemplateMese { get; set; }
    
    [Column("template_anno")]
    public int TemplateAnno { get; set; }
    
    [Column("template_descrizione")]
    [MaxLength(500)]
    public string TemplateDescrizione { get; set; } = string.Empty;
    
    [Column("periodi_json")]
    public string PeriodiJson { get; set; } = string.Empty;
    
    [Column("dati_statistiche_json")]
    public string DatiStatisticheJson { get; set; } = string.Empty;
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    [Column("utente_id")]
    public int UtenteId { get; set; }
    
    [Column("nome_utente")]
    [MaxLength(100)]
    public string NomeUtente { get; set; } = string.Empty;
    
    [Column("note")]
    public string? Note { get; set; }
}
