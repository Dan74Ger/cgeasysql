using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using CGEasy.Core.Repositories;
using CGEasy.App.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using System.Text.Json;

namespace CGEasy.App.ViewModels;

public partial class StatisticheBilanciSPViewModel : ObservableObject
{
    private readonly LiteDbContext _context;
    private readonly BilancioStatisticaService _service;
    private readonly ClienteRepository _clienteRepository;
    private readonly BilancioContabileRepository _bilancioRepository;
    private readonly BilancioTemplateRepository _templateRepository;
    private readonly AssociazioneMastrinoRepository _associazioneRepository;
    private readonly StatisticaSPSalvataRepository _statisticheSalvateRepository;

    [ObservableProperty]
    private ObservableCollection<Cliente> _clienti = new();

    [ObservableProperty]
    private Cliente? _clienteSelezionato;

    [ObservableProperty]
    private ObservableCollection<BilancioGruppo> _bilanciDisponibili = new();

    [ObservableProperty]
    private ObservableCollection<BilancioGruppo> _bilanciSelezionati = new();

    [ObservableProperty]
    private ObservableCollection<BilancioTemplate> _templateDisponibili = new();

    [ObservableProperty]
    private BilancioTemplate? _templateSelezionato;

    [ObservableProperty]
    private ObservableCollection<BilancioStatisticaMultiPeriodo> _statisticheMulti = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasStatistiche;

    [ObservableProperty]
    private ObservableCollection<string> _periodiColonne = new();

    [ObservableProperty]
    private bool _hasBilanciSelezionati;

    [ObservableProperty]
    private string _bilanciSelezionatiDisplay = string.Empty;

    [ObservableProperty]
    private ObservableCollection<StatisticaSPSalvata> _statisticheSalvate = new();

    [ObservableProperty]
    private StatisticaSPSalvata? _statisticaSalvataSelezionata;

    public StatisticheBilanciSPViewModel(LiteDbContext context)
    {
        _context = context;
        _service = new BilancioStatisticaService(context);
        _clienteRepository = new ClienteRepository(context);
        _bilancioRepository = new BilancioContabileRepository(context);
        _templateRepository = new BilancioTemplateRepository(context);
        _associazioneRepository = new AssociazioneMastrinoRepository(context);
        _statisticheSalvateRepository = new StatisticaSPSalvataRepository(context);

        CaricaClienti();
        CaricaTemplateDisponibili();
        CaricaStatisticheSalvate();
    }

    // Costruttore senza parametri per XAML designer - usa Singleton
    public StatisticheBilanciSPViewModel() : this(GetOrCreateContext())
    {
    }

    private static LiteDbContext GetOrCreateContext()
    {
        var context = App.GetService<LiteDbContext>();
        if (context == null)
        {
            context = new LiteDbContext();
            context.MarkAsSingleton(); // Marca anche questo come singleton
        }
        return context;
    }

