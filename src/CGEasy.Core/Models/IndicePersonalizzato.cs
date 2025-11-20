using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

[Table("indici_personalizzati")]
public class IndicePersonalizzato
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("cliente_id")]
    public int ClienteId { get; set; }
    
    [Column("nome_indice")]
    [Required]
    [MaxLength(200)]
    public string NomeIndice { get; set; } = string.Empty;
    
    [Column("descrizione")]
    [MaxLength(500)]
    public string Descrizione { get; set; } = string.Empty;
    
    [Column("operatore")]
    [MaxLength(50)]
    public string Operatore { get; set; } = "divisione";
    
    [Column("moltiplicatore")]
    [Precision(18, 2)]
    public decimal Moltiplicatore { get; set; } = 1;
    
    [Column("unita_misura")]
    [MaxLength(50)]
    public string UnitaMisura { get; set; } = string.Empty;
    
    // JSON storage per List<string>
    [Column("codici_numeratore")]
    public string CodiciNumeratoreJson { get; set; } = "[]";
    
    [Column("codici_denominatore")]
    public string CodiciDenominatoreJson { get; set; } = "[]";
    
    [NotMapped]
    public List<string> CodiciNumeratore 
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(CodiciNumeratoreJson) ?? new();
        set => CodiciNumeratoreJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
    
    [NotMapped]
    public List<string> CodiciDenominatore 
    {
        get => System.Text.Json.JsonSerializer.Deserialize<List<string>>(CodiciDenominatoreJson) ?? new();
        set => CodiciDenominatoreJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
    
    [Column("operazione_numeratore")]
    [MaxLength(50)]
    public string OperazioneNumeratore { get; set; } = "somma";
    
    [Column("operazione_denominatore")]
    [MaxLength(50)]
    public string OperazioneDenominatore { get; set; } = "somma";
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;
    
    [Column("creato_by")]
    public int CreatoBy { get; set; }
    
    [Column("data_modifica")]
    public DateTime? DataModifica { get; set; }
    
    // Alias per compatibilità
    [NotMapped]
    public DateTime? DataUltimaModifica 
    { 
        get => DataModifica; 
        set => DataModifica = value; 
    }
    
    [Column("ordine")]
    public int Ordine { get; set; }
    
    [Column("attivo")]
    public bool Attivo { get; set; } = true;
}
