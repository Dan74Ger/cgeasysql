using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot;
using System.Collections.ObjectModel;
using System.Linq;
using ScottPlot.Plottables;
using Color = ScottPlot.Color;

namespace CGEasy.App.ViewModels;

public partial class GraficoMargineViewModel : ObservableObject
{
    // Filtri VOCI
    [ObservableProperty]
    private bool mostraSaldoCorrente = true;

    [ObservableProperty]
    private bool mostraSaldoDisponibile = true;

    [ObservableProperty]
    private bool mostraFatturatoAnticipato = true;

    [ObservableProperty]
    private bool mostraResiduoAnticipabile = true;

    [ObservableProperty]
    private bool mostraIncassi;

    [ObservableProperty]
    private bool mostraPagamenti;

    [ObservableProperty]
    private bool mostraUtilizzoFido;

    // Mostra/Nascondi Valori
    [ObservableProperty]
    private bool mostraValori = true;

    // Filtro BANCHE
    [ObservableProperty]
    private ObservableCollection<string> bancheDisponibili = new();

    [ObservableProperty]
    private string? bancaSelezionata = "Tutte le banche";

    private readonly MargineTesoreraData _datiMargine;
    public Plot Plot { get; private set; }

    public GraficoMargineViewModel(MargineTesoreraData datiMargine)
    {
        _datiMargine = datiMargine;
        Plot = new Plot();

        // Popola lista banche
        BancheDisponibili.Add("Tutte le banche");
        foreach (var nomeBanca in _datiMargine.Banche)
        {
            BancheDisponibili.Add(nomeBanca);
        }

        // Genera grafico iniziale
        AggiornaGrafico();
    }

    partial void OnMostraSaldoCorrenteChanged(bool value) => AggiornaGrafico();
    partial void OnMostraSaldoDisponibileChanged(bool value) => AggiornaGrafico();
    partial void OnMostraFatturatoAnticipatoChanged(bool value) => AggiornaGrafico();
    partial void OnMostraResiduoAnticipabileChanged(bool value) => AggiornaGrafico();
    partial void OnMostraIncassiChanged(bool value) => AggiornaGrafico();
    partial void OnMostraPagamentiChanged(bool value) => AggiornaGrafico();
    partial void OnMostraUtilizzoFidoChanged(bool value) => AggiornaGrafico();
    partial void OnBancaSelezionataChanged(string? value) => AggiornaGrafico();
    partial void OnMostraValoriChanged(bool value) => AggiornaGrafico();

    private void AggiornaGrafico()
    {
        Plot.Clear();
        bool isTutteLeBanche = BancaSelezionata == "Tutte le banche";

        // Asse X con nomi mesi
        double[] x = Enumerable.Range(0, _datiMargine.Mesi.Count).Select(i => (double)i).ToArray();
        string[] labels = _datiMargine.Mesi.ToArray();

        if (MostraSaldoCorrente)
        {
            var valori = OttieniValori(_datiMargine.SaldoCorrente, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Saldo Corrente";
            scatter.Color = ScottPlot.Colors.DeepSkyBlue;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.DeepSkyBlue;
                    text.LabelBold = true;
                    text.OffsetY = -15;
                }
            }
        }

        if (MostraSaldoDisponibile)
        {
            var valori = OttieniValori(_datiMargine.SaldoDisponibile, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Saldo Disponibile";
            scatter.Color = ScottPlot.Colors.Blue;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.Blue;
                    text.LabelBold = true;
                    text.OffsetY = -30;
                }
            }
        }

        if (MostraFatturatoAnticipato)
        {
            var valori = OttieniValori(_datiMargine.FatturatoAnticipato, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Fatturato Anticipato";
            scatter.Color = ScottPlot.Colors.Purple;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.Purple;
                    text.LabelBold = true;
                    text.OffsetY = -45;
                }
            }
        }

        if (MostraResiduoAnticipabile)
        {
            var valori = OttieniValori(_datiMargine.ResiduoAnticipabile, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Residuo Anticipabile";
            scatter.Color = ScottPlot.Colors.Orange;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.Orange;
                    text.LabelBold = true;
                    text.OffsetY = -60;
                }
            }
        }

        if (MostraIncassi)
        {
            var valori = OttieniValori(_datiMargine.Incassi, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Incassi";
            scatter.Color = ScottPlot.Colors.Green;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.Green;
                    text.LabelBold = true;
                    text.OffsetY = -15;
                }
            }
        }

        if (MostraPagamenti)
        {
            var valori = OttieniValori(_datiMargine.Pagamenti, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Pagamenti";
            scatter.Color = ScottPlot.Colors.Red;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.Red;
                    text.LabelBold = true;
                    text.OffsetY = -30;
                }
            }
        }

        if (MostraUtilizzoFido && _datiMargine.UtilizzoFidoCC != null)
        {
            var valori = OttieniValori(_datiMargine.UtilizzoFidoCC, isTutteLeBanche);
            var valoriDouble = valori.Select(v => (double)v).ToArray();
            var scatter = Plot.Add.Scatter(x, valoriDouble);
            scatter.Label = "Utilizzo Fido C/C";
            scatter.Color = ScottPlot.Colors.DarkRed;
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            
            if (MostraValori)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    var text = Plot.Add.Text($"{valori[i]:N0} €", x[i], valoriDouble[i]);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlot.Colors.DarkRed;
                    text.LabelBold = true;
                    text.OffsetY = -45;
                }
            }
        }

        // Configurazione assi
        Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(x, labels);
        Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        Plot.Axes.Bottom.Label.Text = "Mese";
        Plot.Axes.Bottom.Label.FontSize = 14;
        Plot.Axes.Left.Label.Text = "Importo (€)";
        Plot.Axes.Left.Label.FontSize = 14;

        // Legenda
        Plot.Legend.IsVisible = true;
        Plot.Legend.Location = ScottPlot.Alignment.UpperRight;
        Plot.Legend.FontSize = 12;

        // Griglia
        Plot.Grid.MajorLineColor = ScottPlot.Colors.LightGray.WithAlpha(0.4);
        Plot.Grid.MinorLineColor = ScottPlot.Colors.LightGray.WithAlpha(0.2);
        
        OnPropertyChanged(nameof(Plot));
    }

    private double[] OttieniValori(SaldoPivotRigaConsolidato riga, bool tutteLeBanche)
    {
        if (tutteLeBanche)
        {
            // Totali aggregati
            return riga.ValoriMensili.Values.Select(v => (double)v).ToArray();
        }
        else if (!string.IsNullOrEmpty(BancaSelezionata) && riga.ValoriPerBanca.ContainsKey(BancaSelezionata))
        {
            // Valori per banca specifica
            return riga.ValoriPerBanca[BancaSelezionata].Values.Select(v => (double)v).ToArray();
        }
        
        // Fallback: tutti zero
        return Enumerable.Repeat(0.0, _datiMargine.Mesi.Count).ToArray();
    }
}
