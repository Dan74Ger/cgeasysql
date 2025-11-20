using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CGEasy.Core.Data;
using CGEasy.Core.Models;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Circolari (EF Core async)
    /// </summary>
    public class CircolariRepository
    {
        private readonly CGEasyDbContext _context;

        public CircolariRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        public async Task<List<Circolare>> GetAllAsync()
        {
            var circolari = await _context.Circolari
                .AsNoTracking()
                .OrderByDescending(x => x.DataImportazione)
                .ToListAsync();

            // Carica argomenti per join
            var argomenti = await _context.Argomenti
                .AsNoTracking()
                .ToDictionaryAsync(a => a.Id, a => a.Nome);

            foreach (var circolare in circolari)
            {
                if (argomenti.ContainsKey(circolare.ArgomentoId))
                    circolare.ArgomentoNome = argomenti[circolare.ArgomentoId];
            }

            return circolari;
        }

        public async Task<Circolare?> GetByIdAsync(int id)
        {
            var circolare = await _context.Circolari.FindAsync(id);
            if (circolare != null)
            {
                var argomento = await _context.Argomenti.FindAsync(circolare.ArgomentoId);
                circolare.ArgomentoNome = argomento?.Nome;
            }
            return circolare;
        }

        public async Task<List<Circolare>> SearchAsync(int? argomentoId = null, int? anno = null, string? searchTerm = null)
        {
            var query = _context.Circolari.AsQueryable();

            // Filtra per argomento
            if (argomentoId.HasValue && argomentoId.Value > 0)
            {
                query = query.Where(c => c.ArgomentoId == argomentoId.Value);
            }

            // Filtra per anno
            if (anno.HasValue && anno.Value > 0)
            {
                query = query.Where(c => c.Anno == anno.Value);
            }

            // Filtra per testo
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Descrizione.Contains(searchTerm) || 
                                        c.NomeFile.Contains(searchTerm));
            }

            var circolari = await query.OrderByDescending(x => x.DataImportazione).ToListAsync();

            // Carica argomenti per join
            var argomenti = await _context.Argomenti
                .AsNoTracking()
                .ToDictionaryAsync(a => a.Id, a => a.Nome);

            foreach (var circolare in circolari)
            {
                if (argomenti.ContainsKey(circolare.ArgomentoId))
                    circolare.ArgomentoNome = argomenti[circolare.ArgomentoId];
            }

            return circolari;
        }

        public async Task<List<int>> GetAnniDistintiAsync()
        {
            return await _context.Circolari
                .Select(c => c.Anno)
                .Distinct()
                .OrderByDescending(a => a)
                .ToListAsync();
        }

        public async Task<int> InsertAsync(Circolare circolare)
        {
            _context.Circolari.Add(circolare);
            await _context.SaveChangesAsync();
            return circolare.Id;
        }

        public async Task<bool> UpdateAsync(Circolare circolare)
        {
            _context.Circolari.Update(circolare);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            _context.Circolari.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> DeleteByArgomentoAsync(int argomentoId)
        {
            var circolari = await _context.Circolari
                .Where(c => c.ArgomentoId == argomentoId)
                .ToListAsync();
            _context.Circolari.RemoveRange(circolari);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Circolari.CountAsync();
        }

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public List<Circolare> GetAll() => GetAllAsync().GetAwaiter().GetResult();
        public Circolare? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();
        public int Insert(Circolare circolare) => InsertAsync(circolare).GetAwaiter().GetResult();
        public bool Update(Circolare circolare) => UpdateAsync(circolare).GetAwaiter().GetResult();
        public bool Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
    }
}
