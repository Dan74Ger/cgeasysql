using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models;

[Table("associazione_mastrini")]
public class AssociazioneMastrino
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
    
    [Column("bilancio_descrizione")]
    [MaxLength(500)]
    public string? BilancioDescrizione { get; set; }

    [Column("template_id")]
    public int? TemplateId { get; set; }

    [Column("template_nome")]
    [MaxLength(200)]
    public string? TemplatNome { get; set; }

    [Column("descrizione")]
    public string? Descrizione { get; set; }

    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;

    [Column("creato_by")]
    public int CreatoBy { get; set; }

    [Column("creato_by_name")]
    [MaxLength(100)]
    public string CreatoByName { get; set; } = string.Empty;

    [Column("data_modifica")]
    public DateTime? DataModifica { get; set; }

    [Column("numero_mappature")]
    public int NumeroMappature { get; set; }

    [NotMapped]
    public string PeriodoDisplay => $"{MeseNome} {Anno}";

    [NotMapped]
    public string MeseNome => Mese switch
    {
        1 => "Gen", 2 => "Feb", 3 => "Mar", 4 => "Apr",
        5 => "Mag", 6 => "Giu", 7 => "Lug", 8 => "Ago",
        9 => "Set", 10 => "Ott", 11 => "Nov", 12 => "Dic",
        _ => ""
    };

    [NotMapped]
    public string DescrizioneCompleta => $"{ClienteNome} - {MeseNome} {Anno}";
}
