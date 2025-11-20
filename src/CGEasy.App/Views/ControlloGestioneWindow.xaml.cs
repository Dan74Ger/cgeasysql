using CGEasy.App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace CGEasy.App.Views
{
    public partial class ControlloGestioneWindow : Window
    {
        public ControlloGestioneWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                // Aggiorna i dati quando cambia tab
                if (TabRiepilogo.IsSelected && RiepilogoBancheViewControl.DataContext is RiepilogoBancheViewModel riepilogoVm)
                {
                    riepilogoVm.LoadRiepilogo();
                }
                else if (TabGestioneBanche.IsSelected && GestioneBancheViewControl.DataContext is GestioneBancheViewModel gestioneVm)
                {
                    gestioneVm.LoadBanche();
                }
                else if (TabDettaglioBanca.IsSelected && BancaDettaglioViewControl.DataContext is BancaDettaglioViewModel dettaglioVm)
                {
                    dettaglioVm.LoadBanca();
                }
            }
        }

        /// <summary>
        /// Mostra il tab di dettaglio per una banca specifica
        /// </summary>
        public void MostraDettaglioBanca(int bancaId, string nomeBanca)
        {
            // Ottieni il context Singleton
            var context = App.GetService<Core.Data.CGEasyDbContext>();
            if (context == null)
            {
                context = new Core.Data.CGEasyDbContext();
                // Singleton context - no special marking needed in EF Core
            }

            // Crea nuovo DataContext per il dettaglio
            BancaDettaglioViewControl.DataContext = new BancaDettaglioViewModel(context, bancaId);
            
            // Aggiorna il titolo del tab
            DettaglioBancaTitle.Text = $"Dettaglio: {nomeBanca}";
            
            // Mostra e seleziona il tab
            TabDettaglioBanca.Visibility = Visibility.Visible;
            TabDettaglioBanca.IsSelected = true;
        }
    }
}

