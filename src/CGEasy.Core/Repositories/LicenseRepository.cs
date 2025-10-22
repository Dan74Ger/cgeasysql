using CGEasy.Core.Data;
using CGEasy.Core.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Repositories;

/// <summary>
/// Repository per gestione clienti licenze e chiavi generate
/// </summary>
public class LicenseRepository
{
    private readonly LiteDbContext _context;

    public LicenseRepository(LiteDbContext context)
    {
        _context = context;
    }

    // ===== CLIENTI LICENZE =====

    /// <summary>
    /// Ottiene tutti i clienti licenze
    /// </summary>
    public IEnumerable<LicenseClient> GetAllClients()
    {
        return _context.LicenseClients.FindAll().OrderBy(c => c.NomeCliente);
    }

    /// <summary>
    /// Ottiene cliente per ID
    /// </summary>
    public LicenseClient? GetClientById(int id)
    {
        return _context.LicenseClients.FindById(id);
    }

    /// <summary>
    /// Inserisce nuovo cliente
    /// </summary>
    public int InsertClient(LicenseClient client)
    {
        return _context.LicenseClients.Insert(client);
    }

    /// <summary>
    /// Aggiorna cliente esistente
    /// </summary>
    public bool UpdateClient(LicenseClient client)
    {
        return _context.LicenseClients.Update(client);
    }

    /// <summary>
    /// Elimina cliente (e tutte le sue licenze)
    /// </summary>
    public bool DeleteClient(int id)
    {
        // Elimina prima tutte le chiavi del cliente
        _context.LicenseKeys.DeleteMany(k => k.LicenseClientId == id);
        
        // Poi elimina il cliente
        return _context.LicenseClients.Delete(id);
    }

    // ===== CHIAVI LICENZE =====

    /// <summary>
    /// Ottiene tutte le chiavi
    /// </summary>
    public IEnumerable<LicenseKey> GetAllKeys()
    {
        return _context.LicenseKeys.FindAll().OrderByDescending(k => k.DataGenerazione);
    }

    /// <summary>
    /// Ottiene chiavi per cliente specifico
    /// </summary>
    public IEnumerable<LicenseKey> GetKeysByClient(int clientId)
    {
        return _context.LicenseKeys
            .Find(k => k.LicenseClientId == clientId)
            .OrderByDescending(k => k.DataGenerazione);
    }

    /// <summary>
    /// Ottiene chiavi per modulo specifico
    /// </summary>
    public IEnumerable<LicenseKey> GetKeysByModule(string moduleName)
    {
        return _context.LicenseKeys
            .Find(k => k.ModuleName == moduleName)
            .OrderByDescending(k => k.DataGenerazione);
    }

    /// <summary>
    /// Verifica se una chiave esiste ed è valida
    /// </summary>
    public bool IsKeyValid(string fullKey)
    {
        var key = _context.LicenseKeys.FindOne(k => k.FullKey == fullKey);
        
        if (key == null)
            return false;
        
        // Verifica che sia attiva e non scaduta
        return key.IsActive && !key.IsExpired;
    }

    /// <summary>
    /// Ottiene informazioni su una chiave
    /// </summary>
    public LicenseKey? GetKeyByFullKey(string fullKey)
    {
        return _context.LicenseKeys.FindOne(k => k.FullKey == fullKey);
    }

    /// <summary>
    /// Inserisce nuova chiave
    /// </summary>
    public int InsertKey(LicenseKey key)
    {
        return _context.LicenseKeys.Insert(key);
    }

    /// <summary>
    /// Aggiorna chiave (es. per revocarla)
    /// </summary>
    public bool UpdateKey(LicenseKey key)
    {
        return _context.LicenseKeys.Update(key);
    }

    /// <summary>
    /// Revoca una chiave (soft delete)
    /// </summary>
    public bool RevokeKey(int keyId)
    {
        var key = _context.LicenseKeys.FindById(keyId);
        if (key == null)
            return false;
        
        key.IsActive = false;
        key.Note = $"Revocata il {DateTime.Now:dd/MM/yyyy HH:mm}";
        return _context.LicenseKeys.Update(key);
    }

    /// <summary>
    /// Elimina definitivamente una chiave
    /// </summary>
    public bool DeleteKey(int keyId)
    {
        return _context.LicenseKeys.Delete(keyId);
    }

    /// <summary>
    /// Conta chiavi attive per cliente
    /// </summary>
    public int CountActiveKeysByClient(int clientId)
    {
        return _context.LicenseKeys.Count(k => 
            k.LicenseClientId == clientId && 
            k.IsActive && 
            !k.IsExpired);
    }

    /// <summary>
    /// Ottiene statistiche licenze
    /// </summary>
    public (int TotalClients, int TotalKeys, int ActiveKeys, int ExpiredKeys) GetStatistics()
    {
        var allKeys = GetAllKeys().ToList();
        
        return (
            TotalClients: _context.LicenseClients.Count(),
            TotalKeys: allKeys.Count,
            ActiveKeys: allKeys.Count(k => k.IsActive && !k.IsExpired),
            ExpiredKeys: allKeys.Count(k => k.IsExpired)
        );
    }
}


