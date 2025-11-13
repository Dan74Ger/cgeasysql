using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Repositories;

public class StatisticaSPSalvataRepository
{
    private readonly LiteDbContext _context;

    public StatisticaSPSalvataRepository(LiteDbContext context)
    {
        _context = context;
    }

    public List<StatisticaSPSalvata> GetAll()
    {
        return _context.StatisticheSPSalvate.FindAll().OrderByDescending(s => s.DataCreazione).ToList();
    }

    public List<StatisticaSPSalvata> GetByCliente(int clienteId)
    {
        return _context.StatisticheSPSalvate
            .Find(s => s.ClienteId == clienteId)
            .OrderByDescending(s => s.DataCreazione)
            .ToList();
    }

    public StatisticaSPSalvata? GetById(int id)
    {
        return _context.StatisticheSPSalvate.FindById(id);
    }

    public int Insert(StatisticaSPSalvata statistica)
    {
        return _context.StatisticheSPSalvate.Insert(statistica);
    }

    public bool Update(StatisticaSPSalvata statistica)
    {
        return _context.StatisticheSPSalvate.Update(statistica);
    }

    public bool Delete(int id)
    {
        return _context.StatisticheSPSalvate.Delete(id);
    }

    public int DeleteByCliente(int clienteId)
    {
        var ids = _context.StatisticheSPSalvate
            .Find(s => s.ClienteId == clienteId)
            .Select(s => s.Id)
            .ToList();

        int count = 0;
        foreach (var id in ids)
        {
            if (_context.StatisticheSPSalvate.Delete(id))
                count++;
        }
        return count;
    }
}

