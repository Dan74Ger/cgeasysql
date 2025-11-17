using System.Windows;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class BilancioTemplateDialogView : Window
{
    public BilancioTemplateDialogView()
    {
        InitializeComponent();
    }

    private void Salva_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is BilancioTemplateDialogViewModel vm)
        {
            if (vm.Salva())
            {
                DialogResult = true;
                Close();
            }
        }
    }

    private void Annulla_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

