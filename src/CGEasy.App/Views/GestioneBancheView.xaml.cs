using CGEasy.App.ViewModels;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CGEasy.App.Views
{
    public partial class GestioneBancheView : UserControl
    {
        public GestioneBancheView()
        {
            InitializeComponent();
            DataContext = new GestioneBancheViewModel();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ApriDettaglioBanca();
        }

        private void ApriDettaglio_Click(object sender, RoutedEventArgs e)
        {
            ApriDettaglioBanca();
        }

        private void ApriDettaglioRiga_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is Banca banca)
            {
                // Trova la finestra ControlloGestioneWindow
                var window = Window.GetWindow(this) as ControlloGestioneWindow;
                if (window != null)
                {
                    window.MostraDettaglioBanca(banca.Id, banca.NomeBanca);
                }
            }
        }

        private void ApriDettaglioBanca()
        {
            var viewModel = DataContext as GestioneBancheViewModel;
            if (viewModel?.BancaSelezionata == null)
            {
                MessageBox.Show("Seleziona una banca dalla lista.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Trova la finestra ControlloGestioneWindow
            var window = Window.GetWindow(this) as ControlloGestioneWindow;
            if (window != null)
            {
                window.MostraDettaglioBanca(viewModel.BancaSelezionata.Id, viewModel.BancaSelezionata.NomeBanca);
            }
        }

        private void BancheGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(100); // Wait for binding to complete
                    if (DataContext is GestioneBancheViewModel vm && e.Row.Item is Banca banca)
                    {
                        var bancaRepo = new BancaRepository(App.GetService<Core.Data.LiteDbContext>()!);
                        bancaRepo.Update(banca);
                        vm.LoadBanche(); // Refresh the list
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}

