using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CGEasy.Core.Data;
using CGEasy.Core.Models;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Argomenti (EF Core async)
    /// </summary>
    public class ArgomentiRepository
    {
        private readonly CGEasyDbContext _context;

        public ArgomentiRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Argomento>> GetAllAsync()
        {
            return await _context.Argomenti
                .AsNoTracking()
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public async Task<Argomento?> GetByIdAsync(int id)
        {
            return await _context.Argomenti.FindAsync(id);
        }

        public async Task<List<Argomento>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var term = searchTerm.ToLowerInvariant();
            return await _context.Argomenti
                .Where(x => x.Nome.ToLower().Contains(term) || 
                           (x.Descrizione != null && x.Descrizione.ToLower().Contains(term)))
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public async Task<int> InsertAsync(Argomento argomento)
        {
            _context.Argomenti.Add(argomento);
            await _context.SaveChangesAsync();
            return argomento.Id;
        }

        public async Task<bool> UpdateAsync(Argomento argomento)
        {
            _context.Argomenti.Update(argomento);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            _context.Argomenti.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> HasCircolariAssociateAsync(int argomentoId)
        {
            return await _context.Circolari.AnyAsync(c => c.ArgomentoId == argomentoId);
        }

        public async Task<int> CountCircolariAsync(int argomentoId)
        {
            return await _context.Circolari.CountAsync(c => c.ArgomentoId == argomentoId);
        }

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public List<Argomento> GetAll() => GetAllAsync().GetAwaiter().GetResult();
        public Argomento? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();
        public int Insert(Argomento argomento) => InsertAsync(argomento).GetAwaiter().GetResult();
        public bool Update(Argomento argomento) => UpdateAsync(argomento).GetAwaiter().GetResult();
        public bool Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
    }
}
