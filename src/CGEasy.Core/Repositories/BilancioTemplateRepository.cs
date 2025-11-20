using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories;

public class BilancioTemplateRepository
{
    private readonly CGEasyDbContext _context;

    public BilancioTemplateRepository(CGEasyDbContext context)
    {
        _context = context;
    }

    public async Task<List<BilancioTemplate>> GetAllAsync()
    {
        return await _context.BilancioTemplate.ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioTemplate> GetAll()
    {
        return _context.BilancioTemplate.ToList();
    }

    public async Task<BilancioTemplate?> GetByIdAsync(int id)
    {
        return await _context.BilancioTemplate.FindAsync(id);
    }
    
    // Metodo sincrono per compatibilità
    public BilancioTemplate? GetById(int id)
    {
        return _context.BilancioTemplate.Find(id);
    }

    public async Task<List<BilancioTemplate>> GetByClienteAsync(int clienteId)
    {
        return await _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId)
            .OrderByDescending(b => b.Anno)
            .ThenByDescending(b => b.Mese)
            .ThenBy(b => b.CodiceMastrino)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioTemplate> GetByCliente(int clienteId)
    {
        return _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId)
            .OrderByDescending(b => b.Anno)
            .ThenByDescending(b => b.Mese)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public async Task<List<BilancioTemplate>> GetByPeriodoAsync(int mese, int anno)
    {
        return await _context.BilancioTemplate
            .Where(b => b.Mese == mese && b.Anno == anno)
            .OrderBy(b => b.ClienteNome)
            .ThenBy(b => b.CodiceMastrino)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioTemplate> GetByPeriodo(int mese, int anno)
    {
        return _context.BilancioTemplate
            .Where(b => b.Mese == mese && b.Anno == anno)
            .OrderBy(b => b.ClienteNome)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public async Task<List<BilancioTemplate>> GetByClienteAndPeriodoAsync(int clienteId, int mese, int anno)
    {
        var templates = await _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .ToListAsync();
        
        return templates
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioTemplate> GetByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        return _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .ToList()
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    /// <summary>
    /// Ottiene i template per cliente, periodo E descrizione (per supportare più template con stessa data)
    /// </summary>
    public async Task<List<BilancioTemplate>> GetByClienteAndPeriodoAndDescrizioneAsync(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        var templates = await _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId &&
                       b.Mese == mese &&
                       b.Anno == anno)
            .ToListAsync();
        
        return templates
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioTemplate> GetByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        var templates = _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId &&
                       b.Mese == mese &&
                       b.Anno == anno)
            .ToList();
        
        return templates
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    /// <summary>
    /// Elimina i template per cliente, periodo E descrizione
    /// </summary>
    public async Task<int> DeleteByClienteAndPeriodoAndDescrizioneAsync(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        var templatesDaEliminare = await _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId &&
                       b.Mese == mese &&
                       b.Anno == anno)
            .ToListAsync();

        var toDelete = templatesDaEliminare
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();

        _context.BilancioTemplate.RemoveRange(toDelete);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        var templatesDaEliminare = _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId &&
                       b.Mese == mese &&
                       b.Anno == anno)
            .ToList();

        var toDelete = templatesDaEliminare
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();

        _context.BilancioTemplate.RemoveRange(toDelete);
        return _context.SaveChanges();
    }

    public async Task<int> InsertAsync(BilancioTemplate bilancio)
    {
        await _context.BilancioTemplate.AddAsync(bilancio);
        await _context.SaveChangesAsync();
        return bilancio.Id;
    }
    
    // Metodo sincrono per compatibilità
    public int Insert(BilancioTemplate bilancio)
    {
        _context.BilancioTemplate.Add(bilancio);
        _context.SaveChanges();
        return bilancio.Id;
    }

    public async Task InsertBulkAsync(IEnumerable<BilancioTemplate> bilanci)
    {
        foreach (var bilancio in bilanci)
        {
            await _context.BilancioTemplate.AddAsync(bilancio);
            System.Diagnostics.Debug.WriteLine($"[INSERT TEMPLATE] Aggiunto Codice={bilancio.CodiceMastrino}, Desc='{bilancio.DescrizioneBilancio ?? "(vuota)"}'");
        }
        await _context.SaveChangesAsync();
        
        // Log dopo il salvataggio per verificare gli ID
        foreach (var bilancio in bilanci)
        {
            System.Diagnostics.Debug.WriteLine($"[INSERT TEMPLATE] Salvato ID={bilancio.Id}");
        }
    }
    
    // Metodo sincrono per compatibilità
    public void InsertBulk(IEnumerable<BilancioTemplate> bilanci)
    {
        foreach (var bilancio in bilanci)
        {
            _context.BilancioTemplate.Add(bilancio);
            System.Diagnostics.Debug.WriteLine($"[INSERT TEMPLATE] Aggiunto Codice={bilancio.CodiceMastrino}, Desc='{bilancio.DescrizioneBilancio ?? "(vuota)"}'");
        }
        _context.SaveChanges();
        
        foreach (var bilancio in bilanci)
        {
            System.Diagnostics.Debug.WriteLine($"[INSERT TEMPLATE] Salvato ID={bilancio.Id}");
        }
    }

    public async Task<bool> UpdateAsync(BilancioTemplate bilancio)
    {
        try
        {
            _context.BilancioTemplate.Update(bilancio);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // Metodo sincrono per compatibilità
    public bool Update(BilancioTemplate bilancio)
    {
        try
        {
            _context.BilancioTemplate.Update(bilancio);
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
            var entity = await _context.BilancioTemplate.FindAsync(id);
            if (entity == null) return false;
            
            _context.BilancioTemplate.Remove(entity);
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
            var entity = _context.BilancioTemplate.Find(id);
            if (entity == null) return false;
            
            _context.BilancioTemplate.Remove(entity);
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
        var entities = await _context.BilancioTemplate
            .Where(b => ids.Contains(b.Id))
            .ToListAsync();
        
        _context.BilancioTemplate.RemoveRange(entities);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteMultiple(IEnumerable<int> ids)
    {
        var entities = _context.BilancioTemplate
            .Where(b => ids.Contains(b.Id))
            .ToList();
        
        _context.BilancioTemplate.RemoveRange(entities);
        return _context.SaveChanges();
    }

    public async Task<int> DeleteByClienteAndPeriodoAsync(int clienteId, int mese, int anno)
    {
        var entities = await _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToListAsync();
        
        _context.BilancioTemplate.RemoveRange(entities);
        return await _context.SaveChangesAsync();
    }
    
    // Metodo sincrono per compatibilità
    public int DeleteByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        var entities = _context.BilancioTemplate
            .Where(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToList();
        
        _context.BilancioTemplate.RemoveRange(entities);
        return _context.SaveChanges();
    }

    public async Task<List<int>> GetDistinctAnniAsync()
    {
        return await _context.BilancioTemplate
            .Select(b => b.Anno)
            .Distinct()
            .OrderByDescending(a => a)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<int> GetDistinctAnni()
    {
        return _context.BilancioTemplate
            .Select(b => b.Anno)
            .Distinct()
            .OrderByDescending(a => a)
            .ToList();
    }

    public async Task<List<string>> GetDistinctCodiciMastrinoAsync()
    {
        return await _context.BilancioTemplate
            .Select(b => b.CodiceMastrino)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }
    
    // Metodo sincrono per compatibilità
    public List<string> GetDistinctCodiciMastrino()
    {
        return _context.BilancioTemplate
            .Select(b => b.CodiceMastrino)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public async Task<List<BilancioGruppo>> GetGruppiAsync()
    {
        var allBilanci = await _context.BilancioTemplate.ToListAsync();
        
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Totale righe nel DB: {allBilanci.Count}");
        var descrizioniUniche = allBilanci.Select(b => b.DescrizioneBilancio ?? "(vuota)").Distinct().ToList();
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Descrizioni uniche: {string.Join(", ", descrizioniUniche)}");
        
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
                
                System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Gruppo: Cliente={primaRiga.ClienteNome}, Periodo={g.Key.Mese}/{g.Key.Anno}, Desc='{g.Key.Descrizione}', Righe={g.Count()}");
                
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
        
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Gruppi creati: {gruppi.Count}");
        return gruppi;
    }
    
    // Metodo sincrono per compatibilità
    public List<BilancioGruppo> GetGruppi()
    {
        var allBilanci = _context.BilancioTemplate.ToList();
        
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Totale righe nel DB: {allBilanci.Count}");
        var descrizioniUniche = allBilanci.Select(b => b.DescrizioneBilancio ?? "(vuota)").Distinct().ToList();
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Descrizioni uniche: {string.Join(", ", descrizioniUniche)}");
        
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
                
                System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Gruppo: Cliente={primaRiga.ClienteNome}, Periodo={g.Key.Mese}/{g.Key.Anno}, Desc='{g.Key.Descrizione}', Righe={g.Count()}");
                
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
        
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Gruppi creati: {gruppi.Count}");
        return gruppi;
    }
}
