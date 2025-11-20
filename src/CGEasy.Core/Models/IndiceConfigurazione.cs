using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta la configurazione di un indice (standard o personalizzato)
/// salvata per un cliente specifico
/// </summary>
[Table("indice_configurazione")]
public class IndiceConfigurazione
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Riferimento al cliente (0 = configurazione globale)
    /// </summary>
    [Column("cliente_id")]
    public int ClienteId { get; set; }
    
    /// <summary>
    /// Nome dell'indice (es: "Liquidità Corrente")
    /// </summary>
    [Column("nome_indice")]
    [Required]
    [MaxLength(200)]
    public string NomeIndice { get; set; } = string.Empty;
    
    /// <summary>
    /// Categoria (Liquidità, Solidità, Redditività, Efficienza, Personalizzato)
    /// </summary>
    [Column("categoria")]
    [Required]
    [MaxLength(100)]
    public string Categoria { get; set; } = string.Empty;
    
    /// <summary>
    /// Formula leggibile (es: "Attivo Corrente / Passivo Corrente")
    /// </summary>
    [Column("formula")]
    [Required]
    [MaxLength(500)]
    public string Formula { get; set; } = string.Empty;
    
    /// <summary>
    /// Unità di misura (€, %, giorni, volte, ecc.)
    /// </summary>
    [Column("unita_misura")]
    [Required]
    [MaxLength(50)]
    public string UnitaMisura { get; set; } = string.Empty;
    
    /// <summary>
    /// Se true, l'indice è abilitato per il calcolo
    /// </summary>
    [Column("is_abilitato")]
    public bool IsAbilitato { get; set; } = true;
    
    /// <summary>
    /// Se true, è un indice standard (non modificabile)
    /// </summary>
    [Column("is_standard")]
    public bool IsStandard { get; set; }
    
    /// <summary>
    /// Codice identificativo univoco (es: "LIQ_CORRENTE")
    /// </summary>
    [Column("codice_indice")]
    [Required]
    [MaxLength(100)]
    public string CodiceIndice { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON con i dettagli di calcolo (codici voci, operatori, ecc.)
    /// </summary>
    [Column("dettagli_calcolo_json")]
    public string DettagliCalcoloJson { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrizione estesa dell'indice
    /// </summary>
    [Column("descrizione")]
    [MaxLength(1000)]
    public string? Descrizione { get; set; }
    
    /// <summary>
    /// Utente che ha creato/modificato
    /// </summary>
    [Column("utente_id")]
    public int UtenteId { get; set; }
    
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; }
    
    [Column("ultima_modifica")]
    public DateTime? UltimaModifica { get; set; }
    
    /// <summary>
    /// Ordinamento per visualizzazione
    /// </summary>
    [Column("ordinamento")]
    public int Ordinamento { get; set; }
}




