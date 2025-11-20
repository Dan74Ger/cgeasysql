using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories
{
    public class StatisticaCESalvataRepository
    {
        private readonly CGEasyDbContext _context;

        public StatisticaCESalvataRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        public async Task<List<StatisticaCESalvata>> GetAllAsync()
        {
            return await _context.StatisticheCESalvate
                .OrderByDescending(s => s.DataCreazione)
                .ToListAsync();
        }
        
        // Metodo sincrono per compatibilità
        public List<StatisticaCESalvata> GetAll()
        {
            return _context.StatisticheCESalvate
                .OrderByDescending(s => s.DataCreazione)
                .ToList();
        }

        public async Task<StatisticaCESalvata?> GetByIdAsync(int id)
        {
            return await _context.StatisticheCESalvate.FindAsync(id);
        }
        
        // Metodo sincrono per compatibilità
        public StatisticaCESalvata? GetById(int id)
        {
            return _context.StatisticheCESalvate.Find(id);
        }

        public async Task<int> InsertAsync(StatisticaCESalvata statistica)
        {
            await _context.StatisticheCESalvate.AddAsync(statistica);
            await _context.SaveChangesAsync();
            return statistica.Id;
        }
        
        // Metodo sincrono per compatibilità
        public void Insert(StatisticaCESalvata statistica)
        {
            _context.StatisticheCESalvate.Add(statistica);
            _context.SaveChanges();
        }

        public async Task<bool> UpdateAsync(StatisticaCESalvata statistica)
        {
            try
            {
                _context.StatisticheCESalvate.Update(statistica);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // Metodo sincrono per compatibilità
        public bool Update(StatisticaCESalvata statistica)
        {
            try
            {
                _context.StatisticheCESalvate.Update(statistica);
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
                var entity = await _context.StatisticheCESalvate.FindAsync(id);
                if (entity == null) return false;
                
                _context.StatisticheCESalvate.Remove(entity);
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
                var entity = _context.StatisticheCESalvate.Find(id);
                if (entity == null) return false;
                
                _context.StatisticheCESalvate.Remove(entity);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
