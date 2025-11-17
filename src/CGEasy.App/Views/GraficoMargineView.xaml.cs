using System.Windows;

namespace CGEasy.App.Views;

public partial class GraficoMargineView : Window
{
    private readonly ViewModels.GraficoMargineViewModel _viewModel;

    public GraficoMargineView(ViewModels.GraficoMargineViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Copia il plot dal ViewModel al controllo WpfPlot
        WpfPlot.Plot.Clear();
        foreach (var plottable in viewModel.Plot.GetPlottables())
        {
            WpfPlot.Plot.Add.Plottable(plottable);
        }
        WpfPlot.Plot.Axes.Bottom.TickGenerator = viewModel.Plot.Axes.Bottom.TickGenerator;
        WpfPlot.Plot.Axes.Bottom.Label.Text = viewModel.Plot.Axes.Bottom.Label.Text;
        WpfPlot.Plot.Axes.Left.Label.Text = viewModel.Plot.Axes.Left.Label.Text;
        WpfPlot.Plot.Legend.IsVisible = viewModel.Plot.Legend.IsVisible;
        WpfPlot.Refresh();
        
        // Ascolta i cambiamenti per refreshare il grafico
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(viewModel.Plot))
            {
                WpfPlot.Plot.Clear();
                foreach (var plottable in viewModel.Plot.GetPlottables())
                {
                    WpfPlot.Plot.Add.Plottable(plottable);
                }
                WpfPlot.Plot.Axes.Bottom.TickGenerator = viewModel.Plot.Axes.Bottom.TickGenerator;
                WpfPlot.Plot.Axes.Bottom.Label.Text = viewModel.Plot.Axes.Bottom.Label.Text;
                WpfPlot.Plot.Axes.Left.Label.Text = viewModel.Plot.Axes.Left.Label.Text;
                WpfPlot.Plot.Legend.IsVisible = viewModel.Plot.Legend.IsVisible;
                WpfPlot.Refresh();
            }
        };
    }
}
