using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

public partial class TodoKanbanView : Window
{
    private TodoStudio? _draggedTodo;
    private Border? _draggedBorder;

    public TodoKanbanView()
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

            // Inizializza ViewModel
            DataContext = new TodoKanbanViewModel(todoRepo, clienteRepo, profRepo, tipoRepo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante l'inizializzazione Kanban:\n\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Close();
        }
    }

    /// <summary>
    /// Inizia drag quando si preme sul TODO
    /// </summary>
    private void Todo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is TodoStudio todo)
        {
            _draggedTodo = todo;
            _draggedBorder = border;
        }
    }

    /// <summary>
    /// Esegue drag se il mouse si è mosso abbastanza
    /// </summary>
    private void Todo_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _draggedTodo != null && sender is Border border)
        {
            try
            {
                // Feedback visivo: rendi il border semi-trasparente durante il drag
                border.Opacity = 0.6;

                var data = new DataObject("TodoStudio", _draggedTodo);
                DragDrop.DoDragDrop(border, data, DragDropEffects.Move);

                // Ripristina opacità
                border.Opacity = 1.0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante drag:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _draggedTodo = null;
                _draggedBorder = null;
            }
        }
    }

    /// <summary>
    /// Entra in una colonna durante il drag (feedback visivo)
    /// </summary>
    private void Column_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent("TodoStudio"))
        {
            e.Effects = DragDropEffects.Move;

            // Feedback visivo: evidenzia la colonna
            if (sender is FrameworkElement dropTarget)
            {
                var parent = VisualTreeHelper.GetParent(dropTarget);
                while (parent != null && parent is not Border)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Border columnBorder)
                {
                    columnBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blu
                    columnBorder.BorderThickness = new Thickness(3);
                }
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    /// <summary>
    /// Esce da una colonna durante il drag (rimuovi feedback)
    /// </summary>
    private void Column_DragLeave(object sender, DragEventArgs e)
    {
        // Ripristina bordo normale
        if (sender is FrameworkElement dropTarget)
        {
            var parent = VisualTreeHelper.GetParent(dropTarget);
            while (parent != null && parent is not Border)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is Border columnBorder)
            {
                columnBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Grigio
                columnBorder.BorderThickness = new Thickness(1);
            }
        }
        e.Handled = true;
    }

    /// <summary>
    /// Drop TODO in una colonna (cambia stato)
    /// </summary>
    private void Column_Drop(object sender, DragEventArgs e)
    {
        try
        {
            // Ripristina bordo normale
            FrameworkElement? dropTarget = sender as FrameworkElement;
            if (dropTarget != null)
            {
                var parent = VisualTreeHelper.GetParent(dropTarget);
                while (parent != null && parent is not Border)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is Border columnBorder)
                {
                    columnBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224));
                    columnBorder.BorderThickness = new Thickness(1);
                }
            }

            if (!e.Data.GetDataPresent("TodoStudio"))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var todo = e.Data.GetData("TodoStudio") as TodoStudio;
            if (todo == null || dropTarget == null)
                return;

            // Determina nuovo stato dalla colonna target (Grid o ItemsControl)
            var columnTag = dropTarget.Tag?.ToString();
            if (string.IsNullOrEmpty(columnTag))
                return;

            var nuovoStato = columnTag switch
            {
                "DaFare" => StatoTodo.DaFare,
                "InCorso" => StatoTodo.InCorso,
                "Completata" => StatoTodo.Completata,
                "Annullata" => StatoTodo.Annullata,
                _ => todo.Stato
            };

            // Se lo stato è cambiato, aggiorna
            if (nuovoStato != todo.Stato)
            {
                if (DataContext is TodoKanbanViewModel vm)
                {
                    vm.UpdateTodoStato(todo, nuovoStato);
                }
            }

            e.Effects = DragDropEffects.Move;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante drop:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            e.Handled = true;
        }
    }
}

