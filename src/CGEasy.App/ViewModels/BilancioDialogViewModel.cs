using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class BilancioDialogViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioContabileRepository _repository;
    private readonly ClienteRepository _clienteRepository;
    private readonly int? _bilancioId;

    [ObservableProperty]
    private ObservableCollection<Cliente> clienti = new();

    [ObservableProperty]
    private Cliente? selectedCliente;

    [ObservableProperty]
    private int mese = DateTime.Now.Month;

    [ObservableProperty]
    private int anno = DateTime.Now.Year;

    [ObservableProperty]
    private string codiceMastrino = string.Empty;

    [ObservableProperty]
    private string descrizioneMastrino = string.Empty;

    [ObservableProperty]
    private decimal importo;

    [ObservableProperty]
    private string? note;

    public bool IsEditMode => _bilancioId.HasValue;
    public string Title => IsEditMode ? "✏️ Modifica Riga Bilancio" : "➕ Nuova Riga Bilancio";

    public BilancioDialogViewModel(CGEasyDbContext context, int? bilancioId = null)
    {
        _context = context;
        _repository = new BilancioContabileRepository(context);
        _clienteRepository = new ClienteRepository(context);
        _bilancioId = bilancioId;

        LoadClienti();

        if (_bilancioId.HasValue)
            LoadBilancio(_bilancioId.Value);
    }

    private void LoadClienti()
    {
        var allClienti = _clienteRepository.GetAll()
            .Where(c => c.Attivo)
            .OrderBy(c => c.NomeCliente)
            .ToList();
        
        Clienti = new ObservableCollection<Cliente>(allClienti);
    }

    private void LoadBilancio(int id)
    {
        var bilancio = _repository.GetById(id);
        if (bilancio == null) return;

        SelectedCliente = Clienti.FirstOrDefault(c => c.Id == bilancio.ClienteId);
        Mese = bilancio.Mese;
        Anno = bilancio.Anno;
        CodiceMastrino = bilancio.CodiceMastrino;
        DescrizioneMastrino = bilancio.DescrizioneMastrino;
        Importo = bilancio.Importo;
        Note = bilancio.Note;
    }

    [RelayCommand]
    private void Save(Window window)
    {
        // Validazioni
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleziona un cliente", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Mese < 1 || Mese > 12)
        {
            MessageBox.Show("Mese deve essere tra 1 e 12", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Anno < 2000 || Anno > 2100)
        {
            MessageBox.Show("Anno non valido", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(CodiceMastrino))
        {
            MessageBox.Show("Codice Mastrino obbligatorio", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(DescrizioneMastrino))
        {
            MessageBox.Show("Descrizione obbligatoria", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var currentUser = SessionManager.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("Sessione scaduta", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (IsEditMode)
            {
                // Update
                var existing = _repository.GetById(_bilancioId!.Value);
                if (existing == null)
                {
                    MessageBox.Show("Riga non trovata", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                existing.ClienteId = SelectedCliente.Id;
                existing.ClienteNome = SelectedCliente.NomeCliente;
                existing.Mese = Mese;
                existing.Anno = Anno;
                existing.CodiceMastrino = CodiceMastrino.Trim();
                existing.DescrizioneMastrino = DescrizioneMastrino.Trim();
                existing.Importo = Importo;
                existing.Note = Note?.Trim();

                _repository.Update(existing);
            }
            else
            {
                // Insert
                var bilancio = new BilancioContabile
                {
                    ClienteId = SelectedCliente.Id,
                    ClienteNome = SelectedCliente.NomeCliente,
                    Mese = Mese,
                    Anno = Anno,
                    CodiceMastrino = CodiceMastrino.Trim(),
                    DescrizioneMastrino = DescrizioneMastrino.Trim(),
                    Importo = Importo,
                    Note = Note?.Trim(),
                    DataImport = DateTime.Now,
                    ImportedBy = currentUser.Id,
                    ImportedByName = currentUser.NomeCompleto
                };

                _repository.Insert(bilancio);
            }

            window.DialogResult = true;
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante salvataggio:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        window.DialogResult = false;
        window.Close();
    }
}

