using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Professionisti
    /// </summary>
    public class ProfessionistaRepository : IRepository<Professionista>
    {
        private readonly LiteDbContext _context;

        public ProfessionistaRepository(LiteDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Professionista> GetAll()
        {
            return _context.Professionisti.FindAll();
        }

        public Professionista? GetById(int id)
        {
            return _context.Professionisti.FindById(id);
        }

        public IEnumerable<Professionista> Find(Expression<Func<Professionista, bool>> predicate)
        {
            return _context.Professionisti.Find(predicate);
        }

        public Professionista? FindOne(Expression<Func<Professionista, bool>> predicate)
        {
            return _context.Professionisti.FindOne(predicate);
        }

        public int Insert(Professionista entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataAttivazione = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            return _context.Professionisti.Insert(entity);
        }

        public bool Update(Professionista entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            return _context.Professionisti.Update(entity);
        }

        public bool Delete(int id)
        {
            return _context.Professionisti.Delete(id);
        }

        public int Count()
        {
            return _context.Professionisti.Count();
        }

        public int Count(Expression<Func<Professionista, bool>> predicate)
        {
            return _context.Professionisti.Count(predicate);
        }

        public bool Exists(Expression<Func<Professionista, bool>> predicate)
        {
            return _context.Professionisti.Exists(predicate);
        }

        /// <summary>
        /// Ottiene solo professionisti attivi
        /// </summary>
        public IEnumerable<Professionista> GetActive()
        {
            return _context.Professionisti.Find(p => p.Attivo);
        }

        /// <summary>
        /// Disattiva professionista (soft delete)
        /// </summary>
        public bool Deactivate(int id)
        {
            var prof = GetById(id);
            if (prof == null)
                return false;

            prof.Attivo = false;
            prof.DataCessazione = DateTime.UtcNow;
            prof.DataModifica = DateTime.UtcNow;

            return Update(prof);
        }

        /// <summary>
        /// Riattiva professionista
        /// </summary>
        public bool Activate(int id)
        {
            var prof = GetById(id);
            if (prof == null)
                return false;

            prof.Attivo = true;
            prof.DataCessazione = null;
            prof.DataModifica = DateTime.UtcNow;

            return Update(prof);
        }

        /// <summary>
        /// Cerca professionisti per nome/cognome (case-insensitive)
        /// </summary>
        public IEnumerable<Professionista> SearchByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            var lower = searchTerm.ToLower();
            return _context.Professionisti.Find(p => 
                p.Nome.ToLower().Contains(lower) || 
                p.Cognome.ToLower().Contains(lower));
        }
    }
}






