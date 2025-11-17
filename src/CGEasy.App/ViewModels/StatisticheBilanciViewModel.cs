using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using CGEasy.Core.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;

namespace CGEasy.App.ViewModels;

public partial class StatisticheBilanciViewModel : ObservableObject
{
    private readonly LiteDbContext _context;
    private readonly BilancioStatisticaService _service;
    private readonly ClienteRepository _clienteRepository;
    private readonly BilancioContabileRepository _bilancioRepository;
    private readonly BilancioTemplateRepository _templateRepository;
    private readonly AssociazioneMastrinoRepository _associazioneRepository;

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

    public StatisticheBilanciViewModel(LiteDbContext context)
    {
        _context = context;
        _service = new BilancioStatisticaService(context);
        _clienteRepository = new ClienteRepository(context);
        _bilancioRepository = new BilancioContabileRepository(context);
        _templateRepository = new BilancioTemplateRepository(context);
        _associazioneRepository = new AssociazioneMastrinoRepository(context);

        CaricaClienti();
        CaricaTemplateDisponibili();
    }

    // Costruttore senza parametri per XAML designer - usa Singleton
    public StatisticheBilanciViewModel() : this(GetOrCreateContext())
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
        var templates = _templateRepository.GetAll()
            .GroupBy(t => new { t.Mese, t.Anno })
            .Select(g => g.First())
            .OrderByDescending(t => t.Anno)
            .ThenByDescending(t => t.Mese)
            .ToList();

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

        // Carica bilanci del cliente
        var bilanci = _bilancioRepository.GetGruppiByCliente(value.Id);
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
                TemplateSelezionato.Anno);

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
                FileName = $"Statistiche_{ClienteSelezionato!.NomeCliente}_{periodiStr}.xlsx",
                Title = "Esporta Statistiche in Excel",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Statistiche");

            // Titolo
            var periodiStr2 = string.Join(", ", BilanciSelezionati.OrderBy(b => b.Anno).ThenBy(b => b.Mese).Select(b => b.PeriodoDisplay));
            var titleRange = worksheet.Range("A1:Z1");
            titleRange.Merge();
            titleRange.Value = $"STATISTICHE BILANCIO MULTI-PERIODO - {ClienteSelezionato.NomeCliente}";
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
            
            worksheet.Cell(startRow, col++).Value = "TOTALE";
            worksheet.Cell(startRow, col++).Value = "% TOTALE";
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

                // Totale
                worksheet.Cell(row, col).Value = stat.ImportoTotale;
                worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(row, col).Style.Font.Bold = true;
                col++;

                worksheet.Cell(row, col).Value = $"{stat.PercentualeTotale:F2}%";
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                col++;

                worksheet.Cell(row, col).Value = stat.Formula ?? "";

                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(saveDialog.FileName);

            MessageBox.Show("Statistiche esportate con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore esportazione:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

