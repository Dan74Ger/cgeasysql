using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Clienti
    /// </summary>
    public class ClienteRepository : IRepository<Cliente>
    {
        private readonly LiteDbContext _context;

        public ClienteRepository(LiteDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Cliente> GetAll()
        {
            return _context.Clienti.FindAll();
        }

        public Cliente? GetById(int id)
        {
            return _context.Clienti.FindById(id);
        }

        public IEnumerable<Cliente> Find(Expression<Func<Cliente, bool>> predicate)
        {
            return _context.Clienti.Find(predicate);
        }

        public Cliente? FindOne(Expression<Func<Cliente, bool>> predicate)
        {
            return _context.Clienti.FindOne(predicate);
        }

        public int Insert(Cliente entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataAttivazione = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            return _context.Clienti.Insert(entity);
        }

        public bool Update(Cliente entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.DataModifica = DateTime.UtcNow;
            
            return _context.Clienti.Update(entity);
        }

        public bool Delete(int id)
        {
            return _context.Clienti.Delete(id);
        }

        public int Count()
        {
            return _context.Clienti.Count();
        }

        public int Count(Expression<Func<Cliente, bool>> predicate)
        {
            return _context.Clienti.Count(predicate);
        }

        public bool Exists(Expression<Func<Cliente, bool>> predicate)
        {
            return _context.Clienti.Exists(predicate);
        }

        /// <summary>
        /// Ottiene solo clienti attivi
        /// </summary>
        public IEnumerable<Cliente> GetActive()
        {
            return _context.Clienti.Find(c => c.Attivo);
        }

        /// <summary>
        /// Disattiva cliente (soft delete)
        /// </summary>
        public bool Deactivate(int id)
        {
            var cliente = GetById(id);
            if (cliente == null)
                return false;

            cliente.Attivo = false;
            cliente.DataCessazione = DateTime.UtcNow;
            cliente.DataModifica = DateTime.UtcNow;

            return Update(cliente);
        }

        /// <summary>
        /// Riattiva cliente
        /// </summary>
        public bool Activate(int id)
        {
            var cliente = GetById(id);
            if (cliente == null)
                return false;

            cliente.Attivo = true;
            cliente.DataCessazione = null;
            cliente.DataModifica = DateTime.UtcNow;

            return Update(cliente);
        }

        /// <summary>
        /// Cerca clienti per nome (case-insensitive)
        /// </summary>
        public IEnumerable<Cliente> SearchByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            var lower = searchTerm.ToLower();
            return _context.Clienti.Find(c => 
                c.NomeCliente.ToLower().Contains(lower));
        }

        /// <summary>
        /// Ottiene clienti per professionista
        /// </summary>
        public IEnumerable<Cliente> GetByProfessionista(int professionistaId)
        {
            return _context.Clienti.Find(c => c.IdProfessionista == professionistaId);
        }
    }
}























