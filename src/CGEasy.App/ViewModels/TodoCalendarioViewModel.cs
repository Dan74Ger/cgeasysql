using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class TodoCalendarioViewModel : ObservableObject
{
    private readonly TodoStudioRepository _todoRepository;
    private readonly ClienteRepository _clienteRepository;
    private readonly ProfessionistaRepository _professionistaRepository;
    private readonly TipoPraticaRepository _tipoPraticaRepository;

    // ===== NAVIGAZIONE MESE =====
    [ObservableProperty] private DateTime _currentMonth;
    [ObservableProperty] private string _currentMonthText = string.Empty;

    // ===== FILTRI (8 DIMENSIONI) =====
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private object? _selectedProfessionista;
    [ObservableProperty] private object? _selectedTipoPratica;
    [ObservableProperty] private object? _selectedStato;
    [ObservableProperty] private object? _selectedCategoria;
    [ObservableProperty] private object? _selectedPriorita;
    [ObservableProperty] private object? _selectedCliente;

    // ===== LISTE DROPDOWN =====
    [ObservableProperty] private ObservableCollection<Professionista> _professionisti = new();
    [ObservableProperty] private ObservableCollection<TipoPratica> _tipiPratica = new();
    [ObservableProperty] private ObservableCollection<Cliente> _clienti = new();
    
    public List<string> Stati { get; } = new() { "Tutti", "⏳ Da Fare", "⚙️ In Corso", "✅ Completata", "❌ Annullata" };
    public List<string> Categorie { get; } = new() { "Tutte", "📁 Amministrativa", "💼 Fiscale", "📊 Contabile", "⚖️ Legale", "🎯 Consulenza", "📌 Altro" };
    public List<string> Priorita { get; } = new() { "Tutte", "🔴 Urgente", "🟠 Alta", "🟡 Media", "🟢 Bassa" };

    // ===== DATI CALENDARIO =====
    [ObservableProperty] private ObservableCollection<CalendarDay> _calendarDays = new();
    private List<TodoStudio> _allTodos = new();
    private List<TodoStudio> _filteredTodos = new();

    // ===== STATISTICHE GENERALI =====
    [ObservableProperty] private int _totale;
    [ObservableProperty] private int _scaduti;

    // ===== STATISTICHE PER FASE =====
    [ObservableProperty] private int _daFare;
    [ObservableProperty] private int _daFareUrgenti;
    [ObservableProperty] private int _daFareAlta;
    [ObservableProperty] private int _daFareMedia;
    [ObservableProperty] private int _daFareBassa;

    [ObservableProperty] private int _inCorso;
    [ObservableProperty] private int _inCorsoUrgenti;
    [ObservableProperty] private int _inCorsoAlta;
    [ObservableProperty] private int _inCorsoMedia;
    [ObservableProperty] private int _inCorsoBassa;

    [ObservableProperty] private int _completati;
    [ObservableProperty] private int _completatiOggi;
    [ObservableProperty] private int _completatiSettimana;
    [ObservableProperty] private int _completatiMese;

    [ObservableProperty] private int _annullati;

    // ===== STATISTICHE PER PRIORITÀ =====
    [ObservableProperty] private int _urgenti;
    [ObservableProperty] private int _alta;
    [ObservableProperty] private int _media;
    [ObservableProperty] private int _bassa;

    public TodoCalendarioViewModel(
        TodoStudioRepository todoRepository,
        ClienteRepository clienteRepository,
        ProfessionistaRepository professionistaRepository,
        TipoPraticaRepository tipoPraticaRepository)
    {
        _todoRepository = todoRepository;
        _clienteRepository = clienteRepository;
        _professionistaRepository = professionistaRepository;
        _tipoPraticaRepository = tipoPraticaRepository;

        // Inizializza a mese corrente
        CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        UpdateCurrentMonthText();

        // Inizializza filtri (null = mostra tutti)
        SelectedStato = "Tutti";
        SelectedCategoria = "Tutte";
        SelectedPriorita = "Tutte";
        SelectedProfessionista = null; // null = mostra tutti
        SelectedTipoPratica = null;     // null = mostra tutti
        SelectedCliente = null;          // null = mostra tutti

        // Sottoscrivi evento di modifica TODO per sincronizzare con altre viste
        CGEasy.Core.Services.TodoEventService.TodoChanged += OnTodoChanged;

        LoadData();
    }

    /// <summary>
    /// Gestisce notifica di modifica TODO da altre viste
    /// </summary>
    private void OnTodoChanged(object? sender, EventArgs e)
    {
        // Ricarica dati per sincronizzare con modifiche da lista/kanban
        LoadData();
    }

    private void UpdateCurrentMonthText()
    {
        var culture = new CultureInfo("it-IT");
        CurrentMonthText = CurrentMonth.ToString("MMMM yyyy", culture).ToUpper();
    }

    [RelayCommand]
    private void PreviousMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        UpdateCurrentMonthText();
        BuildCalendar();
    }

    [RelayCommand]
    private void NextMonth()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        UpdateCurrentMonthText();
        BuildCalendar();
    }

    [RelayCommand]
    private void GoToToday()
    {
        CurrentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        UpdateCurrentMonthText();
        BuildCalendar();
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
        
        ApplyFilters(); // Riapplica con filtri resettati
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

    /// <summary>
    /// Applica tutti i filtri ai TODO (REAL-TIME)
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
                (t.Descrizione != null && t.Descrizione.ToLower().Contains(search))
            );
        }

        // Filtro 2: Professionista
        if (SelectedProfessionista is Professionista prof)
        {
            filtered = filtered.Where(t => t.ProfessionistiAssegnatiIds.Contains(prof.Id));
        }

        // Filtro 3: Tipo Pratica
        if (SelectedTipoPratica is TipoPratica tipo)
        {
            filtered = filtered.Where(t => t.TipoPraticaId == tipo.Id);
        }

        // Filtro 4: Stato/Fase
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

        // Filtro 5: Categoria
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

        // Filtro 6: Priorità
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

        // Filtro 7: Cliente
        if (SelectedCliente is Cliente cliente)
        {
            filtered = filtered.Where(t => t.ClienteId == cliente.Id);
        }

        // Converti a lista
        _filteredTodos = filtered.ToList();

        // Aggiorna statistiche
        UpdateStatistics();

        // Ricostruisci calendario
        BuildCalendar();
    }

    /// <summary>
    /// Aggiorna tutte le statistiche
    /// </summary>
    private void UpdateStatistics()
    {
        Totale = _filteredTodos.Count;
        Scaduti = _filteredTodos.Count(t => t.IsScaduto);

        // Statistiche per fase
        var daFareList = _filteredTodos.Where(t => t.Stato == StatoTodo.DaFare).ToList();
        DaFare = daFareList.Count;
        DaFareUrgenti = daFareList.Count(t => t.Priorita == PrioritaTodo.Urgente);
        DaFareAlta = daFareList.Count(t => t.Priorita == PrioritaTodo.Alta);
        DaFareMedia = daFareList.Count(t => t.Priorita == PrioritaTodo.Media);
        DaFareBassa = daFareList.Count(t => t.Priorita == PrioritaTodo.Bassa);

        var inCorsoList = _filteredTodos.Where(t => t.Stato == StatoTodo.InCorso).ToList();
        InCorso = inCorsoList.Count;
        InCorsoUrgenti = inCorsoList.Count(t => t.Priorita == PrioritaTodo.Urgente);
        InCorsoAlta = inCorsoList.Count(t => t.Priorita == PrioritaTodo.Alta);
        InCorsoMedia = inCorsoList.Count(t => t.Priorita == PrioritaTodo.Media);
        InCorsoBassa = inCorsoList.Count(t => t.Priorita == PrioritaTodo.Bassa);

        var completatiList = _filteredTodos.Where(t => t.Stato == StatoTodo.Completata).ToList();
        Completati = completatiList.Count;
        CompletatiOggi = completatiList.Count(t => t.DataCompletamento?.Date == DateTime.Now.Date);
        CompletatiSettimana = completatiList.Count(t =>
            t.DataCompletamento.HasValue &&
            t.DataCompletamento.Value >= DateTime.Now.AddDays(-7));
        CompletatiMese = completatiList.Count(t =>
            t.DataCompletamento.HasValue &&
            t.DataCompletamento.Value.Month == DateTime.Now.Month &&
            t.DataCompletamento.Value.Year == DateTime.Now.Year);

        Annullati = _filteredTodos.Count(t => t.Stato == StatoTodo.Annullata);

        // Statistiche per priorità
        Urgenti = _filteredTodos.Count(t => t.Priorita == PrioritaTodo.Urgente);
        Alta = _filteredTodos.Count(t => t.Priorita == PrioritaTodo.Alta);
        Media = _filteredTodos.Count(t => t.Priorita == PrioritaTodo.Media);
        Bassa = _filteredTodos.Count(t => t.Priorita == PrioritaTodo.Bassa);
    }

    /// <summary>
    /// Costruisce il calendario per il mese corrente
    /// </summary>
    private void BuildCalendar()
    {
        CalendarDays.Clear();

        var firstDay = CurrentMonth;
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        // Trova primo giorno della settimana da mostrare (lunedì precedente)
        var startDate = firstDay;
        while (startDate.DayOfWeek != DayOfWeek.Monday)
        {
            startDate = startDate.AddDays(-1);
        }

        // Trova ultimo giorno della settimana da mostrare (domenica successiva)
        var endDate = lastDay;
        while (endDate.DayOfWeek != DayOfWeek.Sunday)
        {
            endDate = endDate.AddDays(1);
        }

        // Crea tutti i giorni
        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            var day = new CalendarDay
            {
                Date = currentDate,
                IsCurrentMonth = currentDate.Month == CurrentMonth.Month,
                IsToday = currentDate.Date == DateTime.Now.Date,
                IsWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || 
                           currentDate.DayOfWeek == DayOfWeek.Sunday
            };

            // Filtra TODO per questo giorno
            // Mostra TODO che hanno DataScadenza in questo giorno OPPURE DataCreazione se DataScadenza non è impostata
            day.Todos = _filteredTodos
                .Where(t => 
                    (t.DataScadenza.HasValue && t.DataScadenza.Value.Date == currentDate.Date) ||
                    (!t.DataScadenza.HasValue && t.DataCreazione.Date == currentDate.Date))
                .OrderByDescending(t => t.Priorita)
                .ThenBy(t => t.Titolo)
                .ToList();

            CalendarDays.Add(day);
            currentDate = currentDate.AddDays(1);
        }
    }

    /// <summary>
    /// Aggiorna data scadenza TODO (per drag&drop)
    /// </summary>
    public bool UpdateTodoDataScadenza(int todoId, DateTime nuovaData)
    {
        try
        {
            var result = _todoRepository.AggiornaDataScadenza(todoId, nuovaData);
            if (result)
            {
                // Notifica altre viste del cambiamento
                CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
                
                // Ricarica dati
                _allTodos = _todoRepository.GetAll().ToList();
                ApplyFilters();
            }
            return result;
        }
        catch
        {
            return false;
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }

    // ===== PROPERTY CHANGED HANDLERS (FILTRI REAL-TIME) =====

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

    partial void OnCurrentMonthChanged(DateTime value)
    {
        UpdateCurrentMonthText();
        BuildCalendar();
    }

    [RelayCommand]
    private void OpenTodo(TodoStudio todo)
    {
        if (todo == null) return;

        try
        {
            var dialog = new Views.TodoDialogView(todo.Id);
            dialog.ShowDialog();

            if (dialog.DialogResult)
            {
                // Notifica altre viste del cambiamento
                CGEasy.Core.Services.TodoEventService.NotifyTodoChanged();
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore apertura TODO:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EsportaExcel()
    {
        try
        {
            // Prepara nome file
            var culture = new CultureInfo("it-IT");
            var meseAnno = CurrentMonth.ToString("yyyy-MM_MMMM", culture);
            var nomeFile = $"TODO_Calendario_{meseAnno}.xlsx";

            // Dialogo salva file
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = nomeFile,
                DefaultExt = ".xlsx",
                Filter = "Excel Files (*.xlsx)|*.xlsx"
            };

            if (dialog.ShowDialog() != true)
                return;

            // Crea workbook
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("TODO Calendario");

            // ===== INTESTAZIONE =====
            worksheet.Cell(1, 1).Value = "📅 TODO STUDIO - CALENDARIO";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, 11).Merge();

            worksheet.Cell(2, 1).Value = $"Mese: {CurrentMonth.ToString("MMMM yyyy", culture).ToUpper()}";
            worksheet.Cell(2, 1).Style.Font.Bold = true;
            worksheet.Cell(2, 1).Style.Font.FontSize = 12;
            worksheet.Range(2, 1, 2, 11).Merge();

            worksheet.Cell(3, 1).Value = $"Esportato il: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Cell(3, 1).Style.Font.FontSize = 10;
            worksheet.Range(3, 1, 3, 11).Merge();

            // ===== STATISTICHE =====
            int row = 5;
            worksheet.Cell(row, 1).Value = "📊 STATISTICHE MESE";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
            worksheet.Range(row, 1, row, 11).Merge();
            row++;

            // Statistiche per stato
            worksheet.Cell(row, 1).Value = "⏳ Da Fare:";
            worksheet.Cell(row, 2).Value = DaFare;
            worksheet.Cell(row, 3).Value = "⚙️ In Corso:";
            worksheet.Cell(row, 4).Value = InCorso;
            worksheet.Cell(row, 5).Value = "✅ Completati:";
            worksheet.Cell(row, 6).Value = Completati;
            worksheet.Cell(row, 7).Value = "❌ Annullati:";
            worksheet.Cell(row, 8).Value = Annullati;
            worksheet.Cell(row, 9).Value = "⚠️ Scaduti:";
            worksheet.Cell(row, 10).Value = Scaduti;
            row++;

            // Statistiche per priorità
            worksheet.Cell(row, 1).Value = "🔴 Urgenti:";
            worksheet.Cell(row, 2).Value = Urgenti;
            worksheet.Cell(row, 3).Value = "🟠 Alta:";
            worksheet.Cell(row, 4).Value = Alta;
            worksheet.Cell(row, 5).Value = "🟡 Media:";
            worksheet.Cell(row, 6).Value = Media;
            worksheet.Cell(row, 7).Value = "🟢 Bassa:";
            worksheet.Cell(row, 8).Value = Bassa;
            row += 2;

            // ===== HEADER TABELLA =====
            worksheet.Cell(row, 1).Value = "Data";
            worksheet.Cell(row, 2).Value = "Orario";
            worksheet.Cell(row, 3).Value = "Titolo";
            worksheet.Cell(row, 4).Value = "Cliente";
            worksheet.Cell(row, 5).Value = "Categoria";
            worksheet.Cell(row, 6).Value = "Tipo Pratica";
            worksheet.Cell(row, 7).Value = "Stato";
            worksheet.Cell(row, 8).Value = "Priorità";
            worksheet.Cell(row, 9).Value = "Professionisti";
            worksheet.Cell(row, 10).Value = "Descrizione";
            worksheet.Cell(row, 11).Value = "Note";

            // Stile header
            var headerRange = worksheet.Range(row, 1, row, 11);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
            headerRange.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            row++;

            // ===== DATI TODO (ordinati per data) =====
            var todosDelMese = _filteredTodos
                .Where(t => t.DataScadenza.HasValue && 
                           t.DataScadenza.Value.Year == CurrentMonth.Year && 
                           t.DataScadenza.Value.Month == CurrentMonth.Month)
                .OrderBy(t => t.DataScadenza)
                .ThenBy(t => t.OrarioInizio)
                .ToList();

            foreach (var todo in todosDelMese)
            {
                worksheet.Cell(row, 1).Value = todo.DataScadenza?.ToString("dd/MM/yyyy") ?? "";
                worksheet.Cell(row, 2).Value = todo.TestoOrario; // Usa TestoOrario già formattato
                worksheet.Cell(row, 3).Value = todo.Titolo;
                worksheet.Cell(row, 4).Value = todo.ClienteNome;
                worksheet.Cell(row, 5).Value = todo.TestoCategoria;
                worksheet.Cell(row, 6).Value = todo.TipoPraticaNome;
                worksheet.Cell(row, 7).Value = todo.IconaStato;
                worksheet.Cell(row, 8).Value = $"{todo.IconaPriorita} {todo.Priorita}";
                worksheet.Cell(row, 9).Value = todo.ProfessionistiAssegnatiText;
                worksheet.Cell(row, 10).Value = todo.Descrizione;
                worksheet.Cell(row, 11).Value = todo.Note;

                // Colore riga in base allo stato
                var rowRange = worksheet.Range(row, 1, row, 11);
                rowRange.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;

                if (todo.Stato == StatoTodo.Completata)
                    rowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
                else if (todo.Stato == StatoTodo.Annullata)
                    rowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                else if (todo.DataScadenza < DateTime.Today)
                    rowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightPink;

                row++;
            }

            // ===== FORMATTAZIONE FINALE =====
            worksheet.Columns().AdjustToContents();
            worksheet.Column(3).Width = 40; // Titolo
            worksheet.Column(10).Width = 50; // Descrizione
            worksheet.Column(11).Width = 40; // Note

            // Salva file
            workbook.SaveAs(dialog.FileName);

            MessageBox.Show(
                $"✅ Esportazione completata!\n\n📊 {todosDelMese.Count} TODO esportati\n📁 File: {dialog.FileName}",
                "Esportazione Excel",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"❌ Errore durante l'esportazione:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}

