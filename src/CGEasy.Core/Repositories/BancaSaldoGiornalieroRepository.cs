using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BancaSaldoGiornalieroRepository
{
    private readonly CGEasyDbContext _context;

    public BancaSaldoGiornalieroRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<BancaSaldoGiornaliero>> GetAllAsync()
    {
        return await _context.BancaSaldoGiornaliero.AsNoTracking().ToListAsync();
    }

    public async Task<BancaSaldoGiornaliero?> GetByIdAsync(int id)
    {
        return await _context.BancaSaldoGiornaliero.FindAsync(id);
    }

    public async Task<List<BancaSaldoGiornaliero>> GetByBancaIdAsync(int bancaId)
    {
        return await _context.BancaSaldoGiornaliero
            .Where(s => s.BancaId == bancaId)
            .OrderByDescending(s => s.Data)
            .ToListAsync();
    }

    public async Task<BancaSaldoGiornaliero?> GetByDataAsync(int bancaId, DateTime data)
    {
        return await _context.BancaSaldoGiornaliero
            .FirstOrDefaultAsync(s => s.BancaId == bancaId && s.Data.Date == data.Date);
    }

    public async Task<int> InsertAsync(BancaSaldoGiornaliero saldo)
    {
        saldo.DataCreazione = DateTime.Now;
        _context.BancaSaldoGiornaliero.Add(saldo);
        await _context.SaveChangesAsync();
        return saldo.Id;
    }

    public async Task<bool> UpdateAsync(BancaSaldoGiornaliero saldo)
    {
        _context.BancaSaldoGiornaliero.Update(saldo);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var saldo = await GetByIdAsync(id);
        if (saldo == null) return false;
        _context.BancaSaldoGiornaliero.Remove(saldo);
        return await _context.SaveChangesAsync() > 0;
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public List<BancaSaldoGiornaliero> GetAll() => GetAllAsync().Result;
    public BancaSaldoGiornaliero? GetById(int id) => GetByIdAsync(id).Result;
    public List<BancaSaldoGiornaliero> GetByBancaId(int bancaId) => GetByBancaIdAsync(bancaId).Result;
    public BancaSaldoGiornaliero? GetByData(int bancaId, DateTime data) => GetByDataAsync(bancaId, data).Result;
    public BancaSaldoGiornaliero? GetAllaData(int bancaId, DateTime data) => GetByDataAsync(bancaId, data).Result;
    public int Insert(BancaSaldoGiornaliero saldo) => InsertAsync(saldo).Result;
    public bool Update(BancaSaldoGiornaliero saldo) => UpdateAsync(saldo).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    
    public async Task<int> DeleteByBancaIdAsync(int bancaId)
    {
        var saldi = await GetByBancaIdAsync(bancaId);
        _context.BancaSaldoGiornaliero.RemoveRange(saldi);
        return await _context.SaveChangesAsync();
    }
    
    public int DeleteByBancaId(int bancaId) => DeleteByBancaIdAsync(bancaId).Result;
}
