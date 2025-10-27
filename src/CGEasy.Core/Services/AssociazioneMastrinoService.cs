using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Services;

/// <summary>
/// Servizio per gestione Associazioni Mastrini
/// </summary>
public class AssociazioneMastrinoService
{
    private readonly AssociazioneMastrinoRepository _repository;
    private readonly BilancioContabileRepository _bilancioRepository;
    private readonly BilancioTemplateRepository _templateRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly AuditLogService _auditService;

    public AssociazioneMastrinoService(
        LiteDbContext context,
        AuditLogService auditService)
    {
        _repository = new AssociazioneMastrinoRepository(context);
        _bilancioRepository = new BilancioContabileRepository(context);
        _templateRepository = new BilancioTemplateRepository(context);
        _clienteRepository = new ClienteRepository(context);
        _auditService = auditService;
    }

    /// <summary>
    /// Ottiene tutte le associazioni
    /// </summary>
    public IEnumerable<AssociazioneMastrino> GetAll()
    {
        return _repository.GetAll().OrderByDescending(x => x.DataCreazione);
    }

    /// <summary>
    /// Ottiene associazione per ID
    /// </summary>
    public AssociazioneMastrino? GetById(int id)
    {
        return _repository.GetById(id);
    }

    /// <summary>
    /// Ottiene associazioni per cliente
    /// </summary>
    public IEnumerable<AssociazioneMastrino> GetByCliente(int clienteId)
    {
        return _repository.GetByCliente(clienteId).OrderByDescending(x => x.Anno).ThenByDescending(x => x.Mese);
    }

    /// <summary>
    /// Ottiene bilanci disponibili per un cliente
    /// </summary>
    public IEnumerable<BilancioGruppo> GetBilanciDisponibili(int clienteId)
    {
        return _bilancioRepository.GetGruppiByCliente(clienteId);
    }

    /// <summary>
    /// Ottiene template disponibili per un cliente
    /// </summary>
    public IEnumerable<BilancioTemplate> GetTemplatesDisponibili(int clienteId)
    {
        return _templateRepository.GetByCliente(clienteId);
    }

    /// <summary>
    /// Ottiene voci template base (senza formule) per un template
    /// </summary>
    public IEnumerable<BilancioTemplate> GetVociTemplateBase(int clienteId, int mese, int anno)
    {
        return _templateRepository.GetByClienteAndPeriodo(clienteId, mese, anno)
            .Where(x => string.IsNullOrWhiteSpace(x.Formula))
            .OrderBy(x => x.CodiceMastrino);
    }

    /// <summary>
    /// Ottiene dettagli di un'associazione
    /// </summary>
    public IEnumerable<AssociazioneMastrinoDettaglio> GetDettagli(int associazioneId)
    {
        return _repository.GetDettagli(associazioneId);
    }

    /// <summary>
    /// Crea nuova associazione
    /// </summary>
    public int CreaAssociazione(AssociazioneMastrino associazione)
    {
        // Verifica se esiste già un'associazione per questo cliente/periodo
        var esistente = _repository.GetByClienteAndPeriodo(
            associazione.ClienteId, 
            associazione.Mese, 
            associazione.Anno);

        if (esistente != null)
        {
            throw new InvalidOperationException(
                $"Esiste già un'associazione per {associazione.ClienteNome} - {associazione.PeriodoDisplay}");
        }

        associazione.DataCreazione = DateTime.Now;
        associazione.NumeroMappature = 0;

        var id = _repository.Insert(associazione);

        // Audit log
        _auditService.LogFromSession(
            AuditAction.Create,
            "AssociazioneMastrino",
            id,
            $"Creata associazione: {associazione.DescrizioneCompleta}");

        return id;
    }

    /// <summary>
    /// Aggiorna associazione
    /// </summary>
    public bool AggiornaAssociazione(AssociazioneMastrino associazione)
    {
        var result = _repository.Update(associazione);

        if (result)
        {
            // Audit log
            _auditService.LogFromSession(
                AuditAction.Update,
                "AssociazioneMastrino",
                associazione.Id,
                $"Aggiornata associazione: {associazione.DescrizioneCompleta}");
        }

        return result;
    }

    /// <summary>
    /// Elimina associazione
    /// </summary>
    public bool EliminaAssociazione(int id)
    {
        var associazione = _repository.GetById(id);
        if (associazione == null)
            return false;

        // Elimina prima i dettagli
        _repository.DeleteDettagliByAssociazione(id);

        // Poi elimina la testata
        var result = _repository.Delete(id);

        if (result)
        {
            // Audit log
            _auditService.LogFromSession(
                AuditAction.Delete,
                "AssociazioneMastrino",
                id,
                $"Eliminata associazione: {associazione.DescrizioneCompleta}");
        }

        return result;
    }

    /// <summary>
    /// Carica mastrini del bilancio contabile per mappatura
    /// </summary>
    public List<AssociazioneMastrinoDettaglio> CaricaMastriniDaBilancio(int clienteId, int mese, int anno)
    {
        var bilanci = _bilancioRepository.GetByClienteAndPeriodo(clienteId, mese, anno);
        
        var dettagli = new List<AssociazioneMastrinoDettaglio>();
        
        foreach (var bilancio in bilanci)
        {
            dettagli.Add(new AssociazioneMastrinoDettaglio
            {
                CodiceMastrino = bilancio.CodiceMastrino,
                DescrizioneMastrino = bilancio.DescrizioneMastrino,
                Importo = bilancio.Importo,
                TemplateVoceId = null,
                TemplateCodice = null,
                TemplateDescrizione = null,
                TemplateSegno = null
            });
        }

        return dettagli.OrderBy(x => x.CodiceMastrino).ToList();
    }

    /// <summary>
    /// Salva dettagli associazione
    /// </summary>
    public void SalvaDettagli(int associazioneId, List<AssociazioneMastrinoDettaglio> dettagli)
    {
        // Elimina i dettagli esistenti
        _repository.DeleteDettagliByAssociazione(associazioneId);

        // Inserisce i nuovi dettagli
        foreach (var dettaglio in dettagli)
        {
            dettaglio.AssociazioneId = associazioneId;
            _repository.InsertDettaglio(dettaglio);
        }

        // Aggiorna il numero di mappature nella testata
        var associazione = _repository.GetById(associazioneId);
        if (associazione != null)
        {
            associazione.NumeroMappature = dettagli.Count(x => x.IsAssociato);
            _repository.Update(associazione);
        }

        // Audit log
        _auditService.LogFromSession(
            AuditAction.Update,
            "AssociazioneMastrino",
            associazioneId,
            $"Salvate {dettagli.Count} mappature");
    }

    /// <summary>
    /// Cerca associazioni
    /// </summary>
    public IEnumerable<AssociazioneMastrino> Cerca(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return GetAll();

        var lower = searchTerm.ToLower();
        return _repository.GetAll()
            .Where(x => 
                x.ClienteNome.ToLower().Contains(lower) ||
                (x.Descrizione != null && x.Descrizione.ToLower().Contains(lower)) ||
                (x.TemplatNome != null && x.TemplatNome.ToLower().Contains(lower)))
            .OrderByDescending(x => x.DataCreazione);
    }
}


