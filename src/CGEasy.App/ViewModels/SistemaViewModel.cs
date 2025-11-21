using System;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Services;
using CGEasy.App.Views;
using Microsoft.Win32;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ⚠️ MODULO IN MIGRAZIONE A SQL SERVER
/// Versione semplificata - funzionalità complete disponibili dopo migrazione
/// </summary>
public partial class SistemaViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly SqlServerBackupService _backupService;
    
    [ObservableProperty]
    private string _percorsoDatabase = "SQL Server: localhost\\SQLEXPRESS - Database: CGEasy";

    [ObservableProperty]
    private string _dimensioneDatabase = "N/A (SQL Server)";

    [ObservableProperty]
    private string _ultimoBackup = "Caricamento...";

    [ObservableProperty]
    private int _numeroBackups = 0;

    [ObservableProperty]
    private string _statoCrittografia = "SQL Server TDE";

    [ObservableProperty]
    private string _statoCrittografiaIcon = "🔐";

    public SistemaViewModel(CGEasyDbContext context)
    {
        _context = context;
        _backupService = new SqlServerBackupService(context);
        
        // Carica info backup
        CaricaInfoBackup();
    }

    private void CaricaInfoBackup()
    {
        var info = _backupService.GetBackupInfo();
        UltimoBackup = info.UltimoBackup;
        NumeroBackups = info.NumeroBackups;
    }

    [RelayCommand]
    private async void CreaBackup()
    {
        var result = MessageBox.Show(
            "Creare un backup del database?\n\n" +
            "Il backup verrà salvato in:\nC:\\db_CGEASY\\Backups\\",
            "Conferma Backup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        var (success, message, filePath) = await _backupService.CreaBackupAsync();
        
        if (success)
        {
            MessageBox.Show(message, "Backup Completato", MessageBoxButton.OK, MessageBoxImage.Information);
            CaricaInfoBackup();
        }
        else
        {
            MessageBox.Show(message, "Errore Backup", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async void RipristinaBackup()
    {
        // Mostra dialog per selezionare file backup
        var backupFiles = _backupService.GetBackupFiles();
        
        if (!backupFiles.Success || backupFiles.Files.Length == 0)
        {
            MessageBox.Show(
                "Nessun backup disponibile.\n\nCrea prima un backup usando il pulsante 'Crea Backup'.",
                "Nessun Backup",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var openFileDialog = new OpenFileDialog
        {
            Title = "Seleziona Backup da Ripristinare",
            Filter = "File Backup SQL (*.bak)|*.bak|Tutti i file (*.*)|*.*",
            InitialDirectory = @"C:\db_CGEASY\Backups"
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        var confirm = MessageBox.Show(
            "⚠️ ATTENZIONE ⚠️\n\n" +
            "Il ripristino sostituirà TUTTI i dati attuali con quelli del backup.\n\n" +
            "Vuoi procedere?",
            "Conferma Ripristino",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        var (success, message) = await _backupService.RipristinaBackupAsync(openFileDialog.FileName);
        
        if (success)
        {
            MessageBox.Show(
                message + "\n\nL'applicazione si chiuderà ora.\nRiaprila per continuare.",
                "Ripristino Completato",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            
            Application.Current.Shutdown();
        }
        else
        {
            MessageBox.Show(message, "Errore Ripristino", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CriptaDatabase()
    {
        MessageBox.Show("⚠️ FUNZIONALITÀ IN MIGRAZIONE\nSQL Server supporta TDE (Transparent Data Encryption).", "Crittografia", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void DecriptaDatabase()
    {
        MessageBox.Show("⚠️ FUNZIONALITÀ IN MIGRAZIONE", "Decrittografia", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void EsportaExcel()
    {
        MessageBox.Show("⚠️ FUNZIONALITÀ IN MIGRAZIONE", "Esportazione", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ImportaExcel()
    {
        MessageBox.Show("⚠️ FUNZIONALITÀ IN MIGRAZIONE", "Importazione", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ApriConfigurazioneSql()
    {
        var view = new ConfigurazioneSqlView();
        var window = new Window
        {
            Title = "Configurazione SQL Server",
            Content = view,
            Width = 1000,
            Height = 800,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.CanResize
        };
        window.ShowDialog();
    }

    [RelayCommand]
    private void OpenBackupFolder()
    {
        var backupPath = @"C:\db_CGEASY\Backups";
        
        if (!System.IO.Directory.Exists(backupPath))
        {
            System.IO.Directory.CreateDirectory(backupPath);
        }
        
        System.Diagnostics.Process.Start("explorer.exe", backupPath);
    }
}
