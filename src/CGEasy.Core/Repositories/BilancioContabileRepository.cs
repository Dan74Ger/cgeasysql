using CGEasy.Core.Data;
using CGEasy.Core.Models;

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
            .OrderBy(b => b.CodiceMastrino)
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
        var toDelete = _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId && b.Mese == mese && b.Anno == anno)
            .Select(b => b.Id)
            .ToList();

        return DeleteMultiple(toDelete);
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
        var allBilanci = _context.BilancioContabile.FindAll().ToList();
        
        // IMPORTANTE: Raggruppa SOLO per ClienteId, Mese, Anno
        // NON raggruppare per DataImport/ImportedByName altrimenti righe aggiunte manualmente
        // creano gruppi separati!
        var gruppi = allBilanci
            .GroupBy(b => new { b.ClienteId, b.Mese, b.Anno })
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

    public List<BilancioGruppo> GetGruppiByCliente(int clienteId)
    {
        var bilanci = _context.BilancioContabile
            .Find(b => b.ClienteId == clienteId)
            .ToList();
        
        var gruppi = bilanci
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
            .ToList();
        
        return gruppi;
    }
}

