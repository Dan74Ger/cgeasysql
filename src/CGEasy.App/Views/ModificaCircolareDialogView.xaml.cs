using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views
{
    /// <summary>
    /// Interaction logic for ModificaCircolareDialogView.xaml
    /// </summary>
    public partial class ModificaCircolareDialogView : Window
    {
        public new bool DialogResult { get; private set; }

        public ModificaCircolareDialogView(int circolareId)
        {
            InitializeComponent();

            // Ottiene Singleton LiteDbContext
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<LiteDbContext>();

            // Crea ViewModel
            var viewModel = new ModificaCircolareDialogViewModel(context, circolareId);

            // Setup callback per chiusura
            viewModel.OnDialogClosed = (success) =>
            {
                DialogResult = success;
                this.Close();
            };

            DataContext = viewModel;
        }
    }
}

