using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CGEasy.Core.Models;
using CGEasy.Core.Data;
using CGEasy.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class TodoCalendarioView : Window
{
    private TodoStudio? _draggedTodo;

    public TodoCalendarioView()
    {
        InitializeComponent();
        
        try
        {
            // Ottieni context condiviso dall'app
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<LiteDbContext>();
            
            var todoRepo = new TodoStudioRepository(context);
            var clienteRepo = new ClienteRepository(context);
            var profRepo = new ProfessionistaRepository(context);
            var tipoRepo = new TipoPraticaRepository(context);

            // Inizializza ViewModel
            DataContext = new ViewModels.TodoCalendarioViewModel(
                todoRepo,
                clienteRepo,
                profRepo,
                tipoRepo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante l'inizializzazione del calendario:\n\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Close();
        }
    }

    // ===== DRAG & DROP LOGIC =====

    private void Todo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is TodoStudio todo)
        {
            _draggedTodo = todo;
        }
    }

    private void Todo_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && 
            _draggedTodo != null && 
            sender is Border border)
        {
            var data = new DataObject("TodoStudio", _draggedTodo);

            try
            {
                DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
            }
            catch
            {
                // Ignora errori
            }
            finally
            {
                _draggedTodo = null;
            }
        }
    }

    private void CalendarDay_DragEnter(object sender, DragEventArgs e)
    {
        if (sender is Border border && e.Data.GetDataPresent("TodoStudio"))
        {
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            border.BorderThickness = new Thickness(3);
            e.Effects = DragDropEffects.Move;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void CalendarDay_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Border border)
        {
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            border.BorderThickness = new Thickness(1);
        }
        e.Handled = true;
    }

    private void CalendarDay_Drop(object sender, DragEventArgs e)
    {
        if (sender is Border border && border.Tag is DateTime targetDate)
        {
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            border.BorderThickness = new Thickness(1);

            if (e.Data.GetDataPresent("TodoStudio") && 
                e.Data.GetData("TodoStudio") is TodoStudio todo)
            {
                var oldDate = todo.DataScadenza?.ToString("dd/MM/yyyy") ?? "Nessuna";
                var newDate = targetDate.ToString("dd/MM/yyyy");

                var result = MessageBox.Show(
                    $"Spostare il TODO?\n\n" +
                    $"📝 {todo.Titolo}\n\n" +
                    $"Da: {oldDate}\n" +
                    $"A: {newDate}",
                    "Conferma Spostamento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is ViewModels.TodoCalendarioViewModel vm)
                    {
                        var success = vm.UpdateTodoDataScadenza(todo.Id, targetDate);

                        if (success)
                        {
                            MessageBox.Show(
                                "✅ TODO spostato con successo!",
                                "Operazione Completata",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "❌ Errore durante lo spostamento del TODO.",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
            }
        }
        e.Handled = true;
    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count && count > 0)
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}


