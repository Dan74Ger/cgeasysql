using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;

namespace CGEasy.App.ViewModels
{
    /// <summary>
    /// ViewModel per gestione CRUD Argomenti circolari
    /// </summary>
    public partial class ArgomentiViewModel : ObservableObject
    {
        private readonly ArgomentiRepository _repository;
        private readonly AuditLogService _auditService;

        [ObservableProperty]
        private ObservableCollection<Argomento> _argomenti = new();

        [ObservableProperty]
        private Argomento? _argomentoSelezionato;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isEditing = false;

        [ObservableProperty]
        private string _nomeArgomento = string.Empty;

        [ObservableProperty]
        private string _descrizioneArgomento = string.Empty;

        private int? _editingId = null;

        public ArgomentiViewModel()
        {
            // Constructor per XAML Designer
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<LiteDbContext>();
            _repository = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            LoadArgomenti();
        }

        public ArgomentiViewModel(LiteDbContext context)
        {
            _repository = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            LoadArgomenti();
        }

        /// <summary>
        /// Carica tutti gli argomenti
        /// </summary>
        private void LoadArgomenti()
        {
            try
            {
                var lista = string.IsNullOrWhiteSpace(SearchText)
                    ? _repository.GetAll()
                    : _repository.Search(SearchText);

                Argomenti = new ObservableCollection<Argomento>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore caricamento argomenti:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Search()
        {
            LoadArgomenti();
        }

        [RelayCommand]
        private void NuovoArgomento()
        {
            IsEditing = true;
            _editingId = null;
            NomeArgomento = string.Empty;
            DescrizioneArgomento = string.Empty;
        }

        [RelayCommand]
        private void ModificaArgomento()
        {
            if (ArgomentoSelezionato == null)
            {
                MessageBox.Show("Seleziona un argomento da modificare", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsEditing = true;
            _editingId = ArgomentoSelezionato.Id;
            NomeArgomento = ArgomentoSelezionato.Nome;
            DescrizioneArgomento = ArgomentoSelezionato.Descrizione ?? string.Empty;
        }

        [RelayCommand]
        private void SalvaArgomento()
        {
            // Validazione
            if (string.IsNullOrWhiteSpace(NomeArgomento))
            {
                MessageBox.Show("Il nome dell'argomento è obbligatorio", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingId.HasValue)
                {
                    // Modifica
                    var argomento = _repository.GetById(_editingId.Value);
                    if (argomento != null)
                    {
                        argomento.Nome = NomeArgomento.Trim();
                        argomento.Descrizione = string.IsNullOrWhiteSpace(DescrizioneArgomento) 
                            ? null 
                            : DescrizioneArgomento.Trim();

                        _repository.Update(argomento);

                        _auditService.Log(
                            SessionManager.CurrentUser?.Id ?? 0,
                            SessionManager.CurrentUsername,
                            "ARG_UPDATE",
                            "Argomenti",
                            argomento.Id,
                            $"Modificato argomento: {argomento.Nome}"
                        );

                        MessageBox.Show("Argomento modificato con successo!", 
                            "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Nuovo
                    var argomento = new Argomento
                    {
                        Nome = NomeArgomento.Trim(),
                        Descrizione = string.IsNullOrWhiteSpace(DescrizioneArgomento) 
                            ? null 
                            : DescrizioneArgomento.Trim(),
                        DataCreazione = DateTime.Now,
                        UtenteId = SessionManager.CurrentUser?.Id ?? 0
                    };

                    _repository.Insert(argomento);

                    _auditService.Log(
                        SessionManager.CurrentUser?.Id ?? 0,
                        SessionManager.CurrentUsername,
                        "ARG_CREATE",
                        "Argomenti",
                        argomento.Id,
                        $"Creato argomento: {argomento.Nome}"
                    );

                    MessageBox.Show("Argomento creato con successo!", 
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                AnnullaModifica();
                LoadArgomenti();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore salvataggio:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void AnnullaModifica()
        {
            IsEditing = false;
            _editingId = null;
            NomeArgomento = string.Empty;
            DescrizioneArgomento = string.Empty;
        }

        [RelayCommand]
        private void EliminaArgomento()
        {
            if (ArgomentoSelezionato == null)
            {
                MessageBox.Show("Seleziona un argomento da eliminare", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica se ha circolari associate
            if (_repository.HasCircolariAssociate(ArgomentoSelezionato.Id))
            {
                var count = _repository.CountCircolari(ArgomentoSelezionato.Id);
                MessageBox.Show($"Impossibile eliminare l'argomento '{ArgomentoSelezionato.Nome}'.\n\n" +
                    $"Esistono {count} circolare/i associate a questo argomento.\n" +
                    "Elimina prima le circolari o riassegnale ad un altro argomento.", 
                    "Eliminazione Non Consentita", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Sei sicuro di voler eliminare l'argomento '{ArgomentoSelezionato.Nome}'?",
                "Conferma Eliminazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var nomeArgomento = ArgomentoSelezionato.Nome;
                    var idArgomento = ArgomentoSelezionato.Id;

                    _repository.Delete(idArgomento);

                    _auditService.Log(
                        SessionManager.CurrentUser?.Id ?? 0,
                        SessionManager.CurrentUsername,
                        "ARG_DELETE",
                        "Argomenti",
                        idArgomento,
                        $"Eliminato argomento: {nomeArgomento}"
                    );

                    MessageBox.Show("Argomento eliminato con successo!", 
                        "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadArgomenti();
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

