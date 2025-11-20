using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class FinanziamentoImportRepository
{
    private readonly CGEasyDbContext _context;

    public FinanziamentoImportRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<int> InsertAsync(FinanziamentoImport finanziamento)
    {
        finanziamento.DataCreazione = DateTime.Now;
        _context.FinanziamentoImport.Add(finanziamento);
        await _context.SaveChangesAsync();
        return finanziamento.Id;
    }

    public async Task<bool> UpdateAsync(FinanziamentoImport finanziamento)
    {
        try
        {
            _context.FinanziamentoImport.Update(finanziamento);
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
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            _context.FinanziamentoImport.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<FinanziamentoImport?> GetByIdAsync(int id)
    {
        return await _context.FinanziamentoImport.FindAsync(id);
    }

    public async Task<List<FinanziamentoImport>> GetAllAsync()
    {
        return await _context.FinanziamentoImport
            .OrderByDescending(f => f.DataCreazione)
            .ToListAsync();
    }

    public async Task<List<FinanziamentoImport>> GetByBancaIdAsync(int bancaId)
    {
        return await _context.FinanziamentoImport
            .Where(f => f.BancaId == bancaId)
            .OrderByDescending(f => f.DataCreazione)
            .ToListAsync();
    }

    public async Task<List<FinanziamentoImport>> GetByUtenteIdAsync(int utenteId)
    {
        return await _context.FinanziamentoImport
            .Where(f => f.UtenteId == utenteId)
            .OrderByDescending(f => f.DataCreazione)
            .ToListAsync();
    }

    public async Task<FinanziamentoImport?> GetByIncassoIdAsync(int incassoId)
    {
        return await _context.FinanziamentoImport
            .FirstOrDefaultAsync(f => f.IncassoId == incassoId);
    }

    public async Task<FinanziamentoImport?> GetByPagamentoIdAsync(int pagamentoId)
    {
        return await _context.FinanziamentoImport
            .FirstOrDefaultAsync(f => f.PagamentoId == pagamentoId);
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public int Insert(FinanziamentoImport finanziamento) => InsertAsync(finanziamento).Result;
    public bool Update(FinanziamentoImport finanziamento) => UpdateAsync(finanziamento).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    public FinanziamentoImport? GetById(int id) => GetByIdAsync(id).Result;
    public List<FinanziamentoImport> GetAll() => GetAllAsync().Result;
}
