using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BancaUtilizzoAnticipoRepository
{
    private readonly CGEasyDbContext _context;

    public BancaUtilizzoAnticipoRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<BancaUtilizzoAnticipo>> GetAllAsync()
    {
        return await _context.BancaUtilizzoAnticipo.AsNoTracking().ToListAsync();
    }

    public async Task<BancaUtilizzoAnticipo?> GetByIdAsync(int id)
    {
        return await _context.BancaUtilizzoAnticipo.FindAsync(id);
    }

    public async Task<List<BancaUtilizzoAnticipo>> GetByBancaIdAsync(int bancaId)
    {
        return await _context.BancaUtilizzoAnticipo
            .Where(u => u.BancaId == bancaId)
            .OrderByDescending(u => u.DataInizioUtilizzo)
            .ToListAsync();
    }

    public async Task<int> InsertAsync(BancaUtilizzoAnticipo utilizzo)
    {
        utilizzo.DataCreazione = DateTime.Now;
        _context.BancaUtilizzoAnticipo.Add(utilizzo);
        await _context.SaveChangesAsync();
        return utilizzo.Id;
    }

    public async Task<bool> UpdateAsync(BancaUtilizzoAnticipo utilizzo)
    {
        utilizzo.DataUltimaModifica = DateTime.Now;
        _context.BancaUtilizzoAnticipo.Update(utilizzo);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var utilizzo = await GetByIdAsync(id);
        if (utilizzo == null) return false;
        _context.BancaUtilizzoAnticipo.Remove(utilizzo);
        return await _context.SaveChangesAsync() > 0;
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public List<BancaUtilizzoAnticipo> GetAll() => GetAllAsync().Result;
    public BancaUtilizzoAnticipo? GetById(int id) => GetByIdAsync(id).Result;
    public List<BancaUtilizzoAnticipo> GetByBancaId(int bancaId) => GetByBancaIdAsync(bancaId).Result;
    public int Insert(BancaUtilizzoAnticipo utilizzo) => InsertAsync(utilizzo).Result;
    public bool Update(BancaUtilizzoAnticipo utilizzo) => UpdateAsync(utilizzo).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    
    public decimal GetTotaleUtilizziAttivi(int bancaId)
    {
        return _context.BancaUtilizzoAnticipo
            .Where(u => u.BancaId == bancaId && !u.Rimborsato)
            .Sum(u => u.ImportoUtilizzo);
    }
    public List<BancaUtilizzoAnticipo> GetInScadenzaEntro(int bancaId, DateTime dataLimite)
    {
        return _context.BancaUtilizzoAnticipo
            .Where(u => u.BancaId == bancaId && !u.Rimborsato && u.DataScadenzaUtilizzo <= dataLimite)
            .OrderBy(u => u.DataScadenzaUtilizzo)
            .ToList();
    }
    
    public async Task<bool> SegnaRimborsatoAsync(int id, DateTime? dataRimborso = null)
    {
        var utilizzo = await GetByIdAsync(id);
        if (utilizzo == null) return false;
        utilizzo.Rimborsato = true;
        utilizzo.DataRimborsoEffettivo = dataRimborso ?? DateTime.Now;
        return await UpdateAsync(utilizzo);
    }
    
    public bool SegnaRimborsato(int id, DateTime? dataRimborso = null) => SegnaRimborsatoAsync(id, dataRimborso).Result;
    
    public async Task<int> DeleteByBancaIdAsync(int bancaId)
    {
        var utilizzi = await GetByBancaIdAsync(bancaId);
        _context.BancaUtilizzoAnticipo.RemoveRange(utilizzi);
        return await _context.SaveChangesAsync();
    }
    
    public int DeleteByBancaId(int bancaId) => DeleteByBancaIdAsync(bancaId).Result;
}
