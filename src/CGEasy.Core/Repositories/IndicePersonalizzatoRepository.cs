using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class IndicePersonalizzatoRepository
{
    private readonly CGEasyDbContext _context;

    public IndicePersonalizzatoRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<IndicePersonalizzato>> GetAllAsync()
    {
        return await _context.IndiciPersonalizzati.ToListAsync();
    }

    public async Task<List<IndicePersonalizzato>> GetByClienteAsync(int clienteId)
    {
        return await _context.IndiciPersonalizzati
            .Where(i => i.ClienteId == clienteId && i.Attivo)
            .OrderBy(i => i.NomeIndice)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità con ViewModel esistenti
    public List<IndicePersonalizzato> GetByCliente(int clienteId)
    {
        return _context.IndiciPersonalizzati
            .Where(i => i.ClienteId == clienteId && i.Attivo)
            .OrderBy(i => i.NomeIndice)
            .ToList();
    }

    public async Task<IndicePersonalizzato?> GetByIdAsync(int id)
    {
        return await _context.IndiciPersonalizzati.FindAsync(id);
    }

    public async Task<int> InsertAsync(IndicePersonalizzato indice)
    {
        indice.DataCreazione = DateTime.Now;
        await _context.IndiciPersonalizzati.AddAsync(indice);
        await _context.SaveChangesAsync();
        return indice.Id;
    }
    
    // Metodo sincrono per compatibilità
    public IndicePersonalizzato Insert(IndicePersonalizzato indice)
    {
        indice.DataCreazione = DateTime.Now;
        _context.IndiciPersonalizzati.Add(indice);
        _context.SaveChanges();
        return indice;
    }

    public async Task<bool> UpdateAsync(IndicePersonalizzato indice)
    {
        try
        {
            indice.DataUltimaModifica = DateTime.Now;
            _context.IndiciPersonalizzati.Update(indice);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool Update(IndicePersonalizzato indice)
    {
        try
        {
            indice.DataUltimaModifica = DateTime.Now;
            _context.IndiciPersonalizzati.Update(indice);
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
            var entity = await _context.IndiciPersonalizzati.FindAsync(id);
            if (entity == null) return false;
            
            _context.IndiciPersonalizzati.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool Delete(int id)
    {
        try
        {
            var entity = _context.IndiciPersonalizzati.Find(id);
            if (entity == null) return false;
            
            _context.IndiciPersonalizzati.Remove(entity);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int clienteId, string nomeIndice, int? excludeId = null)
    {
        var query = _context.IndiciPersonalizzati
            .Where(i => i.ClienteId == clienteId && 
                       i.NomeIndice.ToLower() == nomeIndice.ToLower() &&
                       i.Attivo);

        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
