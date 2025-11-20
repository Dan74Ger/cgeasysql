using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

/// <summary>
/// Repository per gestione Associazioni Mastrini
/// </summary>
public class AssociazioneMastrinoRepository : IRepository<AssociazioneMastrino>
{
    private readonly CGEasyDbContext _context;

    public AssociazioneMastrinoRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssociazioneMastrino>> GetAllAsync()
    {
        return await _context.AssociazioniMastrini.ToListAsync();
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public IEnumerable<AssociazioneMastrino> GetAll()
    {
        return _context.AssociazioniMastrini.ToList();
    }

    public async Task<AssociazioneMastrino?> GetByIdAsync(int id)
    {
        return await _context.AssociazioniMastrini.FindAsync(id);
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public AssociazioneMastrino? GetById(int id)
    {
        return _context.AssociazioniMastrini.Find(id);
    }

    public async Task<IEnumerable<AssociazioneMastrino>> FindAsync(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return await _context.AssociazioniMastrini.Where(predicate).ToListAsync();
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public IEnumerable<AssociazioneMastrino> Find(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.Where(predicate).ToList();
    }

    public async Task<AssociazioneMastrino?> FindOneAsync(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return await _context.AssociazioniMastrini.FirstOrDefaultAsync(predicate);
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public AssociazioneMastrino? FindOne(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.FirstOrDefault(predicate);
    }

    public async Task<int> InsertAsync(AssociazioneMastrino entity)
    {
        await _context.AssociazioniMastrini.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity.Id;
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public int Insert(AssociazioneMastrino entity)
    {
        _context.AssociazioniMastrini.Add(entity);
        _context.SaveChanges();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(AssociazioneMastrino entity)
    {
        try
        {
            entity.DataModifica = DateTime.Now;
            _context.AssociazioniMastrini.Update(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public bool Update(AssociazioneMastrino entity)
    {
        try
        {
            entity.DataModifica = DateTime.Now;
            _context.AssociazioniMastrini.Update(entity);
            _context.SaveChanges();
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
            var entity = await _context.AssociazioniMastrini.FindAsync(id);
            if (entity == null) return false;
            
            _context.AssociazioniMastrini.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public bool Delete(int id)
    {
        try
        {
            var entity = _context.AssociazioniMastrini.Find(id);
            if (entity == null) return false;
            
            _context.AssociazioniMastrini.Remove(entity);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> CountAsync()
    {
        return await _context.AssociazioniMastrini.CountAsync();
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public int Count()
    {
        return _context.AssociazioniMastrini.Count();
    }

    public async Task<int> CountAsync(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return await _context.AssociazioniMastrini.CountAsync(predicate);
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public int Count(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.Count(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return await _context.AssociazioniMastrini.AnyAsync(predicate);
    }
    
    // Metodo sincrono per compatibilità con IRepository<T>
    public bool Exists(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.Any(predicate);
    }

    /// <summary>
    /// Ottiene associazioni per cliente
    /// </summary>
    public async Task<IEnumerable<AssociazioneMastrino>> GetByClienteAsync(int clienteId)
    {
        return await _context.AssociazioniMastrini.Where(x => x.ClienteId == clienteId).ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public IEnumerable<AssociazioneMastrino> GetByCliente(int clienteId)
    {
        return _context.AssociazioniMastrini.Where(x => x.ClienteId == clienteId).ToList();
    }

    /// <summary>
    /// Ottiene associazione per cliente e periodo
    /// </summary>
    public async Task<AssociazioneMastrino?> GetByClienteAndPeriodoAsync(int clienteId, int mese, int anno)
    {
        return await _context.AssociazioniMastrini.FirstOrDefaultAsync(x => 
            x.ClienteId == clienteId && 
            x.Mese == mese && 
            x.Anno == anno);
    }
    
    // Metodo sincrono per compatibilità
    public AssociazioneMastrino? GetByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        return _context.AssociazioniMastrini.FirstOrDefault(x => 
            x.ClienteId == clienteId && 
            x.Mese == mese && 
            x.Anno == anno);
    }

    /// <summary>
    /// Ottiene associazione per cliente e template (indipendente dal periodo!)
    /// Un cliente può avere UNA SOLA associazione per template
    /// </summary>
    public async Task<AssociazioneMastrino?> GetByClienteAndTemplateAsync(int clienteId, int templateId)
    {
        return await _context.AssociazioniMastrini.FirstOrDefaultAsync(x => 
            x.ClienteId == clienteId && 
            x.TemplateId == templateId);
    }
    
    // Metodo sincrono per compatibilità
    public AssociazioneMastrino? GetByClienteAndTemplate(int clienteId, int templateId)
    {
        return _context.AssociazioniMastrini.FirstOrDefault(x => 
            x.ClienteId == clienteId && 
            x.TemplateId == templateId);
    }

    /// <summary>
    /// Ottiene dettagli associazione
    /// </summary>
    public async Task<IEnumerable<AssociazioneMastrinoDettaglio>> GetDettagliAsync(int associazioneId)
    {
        return await _context.AssociazioniMastriniDettagli.Where(x => x.AssociazioneId == associazioneId).ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public IEnumerable<AssociazioneMastrinoDettaglio> GetDettagli(int associazioneId)
    {
        return _context.AssociazioniMastriniDettagli.Where(x => x.AssociazioneId == associazioneId).ToList();
    }

    /// <summary>
    /// Inserisce dettaglio associazione
    /// </summary>
    public async Task<int> InsertDettaglioAsync(AssociazioneMastrinoDettaglio dettaglio)
    {
        await _context.AssociazioniMastriniDettagli.AddAsync(dettaglio);
        await _context.SaveChangesAsync();
        return dettaglio.Id;
    }
    
    // Metodo sincrono per compatibilità
    public int InsertDettaglio(AssociazioneMastrinoDettaglio dettaglio)
    {
        _context.AssociazioniMastriniDettagli.Add(dettaglio);
        _context.SaveChanges();
        return dettaglio.Id;
    }

    /// <summary>
    /// Aggiorna dettaglio associazione
    /// </summary>
    public async Task<bool> UpdateDettaglioAsync(AssociazioneMastrinoDettaglio dettaglio)
    {
        try
        {
            _context.AssociazioniMastriniDettagli.Update(dettaglio);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool UpdateDettaglio(AssociazioneMastrinoDettaglio dettaglio)
    {
        try
        {
            _context.AssociazioniMastriniDettagli.Update(dettaglio);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Elimina dettaglio associazione
    /// </summary>
    public async Task<bool> DeleteDettaglioAsync(int dettaglioId)
    {
        try
        {
            var entity = await _context.AssociazioniMastriniDettagli.FindAsync(dettaglioId);
            if (entity == null) return false;
            
            _context.AssociazioniMastriniDettagli.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool DeleteDettaglio(int dettaglioId)
    {
        try
        {
            var entity = _context.AssociazioniMastriniDettagli.Find(dettaglioId);
            if (entity == null) return false;
            
            _context.AssociazioniMastriniDettagli.Remove(entity);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Elimina tutti i dettagli di un'associazione
    /// </summary>
    public async Task<int> DeleteDettagliByAssociazioneAsync(int associazioneId)
    {
        var entities = await _context.AssociazioniMastriniDettagli
            .Where(d => d.AssociazioneId == associazioneId)
            .ToListAsync();
        
        _context.AssociazioniMastriniDettagli.RemoveRange(entities);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteDettagliByAssociazione(int associazioneId)
    {
        var entities = _context.AssociazioniMastriniDettagli
            .Where(d => d.AssociazioneId == associazioneId)
            .ToList();
        
        _context.AssociazioniMastriniDettagli.RemoveRange(entities);
        return _context.SaveChanges();
    }

    /// <summary>
    /// Conta dettagli associazione
    /// </summary>
    public async Task<int> CountDettagliAsync(int associazioneId)
    {
        return await _context.AssociazioniMastriniDettagli.CountAsync(x => x.AssociazioneId == associazioneId);
    }
    
    // Metodo sincrono per compatibilità
    public int CountDettagli(int associazioneId)
    {
        return _context.AssociazioniMastriniDettagli.Count(x => x.AssociazioneId == associazioneId);
    }
}
