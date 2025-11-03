using System;
using System.Collections.Generic;
using System.Linq;
using CGEasy.Core.Data;
using CGEasy.Core.Models;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Repository per gestione Circolari
    /// Pattern Shared mode per multi-client
    /// </summary>
    public class CircolariRepository
    {
        private readonly LiteDbContext _context;

        public CircolariRepository(LiteDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ottiene tutte le circolari con nome argomento (join manuale)
        /// </summary>
        public List<Circolare> GetAll()
        {
            var circolari = _context.Circolari
                .FindAll()
                .OrderByDescending(x => x.DataImportazione)
                .ToList();

            // Carica argomenti per join
            var argomenti = _context.Argomenti.FindAll().ToDictionary(a => a.Id, a => a.Nome);

            foreach (var circolare in circolari)
            {
                if (argomenti.ContainsKey(circolare.ArgomentoId))
                    circolare.ArgomentoNome = argomenti[circolare.ArgomentoId];
            }

            return circolari;
        }

        /// <summary>
        /// Ottiene circolare per ID
        /// </summary>
        public Circolare? GetById(int id)
        {
            var circolare = _context.Circolari.FindById(id);
            if (circolare != null)
            {
                var argomento = _context.Argomenti.FindById(circolare.ArgomentoId);
                circolare.ArgomentoNome = argomento?.Nome;
            }
            return circolare;
        }

        /// <summary>
        /// Ricerca circolari con filtri multipli
        /// </summary>
        public List<Circolare> Search(int? argomentoId = null, int? anno = null, string? searchTerm = null)
        {
            var query = _context.Circolari.Query();

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

            var circolari = query.ToList();

            // Filtra per testo (case-insensitive) dopo materializzazione
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLowerInvariant();
                circolari = circolari
                    .Where(c => c.Descrizione.ToLower().Contains(term) || 
                                c.NomeFile.ToLower().Contains(term))
                    .ToList();
            }

            // Carica argomenti per join
            var argomenti = _context.Argomenti.FindAll().ToDictionary(a => a.Id, a => a.Nome);

            foreach (var circolare in circolari)
            {
                if (argomenti.ContainsKey(circolare.ArgomentoId))
                    circolare.ArgomentoNome = argomenti[circolare.ArgomentoId];
            }

            return circolari.OrderByDescending(x => x.DataImportazione).ToList();
        }

        /// <summary>
        /// Ottiene tutti gli anni distinti presenti
        /// </summary>
        public List<int> GetAnniDistinti()
        {
            return _context.Circolari
                .FindAll()
                .Select(c => c.Anno)
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();
        }

        /// <summary>
        /// Inserisce nuova circolare
        /// </summary>
        public int Insert(Circolare circolare)
        {
            return _context.Circolari.Insert(circolare);
        }

        /// <summary>
        /// Aggiorna circolare esistente
        /// </summary>
        public bool Update(Circolare circolare)
        {
            return _context.Circolari.Update(circolare);
        }

        /// <summary>
        /// Elimina circolare per ID
        /// </summary>
        public bool Delete(int id)
        {
            return _context.Circolari.Delete(id);
        }

        /// <summary>
        /// Elimina tutte le circolari di un argomento (batch)
        /// </summary>
        public int DeleteByArgomento(int argomentoId)
        {
            return _context.Circolari.DeleteMany(c => c.ArgomentoId == argomentoId);
        }

        /// <summary>
        /// Conta circolari totali
        /// </summary>
        public int Count()
        {
            return _context.Circolari.Count();
        }
    }
}

