using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class ImportBilancioViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioContabileRepository _repository;
    private readonly ClienteRepository _clienteRepository;

    [ObservableProperty]
    private ObservableCollection<BilancioContabile> bilanci = new();

    [ObservableProperty]
    private ObservableCollection<BilancioContabile> bilanciFiltrati = new();

    [ObservableProperty]
    private ObservableCollection<Cliente> clienti = new();

    [ObservableProperty]
    private Cliente? selectedCliente;

    [ObservableProperty]
    private int? selectedMese;

    [ObservableProperty]
    private int? selectedAnno;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private List<BilancioContabile> selectedBilanci = new();

    // Statistiche
    [ObservableProperty]
    private int totaleRighe;

    [ObservableProperty]
    private decimal totaleImporti;

    [ObservableProperty]
    private decimal totalePositivi;

    [ObservableProperty]
    private decimal totaleNegativi;

    // Liste per filtri
    public List<int> AnniDisponibili { get; set; } = new();
    public List<string> MesiDisponibili { get; } = new()
    {
        "Tutti", "Gen", "Feb", "Mar", "Apr", "Mag", "Giu",
        "Lug", "Ago", "Set", "Ott", "Nov", "Dic"
    };

    public ImportBilancioViewModel(CGEasyDbContext context)
    {
        _context = context;
        _repository = new BilancioContabileRepository(context);
        _clienteRepository = new ClienteRepository(context);

        // Carica dati in modo asincrono per non bloccare l'UI
        _ = LoadDataAsync();
    }

    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        await System.Threading.Tasks.Task.Run(() => LoadData());
    }

    private void LoadData()
    {
        // Carica clienti
        var allClienti = _clienteRepository.GetAll().Where(c => c.Attivo).OrderBy(c => c.NomeCliente).ToList();
        Clienti = new ObservableCollection<Cliente>(allClienti);

        // Carica bilanci
        var allBilanci = _repository.GetAll();
        Bilanci = new ObservableCollection<BilancioContabile>(allBilanci);

        // Carica anni disponibili
        AnniDisponibili = _repository.GetDistinctAnni();
        if (!AnniDisponibili.Contains(DateTime.Now.Year))
            AnniDisponibili.Insert(0, DateTime.Now.Year);

        ApplyFilters();
    }

    partial void OnSelectedClienteChanged(Cliente? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedMeseChanged(int? value)
    {
        ApplyFilters();
    }

    partial void OnSelectedAnnoChanged(int? value)
    {
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = Bilanci.AsEnumerable();

        // Filtro Cliente
        if (SelectedCliente != null)
            filtered = filtered.Where(b => b.ClienteId == SelectedCliente.Id);

        // Filtro Mese
        if (SelectedMese.HasValue && SelectedMese.Value > 0)
            filtered = filtered.Where(b => b.Mese == SelectedMese.Value);

        // Filtro Anno
        if (SelectedAnno.HasValue && SelectedAnno.Value > 0)
            filtered = filtered.Where(b => b.Anno == SelectedAnno.Value);

        // Filtro Ricerca
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(b =>
                b.CodiceMastrino.ToLower().Contains(search) ||
                b.DescrizioneMastrino.ToLower().Contains(search) ||
                (b.Note != null && b.Note.ToLower().Contains(search)));
        }

        var result = filtered.OrderByDescending(b => b.Anno)
                            .ThenByDescending(b => b.Mese)
                            .ThenBy(b => b.ClienteNome)
                            .ThenBy(b => b.CodiceMastrino)
                            .ToList();

        BilanciFiltrati = new ObservableCollection<BilancioContabile>(result);
        UpdateStatistiche();
    }

    private void UpdateStatistiche()
    {
        TotaleRighe = BilanciFiltrati.Count;
        TotaleImporti = BilanciFiltrati.Sum(b => b.Importo);
        TotalePositivi = BilanciFiltrati.Where(b => b.Importo > 0).Sum(b => b.Importo);
        TotaleNegativi = BilanciFiltrati.Where(b => b.Importo < 0).Sum(b => b.Importo);
    }

    [RelayCommand]
    private void ResetFilters()
    {
        SelectedCliente = null;
        SelectedMese = null;
        SelectedAnno = null;
        SearchText = string.Empty;
        ApplyFilters();
    }

    [RelayCommand]
    private void ImportExcel()
    {
        try
        {
            var dialog = new Views.ImportExcelWizardView();
            if (dialog.ShowDialog() == true)
            {
                // ✅ Piccolo delay per permettere al database di completare le operazioni
                System.Threading.Thread.Sleep(100);
                
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
    private void NewBilancio()
    {
        try
        {
            var dialog = new Views.BilancioDialogView();
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditBilancio(BilancioContabile? bilancio)
    {
        if (bilancio == null) return;

        try
        {
            var dialog = new Views.BilancioDialogView(bilancio.Id);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedBilanci == null || SelectedBilanci.Count == 0)
        {
            MessageBox.Show("Seleziona almeno una riga da eliminare", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Sei sicuro di eliminare {SelectedBilanci.Count} righe?",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var ids = SelectedBilanci.Select(b => b.Id).ToList();
            int deleted = _repository.DeleteMultiple(ids);

            LoadData();
            MessageBox.Show($"✅ Eliminate {deleted} righe", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante eliminazione:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        if (BilanciFiltrati.Count == 0)
        {
            MessageBox.Show("Nessuna riga da esportare", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"Bilancio_Contabile_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true) return;

            ExcelBilancioService.ExportToExcel(BilanciFiltrati.ToList(), saveDialog.FileName);

            MessageBox.Show($"✅ Export completato!\n\nFile salvato:\n{saveDialog.FileName}",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante export:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }
}

