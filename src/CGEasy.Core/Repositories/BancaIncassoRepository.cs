using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BancaIncassoRepository
{
    private readonly CGEasyDbContext _context;

    public BancaIncassoRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<BancaIncasso>> GetAllAsync()
    {
        return await _context.BancaIncassi.AsNoTracking().ToListAsync();
    }

    public async Task<BancaIncasso?> GetByIdAsync(int id)
    {
        return await _context.BancaIncassi.FindAsync(id);
    }

    public async Task<List<BancaIncasso>> GetByBancaIdAsync(int bancaId)
    {
        return await _context.BancaIncassi
            .Where(i => i.BancaId == bancaId)
            .OrderBy(i => i.DataScadenza)
            .ToListAsync();
    }

    public async Task<List<BancaIncasso>> GetByPeriodoAsync(int bancaId, int anno, int mese)
    {
        return await _context.BancaIncassi
            .Where(i => i.BancaId == bancaId && i.Anno == anno && i.Mese == mese)
            .OrderBy(i => i.DataScadenza)
            .ToListAsync();
    }

    public async Task<List<BancaIncasso>> GetInScadenzaAsync(int bancaId, DateTime dataLimite)
    {
        return await _context.BancaIncassi
            .Where(i => i.BancaId == bancaId && !i.Incassato && i.DataScadenza <= dataLimite)
            .OrderBy(i => i.DataScadenza)
            .ToListAsync();
    }

    public async Task<int> InsertAsync(BancaIncasso incasso)
    {
        incasso.DataCreazione = DateTime.Now;
        _context.BancaIncassi.Add(incasso);
        await _context.SaveChangesAsync();
        return incasso.Id;
    }

    public async Task<bool> UpdateAsync(BancaIncasso incasso)
    {
        incasso.DataUltimaModifica = DateTime.Now;
        _context.BancaIncassi.Update(incasso);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var incasso = await GetByIdAsync(id);
        if (incasso == null) return false;
        _context.BancaIncassi.Remove(incasso);
        return await _context.SaveChangesAsync() > 0;
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public List<BancaIncasso> GetAll() => GetAllAsync().Result;
    public BancaIncasso? GetById(int id) => GetByIdAsync(id).Result;
    public List<BancaIncasso> GetByBancaId(int bancaId) => GetByBancaIdAsync(bancaId).Result;
    public List<BancaIncasso> GetByPeriodo(int bancaId, int anno, int mese) => GetByPeriodoAsync(bancaId, anno, mese).Result;
    public List<BancaIncasso> GetInScadenzaEntro(int bancaId, DateTime dataLimite) => GetInScadenzaAsync(bancaId, dataLimite).Result;
    public int Insert(BancaIncasso incasso) => InsertAsync(incasso).Result;
    public bool Update(BancaIncasso incasso) => UpdateAsync(incasso).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    
    public async Task<bool> SegnaIncassatoAsync(int id, DateTime? dataIncasso = null)
    {
        var incasso = await GetByIdAsync(id);
        if (incasso == null) return false;
        incasso.Incassato = true;
        incasso.DataIncassoEffettivo = dataIncasso ?? DateTime.Now;
        return await UpdateAsync(incasso);
    }
    
    public bool SegnaIncassato(int id, DateTime? dataIncasso = null) => SegnaIncassatoAsync(id, dataIncasso).Result;
    
    public async Task<int> DeleteByBancaIdAsync(int bancaId)
    {
        var incassi = await GetByBancaIdAsync(bancaId);
        _context.BancaIncassi.RemoveRange(incassi);
        return await _context.SaveChangesAsync();
    }
    
    public int DeleteByBancaId(int bancaId) => DeleteByBancaIdAsync(bancaId).Result;
}
