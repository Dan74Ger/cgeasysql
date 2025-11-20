using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per gestione licenze clienti (EF Core async)
/// </summary>
public partial class LicenseManagerViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly LicenseRepository _repository;

    [ObservableProperty]
    private ObservableCollection<LicenseClient> _clients = new();

    [ObservableProperty]
    private LicenseClient? _selectedClient;

    [ObservableProperty]
    private ObservableCollection<LicenseKey> _selectedClientKeys = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private int _totalClients = 0;

    [ObservableProperty]
    private int _totalKeys = 0;

    [ObservableProperty]
    private int _activeKeys = 0;

    public LicenseManagerViewModel(CGEasyDbContext context)
    {
        _context = context;
        _repository = new LicenseRepository(_context);
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var allClients = await _repository.GetAllClientsAsync();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                allClients = allClients.Where(c =>
                    c.NomeCliente.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email != null && c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (c.PartitaIva != null && c.PartitaIva.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            Clients = new ObservableCollection<LicenseClient>(allClients);

            var stats = await _repository.GetStatisticsAsync();
            TotalClients = stats.TotalClients;
            TotalKeys = stats.TotalKeys;
            ActiveKeys = stats.ActiveKeys;

            if (SelectedClient != null)
            {
                await LoadClientKeysAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedClientChanged(LicenseClient? value)
    {
        _ = LoadClientKeysAsync();
    }

    private async Task LoadClientKeysAsync()
    {
        if (SelectedClient == null)
        {
            SelectedClientKeys.Clear();
            return;
        }

        var keys = await _repository.GetKeysByClientAsync(SelectedClient.Id);
        SelectedClientKeys = new ObservableCollection<LicenseKey>(keys);
    }

    [RelayCommand]
    private async Task NewClientAsync()
    {
        // TODO: Dialog logic
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task EditClientAsync()
    {
        if (SelectedClient == null)
        {
            MessageBox.Show("Seleziona un cliente", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        // TODO: Dialog logic
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteClientAsync()
    {
        if (SelectedClient == null)
        {
            MessageBox.Show("Seleziona un cliente", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"Eliminare il cliente '{SelectedClient.NomeCliente}' e tutte le sue licenze?",
            "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            try
            {
                await _repository.DeleteClientAsync(SelectedClient.Id);
                MessageBox.Show("Cliente eliminato", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task GenerateKeyAsync()
    {
        if (SelectedClient == null)
        {
            MessageBox.Show("Seleziona un cliente", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        // TODO: Dialog logic
        await LoadClientKeysAsync();
    }

    [RelayCommand]
    private async Task RevokeKeyAsync(LicenseKey key)
    {
        if (key == null) return;

        var result = MessageBox.Show("Revocare questa licenza?", "Conferma",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            IsLoading = true;
            try
            {
                key.IsActive = false;
                await _repository.UpdateKeyAsync(key);
                await LoadClientKeysAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
