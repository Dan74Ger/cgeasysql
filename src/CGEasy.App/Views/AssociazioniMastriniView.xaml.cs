using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views
{
    public partial class AssociazioniMastriniView : UserControl
    {
        public AssociazioniMastriniView()
        {
            InitializeComponent();
            
            // Inizializza ViewModel con LiteDbContext da DI
            var context = ((App)System.Windows.Application.Current).Services!.GetRequiredService<CGEasy.Core.Data.LiteDbContext>();
            DataContext = new ViewModels.AssociazioniMastriniViewModel(context);
        }
    }
}

