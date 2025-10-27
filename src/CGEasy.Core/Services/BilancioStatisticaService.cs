using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Services;

/// <summary>
/// Servizio per generare statistiche bilanci basate su associazioni mastrini
/// </summary>
public class BilancioStatisticaService
{
    private readonly AssociazioneMastrinoRepository _associazioneRepo;
    private readonly BilancioTemplateRepository _templateRepo;
    private readonly BilancioContabileRepository _bilancioRepo;

    public BilancioStatisticaService(LiteDbContext context)
    {
        _associazioneRepo = new AssociazioneMastrinoRepository(context);
        _templateRepo = new BilancioTemplateRepository(context);
        _bilancioRepo = new BilancioContabileRepository(context);
    }

    /// <summary>
    /// Genera statistiche per un cliente, periodo e template specifici
    /// </summary>
    public List<BilancioStatistica> GeneraStatistiche(int clienteId, int mese, int anno, int? templateMese = null, int? templateAnno = null)
    {
        // 1. Recupera l'associazione esistente
        var associazione = _associazioneRepo.GetByClienteAndPeriodo(clienteId, mese, anno);
        if (associazione == null)
        {
            throw new InvalidOperationException("Nessuna associazione trovata per questo cliente e periodo. Creare prima un'associazione.");
        }

        // 2. Recupera i dettagli delle mappature SALVATE
        var dettagliSalvati = _associazioneRepo.GetDettagli(associazione.Id).ToList();

        // 3. ⚠️ IMPORTANTE: Carica il bilancio contabile ATTUALE per verificare quali righe esistono ancora
        var mastriniAttuali = _bilancioRepo.GetByClienteAndPeriodo(clienteId, mese, anno).ToList();
        
        // Crea un HashSet per ricerca veloce dei mastrini che ESISTONO nel bilancio attuale
        var mastriniEsistenti = new HashSet<string>(
            mastriniAttuali.Select(m => $"{m.CodiceMastrino}|{m.DescrizioneMastrino}")
        );

        // 4. Filtra i dettagli salvati: considera SOLO quelli che esistono ancora nel bilancio contabile
        var dettagliValidi = dettagliSalvati
            .Where(d => mastriniEsistenti.Contains($"{d.CodiceMastrino}|{d.DescrizioneMastrino}"))
            .ToList();

        // 5. Aggiorna gli importi dai mastrini attuali (potrebbero essere cambiati)
        var mastriniDict = mastriniAttuali.ToDictionary(m => $"{m.CodiceMastrino}|{m.DescrizioneMastrino}");
        foreach (var dettaglio in dettagliValidi)
        {
            var chiave = $"{dettaglio.CodiceMastrino}|{dettaglio.DescrizioneMastrino}";
            if (mastriniDict.TryGetValue(chiave, out var mastrinoAttuale))
            {
                // Aggiorna l'importo con quello attuale del bilancio contabile
                dettaglio.Importo = mastrinoAttuale.Importo;
            }
        }

        // 6. Recupera le voci del template
        var vociTemplate = templateMese.HasValue && templateAnno.HasValue
            ? _templateRepo.GetByPeriodo(templateMese.Value, templateAnno.Value)
            : _templateRepo.GetByPeriodo(mese, anno);

        vociTemplate = vociTemplate.OrderBy(v => v.CodiceMastrino).ToList();

        // 7. Crea dizionario delle mappature per template_voce_id (SOLO quelle valide)
        var mappaturePerVoce = dettagliValidi
            .Where(d => d.TemplateVoceId.HasValue)
            .GroupBy(d => d.TemplateVoceId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 8. Genera statistiche
        var statistiche = new List<BilancioStatistica>();
        var importiCalcolati = new Dictionary<string, decimal>(); // Per calcolo formule

        foreach (var voce in vociTemplate)
        {
            var statistica = new BilancioStatistica
            {
                TemplateId = voce.Id,
                Codice = voce.CodiceMastrino,
                Descrizione = voce.DescrizioneMastrino,
                Segno = voce.Segno,
                Formula = voce.Formula
            };

            // Se ha formula, calcola dopo
            if (!string.IsNullOrWhiteSpace(voce.Formula))
            {
                statistica.Importo = 0; // Sarà calcolato dopo
                statistica.NumeroContiAssociati = 0;
            }
            else
            {
                // Trova i mastrini associati a questa voce template (SOLO quelli che esistono ancora)
                if (mappaturePerVoce.TryGetValue(voce.Id, out var mappature))
                {
                    // Somma gli importi
                    decimal somma = mappature.Sum(m => m.Importo);
                    
                    // Applica il segno del template
                    statistica.Importo = voce.Segno == "-" ? -Math.Abs(somma) : somma;
                    
                    // Salva conti associati
                    statistica.NumeroContiAssociati = mappature.Count;
                    statistica.ContiAssociati = mappature.Select(m => new ContoContabileAssociato
                    {
                        CodiceConto = m.CodiceMastrino,
                        DescrizioneConto = m.DescrizioneMastrino,
                        Importo = m.Importo
                    }).ToList();
                }
                else
                {
                    statistica.Importo = 0;
                    statistica.NumeroContiAssociati = 0;
                }

                // Salva per calcolo formule
                importiCalcolati[voce.CodiceMastrino] = statistica.Importo;
            }

            statistiche.Add(statistica);
        }

        // 9. Calcola formule
        foreach (var stat in statistiche.Where(s => s.HasFormula))
        {
            try
            {
                stat.Importo = CalcolaFormula(stat.Formula!, importiCalcolati);
                importiCalcolati[stat.Codice] = stat.Importo;
            }
            catch
            {
                stat.Importo = 0;
            }
        }

        // 10. Calcola percentuali su fatturato
        CalcolaPercentualiFatturato(statistiche);

        return statistiche;
    }

    /// <summary>
    /// Calcola le percentuali sul fatturato totale
    /// La riga di riferimento è SEMPRE quella con descrizione "TOTALE FATTURATO"
    /// </summary>
    private void CalcolaPercentualiFatturato(List<BilancioStatistica> statistiche)
    {
        // Trova la riga "TOTALE FATTURATO" (ricerca SOLO per descrizione)
        var totaleFatturato = statistiche.FirstOrDefault(s =>
            s.Descrizione.ToUpper().Contains("TOTALE FATTURATO"));

        if (totaleFatturato == null || totaleFatturato.Importo == 0)
        {
            // Nessun "TOTALE FATTURATO" trovato o importo zero
            foreach (var stat in statistiche)
            {
                stat.PercentualeFatturato = 0;
            }
            return;
        }

        // Usa il valore ASSOLUTO del fatturato come base per il calcolo
        decimal baseFatturato = Math.Abs(totaleFatturato.Importo);

        // Calcola percentuale per ogni voce
        // Formula: (|Importo Voce| / |TOTALE FATTURATO|) * 100
        foreach (var stat in statistiche)
        {
            if (baseFatturato != 0)
            {
                stat.PercentualeFatturato = (Math.Abs(stat.Importo) / baseFatturato) * 100;
            }
            else
            {
                stat.PercentualeFatturato = 0;
            }
        }
    }

    /// <summary>
    /// Calcola formula semplice (es: A.1 + B.1 - C.2)
    /// </summary>
    private decimal CalcolaFormula(string formula, Dictionary<string, decimal> importi)
    {
        decimal risultato = 0;
        string formulaPulita = formula.Replace(" ", "");

        // Split per + e -
        var parti = System.Text.RegularExpressions.Regex.Split(formulaPulita, @"([\+\-])");

        string operatore = "+";
        foreach (var parte in parti)
        {
            if (parte == "+" || parte == "-")
            {
                operatore = parte;
            }
            else if (!string.IsNullOrWhiteSpace(parte))
            {
                // Cerca il codice negli importi calcolati
                if (importi.TryGetValue(parte.Trim(), out decimal importo))
                {
                    if (operatore == "+")
                        risultato += importo;
                    else
                        risultato -= importo;
                }
            }
        }

        return risultato;
    }

    /// <summary>
    /// Ottiene i conti associati per una specifica voce template
    /// </summary>
    public List<ContoContabileAssociato> GetContiAssociati(int associazioneId, int templateVoceId)
    {
        var dettagli = _associazioneRepo.GetDettagli(associazioneId)
            .Where(d => d.TemplateVoceId == templateVoceId)
            .ToList();

        return dettagli.Select(d => new ContoContabileAssociato
        {
            CodiceConto = d.CodiceMastrino,
            DescrizioneConto = d.DescrizioneMastrino,
            Importo = d.Importo
        }).ToList();
    }
}

