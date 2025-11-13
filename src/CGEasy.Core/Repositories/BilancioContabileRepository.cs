using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Helpers;

namespace CGEasy.Core.Repositories;

public class BilancioContabileRepository
{
    private readonly LiteDbContext _context;

    public BilancioContabileRepository(LiteDbContext context)
    {
        _context = context;
    }

    public List<BilancioContabile> GetAll()
    {
        return _context.BilancioContabile.FindAll().ToList();
    }

    public BilancioContabile? GetById(int id)
    {
        return _context.BilancioContabile.FindById(id);
    }

    public List<BilancioContabile> GetByCliente(int clienteId)
    {
        return _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId)
            .OrderByDescending(b => b.Anno)
            .ThenByDescending(b => b.Mese)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public List<BilancioContabile> GetByPeriodo(int mese, int anno)
    {
        return _context.BilancioContabile
            .Find(b => b.Mese == mese && b.Anno == anno)
            .OrderBy(b => b.ClienteNome)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public List<BilancioContabile> GetByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        return _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    /// <summary>
    /// 🎯 Ottiene bilanci per Cliente+Periodo+DESCRIZIONE specifica
    /// </summary>
    public List<BilancioContabile> GetByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        // Normalizza la descrizione (null = stringa vuota)
        var desc = descrizione?.Trim() ?? "";
        
        // ✅ Prima prendi tutti per cliente+periodo, poi filtra in memoria per descrizione
        // (LiteDB non supporta .Trim() nelle query)
        var bilanci = _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .ToList();
        
        // Filtra in memoria per descrizione normalizzata
        return bilanci
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    public int Insert(BilancioContabile bilancio)
    {
        var id = _context.BilancioContabile.Insert(bilancio);
        _context.Checkpoint(); // Forza scrittura su disco
        return id;
    }

    public void InsertBulk(IEnumerable<BilancioContabile> bilanci)
    {
        _context.BilancioContabile.InsertBulk(bilanci);
        _context.Checkpoint(); // Forza scrittura su disco
    }

    public bool Update(BilancioContabile bilancio)
    {
        var result = _context.BilancioContabile.Update(bilancio);
        _context.Checkpoint(); // Forza scrittura su disco
        return result;
    }

    public bool Delete(int id)
    {
        return _context.BilancioContabile.Delete(id);
    }

    public int DeleteMultiple(IEnumerable<int> ids)
    {
        int count = 0;
        foreach (var id in ids)
        {
            if (_context.BilancioContabile.Delete(id))
                count++;
        }
        return count;
    }

    public int DeleteByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        // ⚠️ DEPRECATO: Non considera la descrizione, usa DeleteByClienteAndPeriodoAndDescrizione
        // ✅ Usa DeleteMany che è molto più efficiente e non blocca il database
        var deleted = _context.BilancioContabile.DeleteMany(b => 
            b.ClienteId == clienteId && 
            b.Mese == mese && 
            b.Anno == anno);
        
        _context.Checkpoint(); // Forza il flush su disco
        return deleted;
    }

    /// <summary>
    /// 🎯 NUOVO METODO: Elimina bilanci per Cliente+Periodo+DESCRIZIONE
    /// La descrizione è il vero identificativo univoco del bilancio!
    /// </summary>
    public int DeleteByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        // Normalizza la descrizione (null = stringa vuota)
        var desc = descrizione?.Trim() ?? "";
        
