using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CGEasy.Core.Models;

/// <summary>
/// Modello per TODO dello studio professionale
/// </summary>
[Table("todo_studio")]
public class TodoStudio
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    // ===== INFORMAZIONI BASE =====
    [Column("titolo")]
    [Required]
    [MaxLength(300)]
    public string Titolo { get; set; } = string.Empty;
    
    [Column("descrizione")]
    public string? Descrizione { get; set; }
    
    [Column("note")]
    public string? Note { get; set; }
    
    // ===== CLASSIFICAZIONE =====
    [Column("categoria")]
    public CategoriaTodo Categoria { get; set; }
    
    [Column("priorita")]
    public PrioritaTodo Priorita { get; set; }
    
    [Column("stato")]
    public StatoTodo Stato { get; set; }
    
    // ===== RELAZIONI =====
    /// <summary>
    /// ID del tipo di pratica collegata (730, 740, DRIVA, etc.)
    /// </summary>
    [Column("tipo_pratica_id")]
    public int? TipoPraticaId { get; set; }
    
    /// <summary>
    /// Nome del tipo pratica (denormalizzato per performance)
    /// </summary>
    [Column("tipo_pratica_nome")]
    [MaxLength(200)]
    public string? TipoPraticaNome { get; set; }
    
    /// <summary>
    /// ID del cliente collegato
    /// </summary>
    [Column("cliente_id")]
    public int? ClienteId { get; set; }
    
    /// <summary>
    /// Nome cliente (denormalizzato per performance)
    /// </summary>
    [Column("cliente_nome")]
    [MaxLength(200)]
    public string? ClienteNome { get; set; }
    
    /// <summary>
    /// ID del professionista che ha creato il TODO
    /// </summary>
    [Column("creatore_id")]
    public int CreatoreId { get; set; }
    
    /// <summary>
    /// Nome creatore (denormalizzato)
    /// </summary>
    [Column("creatore_nome")]
    [Required]
    [MaxLength(200)]
    public string CreatoreNome { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista ID dei professionisti assegnati (JSON)
    /// </summary>
    [Column("professionisti_assegnati_ids_json")]
    public string ProfessionistiAssegnatiIdsJson { get; set; } = "[]";
    
    /// <summary>
    /// Lista nomi professionisti assegnati (JSON)
    /// </summary>
    [Column("professionisti_assegnati_nomi_json")]
    public string ProfessionistiAssegnatiNomiJson { get; set; } = "[]";
    
    // Per retrocompatibilità con il codice esistente, mappiamo List<> su JSON
    [NotMapped]
    public List<int> ProfessionistiAssegnatiIds
    {
        get => string.IsNullOrWhiteSpace(ProfessionistiAssegnatiIdsJson) || ProfessionistiAssegnatiIdsJson == "[]"
            ? new List<int>()
            : System.Text.Json.JsonSerializer.Deserialize<List<int>>(ProfessionistiAssegnatiIdsJson) ?? new List<int>();
        set => ProfessionistiAssegnatiIdsJson = System.Text.Json.JsonSerializer.Serialize(value ?? new List<int>());
    }
    
    [NotMapped]
    public List<string> ProfessionistiAssegnatiNomi
    {
        get => string.IsNullOrWhiteSpace(ProfessionistiAssegnatiNomiJson) || ProfessionistiAssegnatiNomiJson == "[]"
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(ProfessionistiAssegnatiNomiJson) ?? new List<string>();
        set => ProfessionistiAssegnatiNomiJson = System.Text.Json.JsonSerializer.Serialize(value ?? new List<string>());
    }
    
    // ===== DATE =====
    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; }
    
    /// <summary>
    /// Data di inizio attività (opzionale)
    /// </summary>
    [Column("data_inizio")]
    public DateTime? DataInizio { get; set; }
    
    [Column("data_scadenza")]
    public DateTime? DataScadenza { get; set; }
    
    [Column("data_completamento")]
    public DateTime? DataCompletamento { get; set; }
    
    [Column("data_ultima_modifica")]
    public DateTime? DataUltimaModifica { get; set; }
    
    /// <summary>
    /// Orario di inizio attività (opzionale)
    /// </summary>
    [Column("orario_inizio")]
    public TimeSpan? OrarioInizio { get; set; }
    
    /// <summary>
    /// Orario di fine attività (opzionale)
    /// </summary>
    [Column("orario_fine")]
    public TimeSpan? OrarioFine { get; set; }
    
    // ===== ALLEGATI =====
    /// <summary>
    /// Lista percorsi relativi dei file allegati (JSON)
    /// </summary>
    [Column("allegati_json")]
    public string AllegatiJson { get; set; } = "[]";
    
    [NotMapped]
    public List<string> Allegati
    {
        get => string.IsNullOrWhiteSpace(AllegatiJson) || AllegatiJson == "[]"
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(AllegatiJson) ?? new List<string>();
        set => AllegatiJson = System.Text.Json.JsonSerializer.Serialize(value ?? new List<string>());
    }
    
    // ===== PROPRIETÀ CALCOLATE =====
    
    /// <summary>
    /// Indica se il TODO è scaduto
    /// </summary>
    public bool IsScaduto => DataScadenza.HasValue && 
                             DataScadenza.Value.Date < DateTime.Now.Date && 
                             Stato != StatoTodo.Completata &&
                             Stato != StatoTodo.Annullata;
    
    /// <summary>
    /// Giorni rimanenti alla scadenza (negativo se scaduto)
    /// </summary>
    public int GiorniAllaScadenza => DataScadenza.HasValue ? 
                                     (DataScadenza.Value.Date - DateTime.Now.Date).Days : 0;
    
    /// <summary>
    /// Indica se il TODO scade oggi
    /// </summary>
    public bool ScadeOggi => DataScadenza.HasValue && 
                             DataScadenza.Value.Date == DateTime.Now.Date;
    
    /// <summary>
    /// Indica se il TODO scade entro 3 giorni
    /// </summary>
    public bool ScadePresto => DataScadenza.HasValue && 
                               GiorniAllaScadenza >= 0 && 
                               GiorniAllaScadenza <= 3 &&
                               Stato != StatoTodo.Completata &&
                               Stato != StatoTodo.Annullata;
    
    /// <summary>
    /// Colore badge priorità per UI
    /// </summary>
    public string ColorePriorita => Priorita switch
    {
        PrioritaTodo.Urgente => "#E91E63",  // Fucsia
        PrioritaTodo.Alta => "#FF9800",     // Arancione
        PrioritaTodo.Media => "#FFC107",    // Giallo
        PrioritaTodo.Bassa => "#4CAF50",    // Verde
        _ => "#9E9E9E"                      // Grigio
    };
    
    /// <summary>
    /// Icona priorità per UI
    /// </summary>
    public string IconaPriorita => Priorita switch
    {
        PrioritaTodo.Urgente => "🔴",
        PrioritaTodo.Alta => "🟠",
        PrioritaTodo.Media => "🟡",
        PrioritaTodo.Bassa => "🟢",
        _ => "⚪"
    };
    
    /// <summary>
    /// Icona stato per UI
    /// </summary>
    public string IconaStato => Stato switch
    {
        StatoTodo.DaFare => "⏳",
        StatoTodo.InCorso => "⚙️",
        StatoTodo.Completata => "✅",
        StatoTodo.Annullata => "❌",
        _ => "❓"
    };
    
    /// <summary>
    /// Colore stato per UI
    /// </summary>
    public string ColoreStato => Stato switch
    {
        StatoTodo.DaFare => "#9E9E9E",       // Grigio
        StatoTodo.InCorso => "#2196F3",      // Blu
        StatoTodo.Completata => "#4CAF50",   // Verde
        StatoTodo.Annullata => "#F44336",    // Rosso
        _ => "#9E9E9E"                       // Grigio
    };
    
    /// <summary>
    /// Testo stato per UI
    /// </summary>
    public string TestoStato => Stato switch
    {
        StatoTodo.DaFare => "Da Fare",
        StatoTodo.InCorso => "In Corso",
        StatoTodo.Completata => "Completata",
        StatoTodo.Annullata => "Annullata",
        _ => "Sconosciuto"
    };
    
    /// <summary>
    /// Testo categoria per UI
    /// </summary>
    public string TestoCategoria => Categoria switch
    {
        CategoriaTodo.Amministrativa => "📁 Amministrativa",
        CategoriaTodo.Fiscale => "💼 Fiscale",
        CategoriaTodo.Contabile => "📊 Contabile",
        CategoriaTodo.Legale => "⚖️ Legale",
        CategoriaTodo.Consulenza => "🎯 Consulenza",
        CategoriaTodo.Altro => "📌 Altro",
        _ => "❓ Sconosciuta"
    };
    
    /// <summary>
    /// Nomi professionisti separati da virgola
    /// </summary>
    public string ProfessionistiAssegnatiText => ProfessionistiAssegnatiNomi.Any() 
        ? string.Join(", ", ProfessionistiAssegnatiNomi) 
        : "Nessuno";
    
    /// <summary>
    /// Conta allegati
    /// </summary>
    public int NumeroAllegati => Allegati?.Count ?? 0;
    
    /// <summary>
    /// Ha allegati
    /// </summary>
    public bool HasAllegati => NumeroAllegati > 0;
    
    /// <summary>
    /// Testo orario formattato per il calendario (solo se presente)
    /// </summary>
    public string TestoOrario
    {
        get
        {
            if (OrarioInizio.HasValue && OrarioFine.HasValue)
                return $"🕐 {OrarioInizio.Value:hh\\:mm} - {OrarioFine.Value:hh\\:mm}";
            else if (OrarioInizio.HasValue)
                return $"🕐 {OrarioInizio.Value:hh\\:mm}";
            else if (OrarioFine.HasValue)
                return $"🕐 Fino a {OrarioFine.Value:hh\\:mm}";
            return string.Empty;
        }
    }
}

/// <summary>
/// Categoria del TODO
/// </summary>
public enum CategoriaTodo
{
    Amministrativa = 0,
    Fiscale = 1,
    Contabile = 2,
    Legale = 3,
    Consulenza = 4,
    Altro = 5
}

/// <summary>
/// Priorità del TODO
/// </summary>
public enum PrioritaTodo
{
    Bassa = 0,
    Media = 1,
    Alta = 2,
    Urgente = 3
}

/// <summary>
/// Stato/Fase del TODO
/// </summary>
public enum StatoTodo
{
    DaFare = 0,      // Appena creato, non iniziato
    InCorso = 1,     // In lavorazione
    Completata = 2,  // Terminato con successo
    Annullata = 3    // Non più necessario
}

