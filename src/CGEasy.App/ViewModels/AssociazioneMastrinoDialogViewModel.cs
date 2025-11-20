using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using CGEasy.Core.Repositories;
using CGEasy.Core.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.ViewModels;

public partial class AssociazioneMastrinoDialogViewModel : ObservableObject
{
    // ✅ USA SOLO REPOSITORY (come TODO) - NON service!
    private readonly AssociazioneMastrinoRepository _associazioneRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly BilancioContabileRepository _bilancioRepository;
    private readonly BilancioTemplateRepository _templateRepository;
    private readonly int? _associazioneId;

    // ✅ Callback per chiusura dialog (come TODO)
    public Action<bool>? OnDialogClosed { get; set; }

    [ObservableProperty]
    private ObservableCollection<Cliente> _clienti = new();

    [ObservableProperty]
    private Cliente? _clienteSelezionato;

    [ObservableProperty]
    private ObservableCollection<BilancioGruppo> _bilanciDisponibili = new();

    [ObservableProperty]
    private BilancioGruppo? _bilancioSelezionato;

    [ObservableProperty]
    private ObservableCollection<BilancioTemplate> _templateDisponibili = new();

    [ObservableProperty]
    private BilancioTemplate? _templateSelezionato;

    [ObservableProperty]
    private string _descrizione = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MappaturaMastrinoViewModel> _mappature = new();

    [ObservableProperty]
    private ObservableCollection<BilancioTemplate> _vociTemplateDisponibili = new();

    [ObservableProperty]
    private bool _isModifica;

    [ObservableProperty]
    private bool _isSalvataggio;

    private bool _isLoading = false; // Flag per bloccare eventi durante caricamento

