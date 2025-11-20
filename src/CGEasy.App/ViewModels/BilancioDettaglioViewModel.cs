using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.Core.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class BilancioDettaglioViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioContabileRepository _repository;
    private readonly int _clienteId;
    private readonly int _mese;
    private readonly int _anno;
    private readonly string? _descrizione; // 🎯 Salva la descrizione per filtrare correttamente

    [ObservableProperty]
    private string titolo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BilancioContabile> righe = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BilancioContabile> righeFiltrate = new();

    // Statistiche
    [ObservableProperty]
    private int totaleRighe;

    [ObservableProperty]
    private decimal totaleImporti;
    
    // Proprietà per gestire la visibilità del pulsante di eliminazione multipla
    public bool HasSelectedRighe => RigheFiltrate.Any(r => r.IsSelected);

    public BilancioDettaglioViewModel(CGEasyDbContext context, int clienteId, int mese, int anno, string? descrizione)
    {
        _context = context;
        _repository = new BilancioContabileRepository(context);
        _clienteId = clienteId;
        _mese = mese;
        _anno = anno;
        _descrizione = descrizione;
        Titolo = descrizione ?? $"Bilancio {mese:00}/{anno}";
        
        System.Diagnostics.Debug.WriteLine($"[CONSTRUCTOR] BilancioDettaglioViewModel creato - Cliente:{clienteId}, Periodo:{mese}/{anno}, Desc:'{descrizione}'");
        
        // ✅ Sottoscrivi agli eventi PropertyChanged PRIMA di caricare i dati
        RigheFiltrate.CollectionChanged += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"[COLLECTION CHANGED] RigheFiltrate.CollectionChanged - Action:{e.Action}, NewItems:{e.NewItems?.Count ?? 0}, OldItems:{e.OldItems?.Count ?? 0}");
            
            if (e.NewItems != null)
            {
                foreach (BilancioContabile riga in e.NewItems)
                {
                    // TODO: BilancioContabile non implementa INotifyPropertyChanged
                    // riga.PropertyChanged += (sender, args) =>
                    // {
                    //     if (args.PropertyName == nameof(BilancioContabile.IsSelected))
                    //     {
                    //         OnPropertyChanged(nameof(HasSelectedRighe));
                    //     }
                    // };
                    
                    // Workaround temporaneo: aggiorna manualmente quando cambia la selezione
                    OnPropertyChanged(nameof(HasSelectedRighe));
                }
            }
            
            // Aggiorna anche quando la collection cambia
            OnPropertyChanged(nameof(HasSelectedRighe));
        };

        LoadData();
    }

    private void LoadData()
    {
        var stackTrace = new System.Diagnostics.StackTrace(true);
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA] ============================================");
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA] LoadData() CHIAMATO DA:");
        System.Diagnostics.Debug.WriteLine(stackTrace.ToString());
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA] ============================================");
        
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA] BilancioDettaglioViewModel - INIZIO LoadData()");
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   Cliente ID: {_clienteId}, Periodo: {_mese}/{_anno}, Descrizione: '{_descrizione}'");
        
        // Pulisci le collection esistenti prima di ricaricare
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   Righe prima di Clear: {Righe.Count}");
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   RigheFiltrate prima di Clear: {RigheFiltrate.Count}");
        
        Righe.Clear();
        RigheFiltrate.Clear();
        
        // 🎯 Usa GetByClienteAndPeriodoAndDescrizione invece di GetByClienteAndPeriodo
        var allRighe = _repository.GetByClienteAndPeriodoAndDescrizione(_clienteId, _mese, _anno, _descrizione);
        
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   Righe caricate dal DB: {allRighe.Count}");
        
        // Verifica duplicati nei dati caricati
        var duplicati = allRighe
            .GroupBy(r => r.CodiceMastrino)
            .Where(g => g.Count() > 1)
            .ToList();
        
        if (duplicati.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   ❌ DUPLICATI NEI DATI CARICATI:");
            foreach (var dup in duplicati)
            {
                System.Diagnostics.Debug.WriteLine($"[LOAD DATA]     Codice {dup.Key}: {dup.Count()} righe");
            }
        }
        
        foreach (var riga in allRighe)
        {
            Righe.Add(riga);
        }
        
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   Righe aggiunte a collezione: {Righe.Count}");
        
        ApplyFilters();
        
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA]   RigheFiltrate dopo ApplyFilters: {RigheFiltrate.Count}");
        System.Diagnostics.Debug.WriteLine($"[LOAD DATA] BilancioDettaglioViewModel - FINE LoadData()");
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS] ==================== INIZIO ====================");
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   Righe.Count: {Righe.Count}");
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   RigheFiltrate.Count PRIMA di Clear: {RigheFiltrate.Count}");
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   SearchText: '{SearchText}'");
        
        var filtered = Righe.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(r =>
                r.CodiceMastrino.ToLower().Contains(search) ||
                r.DescrizioneMastrino.ToLower().Contains(search) ||
                (r.Note != null && r.Note.ToLower().Contains(search)));
        }

        // 🔥 ORDINAMENTO SEMPLIFICATO PER DEBUG - USA IL NUOVO HELPER NUMERICO
        var result = filtered
            .OrderByCodiceMastrinoNumerico(r => r.CodiceMastrino)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   Righe DOPO filtro e ordinamento: {result.Count}");
        
        // Verifica duplicati nel result
        var dupResult = result
            .GroupBy(r => r.CodiceMastrino)
            .Where(g => g.Count() > 1)
            .ToList();
        
        if (dupResult.Any())
        {
            System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   ❌ DUPLICATI NEL RESULT DOPO ORDINAMENTO:");
            foreach (var dup in dupResult)
            {
                System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]     Codice {dup.Key}: {dup.Count()} righe");
                foreach (var item in dup)
                {
                    System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]       ID:{item.Id}, Desc:{item.DescrizioneMastrino}, Importo:{item.Importo}");
                }
            }
        }
        
        // Clear e aggiungi invece di ricreare la collection
        RigheFiltrate.Clear();
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   RigheFiltrate.Count DOPO Clear: {RigheFiltrate.Count}");
        
        int addCount = 0;
        foreach (var riga in result)
        {
            RigheFiltrate.Add(riga);
            addCount++;
            if (addCount <= 5)
            {
                System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]     Add #{addCount}: Codice:{riga.CodiceMastrino}, ID:{riga.Id}");
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   RigheFiltrate.Count DOPO Add: {RigheFiltrate.Count}");
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS]   Totale righe aggiunte: {addCount}");
        
        TotaleRighe = RigheFiltrate.Count;
        TotaleImporti = RigheFiltrate.Sum(r => r.Importo);
        System.Diagnostics.Debug.WriteLine($"[APPLY FILTERS] ==================== FINE ====================");
    }

    [RelayCommand]
    private void DebugInfo()
    {
        var righeDB = _repository.GetByClienteAndPeriodoAndDescrizione(_clienteId, _mese, _anno, _descrizione);
        
        var msg = $"🔍 DEBUG INFO:\n\n" +
                  $"Righe nel DB: {righeDB.Count}\n" +
                  $"Righe in collezione 'Righe': {Righe.Count}\n" +
                  $"Righe in collezione 'RigheFiltrate': {RigheFiltrate.Count}\n\n" +
                  $"--- Codice 01.002.01 ---\n" +
                  $"Nel DB: {righeDB.Count(r => r.CodiceMastrino == "01.002.01")}\n" +
                  $"In Righe: {Righe.Count(r => r.CodiceMastrino == "01.002.01")}\n" +
                  $"In RigheFiltrate: {RigheFiltrate.Count(r => r.CodiceMastrino == "01.002.01")}\n\n" +
                  $"--- Prime 5 righe DB (con ID) ---\n";
        
        foreach (var r in righeDB.Take(5))
        {
            msg += $"ID:{r.Id}, Codice:{r.CodiceMastrino}, Importo:{r.Importo:N2}\n";
        }
        
        msg += $"\n--- Prime 5 righe RigheFiltrate (con ID) ---\n";
        foreach (var r in RigheFiltrate.Take(5))
        {
            msg += $"ID:{r.Id}, Codice:{r.CodiceMastrino}, Importo:{r.Importo:N2}\n";
        }
        
        // Trova duplicati per codice+descrizione+importo
        var duplicati = righeDB
            .GroupBy(r => new { r.CodiceMastrino, r.DescrizioneMastrino, r.Importo })
            .Where(g => g.Count() > 1)
            .ToList();
        
        if (duplicati.Any())
        {
            msg += $"\n--- ❌ DUPLICATI TROVATI: {duplicati.Count} ---\n";
            foreach (var dup in duplicati.Take(3))
            {
                msg += $"Codice:{dup.Key.CodiceMastrino}, Count:{dup.Count()}\n";
                foreach (var item in dup)
                {
                    msg += $"  ID:{item.Id}, Importo:{item.Importo:N2}\n";
                }
            }
            
            msg += $"\n🔧 Vuoi eliminare i duplicati?";
            var result = MessageBox.Show(msg, "Debug Info - Duplicati Trovati", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                // Elimina i duplicati (mantiene il primo, elimina gli altri)
                int deleted = 0;
                foreach (var dup in duplicati)
                {
                    var items = dup.OrderBy(x => x.Id).ToList();
                    // Salta il primo (lo mantiene), elimina gli altri
                    for (int i = 1; i < items.Count; i++)
                    {
                        _repository.Delete(items[i].Id);
                        deleted++;
                    }
                }
                
                MessageBox.Show($"✅ Eliminati {deleted} duplicati!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Ricarica i dati
            }
        }
        else
        {
            MessageBox.Show(msg, "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        if (RigheFiltrate.Count == 0)
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
                FileName = $"Bilancio_{Titolo}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true) return;

            ExcelBilancioService.ExportToExcel(RigheFiltrate.ToList(), saveDialog.FileName);

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
    private void NewRiga()
    {
        try
        {
            var dialog = new Views.BilancioRigaNuovaDialogView(_clienteId, _mese, _anno);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditRiga(BilancioContabile? riga)
    {
        if (riga == null) return;

        try
        {
            var dialog = new Views.BilancioRigaDialogView(riga.Id);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteRiga(BilancioContabile? riga)
    {
        if (riga == null) return;

        var result = MessageBox.Show(
            $"Sei sicuro di eliminare questa riga?\n\n" +
            $"Codice: {riga.CodiceMastrino}\n" +
            $"Descrizione: {riga.DescrizioneMastrino}\n" +
            $"Importo: {riga.ImportoFormatted}",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            if (_repository.Delete(riga.Id))
            {
                LoadData();
                MessageBox.Show("✅ Riga eliminata!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Errore durante l'eliminazione", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void DeleteSelectedRighe()
    {
        var selectedRighe = RigheFiltrate.Where(r => r.IsSelected).ToList();
        
        if (selectedRighe.Count == 0)
        {
            MessageBox.Show("Nessuna riga selezionata", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Sei sicuro di eliminare {selectedRighe.Count} riga/righe selezionate?",
            "Conferma Eliminazione Multipla",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            int deleted = _repository.DeleteMultiple(selectedRighe.Select(r => r.Id));
            LoadData();
            
            MessageBox.Show($"✅ Eliminate {deleted} righe!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

