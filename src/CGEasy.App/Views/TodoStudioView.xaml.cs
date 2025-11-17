using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.App.ViewModels;
using CGEasy.Core.Data;

namespace CGEasy.App.Views;

public partial class TodoStudioView : Window
{
    public TodoStudioView()
    {
        InitializeComponent();
        
        try
        {
            // Ottiene il context Singleton dall'app
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<LiteDbContext>();
            
            // Inizializza ViewModel
            DataContext = new TodoStudioViewModel(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante l'inizializzazione TODO Studio:\n\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Close();
        }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is TodoStudioViewModel vm && vm.SelectedTodo != null)
        {
            vm.EditTodoCommand.Execute(null);
        }
    }
}

