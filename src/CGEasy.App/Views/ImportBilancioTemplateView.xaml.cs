using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class ImportBilancioTemplateView : UserControl
{
    public ImportBilancioTemplateView()
    {
        InitializeComponent();
        
        var context = ((App)System.Windows.Application.Current).Services!.GetRequiredService<CGEasy.Core.Data.LiteDbContext>();
        DataContext = new ViewModels.BilancioTemplateViewModel(context);
    }
}

