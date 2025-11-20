using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BilancioContabileRepository
{
    private readonly CGEasyDbContext _context;

    public BilancioContabileRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<BilancioContabile>> GetAllAsync()
    {
        return await _context.BilancioContabile.ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioContabile> GetAll()
    {
        return _context.BilancioContabile.ToList();
    }

    public async Task<BilancioContabile?> GetByIdAsync(int id)
    {
        return await _context.BilancioContabile.FindAsync(id);
    }
    
    // Metodo sincrono per compatibilità
    public BilancioContabile? GetById(int id)
    {
        return _context.BilancioContabile.Find(id);
    }

    public async Task<List<BilancioContabile>> GetByClienteAsync(int clienteId)
    {
        return await _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId)
            .OrderByDescending(b => b.Anno)
            .ThenByDescending(b => b.Mese)
            .ThenBy(b => b.CodiceMastrino)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioContabile> GetByCliente(int clienteId)
    {
        return _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId)
            .OrderByDescending(b => b.Anno)
            .ThenByDescending(b => b.Mese)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public async Task<List<BilancioContabile>> GetByPeriodoAsync(int mese, int anno)
    {
        return await _context.BilancioContabile
            .Where(b => b.Mese == mese && b.Anno == anno)
            .OrderBy(b => b.ClienteNome)
            .ThenBy(b => b.CodiceMastrino)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioContabile> GetByPeriodo(int mese, int anno)
    {
        return _context.BilancioContabile
            .Where(b => b.Mese == mese && b.Anno == anno)
            .OrderBy(b => b.ClienteNome)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public async Task<List<BilancioContabile>> GetByClienteAndPeriodoAsync(int clienteId, int mese, int anno)
    {
        var bilanci = await _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .ToListAsync();
        
        return bilanci
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioContabile> GetByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        return _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .ToList()
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    /// <summary>
    /// Ottiene bilanci per Cliente+Periodo+DESCRIZIONE specifica
    /// </summary>
    public async Task<List<BilancioContabile>> GetByClienteAndPeriodoAndDescrizioneAsync(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanci = await _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToListAsync();
        
        return bilanci
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioContabile> GetByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanci = _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToList();
        
        return bilanci
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    public async Task<int> InsertAsync(BilancioContabile bilancio)
    {
        await _context.BilancioContabile.AddAsync(bilancio);
        await _context.SaveChangesAsync();
        return bilancio.Id;
    }
    
    // Metodo sincrono per compatibilità
    public int Insert(BilancioContabile bilancio)
    {
        _context.BilancioContabile.Add(bilancio);
        _context.SaveChanges();
        return bilancio.Id;
    }

    public async Task InsertBulkAsync(IEnumerable<BilancioContabile> bilanci)
    {
        await _context.BilancioContabile.AddRangeAsync(bilanci);
        await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public void InsertBulk(IEnumerable<BilancioContabile> bilanci)
    {
        _context.BilancioContabile.AddRange(bilanci);
        _context.SaveChanges();
    }

    public async Task<bool> UpdateAsync(BilancioContabile bilancio)
    {
        try
        {
            _context.BilancioContabile.Update(bilancio);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool Update(BilancioContabile bilancio)
    {
        try
        {
            _context.BilancioContabile.Update(bilancio);
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
            var entity = await _context.BilancioContabile.FindAsync(id);
            if (entity == null) return false;
            
            _context.BilancioContabile.Remove(entity);
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
            var entity = _context.BilancioContabile.Find(id);
            if (entity == null) return false;
            
            _context.BilancioContabile.Remove(entity);
            _context.SaveChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> DeleteMultipleAsync(IEnumerable<int> ids)
    {
        var entities = await _context.BilancioContabile
            .Where(b => ids.Contains(b.Id))
            .ToListAsync();
        
        _context.BilancioContabile.RemoveRange(entities);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteMultiple(IEnumerable<int> ids)
    {
        var entities = _context.BilancioContabile
            .Where(b => ids.Contains(b.Id))
            .ToList();
        
        _context.BilancioContabile.RemoveRange(entities);
        return _context.SaveChanges();
    }

    public async Task<int> DeleteByClienteAndPeriodoAsync(int clienteId, int mese, int anno)
    {
        var entities = await _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToListAsync();
        
        _context.BilancioContabile.RemoveRange(entities);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        var entities = _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToList();
        
        _context.BilancioContabile.RemoveRange(entities);
        return _context.SaveChanges();
    }

    /// <summary>
    /// Elimina bilanci per Cliente+Periodo+DESCRIZIONE
    /// </summary>
    public async Task<int> DeleteByClienteAndPeriodoAndDescrizioneAsync(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanciDaEliminare = await _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToListAsync();
        
        var toDelete = bilanciDaEliminare
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"[DELETE] Eliminazione {toDelete.Count} righe per Cliente={clienteId}, Periodo={mese}/{anno}, Descrizione='{desc}'");
        
        _context.BilancioContabile.RemoveRange(toDelete);
        var count = await _context.SaveChangesAsync();
        
        if (count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[DELETE] Eliminazione completata: {count} righe eliminate");
        }
        
        return count;
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanciDaEliminare = _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToList();
        
        var toDelete = bilanciDaEliminare
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"[DELETE] Eliminazione {toDelete.Count} righe per Cliente={clienteId}, Periodo={mese}/{anno}, Descrizione='{desc}'");
        
        _context.BilancioContabile.RemoveRange(toDelete);
        var count = _context.SaveChanges();
        
        if (count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[DELETE] Eliminazione completata: {count} righe eliminate");
        }
        
        return count;
    }

    public async Task<List<int>> GetDistinctAnniAsync()
    {
        return await _context.BilancioContabile
            .Select(b => b.Anno)
            .Distinct()
            .OrderByDescending(a => a)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<int> GetDistinctAnni()
    {
        return _context.BilancioContabile
            .Select(b => b.Anno)
            .Distinct()
            .OrderByDescending(a => a)
            .ToList();
    }
    
    /// <summary>
    /// Verifica duplicati per un cliente/periodo/descrizione
    /// </summary>
    public async Task<Dictionary<string, int>> VerificaDuplicatiAsync(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanci = await _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .ToListAsync();
        
        var filtered = bilanci
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();
        
        var duplicati = filtered
            .GroupBy(b => b.CodiceMastrino)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.Count());
        
        if (duplicati.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[DUPLICATI] Trovati {duplicati.Count} codici duplicati:");
            foreach (var dup in duplicati)
            {
                System.Diagnostics.Debug.WriteLine($"  - Codice {dup.Key}: {dup.Value} righe");
                var righe = filtered.Where(b => b.CodiceMastrino == dup.Key).ToList();
                foreach (var riga in righe)
                {
                    System.Diagnostics.Debug.WriteLine($"      ID:{riga.Id}, Desc:'{riga.DescrizioneMastrino}', Importo:{riga.Importo}, DataImport:{riga.DataImport:HH:mm:ss.fff}");
                }
            }
        }
        
        return duplicati;
    }
    
    // Metodo sincrono per compatibilità
    public Dictionary<string, int> VerificaDuplicati(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanci = _context.BilancioContabile
            .Where(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .ToList();
        
        var filtered = bilanci
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();
        
        var duplicati = filtered
            .GroupBy(b => b.CodiceMastrino)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.Count());
        
        if (duplicati.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[DUPLICATI] Trovati {duplicati.Count} codici duplicati:");
            foreach (var dup in duplicati)
            {
                System.Diagnostics.Debug.WriteLine($"  - Codice {dup.Key}: {dup.Value} righe");
                var righe = filtered.Where(b => b.CodiceMastrino == dup.Key).ToList();
                foreach (var riga in righe)
                {
                    System.Diagnostics.Debug.WriteLine($"      ID:{riga.Id}, Desc:'{riga.DescrizioneMastrino}', Importo:{riga.Importo}, DataImport:{riga.DataImport:HH:mm:ss.fff}");
                }
            }
        }
        
        return duplicati;
    }

    public async Task<List<string>> GetDistinctCodiciMastrinoAsync()
    {
        return await _context.BilancioContabile
            .Select(b => b.CodiceMastrino)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<string> GetDistinctCodiciMastrino()
    {
        return _context.BilancioContabile
            .Select(b => b.CodiceMastrino)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<List<BilancioGruppo>> GetGruppiAsync()
    {
        try
        {
            var allBilanci = await _context.BilancioContabile.ToListAsync();
            
            var gruppi = allBilanci
                .GroupBy(b => new { 
                    b.ClienteId, 
                    b.Mese, 
                    b.Anno, 
                    Descrizione = b.DescrizioneBilancio ?? ""
                })
                .Select(g =>
                {
                    var primaRiga = g.OrderBy(x => x.DataImport).First();
                    
                    return new BilancioGruppo
                    {
                        ClienteId = g.Key.ClienteId,
                        ClienteNome = primaRiga.ClienteNome,
                        Mese = g.Key.Mese,
                        Anno = g.Key.Anno,
                        Descrizione = primaRiga.DescrizioneBilancio,
                        TipoBilancio = primaRiga.TipoBilancio ?? "CE",
                        DataImport = primaRiga.DataImport,
                        ImportedByName = primaRiga.ImportedByName,
                        NumeroRighe = g.Count(),
                        TotaleImporti = g.Sum(b => b.Importo)
                    };
                })
                .OrderByDescending(g => g.Anno)
                .ThenByDescending(g => g.Mese)
                .ThenBy(g => g.ClienteNome)
                .ToList();
            
            return gruppi;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Errore GetGruppi: {ex.Message}");
            throw;
        }
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioGruppo> GetGruppi()
    {
        try
        {
            var allBilanci = _context.BilancioContabile.ToList();
            
            var gruppi = allBilanci
                .GroupBy(b => new { 
                    b.ClienteId, 
                    b.Mese, 
                    b.Anno, 
                    Descrizione = b.DescrizioneBilancio ?? ""
                })
                .Select(g =>
                {
                    var primaRiga = g.OrderBy(x => x.DataImport).First();
                    
                    return new BilancioGruppo
                    {
                        ClienteId = g.Key.ClienteId,
                        ClienteNome = primaRiga.ClienteNome,
                        Mese = g.Key.Mese,
                        Anno = g.Key.Anno,
                        Descrizione = primaRiga.DescrizioneBilancio,
                        TipoBilancio = primaRiga.TipoBilancio ?? "CE",
                        DataImport = primaRiga.DataImport,
                        ImportedByName = primaRiga.ImportedByName,
                        NumeroRighe = g.Count(),
                        TotaleImporti = g.Sum(b => b.Importo)
                    };
                })
                .OrderByDescending(g => g.Anno)
                .ThenByDescending(g => g.Mese)
                .ThenBy(g => g.ClienteNome)
                .ToList();
            
            return gruppi;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Errore GetGruppi: {ex.Message}");
            throw;
        }
    }

    public async Task<List<BilancioGruppo>> GetGruppiByClienteAsync(int clienteId)
    {
        try
        {
            var bilanci = await _context.BilancioContabile
                .Where(b => b.ClienteId == clienteId)
                .ToListAsync();
            
            var gruppi = bilanci
                .GroupBy(b => new { 
                    b.ClienteId, 
                    b.Mese, 
                    b.Anno, 
                    Descrizione = b.DescrizioneBilancio ?? ""
                })
                .Select(g =>
                {
                    var primaRiga = g.OrderBy(x => x.DataImport).First();
                    
                    return new BilancioGruppo
                    {
                        ClienteId = g.Key.ClienteId,
                        ClienteNome = primaRiga.ClienteNome,
                        Mese = g.Key.Mese,
                        Anno = g.Key.Anno,
                        Descrizione = primaRiga.DescrizioneBilancio,
                        TipoBilancio = primaRiga.TipoBilancio ?? "CE",
                        DataImport = primaRiga.DataImport,
                        ImportedByName = primaRiga.ImportedByName,
                        NumeroRighe = g.Count(),
                        TotaleImporti = g.Sum(b => b.Importo)
                    };
                })
                .OrderByDescending(g => g.Anno)
                .ThenByDescending(g => g.Mese)
                .ToList();
            
            return gruppi;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Errore GetGruppiByCliente: {ex.Message}");
            throw;
        }
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioGruppo> GetGruppiByCliente(int clienteId)
    {
        try
        {
            var bilanci = _context.BilancioContabile
                .Where(b => b.ClienteId == clienteId)
                .ToList();
            
            var gruppi = bilanci
                .GroupBy(b => new { 
                    b.ClienteId, 
                    b.Mese, 
                    b.Anno, 
                    Descrizione = b.DescrizioneBilancio ?? ""
                })
                .Select(g =>
                {
                    var primaRiga = g.OrderBy(x => x.DataImport).First();
                    
                    return new BilancioGruppo
                    {
                        ClienteId = g.Key.ClienteId,
                        ClienteNome = primaRiga.ClienteNome,
                        Mese = g.Key.Mese,
                        Anno = g.Key.Anno,
                        Descrizione = primaRiga.DescrizioneBilancio,
                        TipoBilancio = primaRiga.TipoBilancio ?? "CE",
                        DataImport = primaRiga.DataImport,
                        ImportedByName = primaRiga.ImportedByName,
                        NumeroRighe = g.Count(),
                        TotaleImporti = g.Sum(b => b.Importo)
                    };
                })
                .OrderByDescending(g => g.Anno)
                .ThenByDescending(g => g.Mese)
                .ToList();
            
            return gruppi;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Errore GetGruppiByCliente: {ex.Message}");
            throw;
        }
    }
}
