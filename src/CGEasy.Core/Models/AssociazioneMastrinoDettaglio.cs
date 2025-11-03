using LiteDB;

namespace CGEasy.Core.Models;

/// <summary>
/// Modello per Dettaglio Associazione Mastrini (Righe)
/// Rappresenta la mappatura tra un mastrino contabile e una voce template
/// </summary>
public class AssociazioneMastrinoDettaglio
{
    [BsonId]
    public int Id { get; set; }

    [BsonField("associazione_id")]
    public int AssociazioneId { get; set; }

    [BsonField("codice_mastrino")]
    public string CodiceMastrino { get; set; } = string.Empty;

    [BsonField("descrizione_mastrino")]
    public string DescrizioneMastrino { get; set; } = string.Empty;

    [BsonField("importo")]
    public decimal Importo { get; set; }

    [BsonField("template_voce_id")]
    public int? TemplateVoceId { get; set; }

    [BsonField("template_codice")]
    public string? TemplateCodice { get; set; }

    [BsonField("template_descrizione")]
    public string? TemplateDescrizione { get; set; }

    [BsonField("template_segno")]
    public string? TemplateSegno { get; set; } // + o -

    // Proprietà calcolate per UI
    [BsonIgnore]
    public string ImportoFormatted => Importo.ToString("C2");

    [BsonIgnore]
    public string TemplateDisplay => 
        !string.IsNullOrWhiteSpace(TemplateCodice) 
            ? $"{TemplateCodice} - {TemplateDescrizione} ({TemplateSegno})"
            : "Non associato";

    [BsonIgnore]
    public bool IsAssociato => TemplateVoceId.HasValue;
}











