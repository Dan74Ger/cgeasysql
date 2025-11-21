using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.App.Views;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ⚠️ MODULO IN MIGRAZIONE A SQL SERVER
/// Versione semplificata - funzionalità complete disponibili dopo migrazione
/// </summary>
public partial class SistemaViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    
    [ObservableProperty]
    private string _percorsoDatabase = "SQL Server: localhost\\SQLEXPRESS - Database: CGEasy";

    [ObservableProperty]
    private string _dimensioneDatabase = "N/A (SQL Server)";

    [ObservableProperty]
    private string _ultimoBackup = "⚠️ Usa SQL Server Management Studio";

    [ObservableProperty]
    private int _numeroBackups = 0;

    [ObservableProperty]
    private string _statoCrittografia = "SQL Server TDE";

    [ObservableProperty]
    private string _statoCrittografiaIcon = "🔐";

    public SistemaViewModel(CGEasyDbContext context)
    {
        _context = context;
    }

    [RelayCommand]
    private void CreaBackup()
    {
        MessageBox.Show(
            "⚠️ FUNZIONALITÀ IN MIGRAZIONE\n\n" +
            "Per backup SQL Server usa:\n" +
            "• SQL Server Management Studio (SSMS)\n" +
            "• Comando: BACKUP DATABASE CGEasy TO DISK = 'C:\\backup\\CGEasy.bak'",
            "Backup",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void RipristinaBackup()
    {
        MessageBox.Show("⚠️ FUNZIONALITÀ IN MIGRAZIONE\nUsa SSMS per ripristinare.", "Ripristino", MessageBoxButton.OK, MessageBoxImage.Information);
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
}
