using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioDettaglioView : Window
{
    public BilancioDettaglioView(int clienteId, int mese, int anno, string titolo)
    {
        InitializeComponent();

        var context = ((App)Application.Current).Services!.GetRequiredService<CGEasy.Core.Data.LiteDbContext>();
        DataContext = new ViewModels.BilancioDettaglioViewModel(context, clienteId, mese, anno, titolo);
    }
}



