using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class TodoStudioViewModel : ObservableObject
{
    private readonly TodoStudioRepository _todoRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly ProfessionistaRepository _professionistaRepository;
    private readonly TipoPraticaRepository _tipoPraticaRepository;

    // ===== FILTRI =====
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private object? _selectedProfessionista;
    [ObservableProperty] private object? _selectedTipoPratica;
    [ObservableProperty] private object? _selectedStato;
    [ObservableProperty] private object? _selectedCategoria;
    [ObservableProperty] private object? _selectedPriorita;
    [ObservableProperty] private object? _selectedCliente;
    [ObservableProperty] private bool _showOverdueOnly;

    // ===== LISTE DROPDOWN =====
    [ObservableProperty] private ObservableCollection<Professionista> _professionisti = new();
    [ObservableProperty] private ObservableCollection<TipoPratica> _tipiPratica = new();
    [ObservableProperty] private ObservableCollection<Cliente> _clienti = new();
    
    public List<string> Stati { get; } = new() { "Tutti", "DaFare", "InCorso", "Completata", "Annullata" };
    public List<string> Categorie { get; } = new() { "Tutte", "Amministrativa", "Fiscale", "Contabile", "Legale", "Consulenza", "Altro" };
    public List<string> Priorita { get; } = new() { "Tutte", "Urgente", "Alta", "Normale", "Bassa" };

    // ===== DATI =====
    [ObservableProperty] private ObservableCollection<TodoStudio> _filteredTodos = new();
    private List<TodoStudio> _allTodos = new();

    // ===== SELEZIONE =====
    [ObservableProperty] private TodoStudio? _selectedTodo;

    // ===== STATISTICHE =====
    [ObservableProperty] private int _totalTodos;
    [ObservableProperty] private int _daFareCount;
    [ObservableProperty] private int _inCorsoCount;
    [ObservableProperty] private int _completatiCount;
    [ObservableProperty] private int _annullatiCount;
    [ObservableProperty] private int _scadutiCount;
    [ObservableProperty] private int _urgentiCount;

    /// <summary>
    /// Costruttore con context
    /// </summary>
    public TodoStudioViewModel(CGEasyDbContext context)
    {
        _todoRepository = new TodoStudioRepository(context);
        _clienteRepository = new ClienteRepository(context);
        _professionistaRepository = new ProfessionistaRepository(context);
        _tipoPraticaRepository = new TipoPraticaRepository(context);

        // Inizializza filtri
        SelectedStato = "Tutti";
        SelectedCategoria = "Tutte";
        SelectedPriorita = "Tutte";

        // Sottoscrivi evento di modifica TODO per sincronizzare con altre viste
        CGEasy.Core.Services.TodoEventService.TodoChanged += OnTodoChanged;

        // Carica dati in modo asincrono per non bloccare l'UI
        _ = LoadDataAsync();
    }

    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        await System.Threading.Tasks.Task.Run(() => LoadData());
    }

    /// <summary>
    /// Gestisce notifica di modifica TODO da altre viste
    /// </summary>
    private void OnTodoChanged(object? sender, EventArgs e)
    {
        // Ricarica dati per sincronizzare con modifiche da calendario/kanban
        LoadData();
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
            
            Professionisti.Clear();
            foreach (var p in professionistiList)
                Professionisti.Add(p);

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

            // Carica tutti i TODO
            _allTodos = _todoRepository.GetAll().ToList();

            // Applica filtri iniziali
            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allTodos.AsEnumerable();

        // Filtro: Ricerca testuale
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(t =>
                t.Titolo.ToLower().Contains(search) ||
                (t.Descrizione != null && t.Descrizione.ToLower().Contains(search))
            );
        }

        // Filtro: Professionista
        if (SelectedProfessionista is Professionista prof)
        {
            filtered = filtered.Where(t => t.ProfessionistiAssegnatiIds.Contains(prof.Id));
        }

        // Filtro: Tipo Pratica
        if (SelectedTipoPratica is TipoPratica tipo)
        {
            filtered = filtered.Where(t => t.TipoPraticaId == tipo.Id);
        }

        // Filtro: Stato
        if (SelectedStato is string statoStr && statoStr != "Tutti")
        {
            var stato = statoStr switch
            {
                "⏳ Da Fare" => StatoTodo.DaFare,
                "⚙️ In Corso" => StatoTodo.InCorso,
                "✅ Completata" => StatoTodo.Completata,
                "❌ Annullata" => StatoTodo.Annullata,
                _ => (StatoTodo?)null
            };
            
            if (stato.HasValue)
                filtered = filtered.Where(t => t.Stato == stato.Value);
        }

        // Filtro: Categoria
        if (SelectedCategoria is string catStr && catStr != "Tutte")
        {
            var categoria = catStr switch
            {
                "📁 Amministrativa" => CategoriaTodo.Amministrativa,
                "💼 Fiscale" => CategoriaTodo.Fiscale,
                "📊 Contabile" => CategoriaTodo.Contabile,
                "⚖️ Legale" => CategoriaTodo.Legale,
                "🎯 Consulenza" => CategoriaTodo.Consulenza,
                "📌 Altro" => CategoriaTodo.Altro,
                _ => (CategoriaTodo?)null
            };
            
            if (categoria.HasValue)
                filtered = filtered.Where(t => t.Categoria == categoria.Value);
        }

        // Filtro: Priorità
        if (SelectedPriorita is string prioStr && prioStr != "Tutte")
        {
            var priorita = prioStr switch
            {
                "🔴 Urgente" => PrioritaTodo.Urgente,
                "🟠 Alta" => PrioritaTodo.Alta,
                "🟡 Media" => PrioritaTodo.Media,
                "🟢 Bassa" => PrioritaTodo.Bassa,
                _ => (PrioritaTodo?)null
            };
            
            if (priorita.HasValue)
                filtered = filtered.Where(t => t.Priorita == priorita.Value);
        }

        // Filtro: Cliente
        if (SelectedCliente is Cliente cliente)
        {
            filtered = filtered.Where(t => t.ClienteId == cliente.Id);
        }

        // Filtro: Solo scaduti
        if (ShowOverdueOnly)
        {
            filtered = filtered.Where(t => t.IsScaduto);
        }

        // Aggiorna lista
        var result = filtered
            .OrderByDescending(t => t.Priorita)
            .ThenBy(t => t.DataScadenza)
            .ThenBy(t => t.Titolo)
            .ToList();

        // Aggiorna observable collection
        FilteredTodos = new ObservableCollection<TodoStudio>(result);

        // Aggiorna statistiche
        UpdateStatistics();
    }

    private void UpdateStatistics()
    {
        TotalTodos = FilteredTodos.Count;
        DaFareCount = FilteredTodos.Count(t => t.Stato == StatoTodo.DaFare);
        InCorsoCount = FilteredTodos.Count(t => t.Stato == StatoTodo.InCorso);
        CompletatiCount = FilteredTodos.Count(t => t.Stato == StatoTodo.Completata);
        AnnullatiCount = FilteredTodos.Count(t => t.Stato == StatoTodo.Annullata);
        ScadutiCount = FilteredTodos.Count(t => t.IsScaduto);
        UrgentiCount = FilteredTodos.Count(t => t.Priorita == PrioritaTodo.Urgente);
    }

    [RelayCommand]
    private void NewTodo()
    {
        var dialog = new Views.TodoDialogView();
        // Non impostiamo Owner perché TodoStudioView è già una Window separata
        dialog.ShowDialog();

        if (dialog.DialogResult)
        {
            // Notifica altre viste del cambiamento
            CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
            Refresh();
        }
    }

    [RelayCommand]
    private void EditTodo()
    {
        if (SelectedTodo == null)
        {
            MessageBox.Show("Seleziona un TODO da modificare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Views.TodoDialogView(SelectedTodo.Id);
        // Non impostiamo Owner perché TodoStudioView è già una Window separata
        dialog.ShowDialog();

        if (dialog.DialogResult)
        {
            // Notifica altre viste del cambiamento
            CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
            Refresh();
        }
    }

    [RelayCommand]
    private void DeleteTodo()
    {
        if (SelectedTodo == null)
        {
            MessageBox.Show("Seleziona un TODO da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica permessi
        var currentUserId = SessionManager.CurrentUser?.Id ?? 0;
        bool canDelete = SessionManager.IsAdministrator || 
                        SelectedTodo.CreatoreId == currentUserId;

        if (!canDelete)
        {
            MessageBox.Show("Non hai i permessi per eliminare questo TODO.\n\nSolo il creatore o un amministratore possono eliminarlo.", 
                "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Eliminare definitivamente il TODO?\n\n" +
            $"📝 {SelectedTodo.Titolo}\n" +
            $"📅 {SelectedTodo.DataScadenza?.ToString("dd/MM/yyyy") ?? "Nessuna scadenza"}\n\n" +
            $"⚠️ Questa operazione non può essere annullata!",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _todoRepository.Delete(SelectedTodo.Id);
                // Notifica altre viste del cambiamento
                CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
                MessageBox.Show("✅ TODO eliminato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore eliminazione TODO:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedTodo == null)
        {
            MessageBox.Show("Seleziona un TODO per visualizzare i dettagli.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var details = $"📝 TODO DETTAGLI\n\n" +
                     $"Titolo: {SelectedTodo.Titolo}\n" +
                     $"Stato: {SelectedTodo.TestoStato} {SelectedTodo.IconaStato}\n" +
                     $"Priorità: {SelectedTodo.Priorita} {SelectedTodo.IconaPriorita}\n" +
                     $"Categoria: {SelectedTodo.TestoCategoria}\n\n" +
                     $"Tipo Pratica: {SelectedTodo.TipoPraticaNome ?? "Nessuno"}\n" +
                     $"Cliente: {SelectedTodo.ClienteNome ?? "Nessuno"}\n" +
                     $"Creatore: {SelectedTodo.CreatoreNome}\n" +
                     $"Assegnati: {SelectedTodo.ProfessionistiAssegnatiText}\n\n" +
                     $"Data Creazione: {SelectedTodo.DataCreazione:dd/MM/yyyy HH:mm}\n" +
                     $"Data Scadenza: {SelectedTodo.DataScadenza?.ToString("dd/MM/yyyy") ?? "Nessuna"}\n" +
                     $"Data Completamento: {SelectedTodo.DataCompletamento?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}\n\n";

        if (!string.IsNullOrWhiteSpace(SelectedTodo.Descrizione))
            details += $"Descrizione:\n{SelectedTodo.Descrizione}\n\n";

        if (!string.IsNullOrWhiteSpace(SelectedTodo.Note))
            details += $"Note:\n{SelectedTodo.Note}\n\n";

        if (SelectedTodo.IsScaduto)
            details += $"⚠️ TODO SCADUTO!\n";

        MessageBox.Show(details, "Dettagli TODO", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void CambiaStato(string nuovoStatoStr)
    {
        if (SelectedTodo == null) return;

        var nuovoStato = nuovoStatoStr switch
        {
            "DaFare" => StatoTodo.DaFare,
            "InCorso" => StatoTodo.InCorso,
            "Completata" => StatoTodo.Completata,
            "Annullata" => StatoTodo.Annullata,
            _ => SelectedTodo.Stato
        };

        try
        {
            _todoRepository.CambiaStato(SelectedTodo.Id, nuovoStato);
            MessageBox.Show($"✅ Stato cambiato in: {nuovoStato}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore cambio stato:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedProfessionista = "Tutti";
        SelectedTipoPratica = "Tutti";
        SelectedStato = "Tutti";
        SelectedCategoria = "Tutte";
        SelectedPriorita = "Tutte";
        SelectedCliente = "Tutti";
        ShowOverdueOnly = false;
        
        ApplyFilters();
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"TodoStudio_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            using var workbook = new XLWorkbook();
            
            // Foglio 1: Tutti i TODO
            var worksheet = workbook.Worksheets.Add("TODO Studio");
            
            // Headers
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Titolo";
            worksheet.Cell(1, 3).Value = "Stato";
            worksheet.Cell(1, 4).Value = "Priorità";
            worksheet.Cell(1, 5).Value = "Categoria";
            worksheet.Cell(1, 6).Value = "Tipo Pratica";
            worksheet.Cell(1, 7).Value = "Cliente";
            worksheet.Cell(1, 8).Value = "Creatore";
            worksheet.Cell(1, 9).Value = "Professionisti Assegnati";
            worksheet.Cell(1, 10).Value = "Data Creazione";
            worksheet.Cell(1, 11).Value = "Data Inizio";
            worksheet.Cell(1, 12).Value = "Orario Inizio";
            worksheet.Cell(1, 13).Value = "Data Scadenza";
            worksheet.Cell(1, 14).Value = "Orario Fine";
            worksheet.Cell(1, 15).Value = "Data Completamento";
            worksheet.Cell(1, 16).Value = "Scaduto";
            worksheet.Cell(1, 17).Value = "Descrizione";
            worksheet.Cell(1, 18).Value = "Note";

            // Formatta header
            var headerRange = worksheet.Range(1, 1, 1, 18);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2196F3");
            headerRange.Style.Font.FontColor = XLColor.White;

            // Dati
            int row = 2;
            foreach (var todo in FilteredTodos)
            {
                worksheet.Cell(row, 1).Value = todo.Id;
                worksheet.Cell(row, 2).Value = todo.Titolo;
                worksheet.Cell(row, 3).Value = todo.TestoStato;
                worksheet.Cell(row, 4).Value = todo.Priorita.ToString();
                worksheet.Cell(row, 5).Value = todo.Categoria.ToString();
                worksheet.Cell(row, 6).Value = todo.TipoPraticaNome ?? "";
                worksheet.Cell(row, 7).Value = todo.ClienteNome ?? "";
                worksheet.Cell(row, 8).Value = todo.CreatoreNome;
                worksheet.Cell(row, 9).Value = todo.ProfessionistiAssegnatiText;
                worksheet.Cell(row, 10).Value = todo.DataCreazione.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 11).Value = todo.DataInizio?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 12).Value = todo.OrarioInizio?.ToString(@"hh\:mm") ?? "";
                worksheet.Cell(row, 13).Value = todo.DataScadenza?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 14).Value = todo.OrarioFine?.ToString(@"hh\:mm") ?? "";
                worksheet.Cell(row, 15).Value = todo.DataCompletamento?.ToString("dd/MM/yyyy HH:mm") ?? "";
                worksheet.Cell(row, 16).Value = todo.IsScaduto ? "SÌ" : "NO";
                worksheet.Cell(row, 17).Value = todo.Descrizione ?? "";
                worksheet.Cell(row, 18).Value = todo.Note ?? "";

                // Colora riga in base allo stato (fase)
                var rowRange = worksheet.Range(row, 1, row, 18);
                rowRange.Style.Fill.BackgroundColor = todo.Stato switch
                {
                    StatoTodo.DaFare => XLColor.FromHtml("#E0E0E0"),     // Grigio chiaro
                    StatoTodo.InCorso => XLColor.FromHtml("#E3F2FD"),    // Blu chiaro
                    StatoTodo.Completata => XLColor.FromHtml("#E8F5E9"), // Verde chiaro
                    StatoTodo.Annullata => XLColor.FromHtml("#FFEBEE"),  // Rosso chiaro
                    _ => XLColor.White
                };

                row++;
            }

            // Auto-fit colonne
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(saveDialog.FileName);
            MessageBox.Show($"✅ Export completato!\n\nFile salvato:\n{saveDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante export Excel:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

    partial void OnSelectedStatoChanged(object? value)
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

    partial void OnSelectedClienteChanged(object? value)
    {
        ApplyFilters();
    }

    partial void OnShowOverdueOnlyChanged(bool value)
    {
        ApplyFilters();
    }

    /// <summary>
    /// Apre la vista calendario
    /// </summary>
    [RelayCommand]
    private void OpenCalendario()
    {
        try
        {
            var calendarioView = new Views.TodoCalendarioView();
            calendarioView.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura calendario:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Apre la vista Kanban
    /// </summary>
    [RelayCommand]
    private void OpenKanban()
    {
        try
        {
            var kanbanView = new Views.TodoKanbanView();
            kanbanView.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Kanban:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

