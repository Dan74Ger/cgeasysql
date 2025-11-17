using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views
{
    /// <summary>
    /// Interaction logic for CircolariWindow.xaml
    /// </summary>
    public partial class CircolariWindow : Window
    {
        private readonly LiteDbContext _context;
        private ImportaCircolareViewModel? _importaViewModel;
        private RicercaCircolariViewModel? _ricercaViewModel;

        public CircolariWindow()
        {
            InitializeComponent();

            // Ottiene Singleton LiteDbContext
            var app = (App)Application.Current;
            _context = app.Services!.GetRequiredService<LiteDbContext>();

            // Imposta DataContext per ogni tab
            var argomentiTab = (ArgomentiView)((TabItem)((TabControl)Content).Items[0]).Content;
            argomentiTab.DataContext = new ArgomentiViewModel(_context);

            var importaTab = (ImportaCircolareView)((TabItem)((TabControl)Content).Items[1]).Content;
            _importaViewModel = new ImportaCircolareViewModel(_context);
            importaTab.DataContext = _importaViewModel;

            var ricercaTab = (RicercaCircolariView)((TabItem)((TabControl)Content).Items[2]).Content;
            _ricercaViewModel = new RicercaCircolariViewModel(_context);
            ricercaTab.DataContext = _ricercaViewModel;

            // Aggiungi evento per cambiamento tab
            var tabControl = (TabControl)Content;
            tabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl)
            {
                // Quando cambia tab, ricarica i dati
                var selectedIndex = tabControl.SelectedIndex;

                if (selectedIndex == 1 && _importaViewModel != null)
                {
                    // Tab "Importa Circolare" - ricarica argomenti
                    _importaViewModel.RefreshArgomenti();
                }
                else if (selectedIndex == 2 && _ricercaViewModel != null)
                {
                    // Tab "Ricerca" - ricarica tutto
                    _ricercaViewModel.RefreshData();
                }
            }
        }
    }
}

