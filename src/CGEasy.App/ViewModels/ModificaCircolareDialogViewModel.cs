using System;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;

namespace CGEasy.App.ViewModels
{
    /// <summary>
    /// ViewModel per dialog modifica circolare
    /// </summary>
    public partial class ModificaCircolareDialogViewModel : ObservableObject
    {
        private readonly CircolariService _service;
        private readonly CircolariRepository _repository;
        private readonly ArgomentiRepository _argomentiRepo;
        private readonly AuditLogService _auditService;
        private readonly int _circolareId;

        [ObservableProperty]
        private ObservableCollection<Argomento> _argomentiDisponibili = new();

        [ObservableProperty]
        private Argomento? _argomentoSelezionato;

        [ObservableProperty]
        private string _descrizione = string.Empty;

        [ObservableProperty]
        private int _anno;

        [ObservableProperty]
        private string _nomeFile = string.Empty;

        public Action<bool>? OnDialogClosed { get; set; }

        public ModificaCircolareDialogViewModel(LiteDbContext context, int circolareId)
        {
            _circolareId = circolareId;
            _service = new CircolariService(context);
            _repository = new CircolariRepository(context);
            _argomentiRepo = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Carica argomenti disponibili
                var argomenti = _argomentiRepo.GetAll();
                ArgomentiDisponibili = new ObservableCollection<Argomento>(argomenti);

                // Carica circolare corrente
                var circolare = _repository.GetById(_circolareId);
                if (circolare != null)
                {
                    Descrizione = circolare.Descrizione;
                    Anno = circolare.Anno;
                    NomeFile = circolare.NomeFile;

                    // Trova e seleziona argomento
                    ArgomentoSelezionato = argomenti.FirstOrDefault(a => a.Id == circolare.ArgomentoId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore caricamento dati:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Salva()
        {
            // Validazione
            if (ArgomentoSelezionato == null)
            {
                MessageBox.Show("Seleziona un argomento", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Descrizione))
            {
                MessageBox.Show("Inserisci una descrizione", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Anno < 1900 || Anno > 2100)
            {
                MessageBox.Show("Inserisci un anno valido", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_service.ModificaCircolare(_circolareId, ArgomentoSelezionato.Id, Descrizione.Trim(), Anno))
                {
                    _auditService.Log(
                        SessionManager.CurrentUser?.Id ?? 0,
                        SessionManager.CurrentUsername,
                        "CIRCOLARE_UPDATE",
                        "Circolari",
                        _circolareId,
                        $"Modificata circolare: {Descrizione}"
                    );

                    MessageBox.Show("Circolare modificata con successo!", 
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                    OnDialogClosed?.Invoke(true);
                }
                else
                {
                    MessageBox.Show("Impossibile modificare la circolare", 
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore salvataggio:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Annulla()
        {
            OnDialogClosed?.Invoke(false);
        }
    }
}

