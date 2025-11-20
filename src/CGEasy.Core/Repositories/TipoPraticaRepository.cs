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
    /// Repository per gestione Tipo Pratiche (EF Core async)
    /// </summary>
    public class TipoPraticaRepository
    {
        private readonly CGEasyDbContext _context;

        public TipoPraticaRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        // ===== METODI ASYNC EF CORE =====

        public async Task<List<TipoPratica>> GetAllAsync()
        {
            return await _context.TipoPratiche
                .AsNoTracking()
                .OrderBy(p => p.Ordine)
                .ThenBy(p => p.NomePratica)
                .ToListAsync();
        }

        public async Task<TipoPratica?> GetByIdAsync(int id)
        {
            return await _context.TipoPratiche.FindAsync(id);
        }

        public async Task<List<TipoPratica>> FindAsync(Expression<Func<TipoPratica, bool>> predicate)
        {
            return await _context.TipoPratiche
                .Where(predicate)
                .OrderBy(p => p.Ordine)
                .ToListAsync();
        }

        public async Task<TipoPratica?> FindOneAsync(Expression<Func<TipoPratica, bool>> predicate)
        {
            return await _context.TipoPratiche
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<int> InsertAsync(TipoPratica entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Auto-incrementa ordine se non specificato
            if (entity.Ordine == 0)
            {
                var maxOrdine = await _context.TipoPratiche.MaxAsync(x => (int?)x.Ordine);
                entity.Ordine = (maxOrdine ?? 0) + 1;
            }
            
            _context.TipoPratiche.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(TipoPratica entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.TipoPratiche.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            _context.TipoPratiche.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.TipoPratiche.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TipoPratica, bool>> predicate)
        {
            return await _context.TipoPratiche.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<TipoPratica, bool>> predicate)
        {
            return await _context.TipoPratiche.AnyAsync(predicate);
        }

        /// <summary>
        /// Ottiene solo pratiche attive
        /// </summary>
        public async Task<List<TipoPratica>> GetActiveAsync()
        {
            return await _context.TipoPratiche
                .Where(p => p.Attivo)
                .OrderBy(p => p.Ordine)
                .ThenBy(p => p.NomePratica)
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene pratiche per categoria
        /// </summary>
        public async Task<List<TipoPratica>> GetByCategoriaAsync(CategoriaPratica categoria)
        {
            return await _context.TipoPratiche
                .Where(p => p.Categoria == categoria && p.Attivo)
                .OrderBy(p => p.Ordine)
                .ToListAsync();
        }

        /// <summary>
        /// Cerca pratiche per nome (case-insensitive)
        /// </summary>
        public async Task<List<TipoPratica>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lower = searchTerm.ToLower();
            return await _context.TipoPratiche
                .Where(p => 
                    p.NomePratica.ToLower().Contains(lower) ||
                    (p.Descrizione != null && p.Descrizione.ToLower().Contains(lower)))
                .OrderBy(p => p.Ordine)
                .ToListAsync();
        }

        /// <summary>
        /// Riordina pratiche
        /// </summary>
        public async Task ReorderAsync(int tipoPraticaId, int newOrdine)
        {
            var pratica = await GetByIdAsync(tipoPraticaId);
            if (pratica == null)
                return;

            pratica.Ordine = newOrdine;
            await UpdateAsync(pratica);
        }

        /// <summary>
        /// Disattiva pratica (soft delete)
        /// </summary>
        public async Task<bool> DeactivateAsync(int id)
        {
            var pratica = await GetByIdAsync(id);
            if (pratica == null)
                return false;

            pratica.Attivo = false;
            pratica.UpdatedAt = DateTime.UtcNow;

            return await UpdateAsync(pratica);
        }

        /// <summary>
        /// Attiva pratica
        /// </summary>
        public async Task<bool> ActivateAsync(int id)
        {
            var pratica = await GetByIdAsync(id);
            if (pratica == null)
                return false;

            pratica.Attivo = true;
            pratica.UpdatedAt = DateTime.UtcNow;

            return await UpdateAsync(pratica);
        }

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public List<TipoPratica> GetAll() => GetAllAsync().GetAwaiter().GetResult();
        public TipoPratica? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();
        public int Insert(TipoPratica entity) => InsertAsync(entity).GetAwaiter().GetResult();
        public bool Update(TipoPratica entity) => UpdateAsync(entity).GetAwaiter().GetResult();
        public bool Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
        public int Count() => CountAsync().GetAwaiter().GetResult();
        public List<TipoPratica> GetActive() => GetActiveAsync().GetAwaiter().GetResult();
        public List<TipoPratica> SearchByName(string searchTerm) => SearchByNameAsync(searchTerm).GetAwaiter().GetResult();
    }
}
