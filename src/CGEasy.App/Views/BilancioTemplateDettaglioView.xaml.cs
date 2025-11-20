using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CGEasy.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioTemplateDettaglioView : UserControl
{
    public BilancioTemplateDettaglioView()
    {
        InitializeComponent();
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // Quando l'utente finisce di editare una cella, ricalcoliamo gli importi
        if (DataContext is BilancioTemplateDettaglioViewModel vm)
        {
            // Usa Dispatcher per assicurarsi che il binding sia completato prima del ricalcolo
            Dispatcher.BeginInvoke(new Action(() =>
            {
                vm.RicalcolaTuttiImporti();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow row && row.Item is CGEasy.Core.Models.BilancioTemplate riga)
        {
            // Recupera i servizi necessari
            var context = ((App)Application.Current).Services?.GetService<CGEasy.Core.Data.CGEasyDbContext>();
            var auditService = ((App)Application.Current).Services?.GetService<CGEasy.Core.Services.AuditLogService>();

            if (context == null || auditService == null) return;

            // Apri una nuova finestra separata per modificare la riga
            var dialog = new BilancioTemplateDialogView();
            var dialogVm = new BilancioTemplateDialogViewModel(context, auditService);

            // Inizializza con la riga esistente
            dialogVm.Inizializza(riga, riga.ClienteId, riga.Mese, riga.Anno, "");

            dialog.DataContext = dialogVm;
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            // Dopo la chiusura, ricalcola gli importi
            if (DataContext is BilancioTemplateDettaglioViewModel vm)
            {
                vm.RicalcolaTuttiImporti();
            }
        }
    }
}
