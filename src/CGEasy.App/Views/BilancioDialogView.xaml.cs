using System.Windows;
using CGEasy.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioDialogView : Window
{
    public BilancioDialogView(int? bilancioId = null)
    {
        InitializeComponent();
        
        // Inizializza ViewModel con CGEasyDbContext da DI
        var context = ((App)Application.Current).Services!.GetRequiredService<CGEasy.Core.Data.CGEasyDbContext>();
        DataContext = new BilancioDialogViewModel(context, bilancioId);
    }
}

