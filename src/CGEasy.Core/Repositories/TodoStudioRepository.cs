using CGEasy.Core.Data;
using CGEasy.Core.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Repositories;

public class TodoStudioRepository
{
    private readonly LiteDbContext _context;
    
    public TodoStudioRepository(LiteDbContext context)
    {
        _context = context;
    }
    
    private ILiteCollection<TodoStudio> Collection => _context.TodoStudio;
    
    // ===== CRUD BASE =====
    
    public IEnumerable<TodoStudio> GetAll()
    {
        return Collection.FindAll().OrderByDescending(t => t.DataCreazione);
    }
    
    public TodoStudio? GetById(int id)
    {
        return Collection.FindById(id);
    }
    
    public int Insert(TodoStudio todo)
    {
        todo.DataCreazione = DateTime.Now;
        todo.DataUltimaModifica = DateTime.Now;
        return Collection.Insert(todo);
    }
    
    public bool Update(TodoStudio todo)
    {
        todo.DataUltimaModifica = DateTime.Now;
        return Collection.Update(todo);
    }
    
    public bool Delete(int id)
    {
        return Collection.Delete(id);
    }
    
    // ===== QUERY FILTRATE =====
    
    /// <summary>
    /// Ottiene TODO per stato
    /// </summary>
    public IEnumerable<TodoStudio> GetByStato(StatoTodo stato)
    {
        return Collection.Find(t => t.Stato == stato).OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per professionista assegnato
    /// </summary>
    public IEnumerable<TodoStudio> GetByProfessionista(int professionistaId)
    {
        return Collection.Find(t => t.ProfessionistiAssegnatiIds.Contains(professionistaId))
                         .OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per creatore
    /// </summary>
    public IEnumerable<TodoStudio> GetByCreatore(int creatoreId)
    {
        return Collection.Find(t => t.CreatoreId == creatoreId)
                         .OrderByDescending(t => t.DataCreazione);
    }
    
    /// <summary>
    /// Ottiene TODO per cliente
    /// </summary>
    public IEnumerable<TodoStudio> GetByCliente(int clienteId)
    {
        return Collection.Find(t => t.ClienteId == clienteId)
                         .OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per tipo pratica
    /// </summary>
    public IEnumerable<TodoStudio> GetByTipoPratica(int tipoPraticaId)
    {
        return Collection.Find(t => t.TipoPraticaId == tipoPraticaId)
                         .OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per categoria
    /// </summary>
    public IEnumerable<TodoStudio> GetByCategoria(CategoriaTodo categoria)
    {
        return Collection.Find(t => t.Categoria == categoria)
                         .OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per priorità
    /// </summary>
    public IEnumerable<TodoStudio> GetByPriorita(PrioritaTodo priorita)
    {
        return Collection.Find(t => t.Priorita == priorita)
                         .OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO scaduti
    /// </summary>
    public IEnumerable<TodoStudio> GetScaduti()
    {
        var oggi = DateTime.Now.Date;
        return Collection.Find(t => t.DataScadenza.HasValue && 
                                    t.DataScadenza.Value.Date < oggi &&
                                    t.Stato != StatoTodo.Completata &&
                                    t.Stato != StatoTodo.Annullata)
                         .OrderBy(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per range di date scadenza
    /// </summary>
    public IEnumerable<TodoStudio> GetByScadenzaRange(DateTime dataInizio, DateTime dataFine)
    {
        return Collection.Find(t => t.DataScadenza.HasValue &&
                                    t.DataScadenza.Value.Date >= dataInizio.Date &&
                                    t.DataScadenza.Value.Date <= dataFine.Date)
                         .OrderBy(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO per mese specifico
    /// </summary>
    public IEnumerable<TodoStudio> GetByMese(int anno, int mese)
    {
        var primoGiorno = new DateTime(anno, mese, 1);
        var ultimoGiorno = primoGiorno.AddMonths(1).AddDays(-1);
        return GetByScadenzaRange(primoGiorno, ultimoGiorno);
    }
    
    /// <summary>
    /// Cerca TODO per testo nel titolo o descrizione
    /// </summary>
    public IEnumerable<TodoStudio> Search(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return GetAll();
        
        searchText = searchText.ToLower();
        return Collection.Find(t => t.Titolo.ToLower().Contains(searchText) ||
                                    (t.Descrizione != null && t.Descrizione.ToLower().Contains(searchText)))
                         .OrderByDescending(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Ottiene TODO attivi (Da Fare + In Corso)
    /// </summary>
    public IEnumerable<TodoStudio> GetAttivi()
    {
        return Collection.Find(t => t.Stato == StatoTodo.DaFare || t.Stato == StatoTodo.InCorso)
                         .OrderByDescending(t => t.Priorita)
                         .ThenBy(t => t.DataScadenza);
    }
    
    /// <summary>
    /// Cambia stato TODO
    /// </summary>
    public bool CambiaStato(int id, StatoTodo nuovoStato)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        todo.Stato = nuovoStato;
        todo.DataUltimaModifica = DateTime.Now;
        
        // Se completato, registra data completamento
        if (nuovoStato == StatoTodo.Completata)
        {
            todo.DataCompletamento = DateTime.Now;
        }
        else if (todo.DataCompletamento.HasValue)
        {
            // Se riaperto, rimuovi data completamento
            todo.DataCompletamento = null;
        }
        
        return Update(todo);
    }
    
    /// <summary>
    /// Aggiorna data scadenza (per drag&drop calendario)
    /// </summary>
    public bool AggiornaDataScadenza(int id, DateTime nuovaDataScadenza)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        todo.DataScadenza = nuovaDataScadenza;
        todo.DataUltimaModifica = DateTime.Now;
        
        return Update(todo);
    }

    /// <summary>
    /// Aggiorna solo lo stato di un TODO (per Kanban drag & drop)
    /// </summary>
    public bool UpdateStato(int id, StatoTodo nuovoStato)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        todo.Stato = nuovoStato;
        todo.DataUltimaModifica = DateTime.Now;
        
        // Se completata, imposta data completamento
        if (nuovoStato == StatoTodo.Completata && !todo.DataCompletamento.HasValue)
        {
            todo.DataCompletamento = DateTime.Now;
        }
        // Se si rimuove da completata, cancella data
        else if (nuovoStato != StatoTodo.Completata && todo.DataCompletamento.HasValue)
        {
            todo.DataCompletamento = null;
        }
        
        return Update(todo);
    }
    
    /// <summary>
    /// Aggiunge allegato
    /// </summary>
    public bool AggiungiAllegato(int id, string percorsoAllegato)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        if (todo.Allegati == null)
            todo.Allegati = new List<string>();
        
        if (!todo.Allegati.Contains(percorsoAllegato))
        {
            todo.Allegati.Add(percorsoAllegato);
            todo.DataUltimaModifica = DateTime.Now;
            return Update(todo);
        }
        
        return false;
    }
    
    /// <summary>
    /// Rimuove allegato
    /// </summary>
    public bool RimuoviAllegato(int id, string percorsoAllegato)
    {
        var todo = GetById(id);
        if (todo == null) return false;
        
        if (todo.Allegati != null && todo.Allegati.Remove(percorsoAllegato))
        {
            todo.DataUltimaModifica = DateTime.Now;
            return Update(todo);
        }
        
        return false;
    }
    
    // ===== STATISTICHE =====
    
    public int Count() => Collection.Count();
    
    public int CountByStato(StatoTodo stato) => Collection.Count(t => t.Stato == stato);
    
    public int CountByPriorita(PrioritaTodo priorita) => Collection.Count(t => t.Priorita == priorita);
    
    public int CountScaduti()
    {
        var oggi = DateTime.Now.Date;
        return Collection.Count(t => t.DataScadenza.HasValue && 
                                     t.DataScadenza.Value.Date < oggi &&
                                     t.Stato != StatoTodo.Completata &&
                                     t.Stato != StatoTodo.Annullata);
    }
}

