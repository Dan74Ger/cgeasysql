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
    /// Repository per gestione Clienti (EF Core async)
    /// </summary>
    public class ClienteRepository
    {
        private readonly CGEasyDbContext _context;

        public ClienteRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        // ===== METODI ASYNC EF CORE =====

        public async Task<List<Cliente>> GetAllAsync()
        {
            return await _context.Clienti
                .AsNoTracking()
                .OrderBy(c => c.NomeCliente)
                .ToListAsync();
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clienti.FindAsync(id);
        }

        public async Task<List<Cliente>> FindAsync(Expression<Func<Cliente, bool>> predicate)
        {
            return await _context.Clienti
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Cliente?> FindOneAsync(Expression<Func<Cliente, bool>> predicate)
        {
            return await _context.Clienti
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<int> InsertAsync(Cliente entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataAttivazione = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            _context.Clienti.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(Cliente entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            _context.Clienti.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            
            _context.Clienti.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Clienti.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Cliente, bool>> predicate)
        {
            return await _context.Clienti.CountAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Cliente, bool>> predicate)
        {
            return await _context.Clienti.AnyAsync(predicate);
        }

        /// <summary>
        /// Ottiene solo clienti attivi
        /// </summary>
        public async Task<List<Cliente>> GetActiveAsync()
        {
            return await _context.Clienti
                .Where(c => c.Attivo)
                .OrderBy(c => c.NomeCliente)
                .ToListAsync();
        }

        /// <summary>
        /// Disattiva cliente (soft delete)
        /// </summary>
        public async Task<bool> DeactivateAsync(int id)
        {
            var cliente = await GetByIdAsync(id);
            if (cliente == null)
                return false;

            cliente.Attivo = false;
            cliente.DataCessazione = DateTime.UtcNow;
            cliente.DataModifica = DateTime.UtcNow;

            return await UpdateAsync(cliente);
        }

        /// <summary>
        /// Riattiva cliente
        /// </summary>
        public async Task<bool> ActivateAsync(int id)
        {
            var cliente = await GetByIdAsync(id);
            if (cliente == null)
                return false;

            cliente.Attivo = true;
            cliente.DataCessazione = null;
            cliente.DataModifica = DateTime.UtcNow;

            return await UpdateAsync(cliente);
        }

        /// <summary>
        /// Cerca clienti per nome (case-insensitive)
        /// </summary>
        public async Task<List<Cliente>> SearchByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lower = searchTerm.ToLower();
            return await _context.Clienti
                .Where(c => c.NomeCliente.ToLower().Contains(lower))
                .OrderBy(c => c.NomeCliente)
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene clienti per professionista
        /// </summary>
        public async Task<List<Cliente>> GetByProfessionistaAsync(int professionistaId)
        {
            return await _context.Clienti
                .Where(c => c.IdProfessionista == professionistaId)
                .OrderBy(c => c.NomeCliente)
                .ToListAsync();
        }

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public List<Cliente> GetAll() => GetAllAsync().GetAwaiter().GetResult();
        public Cliente? GetById(int id) => GetByIdAsync(id).GetAwaiter().GetResult();
        public int Insert(Cliente entity) => InsertAsync(entity).GetAwaiter().GetResult();
        public bool Update(Cliente entity) => UpdateAsync(entity).GetAwaiter().GetResult();
        public bool Delete(int id) => DeleteAsync(id).GetAwaiter().GetResult();
        public int Count() => CountAsync().GetAwaiter().GetResult();
        public List<Cliente> GetActive() => GetActiveAsync().GetAwaiter().GetResult();
        public List<Cliente> SearchByName(string searchTerm) => SearchByNameAsync(searchTerm).GetAwaiter().GetResult();
        public List<Cliente> GetByProfessionista(int professionistaId) => GetByProfessionistaAsync(professionistaId).GetAwaiter().GetResult();
    }
}
