using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CGEasy.Core.Repositories
{
    /// <summary>
    /// Interface generica per Repository Pattern
    /// </summary>
    /// <typeparam name="T">Tipo entità</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Ottiene tutte le entità
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Ottiene entità per ID
        /// </summary>
        T? GetById(int id);

        /// <summary>
        /// Trova entità con filtro
        /// </summary>
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Trova una singola entità
        /// </summary>
        T? FindOne(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Inserisce nuova entità
        /// </summary>
        int Insert(T entity);

        /// <summary>
        /// Aggiorna entità esistente
        /// </summary>
        bool Update(T entity);

        /// <summary>
        /// Elimina entità per ID
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Conta entità
        /// </summary>
        int Count();

        /// <summary>
        /// Conta entità con filtro
        /// </summary>
        int Count(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Verifica se entità esiste
        /// </summary>
        bool Exists(Expression<Func<T, bool>> predicate);
    }
}






























