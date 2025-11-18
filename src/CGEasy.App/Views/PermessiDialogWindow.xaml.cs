using System;
using System.Windows;
using CGEasy.Core.Models;
using CGEasy.Core.Services;

namespace CGEasy.App.Views;

public partial class PermessiDialogWindow : Window
{
    private readonly AuthService _authService;
    private readonly AuditLogService _auditLogService;
    private readonly Utente _targetUser;
    private UserPermissions? _permissions;
    
    public bool Success { get; private set; }

    public PermessiDialogWindow(AuthService authService, AuditLogService auditLogService, Utente targetUser)
    {
        InitializeComponent();
        _authService = authService;
        _auditLogService = auditLogService;
        _targetUser = targetUser;

        UsernameTextBlock.Text = $"Utente: {targetUser.Username} ({targetUser.RuoloDescrizione})";
        SubtitleTextBlock.Text = $"Configura l'accesso granulare per {targetUser.NomeCompleto}";

        LoadPermissions();
    }

    private void LoadPermissions()
    {
        _permissions = _authService.GetUserPermissions(_targetUser.Id);

        if (_permissions == null)
        {
            // Crea permessi di default in base al ruolo
            _permissions = new UserPermissions
            {
                IdUtente = _targetUser.Id
            };
            ApplyTemplateByRole(_targetUser.Ruolo);
        }
        else
        {
            // Popola checkbox con permessi esistenti
            PopulateCheckboxes();
        }
    }

    private void PopulateCheckboxes()
    {
        if (_permissions == null) return;

        // MODULI
        ModuloTodoCheckBox.IsChecked = _permissions.ModuloTodo;
        ModuloBilanciCheckBox.IsChecked = _permissions.ModuloBilanci;
        ModuloCircolariCheckBox.IsChecked = _permissions.ModuloCircolari;
        ModuloControlloGestioneCheckBox.IsChecked = _permissions.ModuloControlloGestione;

        // CLIENTI
        ClientiCreateCheckBox.IsChecked = _permissions.ClientiCreate;
        ClientiReadCheckBox.IsChecked = _permissions.ClientiRead;
        ClientiUpdateCheckBox.IsChecked = _permissions.ClientiUpdate;
        ClientiDeleteCheckBox.IsChecked = _permissions.ClientiDelete;

        // PROFESSIONISTI
        ProfessionistiCreateCheckBox.IsChecked = _permissions.ProfessionistiCreate;
        ProfessionistiReadCheckBox.IsChecked = _permissions.ProfessionistiRead;
        ProfessionistiUpdateCheckBox.IsChecked = _permissions.ProfessionistiUpdate;
        ProfessionistiDeleteCheckBox.IsChecked = _permissions.ProfessionistiDelete;

        // TIPO PRATICHE
        TipoPraticheCreateCheckBox.IsChecked = _permissions.TipoPraticheCreate;
        TipoPraticheReadCheckBox.IsChecked = _permissions.TipoPraticheRead;
        TipoPraticheUpdateCheckBox.IsChecked = _permissions.TipoPraticheUpdate;
        TipoPraticheDeleteCheckBox.IsChecked = _permissions.TipoPraticheDelete;

        // SISTEMA
        UtentiManageCheckBox.IsChecked = _permissions.UtentiManage;
    }

    private void ApplyTemplateByRole(RuoloUtente ruolo)
    {
        switch (ruolo)
        {
            case RuoloUtente.Administrator:
                ApplyAdminTemplate();
                break;
            case RuoloUtente.UserSenior:
                ApplySeniorTemplate();
                break;
            case RuoloUtente.User:
                ApplyUserTemplate();
                break;
        }
    }

