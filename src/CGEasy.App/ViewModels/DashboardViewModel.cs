using CommunityToolkit.Mvvm.ComponentModel;
using CGEasy.Core.Services;
using CGEasy.Core.Data;

namespace CGEasy.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly LiteDbContext _dbContext;

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

    public DashboardViewModel(LiteDbContext dbContext)
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

        // Carica statistiche database
        var stats = _dbContext.GetStats();
        TotalClienti = stats.TotalClienti;
        TotalProfessionisti = stats.TotalProfessionisti;
        TotalUtenti = stats.TotalUtenti;
        DatabasePath = stats.DatabasePath;
    }
}

