using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// Wrapper per indice con selezione temporanea
/// </summary>
public partial class IndiceSelezionabile : ObservableObject
{
    public IndicePersonalizzato Indice { get; set; }
    public string NomeIndice => Indice.NomeIndice;
    
    [ObservableProperty]
    private bool _isSelezionato = true; // Default: tutti selezionati

    public IndiceSelezionabile(IndicePersonalizzato indice)
    {
        Indice = indice;
    }
}

public partial class IndiciDiBilancioViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly ClienteRepository _clienteRepo;
    private readonly StatisticaCESalvataRepository _statisticaCERepo;
    private readonly StatisticaSPSalvataRepository _statisticaSPRepo;
    private readonly IndicePersonalizzatoRepository _indicePersonalizzatoRepo;
    private readonly IndiciDiBilancioService _indiciService;

    [ObservableProperty]
    private ObservableCollection<Cliente> _clienti = new();

    [ObservableProperty]
    private Cliente? _clienteSelezionato;

    [ObservableProperty]
    private ObservableCollection<StatisticaCESalvata> _statisticheCEDisponibili = new();

    [ObservableProperty]
    private StatisticaCESalvata? _statisticaCESelezionata;

    [ObservableProperty]
    private ObservableCollection<StatisticaSPSalvata> _statisticheSPDisponibili = new();

    [ObservableProperty]
    private StatisticaSPSalvata? _statisticaSPSelezionata;

    [ObservableProperty]
    private ObservableCollection<IndiceCalcolato> _indiciCalcolati = new();

    [ObservableProperty]
    private ObservableCollection<IndiceSelezionabile> _indiciDisponibili = new();

    [ObservableProperty]
    private ObservableCollection<string> _categorieIndici = new() 
    { 
        "Tutti", 
        "Liquidità", 
        "Solidità", 
        "Redditività", 
        "Efficienza",
        "Personalizzato"
    };

    [ObservableProperty]
    private string _categoriaSelezionata = "Tutti";

    [ObservableProperty]
    private bool _isCalcInProgress;

    [ObservableProperty]
    private string _messaggioInfo = "Seleziona Cliente e Statistiche per calcolare gli indici";

    public IndiciDiBilancioViewModel()
    {
        _context = App.GetService<CGEasyDbContext>() ?? new CGEasyDbContext();
        // Singleton context - no special marking needed in EF Core

        _clienteRepo = new ClienteRepository(_context);
        _statisticaCERepo = new StatisticaCESalvataRepository(_context);
        _statisticaSPRepo = new StatisticaSPSalvataRepository(_context);
        _indicePersonalizzatoRepo = new IndicePersonalizzatoRepository(_context);
        _indiciService = new IndiciDiBilancioService();

        CaricaClienti();
    }

    private void CaricaClienti()
    {
        try
        {
            var clienti = _clienteRepo.GetAll()
                .Where(c => c.Attivo)
                .OrderBy(c => c.NomeCliente)
                .ToList();

            Clienti.Clear();
            foreach (var cliente in clienti)
            {
                Clienti.Add(cliente);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento clienti:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    partial void OnClienteSelezionatoChanged(Cliente? value)
    {
        if (value == null) return;

        CaricaStatistiche();
        CaricaIndiciDisponibili();
        IndiciCalcolati.Clear();
        MessaggioInfo = "Seleziona le statistiche CE e SP, poi premi 'Calcola Indici'";
    }

    private void CaricaIndiciDisponibili()
    {
        if (ClienteSelezionato == null) return;

        try
        {
            var indici = _indicePersonalizzatoRepo.GetByCliente(ClienteSelezionato.Id);
            
            IndiciDisponibili.Clear();
            foreach (var indice in indici)
            {
                IndiciDisponibili.Add(new IndiceSelezionabile(indice));
            }
            
            System.Diagnostics.Debug.WriteLine($"[INDICI BILANCIO] Caricati {IndiciDisponibili.Count} indici per selezione");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento indici:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void SelezionaTuttiIndici()
    {
        foreach (var indice in IndiciDisponibili)
        {
            indice.IsSelezionato = true;
        }
    }

    [RelayCommand]
    private void DeselezionaTuttiIndici()
    {
        foreach (var indice in IndiciDisponibili)
        {
            indice.IsSelezionato = false;
        }
    }

    private void CaricaStatistiche()
    {
        if (ClienteSelezionato == null) return;

        try
        {
            // Carica statistiche CE
            var statisticheCE = _statisticaCERepo.GetAll()
                .Where(s => s.ClienteId == ClienteSelezionato.Id)
                .OrderByDescending(s => s.DataCreazione)
                .ToList();

            StatisticheCEDisponibili.Clear();
            foreach (var stat in statisticheCE)
            {
                StatisticheCEDisponibili.Add(stat);
            }

            // Carica statistiche SP
            var statisticheSP = _statisticaSPRepo.GetAll()
                .Where(s => s.ClienteId == ClienteSelezionato.Id)
                .OrderByDescending(s => s.DataCreazione)
                .ToList();

            StatisticheSPDisponibili.Clear();
            foreach (var stat in statisticheSP)
            {
                StatisticheSPDisponibili.Add(stat);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento statistiche:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CalcolaIndici()
    {
        if (ClienteSelezionato == null)
        {
            MessageBox.Show("Seleziona un cliente", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (StatisticaCESelezionata == null || StatisticaSPSelezionata == null)
        {
            MessageBox.Show("Seleziona sia la statistica CE che SP", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica se ci sono indici disponibili
        if (IndiciDisponibili.Count == 0)
        {
            var result = MessageBox.Show(
                "⚠️ NESSUN INDICE CONFIGURATO\n\n" +
                $"Non hai ancora creato indici personalizzati per il cliente '{ClienteSelezionato.NomeCliente}'.\n\n" +
                "Per calcolare gli indici devi prima:\n" +
                "1. Cliccare 'Config. Indici' (pulsante arancione)\n" +
                "2. Creare almeno un indice personalizzato\n" +
                "3. Selezionare le voci dal tuo Conto Economico e Stato Patrimoniale\n" +
                "4. Salvare l'indice\n\n" +
                "Vuoi aprire la configurazione ora?",
                "Configura Prima gli Indici",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information
            );

            if (result == MessageBoxResult.Yes)
            {
                ApriConfigurazione();
            }
            return;
        }

        // Filtra SOLO indici SELEZIONATI (checkbox spuntate)
        var indiciSelezionati = IndiciDisponibili.Where(i => i.IsSelezionato).Select(i => i.Indice).ToList();
        
        if (indiciSelezionati.Count == 0)
        {
            MessageBox.Show(
                "⚠️ NESSUN INDICE SELEZIONATO\n\n" +
                $"Hai {IndiciDisponibili.Count} indici disponibili, ma nessuno è selezionato.\n\n" +
                "✅ Spunta almeno un indice dalla lista '📋 Seleziona Indici da Calcolare'\n" +
                "oppure usa il pulsante '☑ Tutti' per selezionarli tutti.",
                "Seleziona Indici",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        try
        {
            IsCalcInProgress = true;
            MessaggioInfo = "Calcolo indici personalizzati in corso...";

            IndiciCalcolati.Clear();

            // Calcola SOLO indici SELEZIONATI (temporaneamente con checkbox)
            foreach (var indicePers in indiciSelezionati)
            {
                var indiceCalcolato = _indiciService.CalcolaIndicePersonalizzato(
                    indicePers,
                    new List<StatisticaCESalvata> { StatisticaCESelezionata },
                    new List<StatisticaSPSalvata> { StatisticaSPSelezionata }
                );

                if (indiceCalcolato != null)
                {
                    IndiciCalcolati.Add(indiceCalcolato);
                }
            }

            if (IndiciCalcolati.Count == 0)
            {
                MessageBox.Show(
                    "⚠️ NESSUN INDICE CALCOLATO\n\n" +
                    "Gli indici configurati non hanno potuto essere calcolati.\n\n" +
                    "Possibili cause:\n" +
                    "• Le voci selezionate negli indici non esistono nelle statistiche scelte\n" +
                    "• Le statistiche non contengono dati per il periodo\n" +
                    "• Divisione per zero (denominatore = 0)\n\n" +
                    "Verifica la configurazione degli indici o cambia statistiche.",
                    "Attenzione",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                MessaggioInfo = "⚠️ Nessun indice calcolato - Verifica configurazione";
            }
            else
            {
                // Conta i periodi in modo sicuro
                int numPeriodi = 0;
                try
                {
                    var periodiStr = System.Text.Json.JsonSerializer.Deserialize<List<string>>(StatisticaCESelezionata.PeriodiJson);
                    numPeriodi = periodiStr?.Count ?? 0;
                }
                catch
                {
                    try
                    {
                        var periodiObj = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(StatisticaCESelezionata.PeriodiJson);
                        numPeriodi = periodiObj?.Count ?? 0;
                    }
                    catch
                    {
                        numPeriodi = 1; // Fallback
                    }
                }
                
                MessaggioInfo = $"✅ Calcolati {IndiciCalcolati.Count} indici per {numPeriodi} periodo/i";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ ERRORE CALCOLO INDICI\n\n{ex.Message}\n\n" +
                "Verifica che:\n" +
                "• Le statistiche siano state create correttamente\n" +
                "• Gli indici configurati usino voci esistenti\n" +
                "• Non ci siano divisioni per zero",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            MessaggioInfo = "❌ Errore durante il calcolo";
        }
        finally
        {
            IsCalcInProgress = false;
        }
    }

    [RelayCommand]
    private void EsportaExcel()
    {
        if (IndiciCalcolati.Count == 0)
        {
            MessageBox.Show("Nessun indice da esportare. Calcola prima gli indici.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Indici_Bilancio_{ClienteSelezionato?.NomeCliente}_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".xlsx",
                Filter = "Excel files (*.xlsx)|*.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                EsportaIndiciInExcel(saveDialog.FileName);
                MessageBox.Show("Export completato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore export Excel:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void EsportaIndiciInExcel(string filePath)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();

        // ============ FOGLIO 1: CONTO ECONOMICO ============
        if (StatisticaCESelezionata != null)
        {
            var wsCE = workbook.Worksheets.Add("Conto Economico");
            EsportaBilancioInFoglio(wsCE, StatisticaCESelezionata, "CONTO ECONOMICO");
        }

        // ============ FOGLIO 2: STATO PATRIMONIALE ============
        if (StatisticaSPSelezionata != null)
        {
            var wsSP = workbook.Worksheets.Add("Stato Patrimoniale");
            EsportaBilancioSPInFoglio(wsSP, StatisticaSPSelezionata, "STATO PATRIMONIALE");
        }

        // ============ FOGLIO 3: INDICI ============
        var wsIndici = workbook.Worksheets.Add("Indici");
        EsportaIndiciInFoglio(wsIndici);

        workbook.SaveAs(filePath);
    }

    private List<string> DeserializzaPeriodiSafe(string periodiJson)
    {
        var periodi = new List<string>();
        try
        {
            periodi = System.Text.Json.JsonSerializer.Deserialize<List<string>>(periodiJson) ?? new List<string>();
        }
        catch
        {
            try
            {
                var periodiObj = System.Text.Json.JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(periodiJson);
                if (periodiObj != null)
                {
                    foreach (var obj in periodiObj)
                    {
                        if (obj.TryGetProperty("Mese", out var mese) && obj.TryGetProperty("Anno", out var anno))
                        {
                            var mesiIta = new[] { "", "gen", "feb", "mar", "apr", "mag", "giu", "lug", "ago", "set", "ott", "nov", "dic" };
                            periodi.Add($"{mesiIta[mese.GetInt32()]} {anno.GetInt32()}");
                        }
                    }
                }
            }
            catch
            {
                periodi.Add("N/A");
            }
        }
        return periodi;
    }

    private void EsportaBilancioInFoglio(ClosedXML.Excel.IXLWorksheet ws, StatisticaCESalvata statistica, string titolo)
    {
        int row = 1;
        
        // Titolo
        ws.Cell(row, 1).Value = titolo;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkBlue;
        ws.Cell(row, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        row += 2;

        // Info
        ws.Cell(row, 1).Value = "Cliente:";
        ws.Cell(row, 2).Value = ClienteSelezionato?.NomeCliente ?? "";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Statistica:";
        ws.Cell(row, 2).Value = statistica.NomeStatistica;
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Data Creazione:";
        ws.Cell(row, 2).Value = statistica.DataCreazione.ToString("dd/MM/yyyy HH:mm");
        ws.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        // Deserializza dati
        var dati = System.Text.Json.JsonSerializer.Deserialize<List<BilancioStatisticaMultiPeriodo>>(
            statistica.DatiStatisticheJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        var periodi = DeserializzaPeriodiSafe(statistica.PeriodiJson);

        if (dati == null || dati.Count == 0)
        {
            ws.Cell(row, 1).Value = "Nessun dato disponibile";
            return;
        }

        // Header tabella
        int col = 1;
        ws.Cell(row, col++).Value = "Codice";
        ws.Cell(row, col++).Value = "Descrizione";
        
        foreach (var periodo in periodi)
        {
            ws.Cell(row, col++).Value = periodo;
        }

        var headerRange = ws.Range(row, 1, row, col - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
        headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        row++;

        // Dati
        foreach (var voce in dati)
        {
            col = 1;
            ws.Cell(row, col++).Value = voce.Codice;
            ws.Cell(row, col++).Value = voce.Descrizione;

            // Usa lo stesso approccio del calcolo indici - prendi il primo valore disponibile
            if (voce.DatiPerPeriodo.Count > 0)
            {
                var primoValore = voce.DatiPerPeriodo.First().Value;
                ws.Cell(row, col).Value = (double)primoValore.Importo;
                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
            }
            
            row++;
        }

        // Auto-fit
        ws.Columns().AdjustToContents();
    }

    private void EsportaBilancioSPInFoglio(ClosedXML.Excel.IXLWorksheet ws, StatisticaSPSalvata statistica, string titolo)
    {
        int row = 1;
        
        // Titolo
        ws.Cell(row, 1).Value = titolo;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkGreen;
        ws.Cell(row, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        row += 2;

        // Info
        ws.Cell(row, 1).Value = "Cliente:";
        ws.Cell(row, 2).Value = ClienteSelezionato?.NomeCliente ?? "";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Statistica:";
        ws.Cell(row, 2).Value = statistica.NomeStatistica;
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Data Creazione:";
        ws.Cell(row, 2).Value = statistica.DataCreazione.ToString("dd/MM/yyyy HH:mm");
        ws.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        // Deserializza dati
        var dati = System.Text.Json.JsonSerializer.Deserialize<List<BilancioStatisticaMultiPeriodo>>(
            statistica.DatiStatisticheJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        var periodi = DeserializzaPeriodiSafe(statistica.PeriodiJson);

        if (dati == null || dati.Count == 0)
        {
            ws.Cell(row, 1).Value = "Nessun dato disponibile";
            return;
        }

        // Header tabella
        int col = 1;
        ws.Cell(row, col++).Value = "Codice";
        ws.Cell(row, col++).Value = "Descrizione";
        
        foreach (var periodo in periodi)
        {
            ws.Cell(row, col++).Value = periodo;
        }

        var headerRange = ws.Range(row, 1, row, col - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
        headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        row++;

        // Dati
        foreach (var voce in dati)
        {
            col = 1;
            ws.Cell(row, col++).Value = voce.Codice;
            ws.Cell(row, col++).Value = voce.Descrizione;

            // Usa lo stesso approccio del calcolo indici - prendi il primo valore disponibile
            if (voce.DatiPerPeriodo.Count > 0)
            {
                var primoValore = voce.DatiPerPeriodo.First().Value;
                ws.Cell(row, col).Value = (double)primoValore.Importo;
                ws.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
            }
            
            row++;
        }

        // Auto-fit
        ws.Columns().AdjustToContents();
    }

    private void EsportaIndiciInFoglio(ClosedXML.Excel.IXLWorksheet ws)
    {
        int row = 1;
        
        // Titolo
        ws.Cell(row, 1).Value = "INDICI DI BILANCIO";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Orange;
        ws.Cell(row, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
        row += 2;

        // Info
        ws.Cell(row, 1).Value = "Cliente:";
        ws.Cell(row, 2).Value = ClienteSelezionato?.NomeCliente ?? "";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Statistica CE:";
        ws.Cell(row, 2).Value = StatisticaCESelezionata?.NomeStatistica ?? "";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Statistica SP:";
        ws.Cell(row, 2).Value = StatisticaSPSelezionata?.NomeStatistica ?? "";
        ws.Cell(row, 1).Style.Font.Bold = true;
        row++;

        ws.Cell(row, 1).Value = "Data Export:";
        ws.Cell(row, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        ws.Cell(row, 1).Style.Font.Bold = true;
        row += 2;

        if (IndiciCalcolati.Count == 0)
        {
            ws.Cell(row, 1).Value = "Nessun indice calcolato";
            return;
        }

        // Header tabella
        int col = 1;
        ws.Cell(row, col++).Value = "Categoria";
        ws.Cell(row, col++).Value = "Nome Indice";
        ws.Cell(row, col++).Value = "Formula";
        ws.Cell(row, col++).Value = "Unità";

        // Periodi
        var periodi = IndiciCalcolati
            .SelectMany(i => i.ValoriPerPeriodo.Keys)
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        foreach (var periodo in periodi)
        {
            ws.Cell(row, col++).Value = periodo;
        }

        // Stile header
        var headerRange = ws.Range(row, 1, row, col - 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(255, 204, 153); // Arancione chiaro
        headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        row++;

        // Dati indici
        foreach (var indice in IndiciCalcolati.OrderBy(i => i.Categoria).ThenBy(i => i.NomeIndice))
        {
            col = 1;
            ws.Cell(row, col++).Value = indice.Categoria;
            ws.Cell(row, col++).Value = indice.NomeIndice;
            ws.Cell(row, col++).Value = indice.Formula;
            ws.Cell(row, col++).Value = indice.UnitaMisura;

            foreach (var periodo in periodi)
            {
                if (indice.ValoriPerPeriodo.TryGetValue(periodo, out var valore))
                {
                    ws.Cell(row, col).Value = (double)valore;
                    ws.Cell(row, col).Style.NumberFormat.Format = indice.UnitaMisura == "%" ? "0.00%" : "#,##0.00";
                }
                col++;
            }
            row++;
        }

        // Auto-fit
        ws.Columns().AdjustToContents();
    }

    [RelayCommand]
    private void CreaIndicePersonalizzato()
    {
        MessageBox.Show(
            "Funzionalità 'Crea Indice Personalizzato' in arrivo!\n\n" +
            "Permetterà di:\n" +
            "• Selezionare voci da CE e SP\n" +
            "• Scegliere operatori (+, -, ×, ÷)\n" +
            "• Salvare l'indice per riutilizzarlo",
            "In Sviluppo",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    [RelayCommand]
    private void ApriConfigurazione()
    {
        if (ClienteSelezionato == null)
        {
            MessageBox.Show("Seleziona prima un cliente", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (StatisticaCESelezionata == null || StatisticaSPSelezionata == null)
        {
            MessageBox.Show(
                "Seleziona le statistiche CE e SP prima di configurare gli indici.\n\n" +
                "Gli indici personalizzati utilizzeranno le voci disponibili nelle statistiche selezionate.",
                "Attenzione",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }

        try
        {
            // Crea il ViewModel e passa le statistiche selezionate
            var configViewModel = new ConfigurazioneIndiciViewModel(
                ClienteSelezionato,
                StatisticaCESelezionata,
                StatisticaSPSelezionata
            );

            var configWindow = new Window
            {
                Title = $"⚙️ Configurazione Indici - {ClienteSelezionato.NomeCliente}",
                Content = new Views.ConfigurazioneIndiciView { DataContext = configViewModel },
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            configWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura configurazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

