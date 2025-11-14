using LiteDB;
using System;

namespace CGEasy.Core.Models;

/// <summary>
/// Modello per Associazione Mastrini (Testata)
/// Rappresenta un'associazione tra bilancio contabile e template
/// </summary>
public class AssociazioneMastrino
{
    [BsonId]
    public int Id { get; set; }

    [BsonField("cliente_id")]
    public int ClienteId { get; set; }

    [BsonField("cliente_nome")]
    public string ClienteNome { get; set; } = string.Empty;

    [BsonField("mese")]
    public int Mese { get; set; } // 1-12

    [BsonField("anno")]
    public int Anno { get; set; }
    
    [BsonField("bilancio_descrizione")]
    public string? BilancioDescrizione { get; set; }

    [BsonField("template_id")]
    public int? TemplateId { get; set; }

    [BsonField("template_nome")]
    public string? TemplatNome { get; set; }

    [BsonField("descrizione")]
    public string? Descrizione { get; set; }

    [BsonField("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;

    [BsonField("creato_by")]
    public int CreatoBy { get; set; }

    [BsonField("creato_by_name")]
    public string CreatoByName { get; set; } = string.Empty;

    [BsonField("data_modifica")]
    public DateTime? DataModifica { get; set; }

    [BsonField("numero_mappature")]
    public int NumeroMappature { get; set; }

    // Proprietà calcolate per UI
    [BsonIgnore]
    public string PeriodoDisplay => $"{MeseNome} {Anno}";

    [BsonIgnore]
    public string MeseNome => Mese switch
    {
        1 => "Gen",
        2 => "Feb",
        3 => "Mar",
        4 => "Apr",
        5 => "Mag",
        6 => "Giu",
        7 => "Lug",
        8 => "Ago",
        9 => "Set",
        10 => "Ott",
        11 => "Nov",
        12 => "Dic",
        _ => ""
    };

    [BsonIgnore]
    public string DescrizioneCompleta => 
        $"{ClienteNome} - {PeriodoDisplay}" + 
        (!string.IsNullOrWhiteSpace(Descrizione) ? $" - {Descrizione}" : "");
}





















