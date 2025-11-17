using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class TodoDialogViewModel : ObservableObject
{
    private readonly TodoStudioRepository _todoRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly ProfessionistaRepository _professionistaRepository;
    private readonly TipoPraticaRepository _tipoPraticaRepository;

    private int? _todoId; // null = creazione, value = modifica
    private int _currentUserId;

    // ===== CAMPI TODO =====
    [ObservableProperty] private string _titolo = string.Empty;
    [ObservableProperty] private string _descrizione = string.Empty;
    [ObservableProperty] private string _note = string.Empty;
    
    [ObservableProperty] private CategoriaTodo _categoriaSelezionata;
    [ObservableProperty] private PrioritaTodo _prioritaSelezionata;
    [ObservableProperty] private StatoTodo _statoSelezionato;
    
    [ObservableProperty] private TipoPratica? _tipoPraticaSelezionata;
    [ObservableProperty] private Cliente? _clienteSelezionato;
    
    [ObservableProperty] private DateTime? _dataInizio;
    [ObservableProperty] private DateTime? _dataScadenza;
    [ObservableProperty] private TimeSpan? _orarioInizio;
    [ObservableProperty] private TimeSpan? _orarioFine;
    
    // ===== PROFESSIONISTI ASSEGNATI =====
    [ObservableProperty] private ObservableCollection<ProfessionistaCheckbox> _professionistiDisponibili = new();
    
    // ===== LISTE DROPDOWN =====
    [ObservableProperty] private ObservableCollection<TipoPratica> _tipiPratica = new();
    [ObservableProperty] private ObservableCollection<Cliente> _clienti = new();
    
    public List<CategoriaTodo> Categorie { get; } = Enum.GetValues<CategoriaTodo>().ToList();
    public List<PrioritaTodo> Priorita { get; } = Enum.GetValues<PrioritaTodo>().ToList();
    public List<StatoTodo> Stati { get; } = Enum.GetValues<StatoTodo>().ToList();

    [ObservableProperty] private string _windowTitle = "Nuovo TODO";
    [ObservableProperty] private bool _isEditMode;

    public bool IsCreateMode => !IsEditMode;

    public TodoDialogViewModel(
        TodoStudioRepository todoRepository,
        ClienteRepository clienteRepository,
        ProfessionistaRepository professionistaRepository,
        TipoPraticaRepository tipoPraticaRepository,
        int currentUserId,
        int? todoId = null)
    {
        _todoRepository = todoRepository;
        _clienteRepository = clienteRepository;
        _professionistaRepository = professionistaRepository;
        _tipoPraticaRepository = tipoPraticaRepository;
        _currentUserId = currentUserId;
        _todoId = todoId;

        IsEditMode = todoId.HasValue;
        WindowTitle = IsEditMode ? "Modifica TODO" : "Nuovo TODO";

        LoadData();

        if (IsEditMode && todoId.HasValue)
        {
            LoadTodo(todoId.Value);
        }
        else
        {
            // Default per nuovo TODO
            CategoriaSelezionata = CategoriaTodo.Fiscale;
            PrioritaSelezionata = PrioritaTodo.Media;
            StatoSelezionato = StatoTodo.DaFare;
            DataScadenza = DateTime.Now.AddDays(7); // Scadenza tra 1 settimana
        }
    }

    private void LoadData()
    {
        try
        {
            // Carica professionisti attivi
            var professionistiList = _professionistaRepository.GetAll()
                .Where(p => p.Attivo)
                .OrderBy(p => p.Cognome)
                .ThenBy(p => p.Nome)
                .ToList();
            
            ProfessionistiDisponibili.Clear();
            foreach (var p in professionistiList)
            {
                ProfessionistiDisponibili.Add(new ProfessionistaCheckbox
                {
                    Professionista = p,
                    IsSelected = false
                });
            }

            // Carica tipi pratica attivi
            var tipiPraticaList = _tipoPraticaRepository.GetAll()
                .Where(t => t.Attivo)
                .OrderBy(t => t.Ordine)
                .ToList();
            
            TipiPratica.Clear();
            foreach (var t in tipiPraticaList)
                TipiPratica.Add(t);

            // Carica clienti attivi
            var clientiList = _clienteRepository.GetAll()
                .Where(c => c.Attivo)
                .OrderBy(c => c.NomeCliente)
                .ToList();
            
            Clienti.Clear();
            foreach (var c in clientiList)
                Clienti.Add(c);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTodo(int id)
    {
        try
        {
            var todo = _todoRepository.GetById(id);
            if (todo == null)
            {
                MessageBox.Show("TODO non trovato.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Carica dati
            Titolo = todo.Titolo;
            Descrizione = todo.Descrizione ?? string.Empty;
            Note = todo.Note ?? string.Empty;
            CategoriaSelezionata = todo.Categoria;
            PrioritaSelezionata = todo.Priorita;
            StatoSelezionato = todo.Stato;
            DataInizio = todo.DataInizio;
            DataScadenza = todo.DataScadenza;
            OrarioInizio = todo.OrarioInizio;
            OrarioFine = todo.OrarioFine;

            // Seleziona tipo pratica
            if (todo.TipoPraticaId.HasValue)
            {
                TipoPraticaSelezionata = TipiPratica.FirstOrDefault(t => t.Id == todo.TipoPraticaId.Value);
            }

            // Seleziona cliente
            if (todo.ClienteId.HasValue)
            {
                ClienteSelezionato = Clienti.FirstOrDefault(c => c.Id == todo.ClienteId.Value);
            }

            // Seleziona professionisti assegnati
            foreach (var profId in todo.ProfessionistiAssegnatiIds)
            {
                var profCheckbox = ProfessionistiDisponibili.FirstOrDefault(p => p.Professionista.Id == profId);
                if (profCheckbox != null)
                {
                    profCheckbox.IsSelected = true;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento TODO:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool CanSave()
    {
        // Validazione base
        if (string.IsNullOrWhiteSpace(Titolo) || Titolo.Length < 3)
            return false;

        // Almeno un professionista deve essere assegnato
        if (!ProfessionistiDisponibili.Any(p => p.IsSelected))
            return false;

        return true;
    }

    [RelayCommand]
    private void Save()
    {
        try
        {
            // Validazioni aggiuntive
            if (string.IsNullOrWhiteSpace(Titolo) || Titolo.Length < 3)
            {
                MessageBox.Show("Il titolo deve contenere almeno 3 caratteri.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var professionistiSelezionati = ProfessionistiDisponibili
                .Where(p => p.IsSelected)
                .Select(p => p.Professionista)
                .ToList();

            if (!professionistiSelezionati.Any())
            {
                MessageBox.Show("Seleziona almeno un professionista assegnato.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crea o aggiorna TODO
            TodoStudio todo;

            if (IsEditMode && _todoId.HasValue)
            {
                // MODIFICA
                todo = _todoRepository.GetById(_todoId.Value)!;
                
                // Verifica permessi
                var canEdit = SessionManager.IsAdministrator || 
                              todo.CreatoreId == _currentUserId ||
                              todo.ProfessionistiAssegnatiIds.Contains(_currentUserId);

                if (!canEdit)
                {
                    MessageBox.Show("Non hai i permessi per modificare questo TODO.", "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                // CREAZIONE
                todo = new TodoStudio
                {
                    CreatoreId = _currentUserId,
                    DataCreazione = DateTime.Now
                };

                // Ottieni nome creatore
                var creatore = _professionistaRepository.GetById(_currentUserId);
                if (creatore != null)
                {
                    todo.CreatoreNome = $"{creatore.Cognome} {creatore.Nome}";
                }
            }

            // Aggiorna campi
            todo.Titolo = Titolo.Trim();
            todo.Descrizione = string.IsNullOrWhiteSpace(Descrizione) ? null : Descrizione.Trim();
            todo.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();
            todo.Categoria = CategoriaSelezionata;
            todo.Priorita = PrioritaSelezionata;
            todo.Stato = StatoSelezionato;
            todo.DataInizio = DataInizio;
            todo.DataScadenza = DataScadenza;
            todo.OrarioInizio = OrarioInizio;
            todo.OrarioFine = OrarioFine;

            // Tipo pratica
            if (TipoPraticaSelezionata != null)
            {
                todo.TipoPraticaId = TipoPraticaSelezionata.Id;
                todo.TipoPraticaNome = TipoPraticaSelezionata.NomePratica;
            }
            else
            {
                todo.TipoPraticaId = null;
                todo.TipoPraticaNome = null;
            }

            // Cliente
            if (ClienteSelezionato != null)
            {
                todo.ClienteId = ClienteSelezionato.Id;
                todo.ClienteNome = ClienteSelezionato.NomeCliente;
            }
            else
            {
                todo.ClienteId = null;
                todo.ClienteNome = null;
            }

            // Professionisti assegnati
            todo.ProfessionistiAssegnatiIds = professionistiSelezionati.Select(p => p.Id).ToList();
            todo.ProfessionistiAssegnatiNomi = professionistiSelezionati
                .Select(p => $"{p.Cognome} {p.Nome}")
                .ToList();

            // Salva
            if (IsEditMode)
            {
                _todoRepository.Update(todo);
                MessageBox.Show("✅ TODO aggiornato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _todoRepository.Insert(todo);
                MessageBox.Show("✅ TODO creato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Chiudi dialog
            CloseDialog(true);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore salvataggio TODO:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseDialog(false);
    }

    [RelayCommand]
    private void ClearCliente()
    {
        ClienteSelezionato = null;
    }

    [RelayCommand]
    private void ClearOrarioInizio()
    {
        OrarioInizio = null;
    }

    [RelayCommand]
    private void ClearOrarioFine()
    {
        OrarioFine = null;
    }

    public Action<bool>? OnDialogClosed { get; set; }

    private void CloseDialog(bool success)
    {
        OnDialogClosed?.Invoke(success);
    }
}

/// <summary>
/// Helper per checkbox professionisti con binding
/// </summary>
public partial class ProfessionistaCheckbox : ObservableObject
{
    [ObservableProperty] private Professionista _professionista = null!;
    [ObservableProperty] private bool _isSelected;

    public string DisplayName => $"{Professionista.Cognome} {Professionista.Nome}";
}

