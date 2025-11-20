using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Professionisti (EF Core async)
    /// </summary>
    public class ProfessionistaRepository
    {
        private readonly CGEasyDbContext _context;

        public ProfessionistaRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        // ===== METODI ASYNC EF CORE =====

        public async Task<List<Professionista>> GetAllAsync()
        {
            return await _context.Professionisti
                .AsNoTracking()
                .OrderBy(p => p.Cognome)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        public async Task<Professionista?> GetByIdAsync(int id)
        {
            return await _context.Professionisti.FindAsync(id);
        }

        public async Task<List<Professionista>> FindAsync(Expression<Func<Professionista, bool>> predicate)
        {
            return await _context.Professionisti
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Professionista?> FindOneAsync(Expression<Func<Professionista, bool>> predicate)
        {
            return await _context.Professionisti
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<int> InsertAsync(Professionista entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataAttivazione = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            _context.Professionisti.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Professionista entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            _context.Professionisti.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            _context.Professionisti.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Professionisti.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Professionista, bool>> predicate)
        {
            return await _context.Professionisti.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Professionista, bool>> predicate)
        {
            return await _context.Professionisti.AnyAsync(predicate);
        }

        /// <summary>
        /// Ottiene solo professionisti attivi
        /// </summary>
        public async Task<List<Professionista>> GetActiveAsync()
        {
            return await _context.Professionisti
                .Where(p => p.Attivo)
                .OrderBy(p => p.Cognome)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        /// <summary>
        /// Disattiva professionista (soft delete)
        /// </summary>
        public async Task<bool> DeactivateAsync(int id)
        {
            var prof = await GetByIdAsync(id);
            if (prof == null)
                return false;

            prof.Attivo = false;
            prof.DataCessazione = DateTime.UtcNow;
            prof.DataModifica = DateTime.UtcNow;

            return await UpdateAsync(prof);
        }

        /// <summary>
        /// Riattiva professionista
        /// </summary>
        public async Task<bool> ActivateAsync(int id)
        {
            var prof = await GetByIdAsync(id);
            if (prof == null)
                return false;

            prof.Attivo = true;
            prof.DataCessazione = null;
            prof.DataModifica = DateTime.UtcNow;

            return await UpdateAsync(prof);
        }

        /// <summary>
        /// Cerca professionisti per nome/cognome (case-insensitive)
        /// </summary>
        public async Task<List<Professionista>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lower = searchTerm.ToLower();
            return await _context.Professionisti
                .Where(p => 
                    p.Nome.ToLower().Contains(lower) || 
                    p.Cognome.ToLower().Contains(lower))
                .OrderBy(p => p.Cognome)
                .ThenBy(p => p.Nome)
                .ToListAsync();
        }

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public List<Professionista> GetAll() => GetAllAsync().GetAwaiter().GetResult();
        public Professionista? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();
        public int Insert(Professionista entity) => InsertAsync(entity).GetAwaiter().GetResult();
        public bool Update(Professionista entity) => UpdateAsync(entity).GetAwaiter().GetResult();
        public bool Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
        public int Count() => CountAsync().GetAwaiter().GetResult();
        public List<Professionista> GetActive() => GetActiveAsync().GetAwaiter().GetResult();
        public List<Professionista> SearchByName(string searchTerm) => SearchByNameAsync(searchTerm).GetAwaiter().GetResult();
    }
}
