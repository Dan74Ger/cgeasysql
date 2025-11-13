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

    public int Insert(BilancioTemplate bilancio)
    {
        var id = _context.BilancioTemplate.Insert(bilancio);
        _context.Checkpoint();
        return id;
    }

    public void InsertBulk(IEnumerable<BilancioTemplate> bilanci)
    {
        _context.BilancioTemplate.InsertBulk(bilanci);
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
        
        var gruppi = allBilanci
            .GroupBy(b => new { b.ClienteId, b.Mese, b.Anno })
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
            .ThenBy(g => g.ClienteNome)
            .ToList();
        
        return gruppi;
    }
}

