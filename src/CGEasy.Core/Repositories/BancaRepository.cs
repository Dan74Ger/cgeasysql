using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BancaRepository
{
    private readonly CGEasyDbContext _context;

    public BancaRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<Banca>> GetAllAsync()
    {
        return await _context.Banche.AsNoTracking().OrderBy(b => b.NomeBanca).ToListAsync();
    }

    public async Task<Banca?> GetByIdAsync(int id)
    {
        return await _context.Banche.FindAsync(id);
    }

    public async Task<List<Banca>> SearchByNomeAsync(string nome)
    {
        return await _context.Banche.Where(b => b.NomeBanca.Contains(nome)).ToListAsync();
    }

    public async Task<int> InsertAsync(Banca banca)
    {
        banca.DataCreazione = DateTime.Now;
        banca.DataUltimaModifica = DateTime.Now;
        _context.Banche.Add(banca);
        await _context.SaveChangesAsync();
        return banca.Id;
    }

    public async Task<bool> UpdateAsync(Banca banca)
    {
        banca.DataUltimaModifica = DateTime.Now;
        _context.Banche.Update(banca);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var banca = await GetByIdAsync(id);
        if (banca == null) return false;
        _context.Banche.Remove(banca);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Banche.CountAsync();
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public List<Banca> GetAll() => GetAllAsync().Result;
    public Banca? GetById(int id) => GetByIdAsync(id).Result;
    public List<Banca> SearchByNome(string nome) => SearchByNomeAsync(nome).Result;
    public int Insert(Banca banca) => InsertAsync(banca).Result;
    public bool Update(Banca banca) => UpdateAsync(banca).Result;
    public bool Delete(int id) => DeleteAsync(id).Result;
    public int Count() => CountAsync().Result;
}
