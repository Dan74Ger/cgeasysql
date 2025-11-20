using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class StatisticaSPSalvataRepository
{
    private readonly CGEasyDbContext _context;

    public StatisticaSPSalvataRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<StatisticaSPSalvata>> GetAllAsync()
    {
        return await _context.StatisticheSPSalvate
            .OrderByDescending(s => s.DataCreazione)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<StatisticaSPSalvata> GetAll()
    {
        return _context.StatisticheSPSalvate
            .OrderByDescending(s => s.DataCreazione)
            .ToList();
    }

    public async Task<List<StatisticaSPSalvata>> GetByClienteAsync(int clienteId)
    {
        return await _context.StatisticheSPSalvate
            .Where(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.DataCreazione)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<StatisticaSPSalvata> GetByCliente(int clienteId)
    {
        return _context.StatisticheSPSalvate
            .Where(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.DataCreazione)
            .ToList();
    }

    public async Task<StatisticaSPSalvata?> GetByIdAsync(int id)
    {
        return await _context.StatisticheSPSalvate.FindAsync(id);
    }
    
    // Metodo sincrono per compatibilità
    public StatisticaSPSalvata? GetById(int id)
    {
        return _context.StatisticheSPSalvate.Find(id);
    }

    public async Task<int> InsertAsync(StatisticaSPSalvata statistica)
    {
        await _context.StatisticheSPSalvate.AddAsync(statistica);
        await _context.SaveChangesAsync();
        return statistica.Id;
    }
    
    // Metodo sincrono per compatibilità
    public int Insert(StatisticaSPSalvata statistica)
    {
        _context.StatisticheSPSalvate.Add(statistica);
        _context.SaveChanges();
        return statistica.Id;
    }

    public async Task<bool> UpdateAsync(StatisticaSPSalvata statistica)
    {
        try
        {
            _context.StatisticheSPSalvate.Update(statistica);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool Update(StatisticaSPSalvata statistica)
    {
        try
        {
            _context.StatisticheSPSalvate.Update(statistica);
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
            var entity = await _context.StatisticheSPSalvate.FindAsync(id);
            if (entity == null) return false;
            
            _context.StatisticheSPSalvate.Remove(entity);
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
            var entity = _context.StatisticheSPSalvate.Find(id);
            if (entity == null) return false;
            
            _context.StatisticheSPSalvate.Remove(entity);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> DeleteByClienteAsync(int clienteId)
    {
        var entities = await _context.StatisticheSPSalvate
            .Where(s => s.ClienteId == clienteId)
            .ToListAsync();

        _context.StatisticheSPSalvate.RemoveRange(entities);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteByCliente(int clienteId)
    {
        var entities = _context.StatisticheSPSalvate
            .Where(s => s.ClienteId == clienteId)
            .ToList();

        _context.StatisticheSPSalvate.RemoveRange(entities);
        return _context.SaveChanges();
    }
}
