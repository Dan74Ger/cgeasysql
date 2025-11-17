using System;
using System.Windows;
using CGEasy.Core.Models;
using CGEasy.Core.Services;

namespace CGEasy.App.Views;

public partial class CambioPasswordDialogWindow : Window
{
    private readonly AuthService _authService;
    private readonly AuditLogService _auditLogService;
    private readonly Utente _targetUser;
    private readonly bool _isAdminChangingOtherUser;
    
    public bool Success { get; private set; }

    // Costruttore per cambio password
    public CambioPasswordDialogWindow(AuthService authService, AuditLogService auditLogService, Utente targetUser, bool isAdmin)
    {
        InitializeComponent();
        _authService = authService;
        _auditLogService = auditLogService;
        _targetUser = targetUser;
        
        // ADMIN NON richiede MAI vecchia password (neanche per se stesso)
        // UTENTE NORMALE richiede SEMPRE vecchia password
        _isAdminChangingOtherUser = isAdmin;

        UsernameTextBox.Text = targetUser.Username;

        if (isAdmin)
        {
            // Admin: NON richiede vecchia password
            OldPasswordPanel.Visibility = Visibility.Collapsed;
            
            if (SessionManager.CurrentUser?.Id == targetUser.Id)
            {
                // Admin cambia la propria password
                SubtitleTextBlock.Text = "Imposta la tua nuova password (Admin - vecchia password non richiesta)";
            }
            else
            {
                // Admin cambia password di altro utente
                SubtitleTextBlock.Text = $"Imposta nuova password per {targetUser.Username} (Admin)";
            }
        }
        else
        {
            // Utente normale: RICHIEDE vecchia password
            OldPasswordPanel.Visibility = Visibility.Visible;
            SubtitleTextBlock.Text = "Modifica la tua password (richiesta vecchia password)";
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validazioni base
            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                MessageBox.Show("Nuova password obbligatoria.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Password troppo corta (minimo 6 caratteri).", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Le password non corrispondono.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return;
            }

            // Se NON è admin che cambia altro utente, verifica vecchia password
            if (!_isAdminChangingOtherUser)
            {
                if (string.IsNullOrWhiteSpace(OldPasswordBox.Password))
                {
                    MessageBox.Show("Vecchia password obbligatoria.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                    OldPasswordBox.Focus();
                    return;
                }

                // Verifica vecchia password
                var loginResult = _authService.Login(_targetUser.Username, OldPasswordBox.Password);
                if (loginResult == null)
                {
                    MessageBox.Show("Vecchia password non corretta.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    OldPasswordBox.Focus();
                    return;
                }
            }

            // Cambia password
            var success = _authService.ChangePassword(_targetUser.Id, NewPasswordBox.Password);

            if (success)
            {
                // Audit log
                if (_isAdminChangingOtherUser)
                {
                    // Admin che cambia password
                    if (SessionManager.CurrentUser?.Id == _targetUser.Id)
                    {
                        _auditLogService.LogFromSession(
                            AuditAction.Update, 
                            AuditEntity.Utente, 
                            _targetUser.Id, 
                            "Admin ha cambiato la propria password (senza vecchia password)"
                        );
                    }
                    else
                    {
                        _auditLogService.LogFromSession(
                            AuditAction.Update, 
                            AuditEntity.Utente, 
                            _targetUser.Id, 
                            $"Admin ha cambiato password per: {_targetUser.Username}"
                        );
                    }
                }
                else
                {
                    _auditLogService.LogFromSession(
                        AuditAction.Update, 
                        AuditEntity.Utente, 
                        _targetUser.Id, 
                        "Cambio password utente (con vecchia password)"
                    );
                }

                MessageBox.Show("✅ Password modificata e salvata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                Success = true;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Errore durante il cambio password.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Success = false;
        DialogResult = false;
        Close();
    }
}

