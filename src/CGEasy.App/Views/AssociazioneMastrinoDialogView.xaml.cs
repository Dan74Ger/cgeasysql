using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views
{
    public partial class AssociazioneMastrinoDialogView : Window
    {
        public new bool DialogResult { get; private set; }

        public AssociazioneMastrinoDialogView(int? associazioneId = null)
        {
            InitializeComponent();

            try
            {
                // ✅ Ottieni context condiviso dall'app (PATTERN TODO)
                var app = (App)Application.Current;
                var context = app.Services!.GetRequiredService<LiteDbContext>();
                
                // ✅ Crea ViewModel e passa il context (come TODO)
                var viewModel = new AssociazioneMastrinoDialogViewModel(context, associazioneId);
                
                // ✅ Setup callback per chiusura dialog (come TODO)
                viewModel.OnDialogClosed = (success) =>
                {
                    DialogResult = success;
                    Close();
                };
                
                DataContext = viewModel;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Errore inizializzazione form Associazione Mastrino:\n\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Close();
            }
        }
    }
}








