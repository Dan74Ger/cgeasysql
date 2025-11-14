using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Helpers;

namespace CGEasy.Core.Repositories;

public class BilancioTemplateRepository
{
    private readonly LiteDbContext _context;

    public BilancioTemplateRepository(LiteDbContext context)
    {
        _context = context;
    }

    public List<BilancioTemplate> GetAll()
    {
        return _context.BilancioTemplate.FindAll().ToList();
    }

    public BilancioTemplate? GetById(int id)
    {
        return _context.BilancioTemplate.FindById(id);
    }

    public List<BilancioTemplate> GetByCliente(int clienteId)
    {
        return _context.BilancioTemplate
            .Find(b => b.ClienteId == clienteId)
            .OrderByDescending(b => b.Anno)
            .ThenByDescending(b => b.Mese)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public List<BilancioTemplate> GetByPeriodo(int mese, int anno)
    {
        return _context.BilancioTemplate
            .Find(b => b.Mese == mese && b.Anno == anno)
            .OrderBy(b => b.ClienteNome)
            .ThenBy(b => b.CodiceMastrino)
            .ToList();
    }

    public List<BilancioTemplate> GetByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        return _context.BilancioTemplate
            .Find(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .OrderByCodiceMastrinoNumerico(b => b.CodiceMastrino)
            .ToList();
    }

    /// <summary>
    /// Ottiene i template per cliente, periodo E descrizione (per supportare più template con stessa data)
    /// </summary>
    public List<BilancioTemplate> GetByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        var templates = _context.BilancioTemplate
            .Find(b => b.ClienteId == clienteId &&
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
    public int DeleteByClienteAndPeriodoAndDescrizione(int clienteId, int mese, int anno, string? descrizione)
    {
        var desc = descrizione?.Trim() ?? "";
        var templatesDaEliminare = _context.BilancioTemplate
            .Find(b => b.ClienteId == clienteId &&
                       b.Mese == mese &&
                       b.Anno == anno)
            .Where(b => (b.DescrizioneBilancio?.Trim() ?? "") == desc)
            .Select(b => b.Id)
            .ToList();

        int count = 0;
        foreach (var id in templatesDaEliminare)
        {
            if (_context.BilancioTemplate.Delete(id))
                count++;
        }

        _context.Checkpoint();
        return count;
    }

    public int Insert(BilancioTemplate bilancio)
    {
        var id = _context.BilancioTemplate.Insert(bilancio);
        _context.Checkpoint();
        return id;
    }

    public void InsertBulk(IEnumerable<BilancioTemplate> bilanci)
    {
        // ⚠️ IMPORTANTE: InsertBulk di LiteDB NON assegna gli ID agli oggetti in memoria!
        // Per garantire ID univoci, usiamo Insert singoli che assegnano l'ID a ogni oggetto
        foreach (var bilancio in bilanci)
        {
            _context.BilancioTemplate.Insert(bilancio);
            System.Diagnostics.Debug.WriteLine($"[INSERT TEMPLATE] Inserito ID={bilancio.Id}, Codice={bilancio.CodiceMastrino}, Desc='{bilancio.DescrizioneBilancio ?? "(vuota)"}'");
        }
        _context.Checkpoint();
    }

    public bool Update(BilancioTemplate bilancio)
    {
        var result = _context.BilancioTemplate.Update(bilancio);
        _context.Checkpoint();
        return result;
    }

    public bool Delete(int id)
    {
        return _context.BilancioTemplate.Delete(id);
    }

    public int DeleteMultiple(IEnumerable<int> ids)
    {
        int count = 0;
        foreach (var id in ids)
        {
            if (_context.BilancioTemplate.Delete(id))
                count++;
        }
        return count;
    }

    public int DeleteByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        // ✅ Usa DeleteMany che è molto più efficiente e non blocca il database
        var deleted = _context.BilancioTemplate.DeleteMany(b => 
            b.ClienteId == clienteId && 
            b.Mese == mese && 
            b.Anno == anno);
        
        _context.Checkpoint(); // Forza il flush su disco
        return deleted;
    }

    public List<int> GetDistinctAnni()
    {
        return _context.BilancioTemplate
            .FindAll()
            .Select(b => b.Anno)
            .Distinct()
            .OrderByDescending(a => a)
            .ToList();
    }

    public List<string> GetDistinctCodiciMastrino()
    {
        return _context.BilancioTemplate
            .FindAll()
            .Select(b => b.CodiceMastrino)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public List<BilancioGruppo> GetGruppi()
    {
        var allBilanci = _context.BilancioTemplate.FindAll().ToList();
        
        // 🔍 DEBUG: Log totale righe e descrizioni uniche
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Totale righe nel DB: {allBilanci.Count}");
        var descrizioniUniche = allBilanci.Select(b => b.DescrizioneBilancio ?? "(vuota)").Distinct().ToList();
        System.Diagnostics.Debug.WriteLine($"[GET GRUPPI TEMPLATE] Descrizioni uniche: {string.Join(", ", descrizioniUniche)}");
        
        // 🎯 IMPORTANTE: Raggruppa per ClienteId, Mese, Anno E DESCRIZIONE
        var gruppi = allBilanci
            .GroupBy(b => new { 
                b.ClienteId, 
                b.Mese, 
                b.Anno,
                Descrizione = b.DescrizioneBilancio ?? "" // Gestisce descrizioni null
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
                    TipoBilancio = primaRiga.TipoBilancio ?? "CE", // ⭐ Popola TipoBilancio
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

