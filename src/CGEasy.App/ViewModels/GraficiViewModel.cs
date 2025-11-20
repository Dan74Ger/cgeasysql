using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using ScottPlot;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class GraficiViewModel : ObservableObject
{
    private readonly ClienteRepository _clienteRepo;
    private readonly StatisticaCESalvataRepository _statisticaCERepo;
    private readonly StatisticaSPSalvataRepository _statisticaSPRepo;
    private WpfPlot? _wpfPlot;      // ⭐ Per grafici a linee
    private WpfPlot? _wpfPlotPie;   // ⭐ Per grafico a torta

    [ObservableProperty]
    private ObservableCollection<Cliente> _clienti = new();

    [ObservableProperty]
    private Cliente? _clienteSelezionato;

    [ObservableProperty]
    private ObservableCollection<string> _tipiStatistica = new() { "CE - Conto Economico", "SP - Stato Patrimoniale" };

    [ObservableProperty]
    private string? _tipoStatisticaSelezionato;

    [ObservableProperty]
    private ObservableCollection<object> _statisticheSalvate = new();

    [ObservableProperty]
    private object? _statisticaSelezionata;  // ⭐ Selezione singola

    [ObservableProperty]
    private ObservableCollection<VoceGraficoSelezionabile> _vociDisponibili = new();

    [ObservableProperty]
    private bool _isGraficoLinee = true;

    [ObservableProperty]
    private bool _isGraficoTorta = false;

    [ObservableProperty]
    private bool _mostraValoriAssoluti = true;

    [ObservableProperty]
    private bool _mostraPercentuali = false;
    
    [ObservableProperty]
    private bool _mostraValoriSulGrafico = true;  // ⭐ Checkbox per mostrare valori

    [ObservableProperty]
    private Visibility _visibilitaGraficoLinee = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _visibilitaGraficoTorta = Visibility.Collapsed;

    [ObservableProperty]
    private Visibility _visibilitaMessaggioVuoto = Visibility.Visible;

    // ⭐ Costruttore per il code-behind
    public GraficiViewModel(WpfPlot wpfPlot, WpfPlot wpfPlotPie)
    {
        System.Diagnostics.Debug.WriteLine("[GRAFICI VM] Costruttore con WpfPlot chiamato");
        _wpfPlot = wpfPlot;
        _wpfPlotPie = wpfPlotPie;
        
        var context = App.GetService<CGEasy.Core.Data.CGEasyDbContext>() ?? new CGEasy.Core.Data.CGEasyDbContext();
        _clienteRepo = new ClienteRepository(context);
        _statisticaCERepo = new StatisticaCESalvataRepository(context);
        _statisticaSPRepo = new StatisticaSPSalvataRepository(context);

        CaricaDatiIniziali();
    }
    
    // ⭐ Metodo per settare il plot dopo la creazione (chiamato dalla View)
    public void SetPlotControl(WpfPlot wpfPlot)
    {
        _wpfPlot = wpfPlot;
        System.Diagnostics.Debug.WriteLine("[GRAFICI VM] WpfPlot settato manualmente");
    }

    private void CaricaDatiIniziali()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("\n[GRAFICI] ========== CARICAMENTO INIZIALE ==========");
            
            // Verifica quante statistiche ci sono nel database
            System.Diagnostics.Debug.WriteLine("[GRAFICI] Caricamento statistiche CE...");
            var totStatCE = _statisticaCERepo.GetAll().Count();
            
            System.Diagnostics.Debug.WriteLine("[GRAFICI] Caricamento statistiche SP...");
            var totStatSP = _statisticaSPRepo.GetAll().Count();
            
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] Statistiche CE salvate nel DB: {totStatCE}");
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] Statistiche SP salvate nel DB: {totStatSP}");
            
            // ⭐ CARICA TUTTI I CLIENTI ATTIVI (non solo quelli con statistiche)
            System.Diagnostics.Debug.WriteLine("[GRAFICI] Caricamento clienti attivi...");
            var clienti = _clienteRepo.GetAll()
                .Where(c => c.Attivo)
                .OrderBy(c => c.NomeCliente)
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] Clienti attivi caricati: {clienti.Count}");
            
            Clienti.Clear();
            foreach (var c in clienti)
            {
                Clienti.Add(c);
                System.Diagnostics.Debug.WriteLine($"  - {c.NomeCliente} (ID: {c.Id})");
            }
            
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] Collection Clienti.Count: {Clienti.Count}");
            
            // ⚠️ NON selezionare automaticamente, l'utente deve scegliere
            // ClienteSelezionato = Clienti.FirstOrDefault();
            
            System.Diagnostics.Debug.WriteLine("[GRAFICI] Cliente non selezionato automaticamente - l'utente deve scegliere");
            
            System.Diagnostics.Debug.WriteLine("[GRAFICI] ========== CARICAMENTO COMPLETATO ==========\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] ❌ ERRORE in CaricaDatiIniziali: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] StackTrace: {ex.StackTrace}");
            MessageBox.Show($"Errore caricamento dati Grafici:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    partial void OnClienteSelezionatoChanged(Cliente? value)
    {
        if (value == null) return;
        TipoStatisticaSelezionato = null;
        StatisticheSalvate.Clear();
        VociDisponibili.Clear();
    }

    partial void OnTipoStatisticaSelezionatoChanged(string? value)
    {
        if (value == null || ClienteSelezionato == null) return;

        StatisticheSalvate.Clear();
        StatisticaSelezionata = null;  // ⭐ Reset selezione
        VociDisponibili.Clear();

        // Carica statistiche salvate in base al tipo
        if (value.StartsWith("CE"))
        {
            var statisticheCE = _statisticaCERepo.GetAll()
                .Where(s => s.ClienteId == ClienteSelezionato.Id)
                .OrderByDescending(s => s.DataCreazione)
                .ToList();

            foreach (var stat in statisticheCE)
                StatisticheSalvate.Add(stat);
        }
        else if (value.StartsWith("SP"))
        {
            var statisticheSP = _statisticaSPRepo.GetAll()
                .Where(s => s.ClienteId == ClienteSelezionato.Id)
                .OrderByDescending(s => s.DataCreazione)
                .ToList();

            foreach (var stat in statisticheSP)
                StatisticheSalvate.Add(stat);
        }
    }

    partial void OnStatisticaSelezionataChanged(object? value)
    {
        if (value == null) return;

        System.Diagnostics.Debug.WriteLine($"[GRAFICI] Statistica selezionata: {value.GetType().Name}");

        VociDisponibili.Clear();

        List<BilancioStatisticaMultiPeriodo>? datiStatistica = null;

        if (value is StatisticaCESalvata statisticaCE)
        {
            datiStatistica = System.Text.Json.JsonSerializer.Deserialize<List<BilancioStatisticaMultiPeriodo>>(statisticaCE.DatiStatisticheJson);
        }
        else if (value is StatisticaSPSalvata statisticaSP)
        {
            datiStatistica = System.Text.Json.JsonSerializer.Deserialize<List<BilancioStatisticaMultiPeriodo>>(statisticaSP.DatiStatisticheJson);
        }

        if (datiStatistica != null)
        {
            foreach (var voce in datiStatistica)
            {
                VociDisponibili.Add(new VoceGraficoSelezionabile
                {
                    Descrizione = voce.Descrizione,
                    Codice = voce.Codice,
                    IsSelezionata = false,
                    DatiOriginali = voce
                });
            }
            System.Diagnostics.Debug.WriteLine($"[GRAFICI] Caricate {VociDisponibili.Count} voci");
        }
    }

    /// <summary>
    /// Comando per pulire tutti i filtri e resettare la pagina
    /// </summary>
    [RelayCommand]
    private void PulisciFiltri()
    {
        System.Diagnostics.Debug.WriteLine("[GRAFICI] Pulisci filtri...");

        ClienteSelezionato = null;
        TipoStatisticaSelezionato = null;
        StatisticheSalvate.Clear();
        StatisticaSelezionata = null;
        VociDisponibili.Clear();

        // Pulisce anche i grafici
        if (_wpfPlot != null)
        {
            _wpfPlot.Plot.Clear();
            _wpfPlot.Refresh();
        }
        if (_wpfPlotPie != null)
        {
            _wpfPlotPie.Plot.Clear();
            _wpfPlotPie.Refresh();
        }

        VisibilitaGraficoLinee = Visibility.Collapsed;
        VisibilitaGraficoTorta = Visibility.Collapsed;
        VisibilitaMessaggioVuoto = Visibility.Visible;
        
        System.Diagnostics.Debug.WriteLine("[GRAFICI] Filtri puliti!");
    }

    [RelayCommand]
    private void GeneraGrafico()
    {
        try
        {
            // Valida selezioni
            if (ClienteSelezionato == null || StatisticaSelezionata == null)
            {
                MessageBox.Show("Seleziona Cliente e una Statistica Salvata.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vociSelezionate = VociDisponibili.Where(v => v.IsSelezionata).ToList();
            if (vociSelezionate.Count == 0)
            {
                MessageBox.Show("Seleziona almeno una voce da visualizzare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Genera grafico in base al tipo
            if (IsGraficoLinee)
            {
                if (_wpfPlot == null) return;
                _wpfPlot.Plot.Clear();
                GeneraGraficoLinee(vociSelezionate);
                _wpfPlot.Refresh();
                
                VisibilitaGraficoLinee = Visibility.Visible;
                VisibilitaGraficoTorta = Visibility.Collapsed;
            }
            else if (IsGraficoTorta)
            {
                GeneraGraficoTorta(vociSelezionate);
                
                VisibilitaGraficoLinee = Visibility.Collapsed;
                VisibilitaGraficoTorta = Visibility.Visible;
            }

            // Nascondi messaggio vuoto
            VisibilitaMessaggioVuoto = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante la generazione del grafico:\n{ex.Message}\n\n{ex.StackTrace}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GeneraGraficoLinee(List<VoceGraficoSelezionabile> vociSelezionate)
    {
        if (vociSelezionate.Count == 0) return;

        // ⭐ Raccogli TUTTI i periodi da TUTTE le voci selezionate
        var tuttiIPeriodi = new HashSet<string>();
        foreach (var voce in vociSelezionate)
        {
            var dati = voce.DatiOriginali as BilancioStatisticaMultiPeriodo;
            if (dati != null)
            {
                foreach (var periodo in dati.DatiPerPeriodo.Keys)
                {
                    tuttiIPeriodi.Add(periodo);
                }
            }
        }

        if (tuttiIPeriodi.Count == 0) return;

        // Ordina periodi per Anno e Mese
        var periodiOrdinati = tuttiIPeriodi
            .Select(p => {
                // Estrae anno/mese dalla chiave (es: "Gen 2025" -> mese=1, anno=2025)
                var parts = p.Split(' ');
                if (parts.Length != 2) return (periodo: p, anno: 0, mese: 0);
                
                var mesi = new Dictionary<string, int> {
                    {"Gen", 1}, {"Feb", 2}, {"Mar", 3}, {"Apr", 4}, {"Mag", 5}, {"Giu", 6},
                    {"Lug", 7}, {"Ago", 8}, {"Set", 9}, {"Ott", 10}, {"Nov", 11}, {"Dic", 12}
                };
                
                int.TryParse(parts[1], out int anno);
                int mese = mesi.TryGetValue(parts[0], out int m) ? m : 0;
                
                return (periodo: p, anno: anno, mese: mese);
            })
            .OrderBy(x => x.anno)
            .ThenBy(x => x.mese)
            .Select(x => x.periodo)
            .ToArray();
        
        var xPositions = Enumerable.Range(0, periodiOrdinati.Length).Select(i => (double)i).ToArray();

        // Colori moderni
        var colori = new[]
        {
            ScottPlot.Color.FromHex("#2196F3"),  // Blue
            ScottPlot.Color.FromHex("#4CAF50"),  // Green
            ScottPlot.Color.FromHex("#FF9800"),  // Orange
            ScottPlot.Color.FromHex("#9C27B0"),  // Purple
            ScottPlot.Color.FromHex("#F44336"),  // Red
            ScottPlot.Color.FromHex("#00BCD4"),  // Cyan
            ScottPlot.Color.FromHex("#FFEB3B"),  // Yellow
            ScottPlot.Color.FromHex("#795548"),  // Brown
        };

        int colorIndex = 0;

        // Crea una linea per ogni voce selezionata
        foreach (var voce in vociSelezionate)
        {
            var dati = voce.DatiOriginali as BilancioStatisticaMultiPeriodo;
            if (dati == null) continue;

            var valori = new double[periodiOrdinati.Length];
            for (int i = 0; i < periodiOrdinati.Length; i++)
            {
                var periodo = periodiOrdinati[i];
                if (dati.DatiPerPeriodo.TryGetValue(periodo, out var datiPeriodo))
                {
                    if (MostraValoriAssoluti)
                    {
                        valori[i] = (double)datiPeriodo.Importo;
                    }
                    else if (MostraPercentuali)
                    {
                        valori[i] = (double)datiPeriodo.Percentuale;
                    }
                }
                else
                {
                    valori[i] = 0; // ⭐ Valore 0 se il periodo non esiste per questa voce
                }
            }

            var colore = colori[colorIndex % colori.Length];
            var scatter = _wpfPlot?.Plot.Add.Scatter(xPositions, valori);
            if (scatter != null)
            {
                scatter.LegendText = voce.Descrizione;
                scatter.Color = colore;
                scatter.LineWidth = 3;
                scatter.MarkerSize = 10;
                scatter.MarkerShape = MarkerShape.FilledCircle;
            }

            // ⭐ Aggiungi etichette valori se checkbox attivo
            if (MostraValoriSulGrafico && _wpfPlot != null)
            {
                for (int i = 0; i < xPositions.Length; i++)
                {
                    if (valori[i] != 0) // Solo se c'è un valore
                    {
                        var label = _wpfPlot.Plot.Add.Text(
                            MostraValoriAssoluti ? $"{valori[i]:N0}€" : $"{valori[i]:F1}%",
                            xPositions[i],
                            valori[i]
                        );
                        label.LabelFontSize = 10;
                        label.LabelFontColor = ScottPlot.Colors.Black;
                        label.LabelBold = true;
                        label.LabelAlignment = Alignment.LowerCenter;
                        label.OffsetY = -15; // Sopra il punto
                    }
                }
            }

            colorIndex++;
        }

        // Configura assi
        if (_wpfPlot != null)
        {
            _wpfPlot.Plot.Axes.Bottom.Label.Text = "Periodi";
            _wpfPlot.Plot.Axes.Bottom.SetTicks(xPositions, periodiOrdinati);
            _wpfPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            
            _wpfPlot.Plot.Axes.Left.Label.Text = MostraValoriAssoluti ? "Importo (€)" : "Percentuale (%)";
            
            // Legenda
            _wpfPlot.Plot.ShowLegend(Alignment.UpperRight);
        
            // Titolo
            _wpfPlot.Plot.Title($"Andamento - {ClienteSelezionato?.NomeCliente ?? ""}");
            
            // Griglia
            _wpfPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#E0E0E0");
            
            // ⭐ Auto-scale per adattare il grafico ai dati
            _wpfPlot.Plot.Axes.AutoScale();
            
            // Aggiungi margini per evitare che i marker tocchino i bordi
            _wpfPlot.Plot.Axes.Margins(0.1, 0.1);
        }
    }

    private void GeneraGraficoTorta(List<VoceGraficoSelezionabile> vociSelezionate)
    {
        if (_wpfPlotPie == null) return;
        
        _wpfPlotPie.Plot.Clear();

        var values = new List<double>();
        var labels = new List<string>();
        
        // Colori moderni e vivaci
        var colori = new[]
        {
            ScottPlot.Color.FromHex("#2196F3"),  // Blue
            ScottPlot.Color.FromHex("#4CAF50"),  // Green
            ScottPlot.Color.FromHex("#FF9800"),  // Orange
            ScottPlot.Color.FromHex("#9C27B0"),  // Purple
            ScottPlot.Color.FromHex("#F44336"),  // Red
            ScottPlot.Color.FromHex("#00BCD4"),  // Cyan
            ScottPlot.Color.FromHex("#FFEB3B"),  // Yellow
            ScottPlot.Color.FromHex("#795548"),  // Brown
            ScottPlot.Color.FromHex("#E91E63"),  // Pink
            ScottPlot.Color.FromHex("#3F51B5"),  // Indigo
        };

        // Per il grafico a torta, usa il TOTALE di ogni voce
        foreach (var voce in vociSelezionate)
        {
            var dati = voce.DatiOriginali as BilancioStatisticaMultiPeriodo;
            if (dati == null) continue;

            double valore = 0;

            if (MostraValoriAssoluti)
            {
                valore = (double)dati.ImportoTotale;
            }
            else if (MostraPercentuali)
            {
                valore = (double)dati.PercentualeTotale;
            }

            // Solo voci con valore > 0
            if (valore <= 0) continue;

            values.Add(Math.Abs(valore));
            
            // ⭐ Prepara label in base al checkbox "Mostra Valori"
            string label = voce.Descrizione;
            if (MostraValoriSulGrafico)
            {
                if (MostraValoriAssoluti)
                {
                    label = $"{voce.Descrizione}\n{valore:N0} €";
                }
                else
                {
                    label = $"{voce.Descrizione}\n{valore:F1}%";
                }
            }
            labels.Add(label);
        }

        if (values.Count == 0)
        {
            MessageBox.Show("Nessun valore disponibile per il grafico a torta.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // ⭐ Crea il grafico a torta con ScottPlot 5
        var pie = _wpfPlotPie.Plot.Add.Pie(values);
        pie.ExplodeFraction = 0.05; // Separa leggermente le fette
        pie.SliceLabelDistance = 1.3; // ⭐ FUORI dalla torta (> 1)
        
        // Imposta colori personalizzati
        for (int i = 0; i < pie.Slices.Count; i++)
        {
            pie.Slices[i].FillColor = colori[i % colori.Length];
            pie.Slices[i].Label = labels[i];
            pie.Slices[i].LabelStyle.ForeColor = ScottPlot.Colors.Black; // ⭐ Testo NERO
            pie.Slices[i].LabelStyle.FontSize = 12;
            pie.Slices[i].LabelStyle.Bold = true;
        }

        // Titolo
        _wpfPlotPie.Plot.Title($"Composizione - {ClienteSelezionato?.NomeCliente ?? ""}");
        
        // Nascondi assi
        _wpfPlotPie.Plot.HideGrid();
        _wpfPlotPie.Plot.Axes.Frameless();
        
        _wpfPlotPie.Refresh();
    }
}

// Classe helper per le voci selezionabili
public partial class VoceGraficoSelezionabile : ObservableObject
{
    [ObservableProperty]
    private string _descrizione = string.Empty;

    [ObservableProperty]
    private string _codice = string.Empty;

    [ObservableProperty]
    private bool _isSelezionata;

    public object? DatiOriginali { get; set; }
}