        // ✅ Prima trova tutti per cliente+periodo, poi filtra in memoria per descrizione
        // (LiteDB non supporta .Trim() nelle query DeleteMany)
        var bilanciDaEliminare = _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId && 
                       b.Mese == mese && 
                       b.Anno == anno)
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .Select(b => b.Id)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"[DELETE] Eliminazione {bilanciDaEliminare.Count} righe per Cliente={clienteId}, Periodo={mese}/{anno}, Descrizione='{desc}'");
        
        // Elimina per ID
        int count = 0;
        foreach (var id in bilanciDaEliminare)
        {
            if (_context.BilancioContabile.Delete(id))
                count++;
        }
        
        _context.Checkpoint(); // Forza il flush su disco
        
        // ⚠️ IMPORTANTE: In modalità Shared, serve un piccolo delay per essere sicuri che la cancellazione sia completata
        if (count > 0)
        {
            System.Threading.Thread.Sleep(200); // 200ms per sicurezza
            System.Diagnostics.Debug.WriteLine($"[DELETE] Eliminazione completata: {count} righe eliminate");
        }
        
        return count;
    }

    public List<int> GetDistinctAnni()
    {
        return _context.BilancioContabile
            .FindAll()
            .Select(b => b.Anno)
            .Distinct()
            .OrderByDescending(a => a)
            .ToList();
    }
    
    /// <summary>
    /// 🐛 DEBUG: Verifica duplicati per un cliente/periodo/descrizione
    /// </summary>
    public Dictionary<string, int> VerificaDuplicati(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        
        var bilanci = _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .ToList();
        
        var duplicati = bilanci
            .GroupBy(b => b.CodiceMastrino)
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.Count());
        
        if (duplicati.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[DUPLICATI] Trovati {duplicati.Count} codici duplicati:");
            foreach (var dup in duplicati)
            {
                System.Diagnostics.Debug.WriteLine($"  - Codice {dup.Key}: {dup.Value} righe");
                var righe = bilanci.Where(b => b.CodiceMastrino == dup.Key).ToList();
                foreach (var riga in righe)
                {
                    System.Diagnostics.Debug.WriteLine($"      ID:{riga.Id}, Desc:'{riga.DescrizioneMastrino}', Importo:{riga.Importo}, DataImport:{riga.DataImport:HH:mm:ss.fff}");
                }
            }
        }
        
        return duplicati;
    }

    public List<string> GetDistinctCodiciMastrino()
    {
        return _context.BilancioContabile
            .FindAll()
            .Select(b => b.CodiceMastrino)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public List<BilancioGruppo> GetGruppi()
    {
        try
        {
            // ✅ IMPORTANTE: Materializza SUBITO con ToList() per evitare conflitti del reader in modalità Shared
            var allBilanci = _context.BilancioContabile.FindAll().ToList();
            
            // 🎯 NUOVA LOGICA: Raggruppa per ClienteId, Mese, Anno E DESCRIZIONE
            // Se importi stesso cliente/periodo con descrizione diversa = bilancio diverso
            var gruppi = allBilanci
                .GroupBy(b => new { 
                    b.ClienteId, 
                    b.Mese, 
                    b.Anno, 
                    Descrizione = b.DescrizioneBilancio ?? "" // Gestisce descrizioni null
                })
                .Select(g =>
                {
                    // Usa i valori della PRIMA riga del gruppo (ordinata per data import più vecchia)
                    var primaRiga = g.OrderBy(x => x.DataImport).First();
                    
                    return new BilancioGruppo
                    {
                        ClienteId = g.Key.ClienteId,
                        ClienteNome = primaRiga.ClienteNome,
                        Mese = g.Key.Mese,
                        Anno = g.Key.Anno,
                        Descrizione = primaRiga.DescrizioneBilancio,
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
            // Log dell'errore per debug
            System.Diagnostics.Debug.WriteLine($"Errore GetGruppi: {ex.Message}");
            throw;
        }
    }

    public List<BilancioGruppo> GetGruppiByCliente(int clienteId)
    {
        try
        {
            // ✅ IMPORTANTE: Materializza SUBITO con ToList() per evitare conflitti del reader in modalità Shared
            var bilanci = _context.BilancioContabile
                .Find(b => b.ClienteId == clienteId)
                .ToList();
            
            // 🎯 NUOVA LOGICA: Raggruppa per ClienteId, Mese, Anno E DESCRIZIONE
            var gruppi = bilanci
                .GroupBy(b => new { 
                    b.ClienteId, 
                    b.Mese, 
                    b.Anno, 
                    Descrizione = b.DescrizioneBilancio ?? "" // Gestisce descrizioni null
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
            // Log dell'errore per debug
            System.Diagnostics.Debug.WriteLine($"Errore GetGruppiByCliente: {ex.Message}");
            throw;
        }
    }
}

