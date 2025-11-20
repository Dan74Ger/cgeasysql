using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class BilancioTemplateViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioTemplateRepository _repository;

    [ObservableProperty]
    private ObservableCollection<BilancioGruppo> gruppi = new();

    [ObservableProperty]
    private BilancioGruppo? selectedGruppo;

    public BilancioTemplateViewModel(CGEasyDbContext context)
    {
        _context = context;
        _repository = new BilancioTemplateRepository(context);

        // Carica dati in modo asincrono per non bloccare l'UI
        _ = LoadDataAsync();
    }

    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        await System.Threading.Tasks.Task.Run(() => LoadData());
    }

    private void LoadData()
    {
        var allGruppi = _repository.GetGruppi();
        Gruppi = new ObservableCollection<BilancioGruppo>(allGruppi);
    }

    [RelayCommand]
    private void ImportExcel()
    {
        try
        {
            var dialog = new Views.ImportExcelTemplateWizardView();
            if (dialog.ShowDialog() == true)
            {
                LoadData();
                MessageBox.Show($"✅ Import completato!\n\nImportate {dialog.BilanciImportati?.Count ?? 0} righe",
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante import:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenDettaglio(BilancioGruppo? gruppo)
    {
        if (gruppo == null) return;

        try
        {
            // Crea finestra con dettaglio template
            var dettaglioView = new Views.BilancioTemplateDettaglioView();
            var dettaglioVm = new BilancioTemplateDettaglioViewModel(
                _context,
                _repository,
                new Core.Services.AuditLogService(_context)
            );

            // Carica i dati del template
            dettaglioVm.CaricaDati(
                gruppo.ClienteId,
                gruppo.Mese,
                gruppo.Anno,
                gruppo.Descrizione ?? $"{gruppo.ClienteNome}_{gruppo.PeriodoDisplay}"
            );

            dettaglioView.DataContext = dettaglioVm;

            // Gestisci chiusura per ricaricare dati
            dettaglioVm.RichiestaChiusura += (s, e) =>
            {
                LoadData(); // Ricarica dati quando si chiude il dettaglio
            };

            // Crea finestra
            var window = new Window
            {
                Title = $"📊 Template: {gruppo.ClienteNome} - {gruppo.PeriodoDisplay}",
                Content = dettaglioView,
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Closed += (s, e) => LoadData(); // Ricarica anche alla chiusura finestra
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dettaglio:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditGruppo(BilancioGruppo? gruppo)
    {
        if (gruppo == null) return;

        try
        {
            var dialog = new Views.BilancioTemplateGruppoEditDialogView(
                gruppo.ClienteId,
                gruppo.Mese,
                gruppo.Anno,
                gruppo.ClienteNome,
                gruppo.Descrizione
            );

            if (dialog.ShowDialog() == true)
            {
                LoadData();
                MessageBox.Show("✅ Template aggiornato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante modifica:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteGruppo(BilancioGruppo? gruppo)
    {
        if (gruppo == null) return;

        var descDisplay = string.IsNullOrWhiteSpace(gruppo.Descrizione) 
            ? "(nessuna descrizione)" 
            : $"'{gruppo.Descrizione}'";
            
        var result = MessageBox.Show(
            $"Sei sicuro di eliminare il template:\n\n" +
            $"Cliente: {gruppo.ClienteNome}\n" +
            $"Periodo: {gruppo.PeriodoDisplay}\n" +
            $"Descrizione: {descDisplay}\n" +
            $"Righe: {gruppo.NumeroRighe}",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            // ⭐ IMPORTANTE: Elimina SOLO il template con quella DESCRIZIONE specifica
            int deleted = _repository.DeleteByClienteAndPeriodoAndDescrizione(
                gruppo.ClienteId, 
                gruppo.Mese, 
                gruppo.Anno, 
                gruppo.Descrizione);
            LoadData();
            MessageBox.Show($"✅ Eliminate {deleted} righe del template {descDisplay}", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante eliminazione:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }
}

