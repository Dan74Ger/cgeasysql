using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
    /// ViewModel per gestione CRUD Argomenti circolari (EF Core async)
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
        private bool _isLoading = false;

        [ObservableProperty]
        private string _nomeArgomento = string.Empty;

        [ObservableProperty]
        private string _descrizioneArgomento = string.Empty;

        private int? _editingId = null;

        public ArgomentiViewModel()
        {
            // Constructor per XAML Designer
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<CGEasyDbContext>();
            _repository = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            _ = LoadArgomentiAsync();
        }

        public ArgomentiViewModel(CGEasyDbContext context)
        {
            _repository = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            _ = LoadArgomentiAsync();
        }

        /// <summary>
        /// Carica tutti gli argomenti
        /// </summary>
        private async Task LoadArgomentiAsync()
        {
            IsLoading = true;
            try
            {
                var lista = string.IsNullOrWhiteSpace(SearchText)
                    ? await _repository.GetAllAsync()
                    : await _repository.SearchAsync(SearchText);

                Argomenti = new ObservableCollection<Argomento>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore caricamento argomenti:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            await LoadArgomentiAsync();
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
        private async Task SalvaArgomentoAsync()
        {
            // Validazione
            if (string.IsNullOrWhiteSpace(NomeArgomento))
            {
                MessageBox.Show("Il nome dell'argomento è obbligatorio", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            try
            {
                if (_editingId.HasValue)
                {
                    // Modifica
                    var argomento = await _repository.GetByIdAsync(_editingId.Value);
                    if (argomento != null)
                    {
                        argomento.Nome = NomeArgomento.Trim();
                        argomento.Descrizione = string.IsNullOrWhiteSpace(DescrizioneArgomento) 
                            ? null 
                            : DescrizioneArgomento.Trim();

                        await _repository.UpdateAsync(argomento);

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

                    await _repository.InsertAsync(argomento);

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
                await LoadArgomentiAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore salvataggio:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
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
        private async Task EliminaArgomentoAsync()
        {
            if (ArgomentoSelezionato == null)
            {
                MessageBox.Show("Seleziona un argomento da eliminare", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica se ha circolari associate
            if (await _repository.HasCircolariAssociateAsync(ArgomentoSelezionato.Id))
            {
                var count = await _repository.CountCircolariAsync(ArgomentoSelezionato.Id);
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
                IsLoading = true;
                try
                {
                    var nomeArgomento = ArgomentoSelezionato.Nome;
                    var idArgomento = ArgomentoSelezionato.Id;

                    await _repository.DeleteAsync(idArgomento);

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

                    await LoadArgomentiAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore eliminazione:\n{ex.Message}", 
                        "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
