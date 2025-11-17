using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using Microsoft.Win32;

namespace CGEasy.App.ViewModels;

public partial class SistemaViewModel : ObservableObject
{
    private readonly LiteDbContext _context;
    
    /// <summary>
    /// Cartella backup nella stessa directory del database configurato
    /// </summary>
    private static string GetBackupFolder()
    {
        var dbPath = LiteDbContext.DefaultDatabasePath;
        var dbDirectory = Path.GetDirectoryName(dbPath);
        
        if (string.IsNullOrEmpty(dbDirectory))
            dbDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        
        return Path.Combine(dbDirectory, "DatabaseBackup");
    }

    [ObservableProperty]
    private string _percorsoDatabase = string.Empty;

    [ObservableProperty]
    private string _dimensioneDatabase = string.Empty;

    [ObservableProperty]
    private string _ultimoBackup = "Nessun backup disponibile";

    [ObservableProperty]
    private int _numeroBackups = 0;

    [ObservableProperty]
    private string _statoCrittografia = "NON CRIPTATO";

    [ObservableProperty]
    private string _statoCrittografiaIcon = "🔓";

    // ===== LICENZE MODULI =====
    [ObservableProperty]
    private string _licenzaTodoStudio = "❌ Non Attivato";
    [ObservableProperty]
    private string _chiaveTodoStudio = string.Empty;
    [ObservableProperty]
    private string _durataTodoStudio = "—";
    [ObservableProperty]
    private string _scadenzaTodoStudio = "—";

    [ObservableProperty]
    private string _licenzaBilanci = "❌ Non Attivato";
    [ObservableProperty]
    private string _chiaveBilanci = string.Empty;
    [ObservableProperty]
    private string _durataBilanci = "—";
    [ObservableProperty]
    private string _scadenzaBilanci = "—";

    [ObservableProperty]
    private string _licenzaCircolari = "❌ Non Attivato";
    [ObservableProperty]
    private string _chiaveCircolari = string.Empty;
    [ObservableProperty]
    private string _durataCircolari = "—";
    [ObservableProperty]
    private string _scadenzaCircolari = "—";

    [ObservableProperty]
    private string _licenzaControlloGestione = "❌ Non Attivato";
    [ObservableProperty]
    private string _chiaveControlloGestione = string.Empty;
    [ObservableProperty]
    private string _durataControlloGestione = "—";
    [ObservableProperty]
    private string _scadenzaControlloGestione = "—";

    [ObservableProperty]
    private int _moduliAttivati = 0;

    // ===== CONFIGURAZIONE DATABASE =====
    [ObservableProperty]
    private string _nuovoPercorsoDatabase = string.Empty;

    [ObservableProperty]
    private string _infoConfigurazione = "Nessuna configurazione personalizzata";

    [ObservableProperty]
    private string _tipoPercorso = "Locale";

    /// <summary>
    /// Verifica se l'utente è lo sviluppatore (solo account admin può generare chiavi)
    /// SECURITY: Solo l'account "admin" principale può generare chiavi di licenza
    /// </summary>
    public bool IsDeveloper => SessionManager.IsAdministrator && 
                               SessionManager.CurrentUser?.Username?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true;

    public SistemaViewModel(LiteDbContext context)
    {
        _context = context;
        LoadDatabaseInfo();
        UpdateEncryptionStatus();
        UpdateLicenseStatus();
        LoadDatabaseConfiguration();
    }

    private void LoadDatabaseInfo()
    {
        PercorsoDatabase = LiteDbContext.DefaultDatabasePath;
        
        if (File.Exists(PercorsoDatabase))
        {
            var fileInfo = new FileInfo(PercorsoDatabase);
            DimensioneDatabase = fileInfo.Length < 1024 * 1024 
                ? $"{fileInfo.Length / 1024:N0} KB" 
                : $"{fileInfo.Length / (1024 * 1024):N2} MB";
        }

        // Cartella backup nella stessa directory del database
        var backupFolder = GetBackupFolder();
        if (Directory.Exists(backupFolder))
        {
            var backups = Directory.GetFiles(backupFolder, "cgeasy_*.db");
            NumeroBackups = backups.Length;
            
            if (backups.Length > 0)
            {
                var ultimoFile = backups.OrderByDescending(f => File.GetLastWriteTime(f)).First();
                var dataUltimo = File.GetLastWriteTime(ultimoFile);
                UltimoBackup = $"{dataUltimo:dd/MM/yyyy HH:mm:ss}";
            }
        }
    }

