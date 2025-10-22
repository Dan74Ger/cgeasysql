using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta un giorno del calendario con i TODO associati
/// </summary>
public class CalendarDay
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool IsWeekend { get; set; }
    
    public List<TodoStudio> Todos { get; set; } = new();
    
    // ===== CONTATORI GENERALI =====
    public int TotalTodos => Todos.Count;
    public bool HasTodos => TotalTodos > 0;
    
    // ===== CONTATORI PER FASE/STATO =====
    public int DaFareCount => Todos.Count(t => t.Stato == StatoTodo.DaFare);
    public int InCorsoCount => Todos.Count(t => t.Stato == StatoTodo.InCorso);
    public int CompletatiCount => Todos.Count(t => t.Stato == StatoTodo.Completata);
    public int AnnullatiCount => Todos.Count(t => t.Stato == StatoTodo.Annullata);
    
    // ===== CONTATORI PER PRIORITÀ =====
    public int UrgentiCount => Todos.Count(t => t.Priorita == PrioritaTodo.Urgente);
    public int AltaCount => Todos.Count(t => t.Priorita == PrioritaTodo.Alta);
    public int MediaCount => Todos.Count(t => t.Priorita == PrioritaTodo.Media);
    public int BassaCount => Todos.Count(t => t.Priorita == PrioritaTodo.Bassa);
    
    // ===== INDICATORI BOOLEANI =====
    public bool HasUrgenti => UrgentiCount > 0;
    public bool HasDaFare => DaFareCount > 0;
    public bool HasInCorso => InCorsoCount > 0;
    public bool HasCompletati => CompletatiCount > 0;
    public bool HasAnnullati => AnnullatiCount > 0;
    
    // ===== CONTATORI SCADENZE =====
    public int ScadutiCount => Todos.Count(t => t.IsScaduto);
    public bool HasScaduti => ScadutiCount > 0;
    
    // ===== COLORE GIORNO PER UI =====
    public string BackgroundColor
    {
        get
        {
            if (!IsCurrentMonth) return "#F5F5F5";      // Grigio chiaro per mesi diversi
            if (IsToday) return "#E3F2FD";              // Azzurro per oggi
            if (HasScaduti) return "#FFEBEE";           // Rosso chiaro per scaduti
            if (HasUrgenti) return "#FFF3E0";           // Arancione chiaro per urgenti
            if (HasInCorso) return "#FFF9C4";           // Giallo chiaro per in corso
            if (IsWeekend) return "#FAFAFA";            // Grigio molto chiaro weekend
            return "White";
        }
    }
    
    /// <summary>
    /// Nome giorno della settimana
    /// </summary>
    public string DayName => Date.ToString("ddd", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
    
    /// <summary>
    /// Numero giorno
    /// </summary>
    public int DayNumber => Date.Day;
    
    /// <summary>
    /// Testo riepilogativo per tooltip
    /// </summary>
    public string TooltipText
    {
        get
        {
            if (!HasTodos) return $"{Date:dd MMMM yyyy}\nNessun TODO";
            
            var lines = new List<string>
            {
                $"{Date:dd MMMM yyyy}",
                $"Totale TODO: {TotalTodos}",
                ""
            };
            
            if (DaFareCount > 0) lines.Add($"⏳ Da Fare: {DaFareCount}");
            if (InCorsoCount > 0) lines.Add($"⚙️ In Corso: {InCorsoCount}");
            if (CompletatiCount > 0) lines.Add($"✅ Completati: {CompletatiCount}");
            if (AnnullatiCount > 0) lines.Add($"❌ Annullati: {AnnullatiCount}");
            
            lines.Add("");
            
            if (UrgentiCount > 0) lines.Add($"🔴 Urgenti: {UrgentiCount}");
            if (AltaCount > 0) lines.Add($"🟠 Alta: {AltaCount}");
            if (MediaCount > 0) lines.Add($"🟡 Media: {MediaCount}");
            if (BassaCount > 0) lines.Add($"🟢 Bassa: {BassaCount}");
            
            if (HasScaduti) lines.Add($"\n⚠️ SCADUTI: {ScadutiCount}");
            
            return string.Join("\n", lines);
        }
    }
}