    /// <summary>
    /// Carica clienti che hanno bilanci importati
    /// </summary>
    private void CaricaClienti()
    {
        var clientiConBilanci = _bilancioRepository.GetAll()
            .Select(b => b.ClienteId)
            .Distinct()
            .ToList();

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
    /// Carica template disponibili
    /// </summary>
    private void CaricaTemplateDisponibili()
    {
        // ⭐ Raggruppa per Mese, Anno E DESCRIZIONE per distinguere template diversi con stesso periodo
        // ⭐ FILTRA solo template SP
        var tuttiTemplate = _templateRepository.GetAll().ToList();
        
        System.Diagnostics.Debug.WriteLine($"[STATISTICHE SP] Totale template nel DB: {tuttiTemplate.Count}");
        foreach (var t in tuttiTemplate.Take(5))
        {
            System.Diagnostics.Debug.WriteLine($"  - Desc: '{t.DescrizioneBilancio}', Tipo: '{t.TipoBilancio ?? "(null)"}'");
        }
        
        var templates = tuttiTemplate
            .Where(t => t.TipoBilancio == "SP") // Solo template SP
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
        
        System.Diagnostics.Debug.WriteLine($"[STATISTICHE SP] Template SP filtrati: {templates.Count}");

        TemplateDisponibili.Clear();
        foreach (var template in templates)
        {
            TemplateDisponibili.Add(template);
        }
    }

    /// <summary>
    /// Quando cambia il cliente
    /// </summary>
    partial void OnClienteSelezionatoChanged(Cliente? value)
    {
        if (value == null) return;

        // ⭐ Carica SOLO i bilanci SP del cliente
        var bilanci = _bilancioRepository.GetGruppiByCliente(value.Id)
            .Where(b => b.TipoBilancio == "SP")
            .ToList();
            
        BilanciDisponibili.Clear();
        foreach (var bilancio in bilanci)
        {
            bilancio.IsSelected = false; // Reset selezione
            bilancio.PropertyChanged += Bilancio_PropertyChanged; // Ascolta cambiamenti checkbox
            BilanciDisponibili.Add(bilancio);
        }

        // Pulisci selezioni
        BilanciSelezionati.Clear();
        StatisticheMulti.Clear();
        PeriodiColonne.Clear();
        HasStatistiche = false;
        AggiornaBilanciSelezionati();
    }

    /// <summary>
    /// Gestisce il cambio di stato del checkbox
    /// </summary>
    private void Bilancio_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BilancioGruppo.IsSelected))
        {
            AggiornaBilanciSelezionati();
        }
    }

    /// <summary>
    /// Aggiorna la lista dei bilanci selezionati e il riepilogo
    /// </summary>
    private void AggiornaBilanciSelezionati()
    {
        BilanciSelezionati.Clear();
        var selezionati = BilanciDisponibili.Where(b => b.IsSelected).OrderBy(b => b.Anno).ThenBy(b => b.Mese).ToList();
        
        foreach (var bilancio in selezionati)
        {
            BilanciSelezionati.Add(bilancio);
        }

        HasBilanciSelezionati = BilanciSelezionati.Count > 0;
        BilanciSelezionatiDisplay = HasBilanciSelezionati 
            ? string.Join(", ", BilanciSelezionati.Select(b => b.PeriodoDisplay))
            : string.Empty;

        // Pulisci statistiche quando cambia la selezione
        StatisticheMulti.Clear();
        HasStatistiche = false;
    }

    /// <summary>
    /// Aggiungi bilancio alla selezione
    /// </summary>
    [RelayCommand]
    private void AggiungiBilancio(BilancioGruppo? bilancio)
    {
        if (bilancio == null) return;

        if (!BilanciSelezionati.Contains(bilancio))
        {
            BilanciSelezionati.Add(bilancio);
            StatisticheMulti.Clear();
            HasStatistiche = false;
        }
    }

    /// <summary>
    /// Rimuovi bilancio dalla selezione
    /// </summary>
    [RelayCommand]
    private void RimuoviBilancio(BilancioGruppo? bilancio)
    {
        if (bilancio == null) return;

        BilanciSelezionati.Remove(bilancio);
        StatisticheMulti.Clear();
        HasStatistiche = false;
    }

    /// <summary>
    /// Genera statistiche
    /// </summary>
    [RelayCommand]
    private void GeneraStatistiche()
    {
        if (ClienteSelezionato == null)
        {
            MessageBox.Show("Seleziona un cliente.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (BilanciSelezionati.Count == 0)
        {
            MessageBox.Show("Seleziona almeno un bilancio contabile.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (TemplateSelezionato == null)
        {
            MessageBox.Show("Seleziona un template.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // Prepara lista periodi
            var periodi = BilanciSelezionati
                .OrderBy(b => b.Anno)
                .ThenBy(b => b.Mese)
                .Select(b => (b.Mese, b.Anno))
                .ToList();

            // Genera statistiche multi-periodo
            var statistiche = _service.GeneraStatisticheMultiPeriodo(
                ClienteSelezionato.Id,
                periodi,
                TemplateSelezionato.Mese,
                TemplateSelezionato.Anno,
                "SP", // ⭐ Filtra SOLO bilanci SP
                TemplateSelezionato.DescrizioneBilancio); // ⭐ Passa la descrizione per il template esatto

            // 🔴 PER SP: Ricalcola percentuali basate su TOTALE ATTIVITÀ invece di TOTALE FATTURATO
            RicalcolaPercentualiConTotaleAttivita(statistiche);

            StatisticheMulti.Clear();
            foreach (var stat in statistiche)
            {
                StatisticheMulti.Add(stat);
            }

            // Prepara colonne periodi per il DataGrid
            PeriodiColonne.Clear();
            foreach (var bilancio in BilanciSelezionati.OrderBy(b => b.Anno).ThenBy(b => b.Mese))
            {
                PeriodiColonne.Add($"{bilancio.Mese:D2}_{bilancio.Anno}");
            }

            HasStatistiche = StatisticheMulti.Count > 0;

            // Forza il refresh delle proprietà per triggare la generazione delle colonne
            OnPropertyChanged(nameof(StatisticheMulti));
            OnPropertyChanged(nameof(PeriodiColonne));

            if (!HasStatistiche)
            {
                MessageBox.Show("Nessuna statistica generata. Verificare le associazioni.",
                    "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore generazione statistiche:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Mostra dettaglio associazioni per una riga multi-periodo
    /// </summary>
    [RelayCommand]
    private void MostraDettaglioAssociazioni(BilancioStatisticaMultiPeriodo? statistica)
    {
        if (statistica == null || !statistica.HasContiAssociati)
            return;

        try
        {
            var dialog = new Views.DettaglioAssociazioniDialogView();
            var dialogViewModel = new DettaglioAssociazioniViewModel(
                statistica,
                ClienteSelezionato!.NomeCliente);
            
            dialog.DataContext = dialogViewModel;
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dettaglio:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Esporta statistiche in Excel
    /// </summary>
    [RelayCommand]
    private void EsportaExcel()
    {
        if (!HasStatistiche)
        {
            MessageBox.Show("Genera prima le statistiche.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var periodiStr = string.Join("_", BilanciSelezionati.Select(b => $"{b.Mese:00}{b.Anno}"));
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Statistiche_SP_{ClienteSelezionato!.NomeCliente}_{periodiStr}.xlsx",
                Title = "Esporta Statistiche SP in Excel",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            System.Diagnostics.Debug.WriteLine($"\n📊 [EXPORT EXCEL] ========== INIZIO EXPORT ==========");
            
            // 🔍 VERIFICA DATI PRIMA DELL'EXPORT
            System.Diagnostics.Debug.WriteLine($"\n🔍 VERIFICA DATI StatisticheMulti:");
            int totVociConConti = 0;
            int totConti = 0;
            foreach (var stat in StatisticheMulti.Take(10))
            {
                if (stat.DatiPerPeriodo != null && stat.DatiPerPeriodo.Count > 0)
                {
                    foreach (var kvp in stat.DatiPerPeriodo)
                    {
                        int contiCount = kvp.Value.Conti?.Count ?? 0;
                        if (contiCount > 0)
                        {
                            totVociConConti++;
                            totConti += contiCount;
                            System.Diagnostics.Debug.WriteLine($"  ✅ {stat.Codice} - Periodo {kvp.Value.PeriodoDisplay}: {contiCount} conti");
                            
                            // Mostra primi 2 conti
                            foreach (var c in kvp.Value.Conti!.Take(2))
                            {
                                System.Diagnostics.Debug.WriteLine($"      - {c.CodiceConto} | {c.DescrizioneConto} | {c.Importo:N2}");
                            }
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"📊 TOTALE: {totVociConConti} voci/periodi con conti, {totConti} conti totali\n");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Statistiche SP");

            // Titolo
            var periodiStr2 = string.Join(", ", BilanciSelezionati.OrderBy(b => b.Anno).ThenBy(b => b.Mese).Select(b => b.PeriodoDisplay));
            var titleRange = worksheet.Range("A1:Z1");
            titleRange.Merge();
            titleRange.Value = $"STATISTICHE BILANCIO SP MULTI-PERIODO - {ClienteSelezionato.NomeCliente}";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(33, 150, 243);
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(1).Height = 30;

            // Info
            worksheet.Cell(2, 1).Value = "Template:";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Cell(2, 2).Value = TemplateSelezionato!.DescrizioneBilancio;
            
            worksheet.Cell(3, 1).Value = "Periodi:";
            worksheet.Cell(3, 1).Style.Font.Bold = true;
            worksheet.Cell(3, 2).Value = periodiStr2;

            // Intestazioni
            int startRow = 5;
            int col = 1;
            
            worksheet.Cell(startRow, col++).Value = "Codice";
            worksheet.Cell(startRow, col++).Value = "Descrizione";
            
            // Colonne dinamiche per ogni periodo
            foreach (var bilancio in BilanciSelezionati.OrderBy(b => b.Anno).ThenBy(b => b.Mese))
            {
                worksheet.Cell(startRow, col++).Value = $"Importo {bilancio.PeriodoDisplay}";
                worksheet.Cell(startRow, col++).Value = $"% {bilancio.PeriodoDisplay}";
            }
            
            worksheet.Cell(startRow, col++).Value = "Formula";

            // Stile intestazioni
            for (int i = 1; i < col; i++)
            {
                var cell = worksheet.Cell(startRow, i);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Dati
            int row = startRow + 1;
            foreach (var stat in StatisticheMulti)
            {
                col = 1;
                worksheet.Cell(row, col++).Value = stat.Codice;
                worksheet.Cell(row, col++).Value = stat.Descrizione;

                // Dati per ogni periodo
                foreach (var periodoKey in PeriodiColonne)
                {
                    if (stat.DatiPerPeriodo.TryGetValue(periodoKey, out var datiPeriodo))
                    {
                        worksheet.Cell(row, col).Value = datiPeriodo.Importo;
                        worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00 €";
                        col++;
                        
                        worksheet.Cell(row, col).Value = $"{datiPeriodo.Percentuale:F2}%";
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        col++;
                    }
                    else
                    {
                        col += 2;
                    }
                }

                worksheet.Cell(row, col).Value = stat.Formula ?? "";

                row++;
            }

            worksheet.Columns().AdjustToContents();

            // ===== FOGLIO 2: DETTAGLI MASTRINI ASSOCIATI =====
            System.Diagnostics.Debug.WriteLine($"\n📋 [EXPORT EXCEL] Creazione foglio 'Dettagli Mastrini'");
            
            var dettagliSheet = workbook.Worksheets.Add("Dettagli Mastrini");
            
            // Titolo
            var dettagliTitleRange = dettagliSheet.Range("A1:F1");
            dettagliTitleRange.Merge();
            dettagliTitleRange.Value = "DETTAGLI MASTRINI ASSOCIATI";
            dettagliTitleRange.Style.Font.Bold = true;
            dettagliTitleRange.Style.Font.FontSize = 14;
            dettagliTitleRange.Style.Fill.BackgroundColor = XLColor.FromArgb(76, 175, 80);
            dettagliTitleRange.Style.Font.FontColor = XLColor.White;
            dettagliTitleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            dettagliSheet.Row(1).Height = 30;

            // Intestazioni
            int dettagliRow = 3;
            dettagliSheet.Cell(dettagliRow, 1).Value = "Voce Template";
            dettagliSheet.Cell(dettagliRow, 2).Value = "Codice Template";
            dettagliSheet.Cell(dettagliRow, 3).Value = "Periodo";
            dettagliSheet.Cell(dettagliRow, 4).Value = "Codice Mastrino";
            dettagliSheet.Cell(dettagliRow, 5).Value = "Descrizione Mastrino";
            dettagliSheet.Cell(dettagliRow, 6).Value = "Importo";

            // Stile intestazioni
            for (int i = 1; i <= 6; i++)
            {
                var cell = dettagliSheet.Cell(dettagliRow, i);
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(129, 199, 132);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            dettagliRow++;
            int righeDettaglioScritte = 0;

            // ✅ NUOVA LOGICA: Itera su DatiPerPeriodo[].Conti invece di ContiAssociatiTutti
            System.Diagnostics.Debug.WriteLine($"📝 [EXPORT EXCEL] Inizio scrittura dettagli da DatiPerPeriodo");
            
            foreach (var stat in StatisticheMulti)
            {
                // Salta voci con formula (non hanno conti associati)
                if (!string.IsNullOrWhiteSpace(stat.Formula))
                    continue;

                System.Diagnostics.Debug.WriteLine($"\n  🔍 Voce: {stat.Codice} - {stat.Descrizione}");
                System.Diagnostics.Debug.WriteLine($"      DatiPerPeriodo.Count: {stat.DatiPerPeriodo?.Count ?? 0}");

                if (stat.DatiPerPeriodo == null || stat.DatiPerPeriodo.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"      ⚠️ DatiPerPeriodo vuoto, salto");
                    continue;
                }

                // Per ogni periodo, scrivi i conti associati
                foreach (var kvp in stat.DatiPerPeriodo.OrderBy(x => x.Key))
                {
                    var datiPeriodo = kvp.Value;
                    System.Diagnostics.Debug.WriteLine($"      📅 Periodo {datiPeriodo.PeriodoDisplay}: {datiPeriodo.Conti?.Count ?? 0} conti");

                    if (datiPeriodo.Conti == null || datiPeriodo.Conti.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"          ⚠️ Nessun conto per questo periodo");
                        continue;
                    }

                    // Scrivi ogni conto
                    foreach (var conto in datiPeriodo.Conti)
                    {
                        dettagliSheet.Cell(dettagliRow, 1).Value = stat.Descrizione;
                        dettagliSheet.Cell(dettagliRow, 2).Value = stat.Codice;
                        dettagliSheet.Cell(dettagliRow, 3).Value = datiPeriodo.PeriodoDisplay;
                        dettagliSheet.Cell(dettagliRow, 4).Value = conto.CodiceConto;
                        dettagliSheet.Cell(dettagliRow, 5).Value = conto.DescrizioneConto;
                        dettagliSheet.Cell(dettagliRow, 6).Value = conto.Importo;
                        dettagliSheet.Cell(dettagliRow, 6).Style.NumberFormat.Format = "#,##0.00 €";

                        System.Diagnostics.Debug.WriteLine($"          ✅ Riga {dettagliRow}: {conto.CodiceConto} - {conto.DescrizioneConto} - {conto.Importo:N2} €");

                        dettagliRow++;
                        righeDettaglioScritte++;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"\n✅ [EXPORT EXCEL] Totale righe dettaglio scritte: {righeDettaglioScritte}");

            // Se non ci sono dettagli
            if (righeDettaglioScritte == 0)
            {
                dettagliSheet.Cell(4, 1).Value = "Nessun mastrino associato trovato";
                dettagliSheet.Range("A4:F4").Merge();
                dettagliSheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dettagliSheet.Cell(4, 1).Style.Font.Italic = true;
                System.Diagnostics.Debug.WriteLine($"⚠️ [EXPORT EXCEL] Nessun dettaglio trovato!");
            }

            dettagliSheet.Columns().AdjustToContents();

            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show("Statistiche SP esportate con successo!\n\nInclude foglio 'Dettagli Mastrini' con i conti associati.",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore esportazione:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Carica lista statistiche salvate
    /// </summary>
    private void CaricaStatisticheSalvate()
    {
        var salvate = _statisticheSalvateRepository.GetAll();
        StatisticheSalvate.Clear();
        foreach (var stat in salvate)
        {
            StatisticheSalvate.Add(stat);
        }
    }

    /// <summary>
    /// Salva le statistiche correnti
    /// </summary>
    [RelayCommand]
    private void SalvaStatistiche()
    {
        if (!HasStatistiche)
        {
            MessageBox.Show("Genera prima le statistiche da salvare.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Chiedi nome
        var dialog = new Views.InputDialogView
        {
            Title = "Salva Statistiche SP",
            Prompt = "Inserisci un nome descrittivo per queste statistiche:",
            Value = $"Statistiche SP - {ClienteSelezionato!.NomeCliente} - {DateTime.Now:dd/MM/yyyy}",
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.Value))
            return;

        try
        {
            // Serializza periodi
            var periodiList = BilanciSelezionati.Select(b => new { b.Mese, b.Anno }).ToList();
            var periodiJson = JsonSerializer.Serialize(periodiList);

            // Serializza dati statistiche
            var datiJson = JsonSerializer.Serialize(StatisticheMulti.ToList());

            var utenteCorrente = SessionManager.CurrentUser;

            var statistica = new StatisticaSPSalvata
            {
                NomeStatistica = dialog.Value.Trim(),
                ClienteId = ClienteSelezionato!.Id,
                NomeCliente = ClienteSelezionato.NomeCliente,
                TemplateMese = TemplateSelezionato!.Mese,
                TemplateAnno = TemplateSelezionato.Anno,
                TemplateDescrizione = TemplateSelezionato.DescrizioneBilancio ?? "",
                PeriodiJson = periodiJson,
                DatiStatisticheJson = datiJson,
                DataCreazione = DateTime.Now,
                UtenteId = utenteCorrente?.Id ?? 1,
                NomeUtente = utenteCorrente?.Username ?? "admin"
            };

            _statisticheSalvateRepository.Insert(statistica);
            CaricaStatisticheSalvate();

            MessageBox.Show("Statistiche salvate con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore salvataggio statistiche:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Carica statistiche salvate
    /// </summary>
    [RelayCommand]
    private void CaricaStatisticaSalvata(StatisticaSPSalvata? statistica)
    {
        if (statistica == null) return;

        try
        {
            // Deserializza periodi
            var periodiList = JsonSerializer.Deserialize<List<PeriodiInfo>>(statistica.PeriodiJson);

            if (periodiList == null)
            {
                MessageBox.Show("Errore deserializzazione periodi.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ricarica cliente
            var cliente = _clienteRepository.GetById(statistica.ClienteId);
            if (cliente == null)
            {
                MessageBox.Show("Cliente non trovato.",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ClienteSelezionato = cliente;

            // Ricarica template
            // ⭐ IMPORTANTE: Usa anche la descrizione per trovare il template ESATTO
            var template = _templateRepository.GetAll()
                .FirstOrDefault(t => t.Mese == statistica.TemplateMese && 
                                     t.Anno == statistica.TemplateAnno &&
                                     (t.DescrizioneBilancio ?? "") == (statistica.TemplateDescrizione ?? ""));
            
            if (template == null)
            {
                // Fallback: prova senza descrizione (per compatibilità con vecchie statistiche)
                template = _templateRepository.GetByPeriodo(statistica.TemplateMese, statistica.TemplateAnno).FirstOrDefault();
            }
            
            TemplateSelezionato = template;

            // Ricarica bilanci selezionati
            BilanciSelezionati.Clear();
            foreach (var periodo in periodiList!)
            {
                // ⭐ Filtra SOLO bilanci SP (non CE!)
                var gruppo = _bilancioRepository.GetGruppiByCliente(cliente.Id)
                    .FirstOrDefault(g => g.Mese == periodo.Mese && 
                                         g.Anno == periodo.Anno && 
                                         g.TipoBilancio == "SP");
                if (gruppo != null)
                {
                    BilanciSelezionati.Add(gruppo);
                }
            }

            // ✅ IMPORTANTE: RIGENERA le statistiche invece di deserializzare
            // Questo assicura che tutti i dettagli dei conti siano presenti
            var periodi = periodiList.Select(p => (p.Mese, p.Anno)).ToList();
            var statisticheRigenerate = _service.GeneraStatisticheMultiPeriodo(
                cliente.Id,
                periodi,
                statistica.TemplateMese,
                statistica.TemplateAnno,
                "SP", // ⭐ Filtra SOLO bilanci SP
                statistica.TemplateDescrizione); // ⭐ Passa la descrizione per il template esatto

            // Ricalcola percentuali con TOTALE ATTIVITÀ
            RicalcolaPercentualiConTotaleAttivita(statisticheRigenerate);

            // Carica dati statistiche
            StatisticheMulti.Clear();
            foreach (var stat in statisticheRigenerate)
            {
                StatisticheMulti.Add(stat);
            }

            // Prepara colonne periodi
            PeriodiColonne.Clear();
            foreach (var bilancio in BilanciSelezionati.OrderBy(b => b.Anno).ThenBy(b => b.Mese))
            {
                PeriodiColonne.Add($"{bilancio.Mese:D2}_{bilancio.Anno}");
            }

            HasStatistiche = StatisticheMulti.Count > 0;
            HasBilanciSelezionati = BilanciSelezionati.Count > 0;
            BilanciSelezionatiDisplay = string.Join(", ", BilanciSelezionati.Select(b => b.PeriodoDisplay));

            // Forza refresh
            OnPropertyChanged(nameof(StatisticheMulti));
            OnPropertyChanged(nameof(PeriodiColonne));

            MessageBox.Show($"Statistiche '{statistica.NomeStatistica}' caricate e rigenerate!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento statistiche:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Rinomina una statistica salvata
    /// </summary>
    [RelayCommand]
    private void RinominaStatisticaSalvata(StatisticaSPSalvata? statistica)
    {
        if (statistica == null) return;

        var inputDialog = new InputDialogView
        {
            Title = "Rinomina Statistica",
            Prompt = "Nuovo nome:",
            Value = statistica.NomeStatistica,
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (inputDialog.ShowDialog() == true)
        {
            var nuovoNome = inputDialog.Value?.Trim();
            if (string.IsNullOrWhiteSpace(nuovoNome))
            {
                MessageBox.Show("Il nome non può essere vuoto.",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica se esiste già una statistica con questo nome
            var esistente = StatisticheSalvate.FirstOrDefault(s => 
                s.Id != statistica.Id && 
                s.NomeStatistica.Equals(nuovoNome, StringComparison.OrdinalIgnoreCase));
            
            if (esistente != null)
            {
                MessageBox.Show($"Esiste già una statistica salvata con il nome '{nuovoNome}'.",
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Aggiorna il nome
            statistica.NomeStatistica = nuovoNome;
            _statisticheSalvateRepository.Update(statistica);

            MessageBox.Show("Nome statistica aggiornato con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

            // Ricarica la lista
            CaricaStatisticheSalvate();
        }
    }

    /// <summary>
    /// Cancella statistica salvata
    /// </summary>
    [RelayCommand]
    private void CancellaStatisticaSalvata(StatisticaSPSalvata? statistica)
    {
        if (statistica == null) return;

        var result = MessageBox.Show(
            $"Confermi di voler eliminare la statistica '{statistica.NomeStatistica}'?",
            "Conferma eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            _statisticheSalvateRepository.Delete(statistica.Id);
            CaricaStatisticheSalvate();

            MessageBox.Show("Statistica eliminata con successo.",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore eliminazione:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Classe helper per deserializzare periodi
    private class PeriodiInfo
    {
        public int Mese { get; set; }
        public int Anno { get; set; }
    }

    /// <summary>
    /// Ricalcola le percentuali basandosi su TOTALE ATTIVITÀ invece di TOTALE FATTURATO
    /// </summary>
    private void RicalcolaPercentualiConTotaleAttivita(System.Collections.Generic.List<BilancioStatisticaMultiPeriodo> statistiche)
    {
        // Trova la riga TOTALE ATTIVITÀ
        var totaleAttivita = statistiche.FirstOrDefault(s =>
            s.Descrizione.ToUpper().Contains("TOTALE ATTIVITÀ") ||
            s.Descrizione.ToUpper().Contains("TOTALE ATTIVITA"));

        if (totaleAttivita == null)
        {
            System.Diagnostics.Debug.WriteLine("⚠️ TOTALE ATTIVITÀ non trovato nelle statistiche SP");
            return;
        }

        // Ricalcola percentuali per ogni periodo
        foreach (var periodoKey in totaleAttivita.DatiPerPeriodo.Keys)
        {
            var importoTotaleAttivita = totaleAttivita.DatiPerPeriodo[periodoKey].Importo;
            decimal baseTotaleAttivita = Math.Abs(importoTotaleAttivita);

            if (baseTotaleAttivita == 0)
                continue;

            // Ricalcola percentuale per ogni voce in questo periodo
            foreach (var stat in statistiche)
            {
                if (stat.DatiPerPeriodo.ContainsKey(periodoKey))
                {
                    stat.DatiPerPeriodo[periodoKey].Percentuale =
                        (Math.Abs(stat.DatiPerPeriodo[periodoKey].Importo) / baseTotaleAttivita) * 100;
                }
            }
        }

        // Ricalcola percentuali totali
        decimal baseTotaleAttivitaTotale = Math.Abs(totaleAttivita.ImportoTotale);
        
        if (baseTotaleAttivitaTotale != 0)
        {
            foreach (var stat in statistiche)
            {
                stat.PercentualeTotale = (Math.Abs(stat.ImportoTotale) / baseTotaleAttivitaTotale) * 100;
            }
        }

        System.Diagnostics.Debug.WriteLine($"✅ Percentuali SP ricalcolate con base TOTALE ATTIVITÀ: {baseTotaleAttivitaTotale:N2} €");
    }
}