    // TEMPLATE: ADMIN (tutto)
    private void ApplyAdminTemplate()
    {
        if (_permissions == null) return;

        // MODULI: tutto
        _permissions.ModuloTodo = true;
        _permissions.ModuloBilanci = true;
        _permissions.ModuloCircolari = true;
        _permissions.ModuloControlloGestione = true;

        // CLIENTI: CRUD completo
        _permissions.ClientiCreate = true;
        _permissions.ClientiRead = true;
        _permissions.ClientiUpdate = true;
        _permissions.ClientiDelete = true;

        // PROFESSIONISTI: CRUD completo
        _permissions.ProfessionistiCreate = true;
        _permissions.ProfessionistiRead = true;
        _permissions.ProfessionistiUpdate = true;
        _permissions.ProfessionistiDelete = true;

        // TIPO PRATICHE: CRUD completo
        _permissions.TipoPraticheCreate = true;
        _permissions.TipoPraticheRead = true;
        _permissions.TipoPraticheUpdate = true;
        _permissions.TipoPraticheDelete = true;

        // SISTEMA: tutto
        _permissions.UtentiManage = true;

        PopulateCheckboxes();
    }

    // TEMPLATE: SENIOR (tutto tranne gestione utenti)
    private void ApplySeniorTemplate()
    {
        if (_permissions == null) return;

        // MODULI: tutto
        _permissions.ModuloTodo = true;
        _permissions.ModuloBilanci = true;
        _permissions.ModuloCircolari = true;
        _permissions.ModuloControlloGestione = true;

        // CLIENTI: CRUD completo
        _permissions.ClientiCreate = true;
        _permissions.ClientiRead = true;
        _permissions.ClientiUpdate = true;
        _permissions.ClientiDelete = true;

        // PROFESSIONISTI: CRUD completo
        _permissions.ProfessionistiCreate = true;
        _permissions.ProfessionistiRead = true;
        _permissions.ProfessionistiUpdate = true;
        _permissions.ProfessionistiDelete = true;

        // TIPO PRATICHE: CRUD completo
        _permissions.TipoPraticheCreate = true;
        _permissions.TipoPraticheRead = true;
        _permissions.TipoPraticheUpdate = true;
        _permissions.TipoPraticheDelete = true;

        // SISTEMA: NO gestione utenti
        _permissions.UtentiManage = false;

        PopulateCheckboxes();
    }

    // TEMPLATE: USER (solo lettura)
    private void ApplyUserTemplate()
    {
        if (_permissions == null) return;

        // MODULI: solo TODO e Circolari
        _permissions.ModuloTodo = true;
        _permissions.ModuloBilanci = false;
        _permissions.ModuloCircolari = true;
        _permissions.ModuloControlloGestione = false;

        // CLIENTI: solo lettura
        _permissions.ClientiCreate = false;
        _permissions.ClientiRead = true;
        _permissions.ClientiUpdate = false;
        _permissions.ClientiDelete = false;

        // PROFESSIONISTI: solo lettura
        _permissions.ProfessionistiCreate = false;
        _permissions.ProfessionistiRead = true;
        _permissions.ProfessionistiUpdate = false;
        _permissions.ProfessionistiDelete = false;

        // TIPO PRATICHE: solo lettura
        _permissions.TipoPraticheCreate = false;
        _permissions.TipoPraticheRead = true;
        _permissions.TipoPraticheUpdate = false;
        _permissions.TipoPraticheDelete = false;

        // SISTEMA: nessuno
        _permissions.UtentiManage = false;

        PopulateCheckboxes();
    }

    // TEMPLATE: NESSUNO
    private void ApplyNoneTemplate()
    {
        if (_permissions == null) return;

        // MODULI: nessuno
        _permissions.ModuloTodo = false;
        _permissions.ModuloBilanci = false;
        _permissions.ModuloCircolari = false;
        _permissions.ModuloControlloGestione = false;

        // CLIENTI: nessuno
        _permissions.ClientiCreate = false;
        _permissions.ClientiRead = false;
        _permissions.ClientiUpdate = false;
        _permissions.ClientiDelete = false;

        // PROFESSIONISTI: nessuno
        _permissions.ProfessionistiCreate = false;
        _permissions.ProfessionistiRead = false;
        _permissions.ProfessionistiUpdate = false;
        _permissions.ProfessionistiDelete = false;

        // TIPO PRATICHE: nessuno
        _permissions.TipoPraticheCreate = false;
        _permissions.TipoPraticheRead = false;
        _permissions.TipoPraticheUpdate = false;
        _permissions.TipoPraticheDelete = false;

        // SISTEMA: nessuno
        _permissions.UtentiManage = false;

        PopulateCheckboxes();
    }

