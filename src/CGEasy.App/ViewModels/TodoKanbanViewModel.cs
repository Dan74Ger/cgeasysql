using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per vista Kanban TODO Studio
/// Gestisce 4 colonne: Da Fare, In Corso, Completata, Annullata
/// </summary>
public partial class TodoKanbanViewModel : ObservableObject
{
    private readonly TodoStudioRepository _todoRepo;
    private readonly ClienteRepository _clienteRepo;
    private readonly ProfessionistaRepository _profRepo;
    private readonly TipoPraticaRepository _tipoRepo;

    // ===== COLONNE KANBAN =====
    [ObservableProperty] private ObservableCollection<TodoStudio> _todosDaFare = new();
    [ObservableProperty] private ObservableCollection<TodoStudio> _todosInCorso = new();
    [ObservableProperty] private ObservableCollection<TodoStudio> _todosCompletata = new();
    [ObservableProperty] private ObservableCollection<TodoStudio> _todosAnnullata = new();

    // ===== FILTRI =====
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private object? _selectedProfessionista;
    [ObservableProperty] private object? _selectedTipoPratica;
    [ObservableProperty] private object? _selectedCliente;
    [ObservableProperty] private object? _selectedCategoria;
    [ObservableProperty] private object? _selectedPriorita;

    // ===== COLLECTIONS PER FILTRI =====
    [ObservableProperty] private ObservableCollection<Professionista> _professionisti = new();
    [ObservableProperty] private ObservableCollection<TipoPratica> _tipiPratica = new();
    [ObservableProperty] private ObservableCollection<Cliente> _clienti = new();
    [ObservableProperty] private ObservableCollection<string> _categorie = new();
    [ObservableProperty] private ObservableCollection<string> _priorita = new();

    // ===== STATISTICHE =====
    [ObservableProperty] private int _totalTodos;
    [ObservableProperty] private int _countDaFare;
    [ObservableProperty] private int _countInCorso;
    [ObservableProperty] private int _countCompletata;
    [ObservableProperty] private int _countAnnullata;

    // Tutti i TODO (prima di filtrare)
    private List<TodoStudio> _allTodos = new();

    public TodoKanbanViewModel(
        TodoStudioRepository todoRepo,
        ClienteRepository clienteRepo,
        ProfessionistaRepository profRepo,
        TipoPraticaRepository tipoRepo)
    {
        _todoRepo = todoRepo;
        _clienteRepo = clienteRepo;
        _profRepo = profRepo;
        _tipoRepo = tipoRepo;

        // Sottoscrivi evento di modifica TODO per sincronizzare con altre viste
        CGEasy.Core.Services.TodoEventService.TodoChanged += OnTodoChanged;

        LoadData();
    }

    /// <summary>
    /// Gestisce notifica di modifica TODO da altre viste
    /// </summary>
    private void OnTodoChanged(object? sender, EventArgs e)
    {
        // Ricarica dati per sincronizzare con modifiche da lista/calendario
        LoadData();
    }

