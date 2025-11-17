using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using CGEasy.Core.Data;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per gestione Utenti (solo Admin)
/// </summary>
public partial class UtentiViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly LiteDbContext _dbContext;
    private readonly AuditLogService _auditLogService;

    [ObservableProperty]
    private ObservableCollection<Utente> _utenti;

    [ObservableProperty]
    private Utente? _selectedUtente;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyActive = true;

    [ObservableProperty]
    private int _totalUtenti;

    [ObservableProperty]
    private int _totalAdmin;

    [ObservableProperty]
    private int _totalUserSenior;

    [ObservableProperty]
    private int _totalUser;

    public UtentiViewModel(AuthService authService, LiteDbContext dbContext, AuditLogService auditLogService)
    {
        _authService = authService;
        _dbContext = dbContext;
        _auditLogService = auditLogService;
        _utenti = new ObservableCollection<Utente>();
        
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var utenti = _authService.GetAllUsers(!ShowOnlyActive);

            // PROTEZIONE: Solo l'utente "admin" può vedere e gestire l'account "admin"
            // Gli altri amministratori NON vedono l'utente "admin" nella lista
            var currentUsername = SessionManager.CurrentUser?.Username?.ToLower();
            if (currentUsername != "admin")
            {
                utenti = utenti.Where(u => u.Username.ToLower() != "admin");
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lower = SearchText.ToLower();
                utenti = utenti.Where(u => 
                    u.Username.ToLower().Contains(lower) ||
                    u.Email.ToLower().Contains(lower) ||
                    u.Nome.ToLower().Contains(lower) ||
                    u.Cognome.ToLower().Contains(lower));
            }

            Utenti = new ObservableCollection<Utente>(utenti.OrderBy(u => u.Username));

            // Statistiche
            var counts = _authService.CountUsersByRole();
            TotalAdmin = counts.Admins;
            TotalUserSenior = counts.UserSenior;
            TotalUser = counts.Users;
            TotalUtenti = TotalAdmin + TotalUserSenior + TotalUser;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento utenti: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }

    partial void OnSearchTextChanged(string value)
    {
        LoadData();
    }

    partial void OnShowOnlyActiveChanged(bool value)
    {
        LoadData();
    }

    [RelayCommand]
    private void NewUtente()
    {
        try
        {
            var dialog = new Views.UtenteDialogWindow(_authService, _auditLogService);
            
            if (dialog.ShowDialog() == true && dialog.Success)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void EditUtente()
    {
        if (SelectedUtente == null)
        {
            MessageBox.Show("Seleziona un utente da modificare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // PROTEZIONE: Solo "admin" può modificare l'account "admin"
        var currentUsername = SessionManager.CurrentUser?.Username?.ToLower();
        if (SelectedUtente.Username.ToLower() == "admin" && currentUsername != "admin")
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Solo l'account 'admin' principale può modificare se stesso.\n\n" +
                "Questa è una protezione di sicurezza per l'account amministratore principale.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.UtenteDialogWindow(_authService, _auditLogService, SelectedUtente);
            
            if (dialog.ShowDialog() == true && dialog.Success)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteUtente()
    {
        if (SelectedUtente == null)
        {
            MessageBox.Show("Seleziona un utente da disattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // PROTEZIONE: Non permettere di disattivare l'account "admin"
        if (SelectedUtente.Username.ToLower() == "admin")
        {
            MessageBox.Show(
                "OPERAZIONE NON CONSENTITA\n\n" +
                "L'account 'admin' principale non può essere disattivato.\n\n" +
                "Questa è una protezione di sicurezza per garantire sempre l'accesso al sistema.",
                "Protezione Account",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Non permettere di disattivare se stesso
        if (SelectedUtente.Id == SessionManager.CurrentUser?.Id)
        {
            MessageBox.Show("Non puoi disattivare il tuo stesso account!", 
                          "Operazione Non Consentita", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler disattivare l'utente '{SelectedUtente.Username}'?\n\n" +
            "L'utente non potrà più accedere al sistema.",
            "Conferma Disattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = _authService.SetUserActive(SelectedUtente.Id, false);
                
                if (success)
                {
                    // Audit log
                    _auditLogService.LogFromSession(AuditAction.Update, AuditEntity.Utente, 
                        SelectedUtente.Id, $"Disattivato utente {SelectedUtente.Username}");

                    MessageBox.Show("Utente disattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ActivateUtente()
    {
        if (SelectedUtente == null || SelectedUtente.Attivo)
        {
            MessageBox.Show("Seleziona un utente disattivato da riattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler riattivare l'utente '{SelectedUtente.Username}'?",
            "Conferma Riattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = _authService.SetUserActive(SelectedUtente.Id, true);
                
                if (success)
                {
                    // Audit log
                    _auditLogService.LogFromSession(AuditAction.Update, AuditEntity.Utente, 
                        SelectedUtente.Id, $"Riattivato utente {SelectedUtente.Username}");

                    MessageBox.Show("Utente riattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ChangePassword()
    {
        if (SelectedUtente == null)
        {
            MessageBox.Show("Seleziona un utente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Admin cambia password di altro utente
            var dialog = new Views.CambioPasswordDialogWindow(_authService, _auditLogService, SelectedUtente, true);
            dialog.ShowDialog();
            
            // Non serve ricaricare la lista (la password non è visibile)
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ChangeMyPassword()
    {
        if (SessionManager.CurrentUser == null)
        {
            MessageBox.Show("Sessione non valida.", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            // Admin può cambiare la propria password senza inserire la vecchia
            // Utente normale deve inserire la vecchia password
            bool isAdmin = SessionManager.IsAdmin;
            
            var dialog = new Views.CambioPasswordDialogWindow(_authService, _auditLogService, SessionManager.CurrentUser, isAdmin);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedUtente == null)
        {
            MessageBox.Show("Seleziona un utente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var details = $"Username: {SelectedUtente.Username}\n" +
                     $"Nome Completo: {SelectedUtente.NomeCompleto}\n" +
                     $"Email: {SelectedUtente.Email}\n" +
                     $"Ruolo: {SelectedUtente.RuoloDescrizione}\n" +
                     $"Stato: {(SelectedUtente.Attivo ? "Attivo" : "Disattivato")}\n" +
                     $"Data Creazione: {SelectedUtente.DataCreazione:dd/MM/yyyy HH:mm}\n";

        if (SelectedUtente.UltimoAccesso.HasValue)
            details += $"Ultimo Accesso: {SelectedUtente.UltimoAccesso:dd/MM/yyyy HH:mm}";
        else
            details += "Ultimo Accesso: Mai";

        MessageBox.Show(details, "Dettagli Utente", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ViewPermissions()
    {
        if (SelectedUtente == null)
        {
            MessageBox.Show("Seleziona un utente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Apri dialog gestione permessi granulari
            var dialog = new Views.PermessiDialogWindow(_authService, _auditLogService, SelectedUtente);
            dialog.ShowDialog();
            
            // Non serve ricaricare la lista (i permessi non sono visibili nella grid)
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

