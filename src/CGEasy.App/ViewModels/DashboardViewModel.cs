using CommunityToolkit.Mvvm.ComponentModel;
using CGEasy.Core.Services;
using CGEasy.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly CGEasyDbContext _dbContext;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private int _totalClienti;

    [ObservableProperty]
    private int _totalProfessionisti;

    [ObservableProperty]
    private int _totalUtenti;

    [ObservableProperty]
    private string _databasePath = string.Empty;

    public DashboardViewModel(CGEasyDbContext dbContext)
    {
        _dbContext = dbContext;
        LoadData();
    }

    private void LoadData()
    {
        // Welcome message personalizzato
        if (SessionManager.CurrentUser != null)
        {
            var ora = DateTime.Now.Hour;
            var saluto = ora < 12 ? "Buongiorno" : ora < 18 ? "Buon pomeriggio" : "Buonasera";
            WelcomeMessage = $"{saluto}, {SessionManager.CurrentUser.Nome}!";
        }

        // Carica statistiche database SQL Server
        try
        {
            // ⚠️ Queste tabelle non sono ancora migrate - usiamo valori di default
            TotalClienti = 0; // _dbContext.Clienti.Count(); quando sarà migrato
            TotalProfessionisti = _dbContext.Professionisti.Count();
            TotalUtenti = _dbContext.Utenti.Count();
            DatabasePath = "SQL Server: localhost\\SQLEXPRESS - Database: CGEasy";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Errore caricamento statistiche: {ex.Message}");
            TotalClienti = 0;
            TotalProfessionisti = 0;
            TotalUtenti = 0;
            DatabasePath = "Errore connessione database";
        }
    }
}

