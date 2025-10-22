using System;

namespace CGEasy.Core.Services;

/// <summary>
/// Servizio per notificare modifiche ai TODO tra le viste aperte
/// Pattern: Event Aggregator per sincronizzazione real-time
/// </summary>
public static class TodoEventService
{
    /// <summary>
    /// Evento sollevato quando un TODO viene modificato
    /// </summary>
    public static event EventHandler? TodoChanged;

    /// <summary>
    /// Notifica che un TODO è stato modificato (creato, aggiornato, eliminato)
    /// Tutte le viste aperte si aggiorneranno automaticamente
    /// </summary>
    public static void NotifyTodoChanged()
    {
        TodoChanged?.Invoke(null, EventArgs.Empty);
    }
}


