using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

[Table("associazione_mastrini_dettagli")]
public class AssociazioneMastrinoDettaglio
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("associazione_id")]
    public int AssociazioneId { get; set; }

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

    [Column("template_voce_id")]
    public int? TemplateVoceId { get; set; }

    [Column("template_codice")]
    [MaxLength(50)]
    public string? TemplateCodice { get; set; }

    [Column("template_descrizione")]
    [MaxLength(300)]
    public string? TemplateDescrizione { get; set; }

    [Column("template_segno")]
    [MaxLength(1)]
    public string? TemplateSegno { get; set; }

    [NotMapped]
    public string ImportoFormatted => Importo.ToString("C2");

    [NotMapped]
    public string TemplateDisplay => 
        !string.IsNullOrWhiteSpace(TemplateCodice) 
            ? $"{TemplateCodice} - {TemplateDescrizione} ({TemplateSegno})"
            : "Non associato";

    [NotMapped]
    public bool IsAssociato => TemplateVoceId.HasValue;
}