    /// <summary>
    /// Carica tutti i dati e popola le colonne Kanban
    /// </summary>
    [RelayCommand]
    private void LoadData()
    {
        try
        {
            // Carica tutti i TODO
            _allTodos = _todoRepo.GetAll().ToList();

            // Arricchisci con nomi
            foreach (var todo in _allTodos)
            {
                EnrichTodoWithNames(todo);
            }

            // Carica filtri
            LoadFilters();

            // Applica filtri e popola colonne
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento dati Kanban:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Arricchisce TODO con nomi di entità correlate
    /// </summary>
    private void EnrichTodoWithNames(TodoStudio todo)
    {
        // Cliente
        if (todo.ClienteId.HasValue)
        {
            var cliente = _clienteRepo.GetById(todo.ClienteId.Value);
            todo.ClienteNome = cliente?.NomeCliente ?? "";
        }

        // Tipo Pratica
        if (todo.TipoPraticaId.HasValue)
        {
            var tipo = _tipoRepo.GetById(todo.TipoPraticaId.Value);
            todo.TipoPraticaNome = tipo?.NomePratica ?? "";
        }

        // Creatore
        var creatore = _profRepo.GetById(todo.CreatoreId);
        todo.CreatoreNome = creatore != null ? $"{creatore.Cognome} {creatore.Nome}" : "Sconosciuto";

        // Professionisti assegnati
        if (todo.ProfessionistiAssegnatiIds?.Any() == true)
        {
            var nomi = new List<string>();
            foreach (var profId in todo.ProfessionistiAssegnatiIds)
            {
                var prof = _profRepo.GetById(profId);
                if (prof != null)
                {
                    nomi.Add($"{prof.Cognome} {prof.Nome}");
                }
            }
            todo.ProfessionistiAssegnatiNomi = nomi;
        }
    }

    /// <summary>
    /// Carica le liste per i filtri
    /// </summary>
    private void LoadFilters()
    {
        // Professionisti
        Professionisti.Clear();
        foreach (var prof in _profRepo.GetAll().Where(p => p.Attivo).OrderBy(p => p.Cognome))
        {
            Professionisti.Add(prof);
        }

        // Tipi Pratica
        TipiPratica.Clear();
        foreach (var tipo in _tipoRepo.GetAll().Where(t => t.Attivo).OrderBy(t => t.Ordine))
        {
            TipiPratica.Add(tipo);
        }

        // Clienti
        Clienti.Clear();
        foreach (var cliente in _clienteRepo.GetAll().Where(c => c.Attivo).OrderBy(c => c.NomeCliente))
        {
            Clienti.Add(cliente);
        }

        // Categorie
        Categorie.Clear();
        Categorie.Add("Tutte");
        foreach (var cat in Enum.GetValues<CategoriaTodo>())
        {
            Categorie.Add(cat.ToString());
        }

        // Priorità
        Priorita.Clear();
        Priorita.Add("Tutte");
        foreach (var prio in Enum.GetValues<PrioritaTodo>())
        {
            Priorita.Add(prio.ToString());
        }
    }

    /// <summary>
    /// Applica filtri e popola le 4 colonne Kanban
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = _allTodos.AsEnumerable();

        // Filtro 1: Ricerca testuale
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(t =>
                t.Titolo.ToLower().Contains(search) ||
                (t.Descrizione?.ToLower().Contains(search) ?? false) ||
                (t.ClienteNome?.ToLower().Contains(search) ?? false) ||
                (t.TipoPraticaNome?.ToLower().Contains(search) ?? false));
        }

        // Filtro 2: Professionista
        if (SelectedProfessionista is Professionista prof)
        {
            filtered = filtered.Where(t => t.ProfessionistiAssegnatiIds?.Contains(prof.Id) ?? false);
        }

        // Filtro 3: Tipo Pratica
        if (SelectedTipoPratica is TipoPratica tipo)
        {
            filtered = filtered.Where(t => t.TipoPraticaId == tipo.Id);
        }

        // Filtro 4: Cliente
        if (SelectedCliente is Cliente cliente)
        {
            filtered = filtered.Where(t => t.ClienteId == cliente.Id);
        }

        // Filtro 5: Categoria
        if (SelectedCategoria is string catStr && catStr != "Tutte" && Enum.TryParse<CategoriaTodo>(catStr, out var categoria))
        {
            filtered = filtered.Where(t => t.Categoria == categoria);
        }

        // Filtro 6: Priorità
        if (SelectedPriorita is string prioStr && prioStr != "Tutte" && Enum.TryParse<PrioritaTodo>(prioStr, out var priorita))
        {
            filtered = filtered.Where(t => t.Priorita == priorita);
        }

        var filteredList = filtered.ToList();

        // Popola le 4 colonne Kanban
        TodosDaFare.Clear();
        TodosInCorso.Clear();
        TodosCompletata.Clear();
        TodosAnnullata.Clear();

        foreach (var todo in filteredList.OrderBy(t => t.Priorita).ThenBy(t => t.DataScadenza))
        {
            switch (todo.Stato)
            {
                case StatoTodo.DaFare:
                    TodosDaFare.Add(todo);
                    break;
                case StatoTodo.InCorso:
                    TodosInCorso.Add(todo);
                    break;
                case StatoTodo.Completata:
                    TodosCompletata.Add(todo);
                    break;
                case StatoTodo.Annullata:
                    TodosAnnullata.Add(todo);
                    break;
            }
        }

        UpdateStatistics();
    }

    /// <summary>
    /// Aggiorna le statistiche dopo filtri
    /// </summary>
    private void UpdateStatistics()
    {
        CountDaFare = TodosDaFare.Count;
        CountInCorso = TodosInCorso.Count;
        CountCompletata = TodosCompletata.Count;
        CountAnnullata = TodosAnnullata.Count;
        TotalTodos = CountDaFare + CountInCorso + CountCompletata + CountAnnullata;
    }

    /// <summary>
    /// Cambia stato TODO (drag & drop tra colonne)
    /// </summary>
    public void UpdateTodoStato(TodoStudio todo, StatoTodo nuovoStato)
    {
        try
        {
            var originalStato = todo.Stato;
            
            // Aggiorna stato nel database
            _todoRepo.UpdateStato(todo.Id, nuovoStato);
            
            // Aggiorna oggetto locale
            todo.Stato = nuovoStato;
            
            // Se completata, aggiorna data completamento
            if (nuovoStato == StatoTodo.Completata && !todo.DataCompletamento.HasValue)
            {
                todo.DataCompletamento = DateTime.Now;
                _todoRepo.Update(todo);
            }
            else if (nuovoStato != StatoTodo.Completata && originalStato == StatoTodo.Completata)
            {
                // Se si rimuove da "Completata", cancella data completamento
                todo.DataCompletamento = null;
                _todoRepo.Update(todo);
            }

            // Notifica altre viste del cambiamento
            CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();

            // Ricarica per aggiornare UI
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore aggiornamento stato TODO:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Apre dialog per creare nuovo TODO
    /// </summary>
    [RelayCommand]
    private void NewTodo()
    {
        var dialog = new Views.TodoDialogView();
        dialog.ShowDialog();

        if (dialog.DialogResult)
        {
            // Notifica altre viste del cambiamento
            CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
            LoadData();
        }
    }

    /// <summary>
    /// Apre dialog per modificare TODO
    /// </summary>
    [RelayCommand]
    private void EditTodo(TodoStudio todo)
    {
        if (todo == null) return;

        var dialog = new Views.TodoDialogView(todo.Id);
        dialog.ShowDialog();

        if (dialog.DialogResult)
        {
            // Notifica altre viste del cambiamento
            CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
            LoadData();
        }
    }

    /// <summary>
    /// Ripristina tutti i filtri
    /// </summary>
    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedProfessionista = null;
        SelectedTipoPratica = null;
        SelectedCliente = null;
        SelectedCategoria = "Tutte";
        SelectedPriorita = "Tutte";
        ApplyFilters();
    }

    // ===== FILTRI REAL-TIME =====

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedProfessionistaChanged(object? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedTipoPraticaChanged(object? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedClienteChanged(object? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoriaChanged(object? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedPrioritaChanged(object? value)
    {
        ApplyFilters();
    }
}
