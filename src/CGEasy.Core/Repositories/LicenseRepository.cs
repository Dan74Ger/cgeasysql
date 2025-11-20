using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

/// <summary>
/// Repository per gestione clienti licenze e chiavi (EF Core async)
/// </summary>
public class LicenseRepository
{
    private readonly CGEasyDbContext _context;

    public LicenseRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    // ===== CLIENTI LICENZE =====

    public async Task<List<LicenseClient>> GetAllClientsAsync()
    {
        return await _context.LicenseClients
            .AsNoTracking()
            .OrderBy(c => c.NomeCliente)
            .ToListAsync();
    }

    public async Task<LicenseClient?> GetClientByIdAsync(int id)
    {
        return await _context.LicenseClients.FindAsync(id);
    }

    public async Task<int> InsertClientAsync(LicenseClient client)
    {
        _context.LicenseClients.Add(client);
        await _context.SaveChangesAsync();
        return client.Id;
    }

    public async Task<bool> UpdateClientAsync(LicenseClient client)
    {
        _context.LicenseClients.Update(client);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var keys = await _context.LicenseKeys.Where(k => k.LicenseClientId == id).ToListAsync();
        _context.LicenseKeys.RemoveRange(keys);
        
        var client = await GetClientByIdAsync(id);
        if (client == null) return false;
        _context.LicenseClients.Remove(client);
        return await _context.SaveChangesAsync() > 0;
    }

    // ===== CHIAVI LICENZE =====

    public async Task<List<LicenseKey>> GetAllKeysAsync()
    {
        return await _context.LicenseKeys
            .AsNoTracking()
            .OrderByDescending(k => k.DataGenerazione)
            .ToListAsync();
    }

    public async Task<List<LicenseKey>> GetKeysByClientAsync(int clientId)
    {
        return await _context.LicenseKeys
            .Where(k => k.LicenseClientId == clientId)
            .OrderByDescending(k => k.DataGenerazione)
            .ToListAsync();
    }

    public async Task<LicenseKey?> GetKeyByIdAsync(int id)
    {
        return await _context.LicenseKeys.FindAsync(id);
    }

    public async Task<int> InsertKeyAsync(LicenseKey key)
    {
        _context.LicenseKeys.Add(key);
        await _context.SaveChangesAsync();
        return key.Id;
    }

    public async Task<bool> UpdateKeyAsync(LicenseKey key)
    {
        _context.LicenseKeys.Update(key);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteKeyAsync(int id)
    {
        var key = await GetKeyByIdAsync(id);
        if (key == null) return false;
        _context.LicenseKeys.Remove(key);
        return await _context.SaveChangesAsync() > 0;
    }

    // ===== STATISTICHE =====

    public async Task<LicenseStatistics> GetStatisticsAsync()
    {
        var totalClients = await _context.LicenseClients.CountAsync();
        var totalKeys = await _context.LicenseKeys.CountAsync();
        var activeKeys = await _context.LicenseKeys.CountAsync(k => k.IsActive && !k.IsExpired);

        return new LicenseStatistics
        {
            TotalClients = totalClients,
            TotalKeys = totalKeys,
            ActiveKeys = activeKeys
        };
    }

    // ===== RICERCA =====

    public async Task<List<LicenseKey>> SearchKeysAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllKeysAsync();

        return await _context.LicenseKeys
            .Where(k => k.FullKey.Contains(searchTerm) || 
                       k.ModuleName.Contains(searchTerm))
            .OrderByDescending(k => k.DataGenerazione)
            .ToListAsync();
    }

    // ===== METODI PER VALIDAZIONE =====

    public async Task<bool> IsKeyValidAsync(string fullKey)
    {
        var key = await _context.LicenseKeys
            .FirstOrDefaultAsync(k => k.FullKey == fullKey && k.IsActive && !k.IsExpired);
        return key != null;
    }

    public async Task<LicenseKey?> GetKeyByFullKeyAsync(string fullKey)
    {
        return await _context.LicenseKeys
            .FirstOrDefaultAsync(k => k.FullKey == fullKey);
    }

    // ===== WRAPPER SINCRONI per compatibilità =====
    public bool IsKeyValid(string fullKey) => IsKeyValidAsync(fullKey).Result;
    public LicenseKey? GetKeyByFullKey(string fullKey) => GetKeyByFullKeyAsync(fullKey).Result;
    public int InsertKey(LicenseKey key) => InsertKeyAsync(key).Result;
    public LicenseClient? GetClientById(int id) => GetClientByIdAsync(id).Result;
    public bool UpdateKey(LicenseKey key) => UpdateKeyAsync(key).Result;
}

/// <summary>
/// Statistiche licenze
/// </summary>
public class LicenseStatistics
{
    public int TotalClients { get; set; }
    public int TotalKeys { get; set; }
    public int ActiveKeys { get; set; }
}
