using System.Windows.Controls;
using System.Windows;

namespace CGEasy.App.Views;

public partial class GraficiView : UserControl
{
    public GraficiView()
    {
        InitializeComponent();

        try
        {
            var viewModel = new ViewModels.GraficiViewModel(WpfPlot1, WpfPlot2);  // ⭐ Passa entrambi i plot
            DataContext = viewModel;
            System.Diagnostics.Debug.WriteLine("[GRAFICI VIEW] ViewModel creato e assegnato al DataContext");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GRAFICI VIEW] ❌ ERRORE creazione ViewModel: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[GRAFICI VIEW] StackTrace: {ex.StackTrace}");
            MessageBox.Show($"Errore inizializzazione Grafici:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
