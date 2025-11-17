using System;
using System.Windows;
using CGEasy.Core.Models;
using CGEasy.Core.Services;

namespace CGEasy.App.Views;

public partial class UtenteDialogWindow : Window
{
    private readonly AuthService _authService;
    private readonly AuditLogService _auditLogService;
    private readonly Utente? _utenteToEdit;
    
    public bool IsNewUser { get; private set; }
    public bool Success { get; private set; }

    // Costruttore per NUOVO UTENTE
    public UtenteDialogWindow(AuthService authService, AuditLogService auditLogService)
    {
        InitializeComponent();
        _authService = authService;
        _auditLogService = auditLogService;
        _utenteToEdit = null;
        IsNewUser = true;

        DialogTitle.Text = "➕ Nuovo Utente";
        PasswordPanel.Visibility = Visibility.Visible;
        UsernameTextBox.IsEnabled = true;
    }

    // Costruttore per MODIFICA UTENTE
    public UtenteDialogWindow(AuthService authService, AuditLogService auditLogService, Utente utente)
    {
        InitializeComponent();
        _authService = authService;
        _auditLogService = auditLogService;
        _utenteToEdit = utente;
        IsNewUser = false;

        DialogTitle.Text = "✏️ Modifica Utente";
        PasswordPanel.Visibility = Visibility.Collapsed;
        UsernameTextBox.IsEnabled = false;

        // Popola i campi
        UsernameTextBox.Text = utente.Username;
        EmailTextBox.Text = utente.Email;
        NomeTextBox.Text = utente.Nome;
        CognomeTextBox.Text = utente.Cognome;
        AttivoCheckBox.IsChecked = utente.Attivo;

        // Seleziona ruolo
        foreach (System.Windows.Controls.ComboBoxItem item in RuoloComboBox.Items)
        {
            if (item.Tag.ToString() == utente.Ruolo.ToString())
            {
                RuoloComboBox.SelectedItem = item;
                break;
            }
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validazioni
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Username obbligatorio.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Email obbligatoria.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(NomeTextBox.Text))
            {
                MessageBox.Show("Nome obbligatorio.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                NomeTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(CognomeTextBox.Text))
            {
                MessageBox.Show("Cognome obbligatorio.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                CognomeTextBox.Focus();
                return;
            }

            if (RuoloComboBox.SelectedItem == null)
            {
                MessageBox.Show("Seleziona un ruolo.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                RuoloComboBox.Focus();
                return;
            }

            // Password solo per nuovo utente
            if (IsNewUser && string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Password obbligatoria.", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            if (IsNewUser && PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Password troppo corta (minimo 6 caratteri).", "Validazione", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            // Ruolo selezionato
            var selectedItem = (System.Windows.Controls.ComboBoxItem)RuoloComboBox.SelectedItem;
            var ruoloStr = selectedItem.Tag.ToString();
            var ruolo = Enum.Parse<RuoloUtente>(ruoloStr!);

            if (IsNewUser)
            {
                // CREA NUOVO
                var userId = _authService.Register(
                    UsernameTextBox.Text.Trim(),
                    PasswordBox.Password,
                    EmailTextBox.Text.Trim(),
                    NomeTextBox.Text.Trim(),
                    CognomeTextBox.Text.Trim(),
                    ruolo
                );

                if (userId > 0)
                {
                    // Se non attivo, disattivalo subito
                    if (AttivoCheckBox.IsChecked == false)
                    {
                        var newUser = _authService.FindUserByUsername(UsernameTextBox.Text.Trim());
                        if (newUser != null)
                        {
                            _authService.SetUserActive(newUser.Id, false);
                        }
                    }

                    _auditLogService.LogFromSession(
                        AuditAction.Create, 
                        AuditEntity.Utente, 
                        0, 
                        $"Creato nuovo utente: {UsernameTextBox.Text} ({ruolo})"
                    );

                    MessageBox.Show("Utente creato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Success = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Username già esistente!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // MODIFICA ESISTENTE
                if (_utenteToEdit == null) return;

                var success = _authService.UpdateUser(
                    _utenteToEdit.Id,
                    EmailTextBox.Text.Trim(),
                    NomeTextBox.Text.Trim(),
                    CognomeTextBox.Text.Trim(),
                    ruolo,
                    AttivoCheckBox.IsChecked ?? true
                );

                if (success)
                {
                    _auditLogService.LogFromSession(
                        AuditAction.Update, 
                        AuditEntity.Utente, 
                        _utenteToEdit.Id, 
                        $"Modificato utente: {_utenteToEdit.Username}"
                    );

                    MessageBox.Show("Utente modificato con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    Success = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante la modifica.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

