using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Helpers;
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

        vociTemplate = vociTemplate.OrderByCodiceMastrinoNumerico(v => v.CodiceMastrino).ToList();

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

    /// <summary>
    /// Genera statistiche per un cliente con più bilanci contabili
    /// </summary>
    /// <param name="clienteId">ID cliente</param>
    /// <param name="periodi">Lista di periodi (mese, anno)</param>
    /// <param name="templateMese">Mese template (opzionale)</param>
    /// <param name="templateAnno">Anno template (opzionale)</param>
    /// <returns>Lista di statistiche multi-periodo</returns>
    public List<BilancioStatisticaMultiPeriodo> GeneraStatisticheMultiPeriodo(
        int clienteId, 
        List<(int Mese, int Anno)> periodi, 
        int? templateMese = null, 
        int? templateAnno = null,
        string? tipoBilancio = null, // ⭐ Filtro per tipo bilancio
        string? templateDescrizione = null) // ⭐ Aggiunto per identificare il template esatto
    {
        if (periodi == null || periodi.Count == 0)
        {
            throw new ArgumentException("Specificare almeno un periodo");
        }

        // 1. Recupera le voci del template (dal primo periodo o dai parametri)
        var primoPerio = periodi.First();
        int templateMeseDaCercare = templateMese ?? primoPerio.Mese;
        int templateAnnoDaCercare = templateAnno ?? primoPerio.Anno;
        
        List<BilancioTemplate> vociTemplate;
        
        // ⭐ Se è specificata la descrizione, carica il template ESATTO
        if (!string.IsNullOrEmpty(templateDescrizione))
        {
            // Carica il template specifico per descrizione
            vociTemplate = _templateRepo.GetAll()
                .Where(t => t.Mese == templateMeseDaCercare && 
                           t.Anno == templateAnnoDaCercare &&
                           (t.DescrizioneBilancio ?? "") == templateDescrizione)
                .ToList();
                
            System.Diagnostics.Debug.WriteLine($"[GENERA STATISTICHE] Template cercato: Mese={templateMeseDaCercare}, Anno={templateAnnoDaCercare}, Desc='{templateDescrizione}' → Trovate {vociTemplate.Count} voci");
        }
        else
        {
            // Fallback: carica per periodo (comportamento precedente)
            vociTemplate = _templateRepo.GetByPeriodo(templateMeseDaCercare, templateAnnoDaCercare);
            System.Diagnostics.Debug.WriteLine($"[GENERA STATISTICHE] Template cercato: Mese={templateMeseDaCercare}, Anno={templateAnnoDaCercare} (senza descrizione) → Trovate {vociTemplate.Count} voci");
        }

        vociTemplate = vociTemplate.OrderByCodiceMastrinoNumerico(v => v.CodiceMastrino).ToList();

        if (!vociTemplate.Any())
        {
            throw new InvalidOperationException("Nessuna voce template trovata");
        }

        // 2. ⚠️ VERIFICA ASSOCIAZIONE: cerca UNA associazione per questo cliente+template
        // ✅ IMPORTANTE: L'associazione è legata al TEMPLATE (descrizione), NON al periodo!
        // Se esiste un'associazione per templateSP, vale per TUTTI i periodi (set, ott, nov, ecc.)
        var tutteAssociazioni = _associazioneRepo.GetByCliente(clienteId).ToList();
        
        System.Diagnostics.Debug.WriteLine($"\n🔍 DEBUG Ricerca associazione:");
        System.Diagnostics.Debug.WriteLine($"   Template cercato: Desc='{templateDescrizione ?? "(nessuna)"}'");
        System.Diagnostics.Debug.WriteLine($"   Associazioni trovate per cliente: {tutteAssociazioni.Count}");
        
        // ✅ Cerca un'associazione che usa lo STESSO TEMPLATE (per descrizione), indipendentemente dal periodo
        var associazioneEsistente = tutteAssociazioni.FirstOrDefault(a =>
        {
            if (!a.TemplateId.HasValue) return false;
            
            // Prendi la voce template associata
            var voceTemplate = _templateRepo.GetById(a.TemplateId.Value);
            if (voceTemplate == null) return false;
            
            System.Diagnostics.Debug.WriteLine($"      Controllo associazione ID={a.Id}: Template Desc='{voceTemplate.DescrizioneBilancio ?? "(vuota)"}'");
            
            // ✅ Verifica SOLO che la DESCRIZIONE del template corrisponda (NON il periodo!)
            bool descrizioneMatch = string.IsNullOrEmpty(templateDescrizione) || 
                                   (voceTemplate.DescrizioneBilancio ?? "") == templateDescrizione;
            
            System.Diagnostics.Debug.WriteLine($"         Desc Match={descrizioneMatch}");
            
            return descrizioneMatch;
        });
        if (associazioneEsistente != null)
        {
            System.Diagnostics.Debug.WriteLine($"   ✅ Associazione TROVATA: ID={associazioneEsistente.Id}, Periodo={GetNomeMese(associazioneEsistente.Mese)} {associazioneEsistente.Anno}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"   ❌ Nessuna associazione trovata!");
        }
        
        if (associazioneEsistente == null)
        {
            throw new InvalidOperationException(
                "⚠️ ATTENZIONE: Non è stata trovata nessuna associazione tra i codici di bilancio e il template selezionato.\n\n" +
                "Prima di generare le statistiche, devi associare i codici contabili alle voci del template.\n\n" +
                "Vai in 'Associazioni Mastrini' e crea una nuova associazione per questo cliente e template.");
        }

        // 3. Recupera i dettagli dell'associazione (la mappatura codici → voci template)
        var mappaturaSalvata = _associazioneRepo.GetDettagli(associazioneEsistente.Id).ToList();
        
        System.Diagnostics.Debug.WriteLine($"\n📋 Mappature salvate nell'associazione: {mappaturaSalvata.Count}");

        // 4. Raccogli TUTTI i codici univoci presenti in TUTTI i bilanci selezionati
        var tuttiCodiciUnivoci = new HashSet<string>();
        var periodiAnalizzati = new List<string>();
        
        foreach (var periodo in periodi)
        {
            // ⭐ Carica SOLO i bilanci del tipo specificato (SP o CE), se fornito
            var mastriniPeriodo = tipoBilancio != null
                ? _bilancioRepo.GetByClienteAndPeriodo(clienteId, periodo.Mese, periodo.Anno)
                    .Where(b => b.TipoBilancio == tipoBilancio)
                    .ToList()
                : _bilancioRepo.GetByClienteAndPeriodo(clienteId, periodo.Mese, periodo.Anno).ToList();
                
            periodiAnalizzati.Add($"{GetNomeMese(periodo.Mese)} {periodo.Anno} ({mastriniPeriodo.Count} righe)");
            
            System.Diagnostics.Debug.WriteLine($"🔍 DEBUG Periodo {GetNomeMese(periodo.Mese)} {periodo.Anno}:");
            foreach (var mastrino in mastriniPeriodo)
            {
                tuttiCodiciUnivoci.Add(mastrino.CodiceMastrino);
                System.Diagnostics.Debug.WriteLine($"   - Codice: {mastrino.CodiceMastrino} | Descrizione: {mastrino.DescrizioneMastrino} | Importo: {mastrino.Importo}");
            }
        }

        System.Diagnostics.Debug.WriteLine($"\n📊 TOTALE Codici univoci trovati nei bilanci: {tuttiCodiciUnivoci.Count}");
        System.Diagnostics.Debug.WriteLine($"   Codici: {string.Join(", ", tuttiCodiciUnivoci.OrderBy(c => c))}");

        // 5. ⭐ IMPORTANTE: Verifica che i codici dei bilanci correnti siano presenti nella mappatura salvata
        // Se ci sono codici nuovi (non presenti quando è stata creata l'associazione), daranno errore
        var codiciAssociati = new HashSet<string>(mappaturaSalvata.Select(m => m.CodiceMastrino));
        
        System.Diagnostics.Debug.WriteLine($"\n✅ Codici ASSOCIATI nella mappatura salvata: {codiciAssociati.Count}");
        System.Diagnostics.Debug.WriteLine($"   Codici: {string.Join(", ", codiciAssociati.OrderBy(c => c))}");
        
        var codiciNonAssociati = tuttiCodiciUnivoci.Except(codiciAssociati).OrderBy(c => c).ToList();

        if (codiciNonAssociati.Any())
        {
            System.Diagnostics.Debug.WriteLine($"\n❌ Codici NON ASSOCIATI: {codiciNonAssociati.Count}");
            System.Diagnostics.Debug.WriteLine($"   Codici: {string.Join(", ", codiciNonAssociati)}");
            
            string elencoCodici = string.Join(", ", codiciNonAssociati.Take(20));
            if (codiciNonAssociati.Count > 20)
                elencoCodici += $" ... (e altri {codiciNonAssociati.Count - 20})";

            string periodiInfo = string.Join(", ", periodiAnalizzati);

            throw new InvalidOperationException(
                $"⚠️ ATTENZIONE: Non tutti i codici di bilancio sono associati al template!\n\n" +
                $"Periodi analizzati: {periodiInfo}\n\n" +
                $"Codici TOTALI trovati: {tuttiCodiciUnivoci.Count}\n" +
                $"Codici ASSOCIATI nel template: {codiciAssociati.Count}\n\n" +
                $"Codici NON associati ({codiciNonAssociati.Count}): {elencoCodici}\n\n" +
                $"Vai in 'Associazioni Mastrini', modifica l'associazione esistente e associa i codici mancanti.");
        }

        // 6. Inizializza le statistiche multi-periodo
        var statisticheMulti = new List<BilancioStatisticaMultiPeriodo>();
        foreach (var voce in vociTemplate)
        {
            statisticheMulti.Add(new BilancioStatisticaMultiPeriodo
            {
                TemplateId = voce.Id,
                Codice = voce.CodiceMastrino,
                Descrizione = voce.DescrizioneMastrino,
                Segno = voce.Segno,
                Formula = voce.Formula,
                DatiPerPeriodo = new Dictionary<string, DatiPeriodo>()
            });
        }

        // 7. Per ogni periodo, calcola le statistiche USANDO LA MAPPATURA UNICA
        foreach (var periodo in periodi)
        {
            string periodoKey = $"{periodo.Mese:D2}_{periodo.Anno}";
            string periodoDisplay = $"{GetNomeMese(periodo.Mese)} {periodo.Anno}";

            // Carica i mastrini attuali per questo periodo
            // ⭐ Filtra per tipo bilancio se specificato
            var mastriniAttuali = tipoBilancio != null
                ? _bilancioRepo.GetByClienteAndPeriodo(clienteId, periodo.Mese, periodo.Anno)
                    .Where(b => b.TipoBilancio == tipoBilancio)
                    .ToList()
                : _bilancioRepo.GetByClienteAndPeriodo(clienteId, periodo.Mese, periodo.Anno).ToList();
            
            // ✅ IMPORTANTE: Usa SOLO CodiceMastrino come chiave (come in Associazioni Mastrini)
            // Se ci sono più righe con lo stesso codice, somma gli importi
            var mastriniDict = mastriniAttuali
                .GroupBy(m => m.CodiceMastrino)
                .ToDictionary(
                    g => g.Key, 
                    g => new { 
                        CodiceMastrino = g.Key, 
                        DescrizioneMastrino = g.First().DescrizioneMastrino, 
                        Importo = g.Sum(m => m.Importo) 
                    });

            // Crea mappatura: TemplateVoceId → Lista di codici bilancio associati
            var mappaturePerVoce = mappaturaSalvata
                .Where(d => d.TemplateVoceId.HasValue)
                .GroupBy(d => d.TemplateVoceId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Calcola importi per ogni voce
            var importiCalcolati = new Dictionary<string, decimal>();

            foreach (var stat in statisticheMulti)
            {
                decimal importo = 0;
                int numeroConti = 0;
                var conti = new List<ContoContabileAssociato>();

                // Se ha formula, calcola dopo
                if (string.IsNullOrWhiteSpace(stat.Formula))
                {
                    // Trova mappature per questa voce template
                    var voceTemplate = vociTemplate.FirstOrDefault(v => v.CodiceMastrino == stat.Codice);
                    
                    System.Diagnostics.Debug.WriteLine($"\n🔍 Elaborazione voce template: {stat.Codice} - {stat.Descrizione}");
                    
                    if (voceTemplate != null && mappaturePerVoce.TryGetValue(voceTemplate.Id, out var mappature))
                    {
                        System.Diagnostics.Debug.WriteLine($"   ✅ Trovate {mappature.Count} mappature per questo template (ID: {voceTemplate.Id})");
                        
                        // Per ogni codice bilancio associato a questa voce template, prendi l'importo dal bilancio attuale
                        foreach (var mappatura in mappature)
                        {
                            // ✅ Usa SOLO il codice mastrino come chiave (come in Associazioni Mastrini)
                            string codiceMastrino = mappatura.CodiceMastrino;
                            
                            System.Diagnostics.Debug.WriteLine($"   🔑 Cerco codice: '{codiceMastrino}'");
                            
                            if (mastriniDict.TryGetValue(codiceMastrino, out var mastrinoAttuale))
                            {
                                System.Diagnostics.Debug.WriteLine($"      ✅ TROVATO! Importo: {mastrinoAttuale.Importo}");
                                
                                importo += mastrinoAttuale.Importo;
                                numeroConti++;
                                conti.Add(new ContoContabileAssociato
                                {
                                    CodiceConto = mastrinoAttuale.CodiceMastrino,
                                    DescrizioneConto = mastrinoAttuale.DescrizioneMastrino,
                                    Importo = mastrinoAttuale.Importo
                                });
                                
                                // Aggiungi anche alla lista totale per il dettaglio multi-periodo
                                stat.ContiAssociatiTutti.Add(new ContoContabileAssociatoConPeriodo
                                {
                                    CodiceConto = mastrinoAttuale.CodiceMastrino,
                                    DescrizioneConto = mastrinoAttuale.DescrizioneMastrino,
                                    Importo = mastrinoAttuale.Importo,
                                    Periodo = periodoDisplay,
                                    Mese = periodo.Mese,
                                    Anno = periodo.Anno
                                });
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"      ❌ NON TROVATO nel dizionario mastrini!");
                                System.Diagnostics.Debug.WriteLine($"      📋 Chiavi disponibili nel dizionario:");
                                foreach (var key in mastriniDict.Keys.Take(10))
                                {
                                    System.Diagnostics.Debug.WriteLine($"         - '{key}'");
                                }
                            }
                        }
                        
                        // Applica il segno del template
                        importo = stat.Segno == "-" ? -Math.Abs(importo) : importo;
                        
                        System.Diagnostics.Debug.WriteLine($"   💰 Importo FINALE (dopo segno {stat.Segno}): {importo}");
                    }
                    else
                    {
                        if (voceTemplate == null)
                            System.Diagnostics.Debug.WriteLine($"   ❌ Voce template NON TROVATA per codice: {stat.Codice}");
                        else
                            System.Diagnostics.Debug.WriteLine($"   ❌ Nessuna mappatura trovata per voce template ID: {voceTemplate.Id}");
                    }

                    importiCalcolati[stat.Codice] = importo;
                }

                stat.DatiPerPeriodo[periodoKey] = new DatiPeriodo
                {
                    PeriodoDisplay = periodoDisplay,
                    Mese = periodo.Mese,
                    Anno = periodo.Anno,
                    Importo = importo,
                    Percentuale = 0, // Calcolato dopo
                    NumeroConti = numeroConti,
                    Conti = conti
                };
            }

            // Calcola formule per questo periodo
            foreach (var stat in statisticheMulti.Where(s => s.HasFormula))
            {
                try
                {
                    decimal importoFormula = CalcolaFormula(stat.Formula!, importiCalcolati);
                    stat.DatiPerPeriodo[periodoKey].Importo = importoFormula;
                    importiCalcolati[stat.Codice] = importoFormula;
                }
                catch
                {
                    stat.DatiPerPeriodo[periodoKey].Importo = 0;
                }
            }

            // Calcola percentuali per questo periodo
            CalcolaPercentualiPeriodo(statisticheMulti, periodoKey);
        }

        // 8. Calcola totali e percentuali totali
        CalcolaTotaliMultiPeriodo(statisticheMulti);

        return statisticheMulti;
    }

    /// <summary>
    /// Calcola le percentuali per un singolo periodo
    /// </summary>
    private void CalcolaPercentualiPeriodo(List<BilancioStatisticaMultiPeriodo> statistiche, string periodoKey)
    {
        // Trova TOTALE FATTURATO per questo periodo
        // Cerca varie possibili denominazioni
        var totaleFatturato = statistiche
            .FirstOrDefault(s => s.Descrizione.ToUpper().Contains("TOTALE FATTURATO"));
        
        if (totaleFatturato == null)
        {
            // Prova con solo "FATTURATO"
            totaleFatturato = statistiche
                .FirstOrDefault(s => s.Descrizione.ToUpper().Contains("FATTURATO"));
        }
        
        System.Diagnostics.Debug.WriteLine($"\n💰 Calcolo % per periodo {periodoKey}:");
        System.Diagnostics.Debug.WriteLine($"   Voci totali: {statistiche.Count}");
        System.Diagnostics.Debug.WriteLine($"   FATTURATO trovato: {(totaleFatturato != null ? $"'{totaleFatturato.Descrizione}'" : "NESSUNO")}");
        
        if (totaleFatturato != null && totaleFatturato.DatiPerPeriodo.ContainsKey(periodoKey))
        {
            System.Diagnostics.Debug.WriteLine($"   Importo FATTURATO: {totaleFatturato.DatiPerPeriodo[periodoKey].Importo:N2} €");
        }

        if (totaleFatturato == null || 
            !totaleFatturato.DatiPerPeriodo.ContainsKey(periodoKey) ||
            totaleFatturato.DatiPerPeriodo[periodoKey].Importo == 0)
        {
            System.Diagnostics.Debug.WriteLine($"   ⚠️ FATTURATO non valido - percentuali impostate a 0");
            
            // Nessun fatturato - percentuali a 0
            foreach (var stat in statistiche)
            {
                if (stat.DatiPerPeriodo.ContainsKey(periodoKey))
                {
                    stat.DatiPerPeriodo[periodoKey].Percentuale = 0;
                }
            }
            return;
        }

        decimal baseFatturato = Math.Abs(totaleFatturato.DatiPerPeriodo[periodoKey].Importo);
        
        System.Diagnostics.Debug.WriteLine($"   Base FATTURATO per calcolo: {baseFatturato:N2} €");

        // Calcola percentuale per ogni voce
        foreach (var stat in statistiche)
        {
            if (stat.DatiPerPeriodo.ContainsKey(periodoKey) && baseFatturato != 0)
            {
                decimal importoVoce = stat.DatiPerPeriodo[periodoKey].Importo;
                stat.DatiPerPeriodo[periodoKey].Percentuale = 
                    (Math.Abs(importoVoce) / baseFatturato) * 100;
                    
                System.Diagnostics.Debug.WriteLine($"      '{stat.Descrizione}': {importoVoce:N2} € → {stat.DatiPerPeriodo[periodoKey].Percentuale:F2}%");
            }
        }
    }

    /// <summary>
    /// Calcola i totali complessivi e le percentuali totali
    /// </summary>
    private void CalcolaTotaliMultiPeriodo(List<BilancioStatisticaMultiPeriodo> statistiche)
    {
        // 1. Calcola somma importi per ogni voce
        foreach (var stat in statistiche)
        {
            stat.ImportoTotale = stat.DatiPerPeriodo.Values.Sum(d => d.Importo);
            stat.NumeroContiAssociatiTotali = stat.DatiPerPeriodo.Values.Sum(d => d.NumeroConti);

            // Raccogli tutti i conti di tutti i periodi
            stat.ContiAssociatiTutti = new List<ContoContabileAssociatoConPeriodo>();
            foreach (var kvp in stat.DatiPerPeriodo)
            {
                foreach (var conto in kvp.Value.Conti)
                {
                    stat.ContiAssociatiTutti.Add(new ContoContabileAssociatoConPeriodo
                    {
                        Periodo = kvp.Value.PeriodoDisplay,
                        Mese = kvp.Value.Mese,
                        Anno = kvp.Value.Anno,
                        CodiceConto = conto.CodiceConto,
                        DescrizioneConto = conto.DescrizioneConto,
                        Importo = conto.Importo
                    });
                }
            }
        }

        // 2. Calcola percentuali totali
        var totaleFatturato = statistiche
            .FirstOrDefault(s => s.Descrizione.ToUpper().Contains("TOTALE FATTURATO"));
        
        if (totaleFatturato == null)
        {
            // Prova con solo "FATTURATO"
            totaleFatturato = statistiche
                .FirstOrDefault(s => s.Descrizione.ToUpper().Contains("FATTURATO"));
        }
        
        System.Diagnostics.Debug.WriteLine($"\n💰 Calcolo % TOTALE:");
        System.Diagnostics.Debug.WriteLine($"   FATTURATO trovato: {(totaleFatturato != null ? $"'{totaleFatturato.Descrizione}'" : "NESSUNO")}");

        if (totaleFatturato == null || totaleFatturato.ImportoTotale == 0)
        {
            System.Diagnostics.Debug.WriteLine($"   ⚠️ FATTURATO non valido - percentuali totali impostate a 0");
            
            foreach (var stat in statistiche)
            {
                stat.PercentualeTotale = 0;
            }
            return;
        }

        decimal baseFatturatoTotale = Math.Abs(totaleFatturato.ImportoTotale);
        
        System.Diagnostics.Debug.WriteLine($"   Base FATTURATO TOTALE: {baseFatturatoTotale:N2} €");

        foreach (var stat in statistiche)
        {
            if (baseFatturatoTotale != 0)
            {
                stat.PercentualeTotale = (Math.Abs(stat.ImportoTotale) / baseFatturatoTotale) * 100;
                System.Diagnostics.Debug.WriteLine($"      '{stat.Descrizione}': {stat.ImportoTotale:N2} € → {stat.PercentualeTotale:F2}%");
            }
            else
            {
                stat.PercentualeTotale = 0;
            }
        }
    }

    /// <summary>
    /// Ottiene il nome del mese in italiano
    /// </summary>
    private string GetNomeMese(int mese)
    {
        return mese switch
        {
            1 => "Gen",
            2 => "Feb",
            3 => "Mar",
            4 => "Apr",
            5 => "Mag",
            6 => "Giu",
            7 => "Lug",
            8 => "Ago",
            9 => "Set",
            10 => "Ott",
            11 => "Nov",
            12 => "Dic",
            _ => ""
        };
    }
}

