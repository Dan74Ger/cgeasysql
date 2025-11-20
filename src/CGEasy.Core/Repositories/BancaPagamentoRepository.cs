using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BancaPagamentoRepository
{
    private readonly CGEasyDbContext _context;

    public BancaPagamentoRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<BancaPagamento>> GetAllAsync()
    {
        return await _context.BancaPagamenti.AsNoTracking().ToListAsync();
    }

    public async Task<BancaPagamento?> GetByIdAsync(int id)
    {
        return await _context.BancaPagamenti.FindAsync(id);
    }

    public async Task<List<BancaPagamento>> GetByBancaIdAsync(int bancaId)
    {
        return await _context.BancaPagamenti
            .Where(p => p.BancaId == bancaId)
            .OrderBy(p => p.DataScadenza)
            .ToListAsync();
    }

    public async Task<List<BancaPagamento>> GetByPeriodoAsync(int bancaId, int anno, int mese)
    {
        return await _context.BancaPagamenti
            .Where(p => p.BancaId == bancaId && p.Anno == anno && p.Mese == mese)
            .OrderBy(p => p.DataScadenza)
            .ToListAsync();
    }

    public async Task<int> InsertAsync(BancaPagamento pagamento)
    {
        pagamento.DataCreazione = DateTime.Now;
        _context.BancaPagamenti.Add(pagamento);
        await _context.SaveChangesAsync();
        return pagamento.Id;
    }

    public async Task<bool> UpdateAsync(BancaPagamento pagamento)
    {
        pagamento.DataUltimaModifica = DateTime.Now;
        _context.BancaPagamenti.Update(pagamento);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pagamento = await GetByIdAsync(id);
        if (pagamento == null) return false;
        _context.BancaPagamenti.Remove(pagamento);
        return await _context.SaveChangesAsync() > 0;
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public List<BancaPagamento> GetAll() => GetAllAsync().Result;
    public BancaPagamento? GetById(int id) => GetByIdAsync(id).Result;
    public List<BancaPagamento> GetByBancaId(int bancaId) => GetByBancaIdAsync(bancaId).Result;
    public List<BancaPagamento> GetByPeriodo(int bancaId, int anno, int mese) => GetByPeriodoAsync(bancaId, anno, mese).Result;
    public int Insert(BancaPagamento pagamento) => InsertAsync(pagamento).Result;
    public bool Update(BancaPagamento pagamento) => UpdateAsync(pagamento).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    
    public List<BancaPagamento> GetInScadenzaEntro(int bancaId, DateTime dataLimite)
    {
        return _context.BancaPagamenti
            .Where(p => p.BancaId == bancaId && !p.Pagato && p.DataScadenza <= dataLimite)
            .OrderBy(p => p.DataScadenza)
            .ToList();
    }
    
    public async Task<bool> SegnaPagatoAsync(int id, DateTime? dataPagamento = null)
    {
        var pagamento = await GetByIdAsync(id);
        if (pagamento == null) return false;
        pagamento.Pagato = true;
        pagamento.DataPagamentoEffettivo = dataPagamento ?? DateTime.Now;
        return await UpdateAsync(pagamento);
    }
    
    public bool SegnaPagato(int id, DateTime? dataPagamento = null) => SegnaPagatoAsync(id, dataPagamento).Result;
    
    public async Task<int> DeleteByBancaIdAsync(int bancaId)
    {
        var pagamenti = await GetByBancaIdAsync(bancaId);
        _context.BancaPagamenti.RemoveRange(pagamenti);
        return await _context.SaveChangesAsync();
    }
    
    public int DeleteByBancaId(int bancaId) => DeleteByBancaIdAsync(bancaId).Result;
}
