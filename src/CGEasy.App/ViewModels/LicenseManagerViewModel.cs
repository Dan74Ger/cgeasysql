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

/// <summary>
/// ViewModel per gestione licenze clienti
/// SOLO per amministratori/sviluppatori
/// </summary>
public partial class LicenseManagerViewModel : ObservableObject
{
    private readonly LiteDbContext _context;
    private readonly LicenseRepository _repository;

    [ObservableProperty]
    private ObservableCollection<LicenseClient> _clients = new();

    [ObservableProperty]
    private LicenseClient? _selectedClient;

    [ObservableProperty]
    private ObservableCollection<LicenseKey> _selectedClientKeys = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Statistiche
    [ObservableProperty]
    private int _totalClients = 0;

    [ObservableProperty]
    private int _totalKeys = 0;

    [ObservableProperty]
    private int _activeKeys = 0;

    /// <summary>
    /// Costruttore con context passato come parametro (usa Singleton dell'app)
    /// </summary>
    public LicenseManagerViewModel(LiteDbContext context)
    {
        _context = context;
        _repository = new LicenseRepository(_context);
        LoadData();
    }

    /// <summary>
    /// Carica dati
    /// </summary>
    [RelayCommand]
    private void LoadData()
    {
        try
        {
            var allClients = _repository.GetAllClients().ToList();

            // Applica filtro ricerca
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                allClients = allClients.Where(c =>
                    c.NomeCliente.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email != null && c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (c.PartitaIva != null && c.PartitaIva.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            Clients = new ObservableCollection<LicenseClient>(allClients);

            // Aggiorna statistiche
            var stats = _repository.GetStatistics();
            TotalClients = stats.TotalClients;
            TotalKeys = stats.TotalKeys;
            ActiveKeys = stats.ActiveKeys;

            // Ricarica chiavi cliente selezionato
            if (SelectedClient != null)
            {
                LoadClientKeys();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Quando cambia cliente selezionato, carica le sue chiavi
    /// </summary>
    partial void OnSelectedClientChanged(LicenseClient? value)
    {
        LoadClientKeys();
    }

    /// <summary>
    /// Carica chiavi del cliente selezionato
    /// </summary>
    private void LoadClientKeys()
    {
        if (SelectedClient == null)
        {
            SelectedClientKeys.Clear();
            return;
        }

        var keys = _repository.GetKeysByClient(SelectedClient.Id).ToList();
        SelectedClientKeys = new ObservableCollection<LicenseKey>(keys);
    }

    /// <summary>
    /// Nuovo cliente
    /// </summary>
    [RelayCommand]
    private void NewClient()
    {
        var dialog = new Views.LicenseClientDialogView();
        if (dialog.ShowDialog() == true && dialog.Cliente != null)
        {
            try
            {
                _repository.InsertClient(dialog.Cliente);
                LoadData();
                MessageBox.Show($"Cliente '{dialog.Cliente.NomeCliente}' creato con successo!",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore creazione cliente:\n{ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Modifica cliente
    /// </summary>
    [RelayCommand]
    private void EditClient()
    {
        if (SelectedClient == null)
        {
            MessageBox.Show("Seleziona un cliente da modificare", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Views.LicenseClientDialogView(SelectedClient.Id);
        if (dialog.ShowDialog() == true && dialog.Cliente != null)
        {
            try
            {
                _repository.UpdateClient(dialog.Cliente);
                LoadData();
                MessageBox.Show("Cliente aggiornato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore aggiornamento cliente:\n{ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Elimina cliente e tutte le sue chiavi
    /// </summary>
    [RelayCommand]
    private void DeleteClient()
    {
        if (SelectedClient == null) return;

        var keyCount = _repository.GetKeysByClient(SelectedClient.Id).Count();

        var result = MessageBox.Show(
            $"Eliminare il cliente '{SelectedClient.NomeCliente}'?\n\n" +
            $"⚠️ ATTENZIONE: Verranno eliminate anche tutte le {keyCount} chiavi associate!\n\n" +
            "Questa operazione è irreversibile!",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _repository.DeleteClient(SelectedClient.Id);
                LoadData();
                MessageBox.Show("Cliente e chiavi eliminate con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore eliminazione:\n{ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Genera nuova chiave per un modulo
    /// </summary>
    [RelayCommand]
    private void GenerateKey(string moduleName)
    {
        if (SelectedClient == null)
        {
            MessageBox.Show("Seleziona prima un cliente!", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var currentUserId = SessionManager.CurrentUser?.Id ?? 0;
#pragma warning disable CS0618 // Il tipo o il membro è obsoleto
            var newKey = LicenseService.GenerateLicenseKey(moduleName);
#pragma warning restore CS0618

            // 🔥 SALVA LA CHIAVE NEL DATABASE
            var licenseKey = new LicenseKey
            {
                LicenseClientId = SelectedClient.Id,
                ModuleName = moduleName,
                FullKey = newKey,
                DataGenerazione = DateTime.Now,
                DataScadenza = DateTime.Now.AddYears(1), // 1 anno di default
                IsActive = true,
                GeneratedByUserId = currentUserId,
                Note = $"Chiave generata da Gestione Licenze per {SelectedClient.NomeCliente}"
            };
            
            _repository.InsertKey(licenseKey);

            LoadClientKeys();
            LoadData(); // Aggiorna statistiche

            // Copia automaticamente in clipboard
            Clipboard.SetText(newKey);

            MessageBox.Show(
                $"✅ Chiave generata per '{moduleName}':\n\n" +
                $"{newKey}\n\n" +
                "📋 Chiave copiata in clipboard!",
                "Chiave Generata",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore generazione chiave:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Copia chiave in clipboard
    /// </summary>
    [RelayCommand]
    private void CopyKey(LicenseKey key)
    {
        if (key == null) return;

        try
        {
            Clipboard.SetText(key.FullKey);
            MessageBox.Show("Chiave copiata in clipboard!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore copia chiave:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Revoca chiave (soft delete)
    /// </summary>
    [RelayCommand]
    private void RevokeKey(LicenseKey key)
    {
        if (key == null) return;

        var result = MessageBox.Show(
            $"Revocare la chiave per '{key.ModuleName}'?\n\n" +
            $"{key.FullKey}\n\n" +
            "La chiave non sarà più valida.",
            "Conferma Revoca",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _repository.RevokeKey(key.Id);
                LoadClientKeys();
                LoadData();
                MessageBox.Show("Chiave revocata con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore revoca chiave:\n{ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Modifica durata licenza
    /// </summary>
    [RelayCommand]
    private void EditDurata(LicenseKey key)
    {
        if (key == null) return;

        try
        {
            var clienteName = SelectedClient?.NomeCliente ?? "—";
            var dialog = new Views.LicenseKeyEditDialogView(key, clienteName)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                // Ricarica dati dopo modifica
                LicenseService.ReloadLicenses();
                LoadClientKeys();
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore modifica durata:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Elimina definitivamente una chiave
    /// </summary>
    [RelayCommand]
    private void DeleteKey(LicenseKey key)
    {
        if (key == null) return;

        var result = MessageBox.Show(
            $"Eliminare DEFINITIVAMENTE la chiave?\n\n" +
            $"{key.FullKey}\n\n" +
            "⚠️ Questa operazione è irreversibile!",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _repository.DeleteKey(key.Id);
                LoadClientKeys();
                LoadData();
                MessageBox.Show("Chiave eliminata definitivamente!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore eliminazione chiave:\n{ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Ricerca
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        LoadData();
    }
}


