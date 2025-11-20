using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.App.Views;

namespace CGEasy.App.ViewModels
{
    /// <summary>
    /// ViewModel per ricerca e gestione circolari
    /// </summary>
    public partial class RicercaCircolariViewModel : ObservableObject
    {
        private readonly CircolariService _service;
        private readonly CircolariRepository _repository;
        private readonly ArgomentiRepository _argomentiRepo;
        private readonly AuditLogService _auditService;
        private readonly CGEasyDbContext _context;

        [ObservableProperty]
        private ObservableCollection<Circolare> _circolari = new();

        [ObservableProperty]
        private Circolare? _circolareSelezionata;

        [ObservableProperty]
        private ObservableCollection<Argomento> _argomentiDisponibili = new();

        [ObservableProperty]
        private Argomento? _argomentoFiltro;

        [ObservableProperty]
        private ObservableCollection<int> _anniDisponibili = new();

        [ObservableProperty]
        private int? _annoFiltro;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public RicercaCircolariViewModel()
        {
            // Constructor per XAML Designer
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<CGEasyDbContext>();
            _context = context;
            _service = new CircolariService(context);
            _repository = new CircolariRepository(context);
            _argomentiRepo = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            _ = LoadDataAsync();
        }

        public RicercaCircolariViewModel(CGEasyDbContext context)
        {
            _context = context;
            _service = new CircolariService(context);
            _repository = new CircolariRepository(context);
            _argomentiRepo = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var argomenti = await _argomentiRepo.GetAllAsync();
                ArgomentiDisponibili = new ObservableCollection<Argomento>(argomenti);

                var anni = await _repository.GetAnniDistintiAsync();
                AnniDisponibili = new ObservableCollection<int>(anni);

                await LoadCircolariAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshDataAsync()
        {
            await LoadDataAsync();
        }

        public void RefreshData() => RefreshDataAsync().GetAwaiter().GetResult();

        private async Task LoadCircolariAsync()
        {
            try
            {
                var risultati = await _repository.SearchAsync(
                    ArgomentoFiltro?.Id,
                    AnnoFiltro,
                    string.IsNullOrWhiteSpace(SearchText) ? null : SearchText
                );

                Circolari = new ObservableCollection<Circolare>(risultati);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore ricerca circolari:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task CercaAsync()
        {
            await LoadCircolariAsync();
        }

        [RelayCommand]
        private void ResetFiltri()
        {
            ArgomentoFiltro = null;
            AnnoFiltro = null;
            SearchText = string.Empty;
            _ = LoadCircolariAsync(); // Call async without await
        }

        [RelayCommand]
        private void VisualizzaCircolare()
        {
            if (CircolareSelezionata == null)
            {
                MessageBox.Show("Seleziona una circolare da visualizzare", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!_service.ApriCircolare(CircolareSelezionata.Id))
                {
                    MessageBox.Show("Impossibile aprire il file.\nIl file potrebbe essere stato spostato o eliminato.", 
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore apertura file:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ModificaCircolare()
        {
            if (CircolareSelezionata == null)
            {
                MessageBox.Show("Seleziona una circolare da modificare", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dialog = new ModificaCircolareDialogView(CircolareSelezionata.Id);
                dialog.ShowDialog(); // Mostra il dialog!
                
                if (dialog.DialogResult)
                {
                    // Ricarica lista dopo modifica
                    _ = LoadCircolariAsync(); // Call async without await
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore modifica circolare:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void EliminaCircolare()
        {
            if (CircolareSelezionata == null)
            {
                MessageBox.Show("Seleziona una circolare da eliminare", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare la circolare?\n\n" +
                $"Descrizione: {CircolareSelezionata.Descrizione}\n" +
                $"File: {CircolareSelezionata.NomeFile}\n\n" +
                "Questa azione eliminerà sia il record che il file fisico e non può essere annullata.",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var descrizione = CircolareSelezionata.Descrizione;
                    var circolareId = CircolareSelezionata.Id;

                    if (_service.EliminaCircolare(circolareId))
                    {
                        _auditService.Log(
                            SessionManager.CurrentUser?.Id ?? 0,
                            SessionManager.CurrentUsername,
                            "CIRCOLARE_DELETE",
                            "Circolari",
                            circolareId,
                            $"Eliminata circolare: {descrizione}"
                        );

                        MessageBox.Show("Circolare eliminata con successo!", 
                            "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                        _ = LoadCircolariAsync(); // Call async without await
                    }
                    else
                    {
                        MessageBox.Show("Impossibile eliminare la circolare", 
                            "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore eliminazione:\n{ex.Message}", 
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

