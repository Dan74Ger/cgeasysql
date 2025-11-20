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

public partial class BilancioContabileViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioContabileRepository _repository;

    [ObservableProperty]
    private ObservableCollection<BilancioGruppo> gruppi = new();

    [ObservableProperty]
    private BilancioGruppo? selectedGruppo;
    
    // Proprietà per gestire la visibilità del pulsante di eliminazione multipla
    public bool HasSelectedGruppi => Gruppi.Any(g => g.IsSelected);

    public BilancioContabileViewModel(CGEasyDbContext context)
    {
        _context = context;
        _repository = new BilancioContabileRepository(context);

        LoadData();
        
        // Sottoscrivi agli eventi PropertyChanged di ogni gruppo per aggiornare HasSelectedGruppi
        Gruppi.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (BilancioGruppo gruppo in e.NewItems)
                {
                    gruppo.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(BilancioGruppo.IsSelected))
                        {
                            OnPropertyChanged(nameof(HasSelectedGruppi));
                        }
                    };
                }
            }
        };
    }

    private void LoadData()
    {
        try
        {
            var allGruppi = _repository.GetGruppi();
            Gruppi = new ObservableCollection<BilancioGruppo>(allGruppi);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Gruppi = new ObservableCollection<BilancioGruppo>();
        }
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
    private void OpenDettaglio(BilancioGruppo? gruppo)
    {
        if (gruppo == null) return;

        try
        {
            var dettaglioWindow = new Views.BilancioDettaglioView(
                gruppo.ClienteId,
                gruppo.Mese,
                gruppo.Anno,
                gruppo.Descrizione ?? $"{gruppo.ClienteNome} - {gruppo.PeriodoDisplay}"
            );
            dettaglioWindow.Show();
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
            var dialog = new Views.BilancioGruppoEditDialogView(
                gruppo.ClienteId,
                gruppo.Mese,
                gruppo.Anno,
                gruppo.ClienteNome,
                gruppo.Descrizione
            );

            if (dialog.ShowDialog() == true)
            {
                LoadData();
                MessageBox.Show("✅ Bilancio aggiornato con successo!", "Successo",
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

        try
        {
            // ✅ Verifica se esistono associazioni mastrini collegate
            var associazioneRepo = new AssociazioneMastrinoRepository(_context);
            var associazioni = associazioneRepo.GetByCliente(gruppo.ClienteId)
                .Where(a => a.Mese == gruppo.Mese && a.Anno == gruppo.Anno)
                .ToList();

            string messaggioConferma;
            if (associazioni.Any())
            {
                messaggioConferma = 
                    $"⚠️ ATTENZIONE!\n\n" +
                    $"Stai per eliminare il bilancio:\n" +
                    $"Cliente: {gruppo.ClienteNome}\n" +
                    $"Periodo: {gruppo.PeriodoDisplay}\n" +
                    $"Righe: {gruppo.NumeroRighe}\n\n" +
                    $"❗ Questo bilancio è collegato a {associazioni.Count} associazione/i mastrini!\n" +
                    $"Eliminando il bilancio verranno eliminate anche le associazioni.\n\n" +
                    $"Sei sicuro di voler procedere?";
            }
            else
            {
                messaggioConferma = 
                    $"Sei sicuro di eliminare il bilancio:\n\n" +
                    $"Cliente: {gruppo.ClienteNome}\n" +
                    $"Periodo: {gruppo.PeriodoDisplay}\n" +
                    $"Righe: {gruppo.NumeroRighe}";
            }

            var result = MessageBox.Show(
                messaggioConferma,
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                associazioni.Any() ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // ✅ Elimina prima le associazioni collegate
            if (associazioni.Any())
            {
                foreach (var associazione in associazioni)
                {
                    // Elimina prima i dettagli
                    associazioneRepo.DeleteDettagliByAssociazione(associazione.Id);
                    // Poi elimina la testata
                    associazioneRepo.Delete(associazione.Id);
                }
            }

            // 🎯 Poi elimina il bilancio usando Cliente+Periodo+DESCRIZIONE
            int deleted = _repository.DeleteByClienteAndPeriodoAndDescrizione(
                gruppo.ClienteId, 
                gruppo.Mese, 
                gruppo.Anno, 
                gruppo.Descrizione);
            LoadData();
            
            string messaggioSuccesso = $"✅ Eliminate {deleted} righe";
            if (associazioni.Any())
            {
                messaggioSuccesso += $"\n✅ Eliminate {associazioni.Count} associazione/i mastrini";
            }
            
            MessageBox.Show(messaggioSuccesso, "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante eliminazione:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }
    
    [RelayCommand]
    private void DeleteSelectedGruppi()
    {
        var selectedGruppi = Gruppi.Where(g => g.IsSelected).ToList();
        
        if (selectedGruppi.Count == 0)
        {
            MessageBox.Show("Nessun bilancio selezionato", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // ✅ Verifica se esistono associazioni mastrini collegate
            var associazioneRepo = new AssociazioneMastrinoRepository(_context);
            int totaleAssociazioni = 0;
            
            foreach (var gruppo in selectedGruppi)
            {
                var associazioni = associazioneRepo.GetByCliente(gruppo.ClienteId)
                    .Where(a => a.Mese == gruppo.Mese && a.Anno == gruppo.Anno)
                    .ToList();
                totaleAssociazioni += associazioni.Count;
            }

            string messaggioConferma;
            if (totaleAssociazioni > 0)
            {
                messaggioConferma = 
                    $"⚠️ ATTENZIONE!\n\n" +
                    $"Stai per eliminare {selectedGruppi.Count} bilancio/i selezionato/i\n\n" +
                    $"❗ Questi bilanci sono collegati a {totaleAssociazioni} associazione/i mastrini!\n" +
                    $"Eliminando i bilanci verranno eliminate anche le associazioni.\n\n" +
                    $"Sei sicuro di voler procedere?";
            }
            else
            {
                messaggioConferma = 
                    $"Sei sicuro di eliminare {selectedGruppi.Count} bilancio/i selezionato/i?";
            }

            var result = MessageBox.Show(
                messaggioConferma,
                "Conferma Eliminazione Multipla",
                MessageBoxButton.YesNo,
                totaleAssociazioni > 0 ? MessageBoxImage.Warning : MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            int totalDeleted = 0;
            int totalAssociazioniDeleted = 0;
            
            foreach (var gruppo in selectedGruppi)
            {
                // ✅ Elimina prima le associazioni collegate
                var associazioni = associazioneRepo.GetByCliente(gruppo.ClienteId)
                    .Where(a => a.Mese == gruppo.Mese && a.Anno == gruppo.Anno)
                    .ToList();
                
                foreach (var associazione in associazioni)
                {
                    // Elimina prima i dettagli
                    associazioneRepo.DeleteDettagliByAssociazione(associazione.Id);
                    // Poi elimina la testata
                    associazioneRepo.Delete(associazione.Id);
                    totalAssociazioniDeleted++;
                }

                // 🎯 Poi elimina il bilancio usando Cliente+Periodo+DESCRIZIONE
                int deleted = _repository.DeleteByClienteAndPeriodoAndDescrizione(
                    gruppo.ClienteId, 
                    gruppo.Mese, 
                    gruppo.Anno, 
                    gruppo.Descrizione);
                totalDeleted += deleted;
            }
            
            LoadData();
            
            string messaggioSuccesso = $"✅ Eliminate {totalDeleted} righe da {selectedGruppi.Count} bilancio/i";
            if (totalAssociazioniDeleted > 0)
            {
                messaggioSuccesso += $"\n✅ Eliminate {totalAssociazioniDeleted} associazione/i mastrini";
            }
            
            MessageBox.Show(messaggioSuccesso, "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante eliminazione:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}



