using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class TodoDialogView : Window
{
    public new bool DialogResult { get; private set; }

    public TodoDialogView(int? todoId = null)
    {
        InitializeComponent();

        try
        {
            // Ottieni context condiviso dall'app
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<CGEasyDbContext>();
            
            var todoRepo = new TodoStudioRepository(context);
            var clienteRepo = new ClienteRepository(context);
            var profRepo = new ProfessionistaRepository(context);
            var tipoRepo = new TipoPraticaRepository(context);

            // Ottieni ID utente corrente
            var currentUserId = SessionManager.CurrentUser?.Id ?? 0;

            var viewModel = new TodoDialogViewModel(
                todoRepo, 
                clienteRepo, 
                profRepo, 
                tipoRepo,
                currentUserId,
                todoId);

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
                $"Errore inizializzazione form TODO:\n\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Close();
        }
    }
}

