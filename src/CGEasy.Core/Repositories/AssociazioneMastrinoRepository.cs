using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CGEasy.Core.Repositories;

/// <summary>
/// Repository per gestione Associazioni Mastrini
/// </summary>
public class AssociazioneMastrinoRepository : IRepository<AssociazioneMastrino>
{
    private readonly LiteDbContext _context;

    public AssociazioneMastrinoRepository(LiteDbContext context)
    {
        _context = context;
    }

    public IEnumerable<AssociazioneMastrino> GetAll()
    {
        return _context.AssociazioniMastrini.FindAll();
    }

    public AssociazioneMastrino? GetById(int id)
    {
        return _context.AssociazioniMastrini.FindById(id);
    }

    public IEnumerable<AssociazioneMastrino> Find(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.Find(predicate);
    }

    public AssociazioneMastrino? FindOne(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.FindOne(predicate);
    }

    public int Insert(AssociazioneMastrino entity)
    {
        return _context.AssociazioniMastrini.Insert(entity);
    }

    public bool Update(AssociazioneMastrino entity)
    {
        entity.DataModifica = DateTime.Now;
        return _context.AssociazioniMastrini.Update(entity);
    }

    public bool Delete(int id)
    {
        return _context.AssociazioniMastrini.Delete(id);
    }

    public int Count()
    {
        return _context.AssociazioniMastrini.Count();
    }

    public int Count(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.Count(predicate);
    }

    public bool Exists(Expression<Func<AssociazioneMastrino, bool>> predicate)
    {
        return _context.AssociazioniMastrini.Exists(predicate);
    }

    /// <summary>
    /// Ottiene associazioni per cliente
    /// </summary>
    public IEnumerable<AssociazioneMastrino> GetByCliente(int clienteId)
    {
        return _context.AssociazioniMastrini.Find(x => x.ClienteId == clienteId);
    }

    /// <summary>
    /// Ottiene associazione per cliente e periodo
    /// </summary>
    public AssociazioneMastrino? GetByClienteAndPeriodo(int clienteId, int mese, int anno)
    {
        return _context.AssociazioniMastrini.FindOne(x => 
            x.ClienteId == clienteId && 
            x.Mese == mese && 
            x.Anno == anno);
    }

    /// <summary>
    /// Ottiene dettagli associazione
    /// </summary>
    public IEnumerable<AssociazioneMastrinoDettaglio> GetDettagli(int associazioneId)
    {
        return _context.AssociazioniMastriniDettagli.Find(x => x.AssociazioneId == associazioneId);
    }

    /// <summary>
    /// Inserisce dettaglio associazione
    /// </summary>
    public int InsertDettaglio(AssociazioneMastrinoDettaglio dettaglio)
    {
        return _context.AssociazioniMastriniDettagli.Insert(dettaglio);
    }

    /// <summary>
    /// Aggiorna dettaglio associazione
    /// </summary>
    public bool UpdateDettaglio(AssociazioneMastrinoDettaglio dettaglio)
    {
        return _context.AssociazioniMastriniDettagli.Update(dettaglio);
    }

    /// <summary>
    /// Elimina dettaglio associazione
    /// </summary>
    public bool DeleteDettaglio(int dettaglioId)
    {
        return _context.AssociazioniMastriniDettagli.Delete(dettaglioId);
    }

    /// <summary>
    /// Elimina tutti i dettagli di un'associazione
    /// </summary>
    public int DeleteDettagliByAssociazione(int associazioneId)
    {
        var dettagli = GetDettagli(associazioneId);
        int deleted = 0;
        foreach (var dettaglio in dettagli)
        {
            if (DeleteDettaglio(dettaglio.Id))
                deleted++;
        }
        return deleted;
    }

    /// <summary>
    /// Conta dettagli associazione
    /// </summary>
    public int CountDettagli(int associazioneId)
    {
        return _context.AssociazioniMastriniDettagli.Count(x => x.AssociazioneId == associazioneId);
    }
}