    [RelayCommand]
    private void BackupDatabase()
    {
        try
        {
            // Percorso database corrente
            var dbPath = LiteDbContext.DefaultDatabasePath;
            var backupFolder = GetBackupFolder();
            
            var result = MessageBox.Show(
                $"Vuoi creare un backup del database?\n\nIl backup verrà salvato in:\n{backupFolder}",
                "Backup Database",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;
            
            if (!File.Exists(dbPath))
            {
                MessageBox.Show("Database non trovato!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Crea cartella backup se non esiste
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            // Nome file backup con timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupFolder, $"cgeasy_{timestamp}.db");

            // Forza scrittura su disco prima del backup
            _context.Checkpoint();

            // Copia il file
            File.Copy(dbPath, backupPath, true);

            MessageBox.Show(
                $"✅ Backup creato con successo!\n\n📁 File: {Path.GetFileName(backupPath)}\n📂 Percorso: {backupFolder}",
                "Backup Completato",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Ricarica info
            LoadDatabaseInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante il backup:\n{ex.Message}",
                "Errore Backup",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RestoreDatabase()
    {
        try
        {
            var result = MessageBox.Show(
                "ATTENZIONE!\n\nIl ripristino sostituirà il database corrente con quello selezionato.\n\nTutti i dati non salvati andranno persi!\n\nVuoi continuare?",
                "Ripristina Database",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Apri dialog per selezionare file backup
            var backupFolder = GetBackupFolder();
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona file di backup",
                Filter = "Database CGEasy (*.db)|*.db|Tutti i file (*.*)|*.*",
                InitialDirectory = backupFolder
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            var backupPath = openFileDialog.FileName;
            var dbPath = LiteDbContext.DefaultDatabasePath;

            // Verifica che il file di backup esista
            if (!File.Exists(backupPath))
            {
                MessageBox.Show("File di backup non trovato!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // VERIFICA COMPATIBILITÀ CRITTOGRAFIA
            bool appHasPassword = DatabaseEncryptionService.IsDatabaseEncrypted();
            bool backupIsEncrypted = IsBackupEncrypted(backupPath);

            if (appHasPassword && !backupIsEncrypted)
            {
                MessageBox.Show(
                    "⚠️ RIPRISTINO NON COMPATIBILE\n\n" +
                    "Il database corrente è CRIPTATO ma il backup selezionato è NON CRIPTATO.\n\n" +
                    "Non puoi ripristinare un backup non criptato quando l'app ha una password impostata.\n\n" +
                    "Soluzione:\n" +
                    "1. Rimuovi la crittografia dal database corrente\n" +
                    "2. Poi ripristina il backup non criptato",
                    "Ripristino Bloccato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!appHasPassword && backupIsEncrypted)
            {
                MessageBox.Show(
                    "⚠️ RIPRISTINO NON COMPATIBILE\n\n" +
                    "Il database corrente è NON CRIPTATO ma il backup selezionato è CRIPTATO.\n\n" +
                    "Non puoi ripristinare un backup criptato quando l'app non ha password impostata.\n\n" +
                    "Soluzione:\n" +
                    "1. Cripta il database corrente con la stessa password\n" +
                    "2. Poi ripristina il backup criptato\n\n" +
                    "Oppure seleziona un backup non criptato.",
                    "Ripristino Bloccato",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Conferma finale
            var statusText = appHasPassword ? "CRIPTATO" : "NON CRIPTATO";
            result = MessageBox.Show(
                $"Confermi il ripristino da:\n{Path.GetFileName(backupPath)}\n\n" +
                $"Stato backup: {statusText}\n" +
                $"Stato app: {statusText}\n\n" +
                $"✅ Compatibile! L'applicazione verrà riavviata.",
                "Conferma Ripristino",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            // Crea backup automatico di sicurezza nella stessa cartella backup
            var autoBackupFolder = Path.Combine(backupFolder, "Auto");
            if (!Directory.Exists(autoBackupFolder))
            {
                Directory.CreateDirectory(autoBackupFolder);
            }

            var autoBackupPath = Path.Combine(autoBackupFolder, $"cgeasy_pre_restore_{DateTime.Now:yyyyMMdd_HHmmss}.db");
            
            // Forza scrittura e crea backup di sicurezza
            _context.Checkpoint();
            File.Copy(dbPath, autoBackupPath, true);

            // Salva il percorso del backup da ripristinare in un file temporaneo
            var restoreInfoFile = Path.Combine(Path.GetDirectoryName(dbPath)!, "restore_pending.txt");
            File.WriteAllText(restoreInfoFile, backupPath);

            MessageBox.Show(
                "L'applicazione verrà riavviata per completare il ripristino.",
                "Ripristino Database",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Riavvia applicazione - il ripristino avverrà all'avvio successivo
            Application.Current.Shutdown();
            System.Diagnostics.Process.Start(Environment.ProcessPath!);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante il ripristino:\n{ex.Message}",
                "Errore Ripristino",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenBackupFolder()
    {
        try
        {
            var backupFolder = GetBackupFolder();
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            System.Diagnostics.Process.Start("explorer.exe", backupFolder);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore apertura cartella:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Verifica se un backup è criptato tentando di aprirlo
    /// </summary>
    private bool IsBackupEncrypted(string backupPath)
    {
        try
        {
            // Prova ad aprire senza password
            using var testDb = new LiteDbContext(backupPath, null);
            var count = testDb.Utenti.Count(); // Prova a leggere
            return false; // Se funziona, NON è criptato
        }
        catch
        {
            // Se va in errore, è criptato (o corrotto, ma assumiamo criptato)
            return true;
        }
    }

    // ===== CRITTOGRAFIA DATABASE =====

    private void UpdateEncryptionStatus()
    {
        bool isEncrypted = DatabaseEncryptionService.IsDatabaseEncrypted();
        StatoCrittografia = isEncrypted ? "CRIPTATO" : "NON CRIPTATO";
        StatoCrittografiaIcon = isEncrypted ? "🔒" : "🔓";
    }

    [RelayCommand]
    private void SetDatabasePassword(object parameter)
    {
        try
        {
            if (!SessionManager.IsAdministrator)
            {
                MessageBox.Show("Solo gli amministratori possono modificare la crittografia del database.",
                              "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (parameter is not PasswordBox passwordBox)
                return;

            var password = passwordBox.Password;

            // Valida password
            var (isValid, message) = DatabaseEncryptionService.ValidatePassword(password);
            if (!isValid)
            {
                MessageBox.Show(message, "Password Non Valida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ ATTENZIONE ⚠️\n\n" +
                $"Stai per CRIPTARE il database.\n\n" +
                $"Dopo questa operazione:\n" +
                $"• Il database sarà protetto da password\n" +
                $"• Servirà la password per accedere ai dati\n" +
                $"• Password Master: sempre disponibile per recupero\n\n" +
                $"Verrà creato un backup automatico prima della crittografia.\n\n" +
                $"Confermi?",
                "Conferma Crittografia Database",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Crea backup database corrente
            var dbPath = LiteDbContext.DefaultDatabasePath;
            var backupPath = DatabaseEncryptionService.CreateBackupBeforeEncryption(dbPath);

            // Salva password per il prossimo riavvio
            DatabaseEncryptionService.SavePassword(password);

            // Salva operazione da eseguire al prossimo avvio
            var operationFile = Path.Combine(Path.GetDirectoryName(dbPath)!, "encryption_pending.json");
            var operation = new
            {
                Operation = "encrypt",
                Password = password,
                BackupPath = backupPath,
                Timestamp = DateTime.Now
            };
            File.WriteAllText(operationFile, System.Text.Json.JsonSerializer.Serialize(operation));

            MessageBox.Show(
                $"✅ Crittografia preparata!\n\n" +
                $"Backup salvato in:\n{Path.GetFileName(backupPath)}\n\n" +
                $"⚠️ L'applicazione verrà riavviata per completare l'operazione.\n\n" +
                $"IMPORTANTE: Ricorda la password!\n" +
                $"Password Master di recupero: Woodstockac@74",
                "Preparazione Crittografia",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Pulisci password box
            passwordBox.Clear();

            // Riavvia applicazione
            Application.Current.Shutdown();
            System.Diagnostics.Process.Start(Environment.ProcessPath!);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante la crittografia:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RemoveDatabasePassword(object parameter)
    {
        try
        {
            if (!SessionManager.IsAdministrator)
            {
                MessageBox.Show("Solo gli amministratori possono modificare la crittografia del database.",
                              "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatabaseEncryptionService.IsDatabaseEncrypted())
            {
                MessageBox.Show("Il database non è attualmente criptato.",
                              "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (parameter is not PasswordBox passwordBox)
                return;

            var password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Inserisci la password corrente per rimuovere la crittografia.",
                              "Password Richiesta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica password
            if (!DatabaseEncryptionService.VerifyPassword(password))
            {
                MessageBox.Show("Password errata!",
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"⚠️ ATTENZIONE ⚠️\n\n" +
                $"Stai per RIMUOVERE la crittografia dal database.\n\n" +
                $"Dopo questa operazione:\n" +
                $"• Il database NON sarà più protetto\n" +
                $"• Chiunque potrà accedere ai dati\n" +
                $"• Non servirà più password\n\n" +
                $"Verrà creato un backup automatico.\n\n" +
                $"Confermi?",
                "Conferma Rimozione Crittografia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Crea backup database corrente
            var dbPath = LiteDbContext.DefaultDatabasePath;
            var backupPath = DatabaseEncryptionService.CreateBackupBeforeEncryption(dbPath);

            // Ottieni password corrente
            var currentPassword = DatabaseEncryptionService.GetSavedPassword();

            // Rimuovi password salvata
            DatabaseEncryptionService.RemovePassword();

            // Salva operazione da eseguire al prossimo avvio
            var operationFile = Path.Combine(Path.GetDirectoryName(dbPath)!, "encryption_pending.json");
            var operation = new
            {
                Operation = "decrypt",
                Password = currentPassword,
                BackupPath = backupPath,
                Timestamp = DateTime.Now
            };
            File.WriteAllText(operationFile, System.Text.Json.JsonSerializer.Serialize(operation));

            MessageBox.Show(
                $"✅ Decriptazione preparata!\n\n" +
                $"Backup salvato in:\n{Path.GetFileName(backupPath)}\n\n" +
                $"⚠️ L'applicazione verrà riavviata per completare l'operazione.\n\n" +
                $"Il database NON sarà più protetto.",
                "Preparazione Decriptazione",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Pulisci password box
            passwordBox.Clear();

            // Riavvia applicazione
            Application.Current.Shutdown();
            System.Diagnostics.Process.Start(Environment.ProcessPath!);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante la rimozione crittografia:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void VerifyDatabasePassword(object parameter)
    {
        try
        {
            if (!DatabaseEncryptionService.IsDatabaseEncrypted())
            {
                MessageBox.Show("Il database non è attualmente criptato.",
                              "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (parameter is not PasswordBox passwordBox)
                return;

            var password = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Inserisci una password da verificare.",
                              "Password Richiesta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var isMaster = password == DatabaseEncryptionService.GetMasterPassword();
            var isCorrect = DatabaseEncryptionService.VerifyPassword(password);

            if (isCorrect)
            {
                var tipoPwd = isMaster ? "Password Master (Recupero)" : "Password Corrente";
                MessageBox.Show(
                    $"✅ Password Corretta!\n\nTipo: {tipoPwd}",
                    "Verifica Password",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "❌ Password Errata!",
                    "Verifica Password",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            // Pulisci password box
            passwordBox.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante la verifica:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // ===== GESTIONE LICENZE =====

    private void UpdateLicenseStatus()
    {
        var licenseRepo = new LicenseRepository(_context);
        
        // TODO Studio
        if (LicenseService.IsTodoStudioActive())
        {
            LicenzaTodoStudio = "✅ Attivato";
            var key = LicenseService.GetTodoStudioKey();
            ChiaveTodoStudio = key ?? "";
            
            // Carica info durata/scadenza
            var licenseKey = licenseRepo.GetKeyByFullKey(key ?? "");
            if (licenseKey != null)
            {
                DurataTodoStudio = licenseKey.DurataDisplay;
                ScadenzaTodoStudio = licenseKey.DataScadenza?.ToString("dd/MM/yyyy") ?? "Mai";
            }
        }
        else
        {
            LicenzaTodoStudio = "❌ Non Attivato";
            ChiaveTodoStudio = "";
            DurataTodoStudio = "—";
            ScadenzaTodoStudio = "—";
        }

        // Bilanci
        if (LicenseService.IsBilanciActive())
        {
            LicenzaBilanci = "✅ Attivato";
            var key = LicenseService.GetBilanciKey();
            ChiaveBilanci = key ?? "";
            
            // Carica info durata/scadenza
            var licenseKey = licenseRepo.GetKeyByFullKey(key ?? "");
            if (licenseKey != null)
            {
                DurataBilanci = licenseKey.DurataDisplay;
                ScadenzaBilanci = licenseKey.DataScadenza?.ToString("dd/MM/yyyy") ?? "Mai";
            }
        }
        else
        {
            LicenzaBilanci = "❌ Non Attivato";
            ChiaveBilanci = "";
            DurataBilanci = "—";
            ScadenzaBilanci = "—";
        }

        // Circolari
        if (LicenseService.IsCircolariActive())
        {
            LicenzaCircolari = "✅ Attivato";
            var key = LicenseService.GetCircolariKey();
            ChiaveCircolari = key ?? "";
            
            // Carica info durata/scadenza
            var licenseKey = licenseRepo.GetKeyByFullKey(key ?? "");
            if (licenseKey != null)
            {
                DurataCircolari = licenseKey.DurataDisplay;
                ScadenzaCircolari = licenseKey.DataScadenza?.ToString("dd/MM/yyyy") ?? "Mai";
            }
        }
        else
        {
            LicenzaCircolari = "❌ Non Attivato";
            ChiaveCircolari = "";
            DurataCircolari = "—";
            ScadenzaCircolari = "—";
        }

        // Controllo Gestione
        if (LicenseService.IsControlloGestioneActive())
        {
            LicenzaControlloGestione = "✅ Attivato";
            var key = LicenseService.GetControlloGestioneKey();
            ChiaveControlloGestione = key ?? "";
            
            // Carica info durata/scadenza
            var licenseKey = licenseRepo.GetKeyByFullKey(key ?? "");
            if (licenseKey != null)
            {
                DurataControlloGestione = licenseKey.DurataDisplay;
                ScadenzaControlloGestione = licenseKey.DataScadenza?.ToString("dd/MM/yyyy") ?? "Mai";
            }
        }
        else
        {
            LicenzaControlloGestione = "❌ Non Attivato";
            ChiaveControlloGestione = "";
            DurataControlloGestione = "—";
            ScadenzaControlloGestione = "—";
        }

        // Conta moduli attivati
        ModuliAttivati = LicenseService.GetActiveModulesCount();
    }

    // ===== TODO STUDIO =====

    [RelayCommand]
    private void AttivaLicenzaTodoStudio()
    {
        AttivaModulo("TODO-STUDIO", ChiaveTodoStudio, "TODO Studio", 
            () => LicenseService.ActivateTodoStudio(ChiaveTodoStudio));
    }

    [RelayCommand]
    private void DisattivaLicenzaTodoStudio()
    {
        DisattivaModulo("TODO Studio", () => LicenseService.DeactivateTodoStudio());
    }

    [RelayCommand]
    private void GeneraChiaveTodoStudio()
    {
        GeneraChiaveModulo("TODO-STUDIO", "TODO Studio", (key) => ChiaveTodoStudio = key);
    }

    // ===== BILANCI =====

    [RelayCommand]
    private void AttivaLicenzaBilanci()
    {
        AttivaModulo("BILANCI", ChiaveBilanci, "Bilanci", 
            () => LicenseService.ActivateBilanci(ChiaveBilanci));
    }

    [RelayCommand]
    private void DisattivaLicenzaBilanci()
    {
        DisattivaModulo("Bilanci", () => LicenseService.DeactivateBilanci());
    }

    [RelayCommand]
    private void GeneraChiaveBilanci()
    {
        GeneraChiaveModulo("BILANCI", "Bilanci", (key) => ChiaveBilanci = key);
    }

    // ===== CIRCOLARI =====

    [RelayCommand]
    private void AttivaLicenzaCircolari()
    {
        AttivaModulo("CIRCOLARI", ChiaveCircolari, "Circolari", 
            () => LicenseService.ActivateCircolari(ChiaveCircolari));
    }

    [RelayCommand]
    private void DisattivaLicenzaCircolari()
    {
        DisattivaModulo("Circolari", () => LicenseService.DeactivateCircolari());
    }

    [RelayCommand]
    private void GeneraChiaveCircolari()
    {
        GeneraChiaveModulo("CIRCOLARI", "Circolari", (key) => ChiaveCircolari = key);
    }

    // ===== CONTROLLO GESTIONE =====

    [RelayCommand]
    private void AttivaLicenzaControlloGestione()
    {
        AttivaModulo("CONTROLLO-GESTIONE", ChiaveControlloGestione, "Controllo Gestione", 
            () => LicenseService.ActivateControlloGestione(ChiaveControlloGestione));
    }

    [RelayCommand]
    private void DisattivaLicenzaControlloGestione()
    {
        DisattivaModulo("Controllo Gestione", () => LicenseService.DeactivateControlloGestione());
    }

    [RelayCommand]
    private void GeneraChiaveControlloGestione()
    {
        GeneraChiaveModulo("CONTROLLO-GESTIONE", "Controllo Gestione", (key) => ChiaveControlloGestione = key);
    }

    // ===== METODI HELPER =====

    private void AttivaModulo(string moduleCode, string chiave, string moduleName, Func<bool> activateFunc)
    {
        if (string.IsNullOrWhiteSpace(chiave))
        {
            MessageBox.Show(
                $"Inserisci la chiave di attivazione per {moduleName}.",
                "Attivazione Licenza",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var success = activateFunc();

            if (success)
            {
                MessageBox.Show(
                    $"✅ MODULO {moduleName.ToUpper()} ATTIVATO!\n\n" +
                    $"Il modulo è ora disponibile.\n\n" +
                    $"Riavvia l'applicazione per utilizzare il modulo.",
                    "Attivazione Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                UpdateLicenseStatus();
            }
            else
            {
                MessageBox.Show(
                    $"❌ CHIAVE NON VALIDA\n\n" +
                    $"La chiave inserita per {moduleName} non è corretta.\n\n" +
                    $"Verifica la chiave e riprova.",
                    "Attivazione Fallita",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante attivazione {moduleName}:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void DisattivaModulo(string moduleName, Action deactivateAction)
    {
        var result = MessageBox.Show(
            $"Vuoi disattivare il modulo {moduleName}?\n\n" +
            $"Il modulo non sarà più accessibile.",
            "Disattivazione Modulo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                deactivateAction();
                MessageBox.Show(
                    $"Modulo {moduleName} disattivato.",
                    "Disattivazione Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                UpdateLicenseStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Errore durante disattivazione {moduleName}:\n{ex.Message}",
                    "Errore",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    private void GeneraChiaveModulo(string moduleCode, string moduleName, Action<string> setKeyAction)
    {
        try
        {
            // Ottiene o crea cliente "SISTEMA" per chiavi generate velocemente
            var context = ((App)Application.Current).Services!.GetRequiredService<LiteDbContext>();
            var licenseRepo = new LicenseRepository(context);
            
            // Cerca cliente SISTEMA
            var clienteSistema = licenseRepo.GetAllClients()
                .FirstOrDefault(c => c.NomeCliente.Equals("SISTEMA", StringComparison.OrdinalIgnoreCase));
            
            if (clienteSistema == null)
            {
                // Crea cliente SISTEMA se non esiste
                clienteSistema = new LicenseClient
                {
                    NomeCliente = "SISTEMA",
                    Email = "sistema@cgeasy.local",
                    Note = "Cliente di sistema per chiavi generate velocemente",
                    IsActive = true,
                    DataRegistrazione = DateTime.Now
                };
                clienteSistema.Id = licenseRepo.InsertClient(clienteSistema);
            }
            
            // Genera chiave
            var currentUserId = SessionManager.CurrentUser?.Id ?? 0;
#pragma warning disable CS0618 // Il tipo o il membro è obsoleto
            var key = LicenseService.GenerateLicenseKey(moduleName);
#pragma warning restore CS0618
            
            // 🔥 SALVA LA CHIAVE NEL DATABASE
            var licenseKey = new LicenseKey
            {
                LicenseClientId = clienteSistema.Id,
                ModuleName = moduleCode,
                FullKey = key,
                DataGenerazione = DateTime.Now,
                DataScadenza = DateTime.Now.AddYears(10), // 10 anni per sistema
                IsActive = true,
                GeneratedByUserId = currentUserId,
                Note = $"Chiave generata da pagina Sistema per modulo {moduleName}"
            };
            
            licenseRepo.InsertKey(licenseKey);
            
            MessageBox.Show(
                $"📋 CHIAVE DI ATTIVAZIONE {moduleName.ToUpper()}\n\n" +
                $"{key}\n\n" +
                $"✅ Chiave salvata nel database (Cliente: SISTEMA)\n" +
                $"Copia questa chiave e usala per attivare il modulo.",
                "Chiave Generata",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            setKeyAction(key);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante generazione chiave {moduleName}:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Apre la finestra di gestione licenze clienti (solo per admin)
    /// </summary>
    [RelayCommand]
    private void OpenLicenseManager()
    {
        try
        {
            var licenseManager = new Views.LicenseManagerView();
            licenseManager.ShowDialog();
            
            // Ricarica lo stato delle licenze dopo la chiusura
            UpdateLicenseStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore apertura gestione licenze:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // ===== CONFIGURAZIONE DATABASE =====

    /// <summary>
    /// Carica le informazioni sulla configurazione database corrente
    /// </summary>
    private void LoadDatabaseConfiguration()
    {
        var config = DatabaseConfigService.GetConfiguration();
        if (config != null)
        {
            NuovoPercorsoDatabase = config.DatabasePath;
            TipoPercorso = config.IsNetworkPath ? "Rete" : "Locale";
            InfoConfigurazione = $"Configurato il: {config.LastModified:dd/MM/yyyy HH:mm:ss}\n" +
                                $"Tipo: {(config.IsNetworkPath ? "Percorso di rete" : "Percorso locale")}";
        }
        else
        {
            NuovoPercorsoDatabase = string.Empty;
            TipoPercorso = "Default";
            InfoConfigurazione = "Nessuna configurazione personalizzata\n(uso percorso di default)";
        }
    }

    /// <summary>
    /// Sfoglia per selezionare un percorso database
    /// </summary>
    [RelayCommand]
    private void BrowseDatabasePath()
    {
        try
        {
            // Dialog per scegliere se locale o rete
            var result = MessageBox.Show(
                "Vuoi selezionare un percorso LOCALE?\n\n" +
                "• Sì = Sfoglia cartella locale\n" +
                "• No = Inserisci manualmente percorso di rete (es: \\\\SERVER\\Share\\cgeasy.db)",
                "Tipo Percorso",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
            {
                // Percorso locale - usa dialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Seleziona percorso database",
                    Filter = "Database CGEasy (*.db)|*.db|Tutti i file (*.*)|*.*",
                    DefaultExt = ".db",
                    FileName = "cgeasy.db"
                };

                if (dialog.ShowDialog() == true)
                {
                    NuovoPercorsoDatabase = dialog.FileName;
                }
            }
            else
            {
                // Percorso di rete - mostra esempio
                MessageBox.Show(
                    "Inserisci manualmente il percorso di rete nel campo 'Nuovo Percorso Database'.\n\n" +
                    "Esempi:\n" +
                    "• \\\\SERVER\\CGEasyData\\cgeasy.db\n" +
                    "• \\\\192.168.1.100\\Share\\cgeasy.db\n" +
                    "• \\\\NOME-PC\\Condivisa\\db\\cgeasy.db",
                    "Percorso di Rete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante selezione percorso:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Testa la connessione al percorso database specificato
    /// </summary>
    [RelayCommand]
    private void TestDatabaseConnection()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NuovoPercorsoDatabase))
            {
                MessageBox.Show(
                    "Inserisci un percorso database da testare.",
                    "Percorso Mancante",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Prova PRIMA con la password master (la maggior parte dei DB sono criptati)
            var masterPassword = DatabaseEncryptionService.GetMasterPassword();
            bool isValid = false;
            
            try
            {
                isValid = DatabaseConfigService.TestDatabasePath(NuovoPercorsoDatabase, masterPassword);
            }
            catch
            {
                // Se fallisce con password, prova senza (database non criptato)
                try
                {
                    isValid = DatabaseConfigService.TestDatabasePath(NuovoPercorsoDatabase, null);
                }
                catch
                {
                    isValid = false;
                }
            }

            if (isValid)
            {
                var isNetwork = NuovoPercorsoDatabase.StartsWith(@"\\") || NuovoPercorsoDatabase.StartsWith("//");
                MessageBox.Show(
                    $"✅ CONNESSIONE RIUSCITA!\n\n" +
                    $"Percorso: {NuovoPercorsoDatabase}\n" +
                    $"Tipo: {(isNetwork ? "Percorso di rete" : "Percorso locale")}\n\n" +
                    $"Il percorso è valido e accessibile.\n" +
                    $"Puoi salvare questa configurazione.",
                    "Test Connessione",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    $"❌ CONNESSIONE FALLITA\n\n" +
                    $"Il percorso non è raggiungibile o non è valido.\n\n" +
                    $"Possibili cause:\n" +
                    $"• Server non raggiungibile\n" +
                    $"• Permessi insufficienti\n" +
                    $"• Percorso inesistente\n" +
                    $"• Password database errata",
                    "Test Connessione",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante test connessione:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Salva la configurazione database e riavvia l'applicazione
    /// </summary>
    [RelayCommand]
    private void SaveDatabaseConfiguration()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NuovoPercorsoDatabase))
            {
                MessageBox.Show(
                    "Inserisci un percorso database valido.",
                    "Percorso Mancante",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Conferma
            var result = MessageBox.Show(
                $"Vuoi salvare questa configurazione?\n\n" +
                $"Percorso: {NuovoPercorsoDatabase}\n\n" +
                $"L'applicazione verrà riavviata per applicare le modifiche.\n\n" +
                $"ATTENZIONE: Assicurati che il percorso sia accessibile,\n" +
                $"altrimenti l'applicazione non si avvierà correttamente!",
                "Conferma Configurazione",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Salva configurazione
            DatabaseConfigService.SaveDatabasePath(NuovoPercorsoDatabase);

            MessageBox.Show(
                $"✅ Configurazione salvata!\n\n" +
                $"L'applicazione verrà riavviata.\n\n" +
                $"Al prossimo avvio, CGEasy userà:\n" +
                $"{NuovoPercorsoDatabase}",
                "Configurazione Salvata",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Riavvia applicazione
            System.Diagnostics.Process.Start(
                Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location);
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante salvataggio configurazione:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Ripristina il percorso database di default
    /// </summary>
    [RelayCommand]
    private void ResetDatabaseConfiguration()
    {
        try
        {
            var config = DatabaseConfigService.GetConfiguration();
            if (config == null)
            {
                MessageBox.Show(
                    "Nessuna configurazione personalizzata presente.\n\n" +
                    "L'applicazione sta già usando il percorso di default.",
                    "Già Default",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Vuoi ripristinare il percorso di default?\n\n" +
                $"Percorso corrente:\n{config.DatabasePath}\n\n" +
                $"Percorso default:\n{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "CGEasy", "cgeasy.db")}\n\n" +
                $"L'applicazione verrà riavviata.",
                "Ripristina Default",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            // Elimina configurazione
            DatabaseConfigService.DeleteConfiguration();

            MessageBox.Show(
                "✅ Configurazione rimossa!\n\n" +
                "L'applicazione verrà riavviata e userà il percorso di default.",
                "Ripristino Completato",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Riavvia applicazione
            System.Diagnostics.Process.Start(
                Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location);
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante ripristino configurazione:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}


