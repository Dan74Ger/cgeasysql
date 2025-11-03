using System.Collections.Generic;
using System.Linq;
using CGEasy.Core.Data;
using CGEasy.Core.Models;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Argomenti (categorie circolari)
    /// Pattern Shared mode per multi-client
    /// </summary>
    public class ArgomentiRepository
    {
        private readonly LiteDbContext _context;

        public ArgomentiRepository(LiteDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ottiene tutti gli argomenti ordinati per nome
        /// </summary>
        public List<Argomento> GetAll()
        {
            return _context.Argomenti
                .FindAll()
                .OrderBy(x => x.Nome)
                .ToList(); // Materializza per evitare conflitti reader in Shared mode
        }

        /// <summary>
        /// Ottiene un argomento per ID
        /// </summary>
        public Argomento? GetById(int id)
        {
            return _context.Argomenti.FindById(id);
        }

        /// <summary>
        /// Cerca argomenti per nome (case-insensitive)
        /// </summary>
        public List<Argomento> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            var term = searchTerm.ToLowerInvariant();
            return _context.Argomenti
                .Find(x => x.Nome.ToLower().Contains(term) || 
                           (x.Descrizione != null && x.Descrizione.ToLower().Contains(term)))
                .OrderBy(x => x.Nome)
                .ToList();
        }

        /// <summary>
        /// Inserisce nuovo argomento
        /// </summary>
        public int Insert(Argomento argomento)
        {
            return _context.Argomenti.Insert(argomento);
        }

        /// <summary>
        /// Aggiorna argomento esistente
        /// </summary>
        public bool Update(Argomento argomento)
        {
            return _context.Argomenti.Update(argomento);
        }

        /// <summary>
        /// Elimina argomento per ID
        /// </summary>
        public bool Delete(int id)
        {
            return _context.Argomenti.Delete(id);
        }

        /// <summary>
        /// Verifica se esistono circolari associate all'argomento
        /// </summary>
        public bool HasCircolariAssociate(int argomentoId)
        {
            return _context.Circolari.Exists(c => c.ArgomentoId == argomentoId);
        }

        /// <summary>
        /// Conta circolari per argomento
        /// </summary>
        public int CountCircolari(int argomentoId)
        {
            return _context.Circolari.Count(c => c.ArgomentoId == argomentoId);
        }
    }
}

