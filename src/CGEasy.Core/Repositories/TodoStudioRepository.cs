using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class TodoStudioRepository
{
    private readonly CGEasyDbContext _context;
    
    public TodoStudioRepository(CGEasyDbContext context)
    {
        _context = context;
    }
    
    // ===== CRUD BASE ASYNC =====
    
    public async Task<List<TodoStudio>> GetAllAsync()
    {
        return await _context.TodoStudio
            .OrderByDescending(t => t.DataCreazione)
            .ToListAsync();
    }
    
    public async Task<TodoStudio?> GetByIdAsync(int id)
    {
        return await _context.TodoStudio.FindAsync(id);
    }
    
    public async Task<int> InsertAsync(TodoStudio todo)
    {
        todo.DataCreazione = DateTime.Now;
        todo.DataUltimaModifica = DateTime.Now;
        
        _context.TodoStudio.Add(todo);
        await _context.SaveChangesAsync();
        
        return todo.Id;
    }
    
    public async Task<bool> UpdateAsync(TodoStudio todo)
    {
        try
        {
            todo.DataUltimaModifica = DateTime.Now;
            
            _context.TodoStudio.Update(todo);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var todo = await GetByIdAsync(id);
            if (todo == null) return false;
            
            _context.TodoStudio.Remove(todo);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // ===== QUERY FILTRATE ASYNC =====
    
    /// <summary>
    /// Ottiene TODO per stato
    /// </summary>
    public async Task<List<TodoStudio>> GetByStatoAsync(StatoTodo stato)
    {
        return await _context.TodoStudio
            .Where(t => t.Stato == stato)
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per professionista assegnato (controlla nel JSON)
    /// </summary>
    public async Task<List<TodoStudio>> GetByProfessionistaAsync(int professionistaId)
    {
        return await _context.TodoStudio
            .Where(t => EF.Functions.Like(t.ProfessionistiAssegnatiIdsJson, $"%{professionistaId}%"))
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per creatore
    /// </summary>
    public async Task<List<TodoStudio>> GetByCreatoreAsync(int creatoreId)
    {
        return await _context.TodoStudio
            .Where(t => t.CreatoreId == creatoreId)
            .OrderByDescending(t => t.DataCreazione)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per cliente
    /// </summary>
    public async Task<List<TodoStudio>> GetByClienteAsync(int clienteId)
    {
        return await _context.TodoStudio
            .Where(t => t.ClienteId == clienteId)
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per tipo pratica
    /// </summary>
    public async Task<List<TodoStudio>> GetByTipoPraticaAsync(int tipoPraticaId)
    {
        return await _context.TodoStudio
            .Where(t => t.TipoPraticaId == tipoPraticaId)
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per categoria
    /// </summary>
    public async Task<List<TodoStudio>> GetByCategoriaAsync(CategoriaTodo categoria)
    {
        return await _context.TodoStudio
            .Where(t => t.Categoria == categoria)
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per priorità
    /// </summary>
    public async Task<List<TodoStudio>> GetByPrioritaAsync(PrioritaTodo priorita)
    {
        return await _context.TodoStudio
            .Where(t => t.Priorita == priorita)
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO scaduti
    /// </summary>
    public async Task<List<TodoStudio>> GetScadutiAsync()
    {
        var oggi = DateTime.Now.Date;
        return await _context.TodoStudio
            .Where(t => t.DataScadenza.HasValue && 
                        t.DataScadenza.Value.Date < oggi &&
                        t.Stato != StatoTodo.Completata &&
                        t.Stato != StatoTodo.Annullata)
            .OrderBy(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per range di date scadenza
    /// </summary>
    public async Task<List<TodoStudio>> GetByScadenzaRangeAsync(DateTime dataInizio, DateTime dataFine)
    {
        return await _context.TodoStudio
            .Where(t => t.DataScadenza.HasValue &&
                        t.DataScadenza.Value.Date >= dataInizio.Date &&
                        t.DataScadenza.Value.Date <= dataFine.Date)
            .OrderBy(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO per mese specifico
    /// </summary>
    public async Task<List<TodoStudio>> GetByMeseAsync(int anno, int mese)
    {
        var primoGiorno = new DateTime(anno, mese, 1);
        var ultimoGiorno = primoGiorno.AddMonths(1).AddDays(-1);
        return await GetByScadenzaRangeAsync(primoGiorno, ultimoGiorno);
    }
    
    /// <summary>
    /// Cerca TODO per testo nel titolo o descrizione
    /// </summary>
    public async Task<List<TodoStudio>> SearchAsync(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return await GetAllAsync();
        
        searchText = searchText.ToLower();
        return await _context.TodoStudio
            .Where(t => EF.Functions.Like(t.Titolo.ToLower(), $"%{searchText}%") ||
                        (t.Descrizione != null && EF.Functions.Like(t.Descrizione.ToLower(), $"%{searchText}%")))
            .OrderByDescending(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Ottiene TODO attivi (Da Fare + In Corso)
    /// </summary>
    public async Task<List<TodoStudio>> GetAttiviAsync()
    {
        return await _context.TodoStudio
            .Where(t => t.Stato == StatoTodo.DaFare || t.Stato == StatoTodo.InCorso)
            .OrderByDescending(t => t.Priorita)
            .ThenBy(t => t.DataScadenza)
            .ToListAsync();
    }
    
    /// <summary>
    /// Cambia stato TODO
    /// </summary>
    public async Task<bool> CambiaStatoAsync(int id, StatoTodo nuovoStato)
    {
        var todo = await GetByIdAsync(id);
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
        
        return await UpdateAsync(todo);
    }
    
    /// <summary>
    /// Aggiorna data scadenza (per drag&drop calendario)
    /// </summary>
    public async Task<bool> AggiornaDataScadenzaAsync(int id, DateTime nuovaDataScadenza)
    {
        var todo = await GetByIdAsync(id);
        if (todo == null) return false;
        
        todo.DataScadenza = nuovaDataScadenza;
        todo.DataUltimaModifica = DateTime.Now;
        
        return await UpdateAsync(todo);
    }

    /// <summary>
    /// Aggiorna solo lo stato di un TODO (per Kanban drag & drop)
    /// </summary>
    public async Task<bool> UpdateStatoAsync(int id, StatoTodo nuovoStato)
    {
        var todo = await GetByIdAsync(id);
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
        
        return await UpdateAsync(todo);
    }
    
    /// <summary>
    /// Aggiunge allegato
    /// </summary>
    public async Task<bool> AggiungiAllegatoAsync(int id, string percorsoAllegato)
    {
        var todo = await GetByIdAsync(id);
        if (todo == null) return false;
        
        // Usa la proprietà NotMapped che gestisce il JSON
        var allegati = todo.Allegati ?? new List<string>();
        
        if (!allegati.Contains(percorsoAllegato))
        {
            allegati.Add(percorsoAllegato);
            todo.Allegati = allegati; // Questo aggiorna automaticamente il JSON
            todo.DataUltimaModifica = DateTime.Now;
            return await UpdateAsync(todo);
        }
        
        return false;
    }
    
    /// <summary>
    /// Rimuove allegato
    /// </summary>
    public async Task<bool> RimuoviAllegatoAsync(int id, string percorsoAllegato)
    {
        var todo = await GetByIdAsync(id);
        if (todo == null) return false;
        
        // Usa la proprietà NotMapped che gestisce il JSON
        var allegati = todo.Allegati ?? new List<string>();
        
        if (allegati.Remove(percorsoAllegato))
        {
            todo.Allegati = allegati; // Questo aggiorna automaticamente il JSON
            todo.DataUltimaModifica = DateTime.Now;
            return await UpdateAsync(todo);
        }
        
        return false;
    }
    
    // ===== STATISTICHE ASYNC =====
    
    public async Task<int> CountAsync() => await _context.TodoStudio.CountAsync();
    
    public async Task<int> CountByStatoAsync(StatoTodo stato) => 
        await _context.TodoStudio.CountAsync(t => t.Stato == stato);
    
    public async Task<int> CountByPrioritaAsync(PrioritaTodo priorita) => 
        await _context.TodoStudio.CountAsync(t => t.Priorita == priorita);
    
    public async Task<int> CountScadutiAsync()
    {
        var oggi = DateTime.Now.Date;
        return await _context.TodoStudio.CountAsync(t => t.DataScadenza.HasValue && 
                                                         t.DataScadenza.Value.Date < oggi &&
                                                         t.Stato != StatoTodo.Completata &&
                                                         t.Stato != StatoTodo.Annullata);
    }

    // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

    public List<TodoStudio> GetAll() => GetAllAsync().GetAwaiter().GetResult();
    public TodoStudio? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();
    public int Insert(TodoStudio todo) => InsertAsync(todo).GetAwaiter().GetResult();
    public bool Update(TodoStudio todo) => UpdateAsync(todo).GetAwaiter().GetResult();
    public bool Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
    public bool CambiaStato(int id, StatoTodo nuovoStato) => CambiaStatoAsync(id, nuovoStato).GetAwaiter().GetResult();
    public bool UpdateStato(int id, StatoTodo nuovoStato) => UpdateStatoAsync(id, nuovoStato).GetAwaiter().GetResult();
    public bool AggiornaDataScadenza(int id, DateTime nuovaDataScadenza) => AggiornaDataScadenzaAsync(id, nuovaDataScadenza).GetAwaiter().GetResult();
}
