using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Services;
using CGEasy.Core.Models;

namespace CGEasy.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _currentUserName = string.Empty;

    [ObservableProperty]
    private string _currentUserRole = string.Empty;

    [ObservableProperty]
    private string _pageTitle = "Dashboard";

    public MainViewModel()
    {
        // Carica info utente corrente
        LoadCurrentUser();
        
        // Mostra Dashboard di default
        NavigateToDashboard();
    }

    private void LoadCurrentUser()
    {
        if (SessionManager.CurrentUser != null)
        {
            CurrentUserName = SessionManager.CurrentUser.NomeCompleto;
            CurrentUserRole = SessionManager.CurrentUser.Ruolo switch
            {
                RuoloUtente.Administrator => "Amministratore",
                RuoloUtente.UserSenior => "Utente Senior",
                RuoloUtente.User => "Utente",
                _ => "Sconosciuto"
            };
        }
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        PageTitle = "Dashboard";
        var dashboardViewModel = App.GetService<DashboardViewModel>();
        CurrentView = App.GetService<Views.DashboardView>();
    }

    [RelayCommand]
    private void NavigateToTodo()
    {
        // VERIFICA PERMESSI UTENTE
        if (!SessionManager.HasModuleAccess("todo"))
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Non hai i permessi per accedere al modulo TODO Studio.\n\n" +
                "Contatta l'amministratore per richiedere l'accesso.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Verifica licenza modulo
        if (!LicenseService.IsTodoStudioActive())
        {
            MessageBox.Show(
                "MODULO NON ATTIVATO\n\n" +
                "Il modulo TODO Studio richiede una licenza.\n\n" +
                "Per attivarlo:\n" +
                "1. Vai in Impostazioni > Sistema\n" +
                "2. Inserisci la chiave di attivazione\n" +
                "3. Riavvia l'applicazione\n\n" +
                "Contatta l'amministratore per ricevere la chiave.",
                "Modulo TODO Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        PageTitle = "TODO Studio - Lista";
        
        // Apri vista lista (che poi puo aprire calendario)
        var listView = new Views.TodoStudioView();
        listView.Show();
    }

    [RelayCommand]
    private void NavigateToBilanci()
    {
        // VERIFICA PERMESSI UTENTE
        if (!SessionManager.HasModuleAccess("bilanci"))
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Non hai i permessi per accedere al modulo Bilanci.\n\n" +
                "Contatta l'amministratore per richiedere l'accesso.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Verifica licenza modulo
        if (!LicenseService.IsBilanciActive())
        {
            MessageBox.Show(
                "MODULO BILANCI NON ACCESSIBILE\n\n" +
                "Il modulo Bilanci richiede una licenza valida.\n\n" +
                "La licenza potrebbe essere:\n" +
                "• Non attivata\n" +
                "• Scaduta\n" +
                "• Revocata\n\n" +
                "Per verificare:\n" +
                "1. Vai in Sistema > Licenze Moduli\n" +
                "2. Controlla la durata e scadenza della licenza BILANCI\n" +
                "3. Se scaduta, modifica la durata premendo in Gestione Licenze\n\n" +
                "Contatta l'amministratore per assistenza.",
                "Modulo Bilanci - Licenza Non Valida",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        PageTitle = "Bilanci";
        var view = new Views.BilanciView();
        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateToAssociazioniMastrini()
    {
        // VERIFICA PERMESSI UTENTE
        if (!SessionManager.HasModuleAccess("bilanci"))
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Non hai i permessi per accedere al modulo Bilanci.\n\n" +
                "Contatta l'amministratore per richiedere l'accesso.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Verifica licenza modulo
        if (!LicenseService.IsBilanciActive())
        {
            MessageBox.Show(
                "MODULO BILANCI NON ACCESSIBILE\n\n" +
                "Il modulo Bilanci richiede una licenza valida.",
                "Modulo Bilanci - Licenza Non Valida",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        
        PageTitle = "Associazioni Mastrini";
        var view = new Views.AssociazioniMastriniView();
        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateToStatisticheBilanci()
    {
        // VERIFICA PERMESSI UTENTE
        if (!SessionManager.HasModuleAccess("bilanci"))
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Non hai i permessi per accedere al modulo Bilanci.\n\n" +
                "Contatta l'amministratore per richiedere l'accesso.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Verifica licenza modulo
        if (!LicenseService.IsBilanciActive())
        {
            MessageBox.Show(
                "MODULO BILANCI NON ACCESSIBILE\n\n" +
                "Il modulo Bilanci richiede una licenza valida.",
                "Modulo Bilanci - Licenza Non Valida",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        
        PageTitle = "Statistiche Bilanci";
        var view = new Views.StatisticheBilanciView();
        CurrentView = view;
    }

    [RelayCommand]
    private void NavigateToCircolari()
    {
        // VERIFICA PERMESSI UTENTE
        if (!SessionManager.HasModuleAccess("circolari"))
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Non hai i permessi per accedere al modulo Circolari.\n\n" +
                "Contatta l'amministratore per richiedere l'accesso.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        PageTitle = "Circolari";
        // TODO: implementare quando sara pronto il modulo
        MessageBox.Show("Modulo Circolari in sviluppo...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void NavigateToControllo()
    {
        // VERIFICA PERMESSI UTENTE
        if (!SessionManager.HasModuleAccess("controllo"))
        {
            MessageBox.Show(
                "ACCESSO NEGATO\n\n" +
                "Non hai i permessi per accedere al modulo Controllo Gestione.\n\n" +
                "Contatta l'amministratore per richiedere l'accesso.",
                "Permessi Insufficienti",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        PageTitle = "Controllo Gestione";
        // TODO: implementare quando sara pronto il modulo
        MessageBox.Show("Modulo Controllo Gestione in sviluppo...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void NavigateToClienti()
    {
        PageTitle = "Anagrafica Clienti";
        CurrentView = App.GetService<Views.ClientiView>();
    }

    [RelayCommand]
    private void NavigateToProfessionisti()
    {
        PageTitle = "Anagrafica Professionisti";
        CurrentView = App.GetService<Views.ProfessionistiView>();
    }

    [RelayCommand]
    private void NavigateToTipoPratiche()
    {
        PageTitle = "Gestione Tipo Pratiche";
        CurrentView = App.GetService<Views.TipoPraticaView>();
    }

    [RelayCommand]
    private void NavigateToUtenti()
    {
        PageTitle = "Gestione Utenti";
        
        // Solo admin puo accedere
        if (!SessionManager.IsAdmin)
        {
            MessageBox.Show("Accesso riservato agli amministratori.", "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        CurrentView = App.GetService<Views.UtentiView>();
    }

    [RelayCommand]
    private void NavigateToSistema()
    {
        PageTitle = "Sistema";
        
        // Solo admin puo accedere
        if (!SessionManager.IsAdmin)
        {
            MessageBox.Show("Accesso riservato agli amministratori.", "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        CurrentView = App.GetService<Views.SistemaView>();
    }

    [RelayCommand]
    private void Logout()
    {
        var result = MessageBox.Show("Confermi di voler uscire?", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            // Effettua logout
            SessionManager.Logout();
            
            // Crea NUOVA istanza LoginWindow (non riusare quella vecchia!)
            var authService = App.GetService<AuthService>();
            var loginViewModel = new LoginViewModel(authService!);
            var loginWindow = new Views.LoginWindow(loginViewModel);
            loginWindow.Show();
            
            // Chiudi MainWindow
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}


