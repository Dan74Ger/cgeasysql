using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class ConfigurazioneIndiciViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly IndicePersonalizzatoRepository _indicePersonalizzatoRepo;
    private readonly Cliente _clienteFisso;
    private readonly StatisticaCESalvata _statisticaCE;
    private readonly StatisticaSPSalvata _statisticaSP;

    [ObservableProperty]
    private ObservableCollection<string> _categorie = new()
    {
        "Tutti",
        "Liquidità",
        "Solidità",
        "Redditività",
        "Efficienza",
        "Personalizzato"
    };

    [ObservableProperty]
    private string _categoriaSelezionata = "Tutti";

    [ObservableProperty]
    private ObservableCollection<IndicePersonalizzato> _indiciPersonalizzati = new();

    [ObservableProperty]
    private IndicePersonalizzato? _indiceSelezionato;

    [ObservableProperty]
    private string _infoCliente = "";

    /// <summary>
    /// Costruttore che riceve cliente e statistiche selezionate
    /// </summary>
    public ConfigurazioneIndiciViewModel(Cliente cliente, StatisticaCESalvata statisticaCE, StatisticaSPSalvata statisticaSP)
    {
        _context = App.GetService<CGEasyDbContext>() ?? new CGEasyDbContext();
        // Singleton context - no special marking needed in EF Core

        _indicePersonalizzatoRepo = new IndicePersonalizzatoRepository(_context);
        _clienteFisso = cliente;
        _statisticaCE = statisticaCE;
        _statisticaSP = statisticaSP;

        InfoCliente = $"📊 {cliente.NomeCliente} | CE: {statisticaCE.NomeStatistica} | SP: {statisticaSP.NomeStatistica}";

        CaricaIndiciPersonalizzati();
    }

    private void CaricaIndiciPersonalizzati()
    {
        try
        {
            var indici = _indicePersonalizzatoRepo.GetByCliente(_clienteFisso.Id);

            IndiciPersonalizzati.Clear();
            foreach (var indice in indici)
            {
                IndiciPersonalizzati.Add(indice);
            }

            System.Diagnostics.Debug.WriteLine($"[CONFIG INDICI] Caricati {IndiciPersonalizzati.Count} indici personalizzati");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento indici:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void NuovoIndice()
    {
        try
        {
            // Apri dialog per creare nuovo indice
            var dialogViewModel = new IndicePersonalizzatoDialogViewModel(
                _clienteFisso,
                _statisticaCE,
                _statisticaSP,
                null // null = nuovo indice
            );

            var dialog = new Views.IndicePersonalizzatoDialogView
            {
                DataContext = dialogViewModel,
                Owner = Application.Current.MainWindow
            };

            var result = dialog.ShowDialog();
            
            System.Diagnostics.Debug.WriteLine($"[CONFIG] Dialog result: {result}");
            
            if (result == true)
            {
                // Ricarica lista
                CaricaIndiciPersonalizzati();
                
                MessageBox.Show(
                    "✅ INDICE CREATO CON SUCCESSO!\n\n" +
                    "L'indice è stato salvato e ora è disponibile.\n" +
                    "Puoi usarlo nella pagina 'Indici di Bilancio' per calcolare i valori.",
                    "Successo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ ERRORE CREAZIONE INDICE\n\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ModificaIndice(IndicePersonalizzato? indice)
    {
        if (indice == null) return;

        try
        {
            // Apri dialog per modificare indice
            var dialogViewModel = new IndicePersonalizzatoDialogViewModel(
                _clienteFisso,
                _statisticaCE,
                _statisticaSP,
                indice // passa l'indice da modificare
            );

            var dialog = new Views.IndicePersonalizzatoDialogView
            {
                DataContext = dialogViewModel,
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                // Ricarica lista
                CaricaIndiciPersonalizzati();
                MessageBox.Show("Indice modificato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore modifica indice:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EliminaIndice(IndicePersonalizzato? indice)
    {
        System.Diagnostics.Debug.WriteLine($"[CONFIG] EliminaIndice chiamato - Indice: {indice?.Id} - {indice?.NomeIndice}");
        
        if (indice == null)
        {
            MessageBox.Show("Nessun indice selezionato", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Sei sicuro di voler eliminare l'indice:\n\n" +
            $"📊 {indice.NomeIndice}\n\n" +
            $"ID: {indice.Id}\n" +
            $"Cliente: {_clienteFisso.NomeCliente}\n\n" +
            "⚠️ Questa operazione non può essere annullata.",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        System.Diagnostics.Debug.WriteLine($"[CONFIG] Conferma eliminazione: {result}");

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CONFIG] Eliminazione indice ID: {indice.Id}");
                
                var deleted = _indicePersonalizzatoRepo.Delete(indice.Id);
                
                System.Diagnostics.Debug.WriteLine($"[CONFIG] Delete risultato: {deleted}");
                
                if (deleted)
                {
                    CaricaIndiciPersonalizzati();
                    MessageBox.Show(
                        $"✅ INDICE ELIMINATO!\n\n" +
                        $"L'indice '{indice.NomeIndice}' è stato rimosso con successo.",
                        "Successo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        $"⚠️ IMPOSSIBILE ELIMINARE\n\n" +
                        $"L'indice non è stato trovato nel database.",
                        "Attenzione",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CONFIG] ERRORE eliminazione: {ex.Message}");
                MessageBox.Show(
                    $"❌ ERRORE ELIMINAZIONE\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }

    [RelayCommand]
    private void AggiornaLista()
    {
        CaricaIndiciPersonalizzati();
    }

    partial void OnCategoriaSelezionataChanged(string value)
    {
        // TODO: Filtra per categoria
        CaricaIndiciPersonalizzati();
    }
}
