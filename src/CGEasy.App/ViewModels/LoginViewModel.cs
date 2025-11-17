using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Services;
using CGEasy.App.Views;

namespace CGEasy.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private void Login(object parameter)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Inserire username";
            return;
        }

        // Il password viene passato come parametro dalla PasswordBox
        var password = parameter as string;
        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Inserire password";
            return;
        }

        IsLoading = true;

        try
        {
            var utente = _authService.Login(Username, password);

            if (utente == null)
            {
                ErrorMessage = "Username o password non validi";
                return;
            }

            if (!utente.Attivo)
            {
                ErrorMessage = "Utente disabilitato. Contattare l'amministratore.";
                return;
            }

            // Login effettuato: imposta sessione
            var permissions = _authService.GetUserPermissions(utente.Id);
            SessionManager.Login(utente, permissions);

            // Mostra MainWindow
            var mainWindow = App.GetService<MainWindow>();
            if (mainWindow != null)
            {
                mainWindow.Show();
            }

            // Chiudi LoginWindow - trova la finestra di tipo LoginWindow
            var loginWindow = Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault();
            if (loginWindow != null)
                {
                loginWindow.Close();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Errore login: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}

