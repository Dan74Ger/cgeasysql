using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Tipo Pratiche
    /// </summary>
    public class TipoPraticaRepository : IRepository<TipoPratica>
    {
        private readonly LiteDbContext _context;

        public TipoPraticaRepository(LiteDbContext context)
        {
            _context = context;
        }

        public IEnumerable<TipoPratica> GetAll()
        {
            return _context.TipoPratiche.FindAll();
        }

        public TipoPratica? GetById(int id)
        {
            return _context.TipoPratiche.FindById(id);
        }

        public IEnumerable<TipoPratica> Find(Expression<Func<TipoPratica, bool>> predicate)
        {
            return _context.TipoPratiche.Find(predicate);
        }

        public TipoPratica? FindOne(Expression<Func<TipoPratica, bool>> predicate)
        {
            return _context.TipoPratiche.FindOne(predicate);
        }

        public int Insert(TipoPratica entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Auto-incrementa ordine se non specificato
            if (entity.Ordine == 0)
            {
                var maxOrdine = _context.TipoPratiche.Max(x => (int?)x.Ordine);
                entity.Ordine = (maxOrdine ?? 0) + 1;
            }
            
            return _context.TipoPratiche.Insert(entity);
        }

        public bool Update(TipoPratica entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            return _context.TipoPratiche.Update(entity);
        }

        public bool Delete(int id)
        {
            return _context.TipoPratiche.Delete(id);
        }

        public int Count()
        {
            return _context.TipoPratiche.Count();
        }

        public int Count(Expression<Func<TipoPratica, bool>> predicate)
        {
            return _context.TipoPratiche.Count(predicate);
        }

        public bool Exists(Expression<Func<TipoPratica, bool>> predicate)
        {
            return _context.TipoPratiche.Exists(predicate);
        }

        /// <summary>
        /// Ottiene solo pratiche attive
        /// </summary>
        public IEnumerable<TipoPratica> GetActive()
        {
            return _context.TipoPratiche.Find(p => p.Attivo);
        }

        /// <summary>
        /// Ottiene pratiche per categoria
        /// </summary>
        public IEnumerable<TipoPratica> GetByCategoria(CategoriaPratica categoria)
        {
            return _context.TipoPratiche.Find(p => p.Categoria == categoria && p.Attivo);
        }

        /// <summary>
        /// Cerca pratiche per nome (case-insensitive)
        /// </summary>
        public IEnumerable<TipoPratica> SearchByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            var lower = searchTerm.ToLower();
            return _context.TipoPratiche.Find(p => 
                p.NomePratica.ToLower().Contains(lower) ||
                (p.Descrizione != null && p.Descrizione.ToLower().Contains(lower)));
        }

        /// <summary>
        /// Riordina pratiche
        /// </summary>
        public void Reorder(int tipoPraticaId, int newOrdine)
        {
            var pratica = GetById(tipoPraticaId);
            if (pratica == null)
                return;

            pratica.Ordine = newOrdine;
            Update(pratica);
        }

        /// <summary>
        /// Disattiva pratica (soft delete)
        /// </summary>
        public bool Deactivate(int id)
        {
            var pratica = GetById(id);
            if (pratica == null)
                return false;

            pratica.Attivo = false;
            pratica.UpdatedAt = DateTime.UtcNow;

            return Update(pratica);
        }

        /// <summary>
        /// Attiva pratica
        /// </summary>
        public bool Activate(int id)
        {
            var pratica = GetById(id);
            if (pratica == null)
                return false;

            pratica.Attivo = true;
            pratica.UpdatedAt = DateTime.UtcNow;

            return Update(pratica);
        }
    }
}