    // Eventi click template
    private void ApplyAdminTemplate_Click(object sender, RoutedEventArgs e)
    {
        ApplyAdminTemplate();
        MessageBox.Show("Template Admin applicato: TUTTI i permessi attivati.", "Template", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ApplySeniorTemplate_Click(object sender, RoutedEventArgs e)
    {
        ApplySeniorTemplate();
        MessageBox.Show("Template Senior applicato: CRUD completo su tutto (tranne Gestione Utenti).", "Template", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ApplyUserTemplate_Click(object sender, RoutedEventArgs e)
    {
        ApplyUserTemplate();
        MessageBox.Show("Template User applicato: Solo LETTURA su anagrafiche, accesso a TODO e Circolari.", "Template", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ApplyNoneTemplate_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Confermi di voler RIMUOVERE tutti i permessi?", "Attenzione", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            ApplyNoneTemplate();
            MessageBox.Show("Tutti i permessi rimossi.", "Template", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_permissions == null)
            {
                MessageBox.Show("Errore: permessi non inizializzati.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Leggi valori da checkbox
            // MODULI
            _permissions.ModuloTodo = ModuloTodoCheckBox.IsChecked ?? false;
            _permissions.ModuloBilanci = ModuloBilanciCheckBox.IsChecked ?? false;
            _permissions.ModuloCircolari = ModuloCircolariCheckBox.IsChecked ?? false;
            _permissions.ModuloControlloGestione = ModuloControlloGestioneCheckBox.IsChecked ?? false;

            // CLIENTI
            _permissions.ClientiCreate = ClientiCreateCheckBox.IsChecked ?? false;
            _permissions.ClientiRead = ClientiReadCheckBox.IsChecked ?? false;
            _permissions.ClientiUpdate = ClientiUpdateCheckBox.IsChecked ?? false;
            _permissions.ClientiDelete = ClientiDeleteCheckBox.IsChecked ?? false;

            // PROFESSIONISTI
            _permissions.ProfessionistiCreate = ProfessionistiCreateCheckBox.IsChecked ?? false;
            _permissions.ProfessionistiRead = ProfessionistiReadCheckBox.IsChecked ?? false;
            _permissions.ProfessionistiUpdate = ProfessionistiUpdateCheckBox.IsChecked ?? false;
            _permissions.ProfessionistiDelete = ProfessionistiDeleteCheckBox.IsChecked ?? false;

            // TIPO PRATICHE
            _permissions.TipoPraticheCreate = TipoPraticheCreateCheckBox.IsChecked ?? false;
            _permissions.TipoPraticheRead = TipoPraticheReadCheckBox.IsChecked ?? false;
            _permissions.TipoPraticheUpdate = TipoPraticheUpdateCheckBox.IsChecked ?? false;
            _permissions.TipoPraticheDelete = TipoPraticheDeleteCheckBox.IsChecked ?? false;

            // SISTEMA
            _permissions.UtentiManage = UtentiManageCheckBox.IsChecked ?? false;

            // Salva nel database
            var success = _authService.SaveUserPermissions(_permissions);

            if (success)
            {
                // Audit log
                _auditLogService.LogFromSession(
                    AuditAction.Update,
                    AuditEntity.Utente,
                    _targetUser.Id,
                    $"Modificati permessi per: {_targetUser.Username}"
                );

                MessageBox.Show("Permessi salvati con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                Success = true;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Errore durante il salvataggio dei permessi.", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
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






























