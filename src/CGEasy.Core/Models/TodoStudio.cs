using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Models;

/// <summary>
/// Modello per TODO dello studio professionale
/// </summary>
public class TodoStudio
{
    public int Id { get; set; }
    
    // ===== INFORMAZIONI BASE =====
    public string Titolo { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public string? Note { get; set; }
    
    // ===== CLASSIFICAZIONE =====
    public CategoriaTodo Categoria { get; set; }
    public PrioritaTodo Priorita { get; set; }
    public StatoTodo Stato { get; set; }
    
    // ===== RELAZIONI =====
    /// <summary>
    /// ID del tipo di pratica collegata (730, 740, DRIVA, etc.)
    /// </summary>
    public int? TipoPraticaId { get; set; }
    
    /// <summary>
    /// Nome del tipo pratica (denormalizzato per performance)
    /// </summary>
    public string? TipoPraticaNome { get; set; }
    
    /// <summary>
    /// ID del cliente collegato
    /// </summary>
    public int? ClienteId { get; set; }
    
    /// <summary>
    /// Nome cliente (denormalizzato per performance)
    /// </summary>
    public string? ClienteNome { get; set; }
    
    /// <summary>
    /// ID del professionista che ha creato il TODO
    /// </summary>
    public int CreatoreId { get; set; }
    
    /// <summary>
    /// Nome creatore (denormalizzato)
    /// </summary>
    public string CreatoreNome { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista ID dei professionisti assegnati
    /// </summary>
    public List<int> ProfessionistiAssegnatiIds { get; set; } = new();
    
    /// <summary>
    /// Lista nomi professionisti assegnati (denormalizzato)
    /// </summary>
    public List<string> ProfessionistiAssegnatiNomi { get; set; } = new();
    
    // ===== DATE =====
    public DateTime DataCreazione { get; set; }
    
    /// <summary>
    /// Data di inizio attività (opzionale)
    /// </summary>
    public DateTime? DataInizio { get; set; }
    
    public DateTime? DataScadenza { get; set; }
    public DateTime? DataCompletamento { get; set; }
    public DateTime? DataUltimaModifica { get; set; }
    
    /// <summary>
    /// Orario di inizio attività (opzionale)
    /// </summary>
    public TimeSpan? OrarioInizio { get; set; }
    
    /// <summary>
    /// Orario di fine attività (opzionale)
    /// </summary>
    public TimeSpan? OrarioFine { get; set; }
    
    // ===== ALLEGATI =====
    /// <summary>
    /// Lista percorsi relativi dei file allegati
    /// </summary>
    public List<string> Allegati { get; set; } = new();
    
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