    public AssociazioneMastrinoDialogViewModel(CGEasyDbContext context, int? associazioneId)
    {
        // ✅ Crea SOLO repository (come TODO)
        _associazioneRepository = new AssociazioneMastrinoRepository(context);
        _clienteRepository = new ClienteRepository(context);
        _bilancioRepository = new BilancioContabileRepository(context);
        _templateRepository = new BilancioTemplateRepository(context);
        
        _associazioneId = associazioneId;
        IsModifica = associazioneId.HasValue;

        try
        {
            CaricaClienti();
            CaricaTemplateDisponibili();

            if (IsModifica)
            {
                CaricaAssociazione();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore inizializzazione:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Carica clienti che hanno bilanci contabili importati
    /// </summary>
    private void CaricaClienti()
    {
        // Ottiene gli ID univoci dei clienti che hanno bilanci contabili
        var clientiConBilanci = _bilancioRepository.GetAll()
            .Select(b => b.ClienteId)
            .Distinct()
            .ToList();

        // Carica solo i clienti attivi che hanno bilanci contabili
        var clienti = _clienteRepository.GetAll()
            .Where(c => c.Attivo && clientiConBilanci.Contains(c.Id))
            .OrderBy(c => c.NomeCliente);

        Clienti.Clear();
        foreach (var cliente in clienti)
        {
            Clienti.Add(cliente);
        }
    }

    /// <summary>
    /// Carica associazione esistente (modalità modifica)
    /// </summary>
    private void CaricaAssociazione()
    {
        if (!_associazioneId.HasValue)
            return;

        // ✅ USA repository direttamente (come TODO)
        var associazione = _associazioneRepository.GetById(_associazioneId.Value);
        if (associazione == null)
        {
            MessageBox.Show("Associazione non trovata.",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _isLoading = true; // BLOCCA gli eventi automatici

        try
        {
            // 1. Imposta cliente
            ClienteSelezionato = Clienti.FirstOrDefault(c => c.Id == associazione.ClienteId);
            
            if (ClienteSelezionato != null)
            {
                // 2. ⭐ PRIMA carica il template per sapere se è CE o SP
                CaricaTemplateDisponibili();
                
                // Seleziona template USANDO L'ID SALVATO (non solo Mese+Anno!)
                if (associazione.TemplateId.HasValue)
                {
                    TemplateSelezionato = TemplateDisponibili.FirstOrDefault(
                        t => t.Id == associazione.TemplateId.Value);
                }
                
                // Se non trova per ID, prova con Mese+Anno (fallback)
                if (TemplateSelezionato == null)
                {
                    TemplateSelezionato = TemplateDisponibili.FirstOrDefault(
                        t => t.Mese == associazione.Mese && t.Anno == associazione.Anno);
                }
                
                System.Diagnostics.Debug.WriteLine($"\n========== CARICA ASSOCIAZIONE ==========");
                System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Template selezionato: {TemplateSelezionato?.DescrizioneBilancio ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] TipoBilancio Template: '{TemplateSelezionato?.TipoBilancio ?? "NULL/VUOTO"}'");
                
                // 3. ⭐ Ora carica i bilanci FILTRATI per tipo (CE o SP)
                var tuttiIBilanci = _bilancioRepository.GetGruppiByCliente(ClienteSelezionato.Id);
                
                // Filtra per tipo se il template ha il TipoBilancio
                IEnumerable<BilancioGruppo> bilanciFiltrati;
                if (TemplateSelezionato != null && !string.IsNullOrEmpty(TemplateSelezionato.TipoBilancio))
                {
                    bilanciFiltrati = tuttiIBilanci.Where(b => b.TipoBilancio == TemplateSelezionato.TipoBilancio);
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ✅ Bilanci filtrati per tipo {TemplateSelezionato.TipoBilancio}: {bilanciFiltrati.Count()}");
                }
                else
                {
                    bilanciFiltrati = tuttiIBilanci;
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ⚠️ Nessun filtro tipo, caricati tutti i bilanci");
                }
                
                BilanciDisponibili.Clear();
                foreach (var bilancio in bilanciFiltrati)
                {
                    BilanciDisponibili.Add(bilancio);
                }

                // 4. ⭐ IMPORTANTE: Cerca bilancio per Mese, Anno E DESCRIZIONE
                BilancioSelezionato = BilanciDisponibili.FirstOrDefault(
                    b => b.Mese == associazione.Mese && 
                         b.Anno == associazione.Anno &&
                         (b.Descrizione ?? "") == (associazione.BilancioDescrizione ?? ""));

                // 🔄 FALLBACK: Se non trova (associazioni vecchie senza BilancioDescrizione)
                if (BilancioSelezionato == null)
                {
                    var bilanciPerPeriodo = BilanciDisponibili
                        .Where(b => b.Mese == associazione.Mese && b.Anno == associazione.Anno)
                        .ToList();
                    
                    if (bilanciPerPeriodo.Count == 1)
                    {
                        // C'è un solo bilancio per quel periodo, lo uso
                        BilancioSelezionato = bilanciPerPeriodo[0];
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] FALLBACK: Bilancio unico trovato per Cliente={associazione.ClienteNome}, Periodo={associazione.Mese}/{associazione.Anno}");
                    }
                    else if (bilanciPerPeriodo.Count > 1)
                    {
                        // 🔍 Ci sono più bilanci, provo a capire quale era quello originale
                        // guardando i codici mastrino salvati nei dettagli
                        var dettagliSalvati = _associazioneRepository.GetDettagli(_associazioneId.Value);
                        var codiciSalvati = dettagliSalvati.Select(d => d.CodiceMastrino).Distinct().ToList();
                        
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] FALLBACK: Trovati {bilanciPerPeriodo.Count} bilanci, verifico codici salvati...");
                        
                        // Per ogni bilancio, vedo quanti codici salvati contiene
                        var bilancioMigliore = bilanciPerPeriodo
                            .Select(bil => {
                                var codiciDelBilancio = _bilancioRepository
                                    .GetByClienteAndPeriodoAndDescrizione(associazione.ClienteId, bil.Mese, bil.Anno, bil.Descrizione)
                                    .Select(b => b.CodiceMastrino)
                                    .Distinct()
                                    .ToList();
                                    
                                var codiciInComune = codiciSalvati.Intersect(codiciDelBilancio).Count();
                                
                                System.Diagnostics.Debug.WriteLine($"  - Bilancio '{bil.Descrizione}': {codiciInComune}/{codiciSalvati.Count} codici in comune");
                                
                                return new { Bilancio = bil, Match = codiciInComune };
                            })
                            .OrderByDescending(x => x.Match)
                            .First();
                        
                        BilancioSelezionato = bilancioMigliore.Bilancio;
                        
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] FALLBACK: Selezionato bilancio '{BilancioSelezionato.Descrizione}' con {bilancioMigliore.Match} codici corrispondenti");
                        
                        // Avvisa l'utente solo se il match non è perfetto
                        if (bilancioMigliore.Match < codiciSalvati.Count)
                        {
                            var descrizioni = string.Join("\n", bilanciPerPeriodo.Select(b => $"- {b.Descrizione ?? "(nessuna descrizione)"}"));
                            MessageBox.Show(
                                $"⚠️ ATTENZIONE!\n\n" +
                                $"Questa associazione è stata salvata prima dell'aggiornamento.\n\n" +
                                $"Sono stati trovati {bilanciPerPeriodo.Count} bilanci per {associazione.PeriodoDisplay}:\n" +
                                $"{descrizioni}\n\n" +
                                $"È stato selezionato: {BilancioSelezionato.Descrizione}\n\n" +
                                $"Se non è corretto, seleziona manualmente il bilancio giusto e salva di nuovo.",
                                "Bilancio Ambiguo",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                }

                if (BilancioSelezionato != null)
                {
                    // 5. Carica voci template (il template è già stato selezionato sopra)
                    if (TemplateSelezionato != null)
                    {
                        CaricaVociTemplateBase(TemplateSelezionato.Mese, TemplateSelezionato.Anno);
                    }

                    // 5. ⭐ IMPORTANTE: Carica TUTTI i mastrini dello stesso TIPO (SP o CE) del cliente
                    // Non solo quelli del periodo salvato, ma TUTTI i periodi disponibili
                    // Questo permette di trovare codici nuovi in periodi successivi
                    
                    List<BilancioContabile> bilanciContabili;
                    
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] BilancioSelezionato: {BilancioSelezionato?.Descrizione ?? "NULL"}");
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] TemplateSelezionato: {TemplateSelezionato?.DescrizioneBilancio ?? "NULL"}");
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] TipoBilancio Template: '{TemplateSelezionato?.TipoBilancio ?? "NULL/EMPTY"}'");
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] TipoBilancio Bilancio: '{BilancioSelezionato?.TipoBilancio ?? "NULL/EMPTY"}'");
                    
                    // ⭐ DETERMINA IL TIPO: usa Template, altrimenti Bilancio, altrimenti descrizione
                    string? tipoBilancio = null;
                    string metodoUsato = "";
                    
                    if (TemplateSelezionato != null && !string.IsNullOrEmpty(TemplateSelezionato.TipoBilancio))
                    {
                        tipoBilancio = TemplateSelezionato.TipoBilancio;
                        metodoUsato = "TEMPLATE";
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Tipo determinato da TEMPLATE: {tipoBilancio}");
                    }
                    else if (BilancioSelezionato != null && !string.IsNullOrEmpty(BilancioSelezionato.TipoBilancio))
                    {
                        tipoBilancio = BilancioSelezionato.TipoBilancio;
                        metodoUsato = "BILANCIO";
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Tipo determinato da BILANCIO: {tipoBilancio}");
                    }
                    else if (TemplateSelezionato != null && TemplateSelezionato.DescrizioneBilancio != null)
                    {
                        // Fallback: deduce dal nome (templateSP, templateCE, ecc.)
                        if (TemplateSelezionato.DescrizioneBilancio.ToUpper().Contains("SP"))
                            tipoBilancio = "SP";
                        else if (TemplateSelezionato.DescrizioneBilancio.ToUpper().Contains("CE"))
                            tipoBilancio = "CE";
                        metodoUsato = "DESCRIZIONE";
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Tipo dedotto da DESCRIZIONE: {tipoBilancio ?? "NULL"}");
                    }
                    
                    // DEBUG: Log del tipo rilevato
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] 🔍 Tipo rilevato: {tipoBilancio ?? "NESSUNO"} (Metodo: {metodoUsato})");
                    
                    
                    // USA IL TIPO per filtrare i mastrini
                    if (!string.IsNullOrEmpty(tipoBilancio))
                    {
                        // Carica TUTTI i mastrini dello stesso tipo (SP o CE) del cliente
                        bilanciContabili = _bilancioRepository.GetAll()
                            .Where(b => b.ClienteId == associazione.ClienteId && 
                                       b.TipoBilancio == tipoBilancio)
                            .ToList();
                            
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ✅ Caricati TUTTI i mastrini {tipoBilancio} del cliente: {bilanciContabili.Count} righe");
                        
                        // Debug: mostra periodi unici
                        var periodiUniciDebug = bilanciContabili
                            .Select(b => $"{b.PeriodoDisplay} ({b.DescrizioneBilancio})")
                            .Distinct()
                            .OrderBy(p => p)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Periodi caricati: {string.Join(", ", periodiUniciDebug)}");
                        
                        // Debug: Mostra i primi 10 codici caricati
                        var primiCodici = bilanciContabili
                            .Select(b => $"{b.CodiceMastrino} ({b.TipoBilancio})")
                            .Distinct()
                            .Take(10)
                            .ToList();
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Primi 10 codici: {string.Join(", ", primiCodici)}");
                    }
                    else
                    {
                        // 🔴 ERRORE: Non è riuscito a determinare il tipo!
                        MessageBox.Show(
                            $"❌ ERRORE CRITICO!\n\n" +
                            $"Impossibile determinare il tipo di bilancio (SP/CE).\n\n" +
                            $"Template: {TemplateSelezionato?.DescrizioneBilancio ?? "NULL"}\n" +
                            $"TipoBilancio Template: '{TemplateSelezionato?.TipoBilancio ?? "VUOTO"}'\n" +
                            $"TipoBilancio Bilancio: '{BilancioSelezionato?.TipoBilancio ?? "VUOTO"}'\n\n" +
                            $"Il database ha un problema! Il template non ha TipoBilancio valorizzato.",
                            "ERRORE - Tipo non determinato",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                            
                        // Fallback: carica solo del periodo salvato
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ⚠️ FALLBACK attivato! Impossibile determinare TipoBilancio");
                        
                        if (!string.IsNullOrWhiteSpace(associazione.BilancioDescrizione))
                        {
                            bilanciContabili = _bilancioRepository.GetByClienteAndPeriodoAndDescrizione(
                                associazione.ClienteId, 
                                associazione.Mese, 
                                associazione.Anno, 
                                associazione.BilancioDescrizione);
                        }
                        else
                        {
                            bilanciContabili = _bilancioRepository.GetByClienteAndPeriodo(
                                associazione.ClienteId, 
                                associazione.Mese, 
                                associazione.Anno);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] FALLBACK: Caricati mastrini del periodo salvato: {bilanciContabili.Count} righe");
                    }
                    
                    var mastriniAttuali = bilanciContabili
                        .GroupBy(b => new { b.CodiceMastrino, b.DescrizioneMastrino })
                        .Select(g => new AssociazioneMastrinoDettaglio
                        {
                            CodiceMastrino = g.Key.CodiceMastrino,
                            DescrizioneMastrino = g.Key.DescrizioneMastrino,
                            Importo = g.Sum(b => b.Importo)
                        })
                        .OrderByCodiceMastrinoNumerico(m => m.CodiceMastrino)
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Mastrini ATTUALI dopo GroupBy: {mastriniAttuali.Count} codici unici");

                    // 6. Carica mappature SALVATE
                    var dettagliSalvati = _associazioneRepository.GetDettagli(_associazioneId.Value).ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] Dettagli SALVATI dal DB: {dettagliSalvati.Count} righe");

                    // 7. Crea dizionario dei mastrini salvati (gestisce eventuali duplicati prendendo il primo)
                    var salvatiDict = dettagliSalvati
                        .GroupBy(d => d.CodiceMastrino)
                        .ToDictionary(g => g.Key, g => g.First());

                    // 8. Separa mastrini in 3 categorie:
                    // - NON ASSOCIATI: nuovi o esistenti ma senza voce template
                    // - ASSOCIATI: hanno già una voce template
                    var mastriniNonAssociati = new List<MappaturaMastrinoViewModel>();
                    var mastriniAssociati = new List<MappaturaMastrinoViewModel>();

                    Mappature.Clear();

                    // 9. Processa tutti i mastrini ATTUALI
                    foreach (var mastrino in mastriniAttuali)
                    {
                        var mappatura = new MappaturaMastrinoViewModel(mastrino, VociTemplateDisponibili);
                        
                        // Se era già salvato, ripristina la selezione
                        if (salvatiDict.TryGetValue(mastrino.CodiceMastrino, out var salvato))
                        {
                            if (salvato.TemplateVoceId.HasValue)
                            {
                                mappatura.VoceTemplateSelezionata = VociTemplateDisponibili
                                    .FirstOrDefault(v => v.Id == salvato.TemplateVoceId.Value);
                            }
                            
                            // ⭐ VERIFICA: Se NON ha una voce template, consideralo NON ASSOCIATO
                            if (!salvato.TemplateVoceId.HasValue || salvato.TemplateVoceId.Value == 0)
                            {
                                // Mastrino salvato ma NON associato
                                mastriniNonAssociati.Add(mappatura);
                            }
                            else
                            {
                                // Mastrino già associato
                                mastriniAssociati.Add(mappatura);
                            }
                        }
                        else
                        {
                            // ⭐ NUOVO MASTRINO! (non era presente quando hai salvato l'associazione)
                            // Aggiungi ai mastrini non associati
                            mastriniNonAssociati.Add(mappatura);
                        }
                    }

                    // 10. ⭐ IMPORTANTE: Aggiungi PRIMA i mastrini NON ASSOCIATI (in cima alla lista)
                    foreach (var nonAssociato in mastriniNonAssociati)
                    {
                        Mappature.Add(nonAssociato);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ✅ Aggiunti {mastriniNonAssociati.Count} mastrini NON ASSOCIATI in cima");
                    
                    // 11. Poi aggiungi i mastrini già associati
                    foreach (var associato in mastriniAssociati)
                    {
                        Mappature.Add(associato);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ✅ Aggiunti {mastriniAssociati.Count} mastrini già ASSOCIATI");
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] 📊 TOTALE nella grid: {Mappature.Count} righe");

                    // 12. ⚠️ NON aggiungere mastrini salvati che non esistono più nel bilancio filtrato
                    // Se un mastrino salvato non è nei mastriniAttuali, significa che:
                    // - O non esiste più nel database (eliminato)
                    // - O appartiene a un tipo diverso (CE vs SP)
                    // In entrambi i casi NON lo mostriamo nella grid
                    
                    System.Diagnostics.Debug.WriteLine($"[CARICA ASSOCIAZIONE] ⚠️ Mastrini salvati ma non più presenti nei bilanci {tipoBilancio ?? "TUTTI"}: {dettagliSalvati.Count(d => !mastriniAttuali.Any(m => m.CodiceMastrino == d.CodiceMastrino))}");

                    // 13. ⭐ Mostra messaggio se ci sono mastrini non associati
                    if (mastriniNonAssociati.Count > 0)
                    {
                        var codiciNonAssociati = string.Join(", ", mastriniNonAssociati.Select(m => m.CodiceMastrino).Take(10));
                        if (mastriniNonAssociati.Count > 10)
                            codiciNonAssociati += $" ... (e altri {mastriniNonAssociati.Count - 10})";
                            
                        MessageBox.Show(
                            $"⚠️ ATTENZIONE - {mastriniNonAssociati.Count} CONTI NON ASSOCIATI!\n\n" +
                            $"Sono stati trovati conti del bilancio {BilancioSelezionato?.TipoBilancio} " +
                            $"che NON sono ancora associati a una voce del template:\n\n" +
                            $"{codiciNonAssociati}\n\n" +
                            $"Questi conti sono stati messi IN CIMA alla lista per facilitare l'associazione.\n\n" +
                            $"💡 Associa questi conti alle voci del template e salva per aggiornare.",
                            "Conti Non Associati",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }

            Descrizione = associazione.Descrizione ?? string.Empty;
        }
        finally
        {
            _isLoading = false; // RIABILITA gli eventi
        }
    }

    /// <summary>
    /// Quando cambia cliente selezionato
    /// </summary>
    partial void OnClienteSelezionatoChanged(Cliente? oldValue, Cliente? newValue)
    {
        if (_isLoading || newValue == null)
            return;

        // Carica bilanci disponibili per il cliente
        var bilanci = _bilancioRepository.GetGruppiByCliente(newValue.Id);
        BilanciDisponibili.Clear();
        foreach (var bilancio in bilanci)
        {
            BilanciDisponibili.Add(bilancio);
        }

        // Carica template disponibili per il cliente
        CaricaTemplateDisponibili();
    }

    /// <summary>
    /// Quando cambia bilancio selezionato
    /// </summary>
    partial void OnBilancioSelezionatoChanged(BilancioGruppo? oldValue, BilancioGruppo? newValue)
    {
        if (_isLoading || newValue == null || ClienteSelezionato == null)
            return;

        // ⭐ FILTRA i template disponibili in base al TipoBilancio del bilancio selezionato
        var templatesFiltrati = _templateRepository.GetAll()
            .Where(t => t.TipoBilancio == newValue.TipoBilancio) // Filtra per tipo
            .GroupBy(t => new { 
                t.Mese, 
                t.Anno, 
                Descrizione = t.DescrizioneBilancio ?? "" 
            })
            .Select(g => g.First())
            .OrderByDescending(t => t.Anno)
            .ThenByDescending(t => t.Mese)
            .ThenBy(t => t.DescrizioneBilancio)
            .ToList();
        
        TemplateDisponibili.Clear();
        foreach (var template in templatesFiltrati)
        {
            TemplateDisponibili.Add(template);
        }
        
        System.Diagnostics.Debug.WriteLine($"[BILANCIO SELEZIONATO] Tipo={newValue.TipoBilancio}, Template disponibili filtrati: {TemplateDisponibili.Count}");

        // ⭐ IMPORTANTE: Carica SOLO i mastrini del bilancio SPECIFICO (Mese, Anno, Descrizione)
        var mastrini = _bilancioRepository.GetByClienteAndPeriodoAndDescrizione(
                ClienteSelezionato.Id, 
                newValue.Mese, 
                newValue.Anno, 
                newValue.Descrizione)
            .GroupBy(b => new { b.CodiceMastrino, b.DescrizioneMastrino })
            .Select(g => new AssociazioneMastrinoDettaglio
            {
                CodiceMastrino = g.Key.CodiceMastrino,
                DescrizioneMastrino = g.Key.DescrizioneMastrino,
                Importo = g.Sum(b => b.Importo)
            })
            .OrderByCodiceMastrinoNumerico(m => m.CodiceMastrino)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"[BILANCIO SELEZIONATO] Cliente={ClienteSelezionato.NomeCliente}, Periodo={newValue.Mese}/{newValue.Anno}, Desc='{newValue.Descrizione ?? "(vuota)"}', Mastrini caricati={mastrini.Count}");

        Mappature.Clear();
        foreach (var mastrino in mastrini)
        {
            Mappature.Add(new MappaturaMastrinoViewModel(mastrino, VociTemplateDisponibili));
        }

        // Carica voci template base (senza formule) se template già selezionato
        if (TemplateSelezionato != null)
        {
            CaricaVociTemplateBase(TemplateSelezionato.Mese, TemplateSelezionato.Anno);
        }
    }

    /// <summary>
    /// Quando cambia template selezionato - CARICA LE VOCI E FILTRA I BILANCI
    /// </summary>
    partial void OnTemplateSelezionatoChanged(BilancioTemplate? oldValue, BilancioTemplate? newValue)
    {
        if (_isLoading)
            return;

        if (newValue == null)
        {
            VociTemplateDisponibili.Clear();
            return;
        }

        // Carica voci del template selezionato
        CaricaVociTemplateBase(newValue.Mese, newValue.Anno);
        
        // ⭐ IMPORTANTE: Filtra i bilanci disponibili in base al tipo del template (CE o SP)
        if (ClienteSelezionato != null && !string.IsNullOrEmpty(newValue.TipoBilancio))
        {
            System.Diagnostics.Debug.WriteLine($"[TEMPLATE SELEZIONATO] Tipo={newValue.TipoBilancio}, Filtro bilanci...");
            
            var tuttiIBilanci = _bilancioRepository.GetGruppiByCliente(ClienteSelezionato.Id);
            var bilanciFiltrati = tuttiIBilanci.Where(b => b.TipoBilancio == newValue.TipoBilancio);
            
            BilanciDisponibili.Clear();
            foreach (var bilancio in bilanciFiltrati)
            {
                BilanciDisponibili.Add(bilancio);
            }
            
            System.Diagnostics.Debug.WriteLine($"[TEMPLATE SELEZIONATO] ✅ Bilanci disponibili filtrati per tipo {newValue.TipoBilancio}: {BilanciDisponibili.Count}");
        }
    }

    /// <summary>
    /// Carica template disponibili (TUTTI i template, non solo del cliente)
    /// </summary>
    private void CaricaTemplateDisponibili()
    {
        // ⭐ Carica TUTTI i template disponibili raggruppati per Mese+Anno+Descrizione
        // Ogni combinazione Mese+Anno+Descrizione è un template distinto
        var templates = _templateRepository.GetAll()
            .GroupBy(t => new { 
                t.Mese, 
                t.Anno, 
                Descrizione = t.DescrizioneBilancio ?? "" 
            })
            .Select(g => g.First()) // Prendo la prima voce per avere Mese, Anno, Descrizione
            .OrderByDescending(t => t.Anno)
            .ThenByDescending(t => t.Mese)
            .ThenBy(t => t.DescrizioneBilancio)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"\n[CARICA TEMPLATE DISPONIBILI] Totale template raggruppati: {templates.Count}");
        foreach (var t in templates.Take(5))
        {
            System.Diagnostics.Debug.WriteLine($"   - {t.DescrizioneBilancio ?? "(vuota)"} | Tipo: '{t.TipoBilancio ?? "NULL"}'");
        }

        TemplateDisponibili.Clear();
        foreach (var template in templates)
        {
            TemplateDisponibili.Add(template);
        }
    }

    /// <summary>
    /// Carica voci template base (senza formule) dal template selezionato
    /// </summary>
    private void CaricaVociTemplateBase(int mese, int anno)
    {
        // ⭐ IMPORTANTE: Carica voci del template specifico usando anche la descrizione
        string? descrizioneTemplate = TemplateSelezionato?.DescrizioneBilancio;
        
        // Ottiene il ClienteId dal template selezionato
        int? clienteId = TemplateSelezionato?.ClienteId;
        
        IEnumerable<BilancioTemplate> vociBase;
        
        if (clienteId.HasValue)
        {
            // Carica voci del template specifico (Cliente + Periodo + Descrizione)
            vociBase = _templateRepository.GetByClienteAndPeriodoAndDescrizione(
                clienteId.Value, mese, anno, descrizioneTemplate)
                .Where(x => string.IsNullOrWhiteSpace(x.Formula))
                .OrderByCodiceMastrinoNumerico(x => x.CodiceMastrino);
        }
        else
        {
            // Fallback: carica per periodo (solo se non c'è ClienteId)
            vociBase = _templateRepository.GetByPeriodo(mese, anno)
                .Where(x => string.IsNullOrWhiteSpace(x.Formula))
                .OrderByCodiceMastrinoNumerico(x => x.CodiceMastrino);
        }
        
        VociTemplateDisponibili.Clear();
        
        // ⭐ AGGIUNGI OPZIONE "NON ASSOCIARE" come prima opzione
        VociTemplateDisponibili.Add(new BilancioTemplate
        {
            Id = 0,
            CodiceMastrino = "",
            DescrizioneMastrino = "-- Non associare --",
            Mese = mese,
            Anno = anno
        });
        
        foreach (var voce in vociBase)
        {
            VociTemplateDisponibili.Add(voce);
        }

        // Aggiorna le voci disponibili in tutte le mappature
        foreach (var mappatura in Mappature)
        {
            mappatura.VociDisponibili = VociTemplateDisponibili;
        }
    }

    /// <summary>
    /// Salva associazione
    /// </summary>
    [RelayCommand]
    private void Salva()
    {
        // Validazione
        if (ClienteSelezionato == null)
        {
            MessageBox.Show("Seleziona un cliente.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (BilancioSelezionato == null)
        {
            MessageBox.Show("Seleziona un bilancio contabile.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (TemplateSelezionato == null)
        {
            MessageBox.Show("Seleziona un template di riclassificazione.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsSalvataggio = true;

            // ✅ USA repository direttamente (come TODO)
            int associazioneId;
            AssociazioneMastrino? associazione;

            if (IsModifica && _associazioneId.HasValue)
            {
                // Aggiorna associazione esistente
                associazione = _associazioneRepository.GetById(_associazioneId.Value);
                if (associazione == null)
                {
                    MessageBox.Show("Associazione non trovata", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                associazione.TemplateId = TemplateSelezionato?.Id;
                associazione.TemplatNome = TemplateSelezionato?.DescrizioneBilancio;
                associazione.Descrizione = Descrizione;
                associazione.BilancioDescrizione = BilancioSelezionato.Descrizione; // ⭐ Aggiorna descrizione bilancio
                
                _associazioneRepository.Update(associazione);
                associazioneId = associazione.Id;
            }
            else
            {
                // Crea nuova associazione
                associazione = new AssociazioneMastrino
                {
                    ClienteId = ClienteSelezionato.Id,
                    ClienteNome = ClienteSelezionato.NomeCliente,
                    Mese = BilancioSelezionato.Mese,
                    Anno = BilancioSelezionato.Anno,
                    BilancioDescrizione = BilancioSelezionato.Descrizione, // ⭐ Salva descrizione bilancio
                    TemplateId = TemplateSelezionato?.Id,
                    TemplatNome = TemplateSelezionato?.DescrizioneBilancio,
                    Descrizione = Descrizione,
                    CreatoBy = SessionManager.CurrentUser?.Id ?? 0,
                    CreatoByName = SessionManager.CurrentUser?.Username ?? "System"
                };

                associazioneId = _associazioneRepository.Insert(associazione);
            }

            // ✅ Salva dettagli usando repository direttamente
            // Prima elimina tutti i dettagli esistenti in un colpo solo
            _associazioneRepository.DeleteDettagliByAssociazione(associazioneId);

            // Poi inserisci TUTTI i nuovi dettagli (anche non associati)
            var dettagli = Mappature
                .Select(m => new AssociazioneMastrinoDettaglio
                {
                    AssociazioneId = associazioneId,
                    CodiceMastrino = m.CodiceMastrino,
                    DescrizioneMastrino = m.DescrizioneMastrino,
                    Importo = m.Importo,
                    TemplateVoceId = m.VoceTemplateSelezionata?.Id,
                    TemplateCodice = m.VoceTemplateSelezionata?.CodiceMastrino,
                    TemplateDescrizione = m.VoceTemplateSelezionata?.DescrizioneMastrino,
                    TemplateSegno = m.VoceTemplateSelezionata?.Segno
                })
                .ToList();

            foreach (var dettaglio in dettagli)
            {
                _associazioneRepository.InsertDettaglio(dettaglio);
            }

            // Aggiorna conteggio mappature
            associazione.NumeroMappature = dettagli.Count(d => d.TemplateVoceId.HasValue);
            _associazioneRepository.Update(associazione);

            // ✅ Log audit
            var app = (App)Application.Current;
            var auditService = new AuditLogService(app.Services!.GetRequiredService<CGEasyDbContext>());
            auditService.Log(
                SessionManager.CurrentUser?.Id ?? 0,
                SessionManager.CurrentUser?.Username ?? "System",
                IsModifica ? "Modifica" : "Creazione",
                $"AssociazioneMastrino",
                associazioneId,
                IsModifica ? 
                    $"Modificata associazione per {ClienteSelezionato.NomeCliente} - {BilancioSelezionato.Mese:00}/{BilancioSelezionato.Anno}" :
                    $"Creata nuova associazione per {ClienteSelezionato.NomeCliente} - {BilancioSelezionato.Mese:00}/{BilancioSelezionato.Anno}"
            );

            MessageBox.Show("Associazione salvata con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

            // ✅ Usa callback per chiudere dialog (come TODO)
            OnDialogClosed?.Invoke(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore salvataggio associazione:\n{ex.Message}\n\nStack: {ex.StackTrace}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSalvataggio = false;
        }
    }

    /// <summary>
    /// Annulla e chiude dialog
    /// </summary>
    [RelayCommand]
    private void Annulla()
    {
        // ✅ Usa callback per chiudere dialog (come TODO)
        OnDialogClosed?.Invoke(false);
    }

    /// <summary>
    /// Esporta associazione corrente in Excel
    /// </summary>
    [RelayCommand]
    private void EsportaExcel()
    {
        if (ClienteSelezionato == null || BilancioSelezionato == null)
        {
            MessageBox.Show("Seleziona prima cliente e bilancio.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Mappature.Count == 0)
        {
            MessageBox.Show("Nessuna mappatura da esportare.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Dialog per salvare file
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Associazione_{ClienteSelezionato.NomeCliente}_{BilancioSelezionato.Mese:00}_{BilancioSelezionato.Anno}.xlsx",
                Title = "Esporta Associazione in Excel",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            // Crea file Excel con ClosedXML
            using var workbook = new XLWorkbook();

            // === FOGLIO 1: MAPPATURE ===
            var worksheet = workbook.Worksheets.Add("Mappature");

            // Titolo
            var titleRange = worksheet.Range("A1:F1");
            titleRange.Merge();
            titleRange.Value = $"ASSOCIAZIONE MASTRINI - {ClienteSelezionato.NomeCliente} - {BilancioSelezionato.PeriodoDisplay}";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(33, 150, 243);
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Height = 30;

            // Info associazione
            worksheet.Cell(2, 1).Value = "Template:";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Cell(2, 2).Value = TemplateSelezionato?.DescrizioneBilancio ?? "Non selezionato";
            worksheet.Range("B2:F2").Merge();

            if (!string.IsNullOrWhiteSpace(Descrizione))
            {
                worksheet.Cell(3, 1).Value = "Descrizione:";
                worksheet.Cell(3, 1).Style.Font.Bold = true;
                worksheet.Cell(3, 2).Value = Descrizione;
                worksheet.Range("B3:F3").Merge();
            }

            // Intestazioni tabella (riga 5)
            int startRow = 5;
            var headers = new[]
            {
                "Codice Mastrino",
                "Descrizione Mastrino",
                "Importo",
                "➜ Codice Template",
                "➜ Descrizione Template",
                "➜ Segno"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(startRow, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Dati mappature
            int row = startRow + 1;
            int mappateCount = 0;
            int nonMappateCount = 0;

            foreach (var mappatura in Mappature.OrderByCodiceMastrinoNumerico(m => m.CodiceMastrino))
            {
                worksheet.Cell(row, 1).Value = mappatura.CodiceMastrino;
                worksheet.Cell(row, 2).Value = mappatura.DescrizioneMastrino;
                worksheet.Cell(row, 3).Value = mappatura.Importo;
                worksheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 €";
                
                if (mappatura.VoceTemplateSelezionata != null)
                {
                    worksheet.Cell(row, 4).Value = mappatura.VoceTemplateSelezionata.CodiceMastrino;
                    worksheet.Cell(row, 5).Value = mappatura.VoceTemplateSelezionata.DescrizioneMastrino;
                    worksheet.Cell(row, 6).Value = mappatura.VoceTemplateSelezionata.Segno;
                    
                    // Sfondo verde chiaro per righe mappate
                    var rowRange = worksheet.Range(row, 1, row, 6);
                    rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(200, 255, 200);
                    mappateCount++;
                }
                else
                {
                    worksheet.Cell(row, 4).Value = "NON ASSOCIATO";
                    worksheet.Cell(row, 5).Value = "-";
                    worksheet.Cell(row, 6).Value = "-";
                    
                    // Sfondo rosso chiaro per righe non mappate
                    var rowRange = worksheet.Range(row, 1, row, 6);
                    rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 230);
                    nonMappateCount++;
                }

                // Bordi
                worksheet.Range(row, 1, row, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row++;
            }

            // Statistiche in fondo
            row += 2;
            worksheet.Cell(row, 1).Value = "📊 STATISTICHE:";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Font.FontSize = 12;
            
            row++;
            worksheet.Cell(row, 1).Value = "Totale Mastrini:";
            worksheet.Cell(row, 2).Value = Mappature.Count;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            
            row++;
            worksheet.Cell(row, 1).Value = "Mappati:";
            worksheet.Cell(row, 2).Value = mappateCount;
            worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.Green;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            
            row++;
            worksheet.Cell(row, 1).Value = "Non Mappati:";
            worksheet.Cell(row, 2).Value = nonMappateCount;
            worksheet.Cell(row, 2).Style.Font.FontColor = XLColor.Red;
            worksheet.Cell(row, 2).Style.Font.Bold = true;
            
            row++;
            worksheet.Cell(row, 1).Value = "Percentuale Completamento:";
            worksheet.Cell(row, 2).Value = Mappature.Count > 0 
                ? $"{(mappateCount * 100.0 / Mappature.Count):F1}%" 
                : "0%";
            worksheet.Cell(row, 2).Style.Font.Bold = true;

            // Auto-fit colonne
            worksheet.Columns().AdjustToContents();

            // Salva file
            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show(
                $"✅ Associazione esportata con successo!\n\n" +
                $"File: {System.IO.Path.GetFileName(saveDialog.FileName)}\n" +
                $"Percorso: {saveDialog.FileName}\n\n" +
                $"📊 Statistiche:\n" +
                $"• Totale: {Mappature.Count} mastrini\n" +
                $"• Mappati: {mappateCount}\n" +
                $"• Non Mappati: {nonMappateCount}",
                "Export Completato",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore esportazione Excel:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

/// <summary>
/// ViewModel per singola riga di mappatura
/// </summary>
public partial class MappaturaMastrinoViewModel : ObservableObject
{
    public string CodiceMastrino { get; set; } = string.Empty;
    public string DescrizioneMastrino { get; set; } = string.Empty;
    public decimal Importo { get; set; }
    public string ImportoFormatted => Importo.ToString("C2");

    [ObservableProperty]
    private BilancioTemplate? _voceTemplateSelezionata;

    [ObservableProperty]
    private ObservableCollection<BilancioTemplate> _vociDisponibili;

    public MappaturaMastrinoViewModel(
        AssociazioneMastrinoDettaglio dettaglio,
        ObservableCollection<BilancioTemplate> vociDisponibili)
    {
        CodiceMastrino = dettaglio.CodiceMastrino;
        DescrizioneMastrino = dettaglio.DescrizioneMastrino;
        Importo = dettaglio.Importo;
        _vociDisponibili = vociDisponibili;
    }
}

