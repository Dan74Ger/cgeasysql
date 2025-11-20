using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Models;
using System.Linq;
using System.ComponentModel;

namespace CGEasy.App.Views
{
    public partial class StatisticheBilanciCEView : UserControl
    {
        private ViewModels.StatisticheBilanciCEViewModel? _viewModel;

        public StatisticheBilanciCEView()
        {
            InitializeComponent();
            
            var context = ((App)Application.Current).Services!.GetRequiredService<CGEasy.Core.Data.CGEasyDbContext>();
            _viewModel = new ViewModels.StatisticheBilanciCEViewModel(context);
            DataContext = _viewModel;

            // Sottoscrivi agli eventi per rigenerare colonne
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModels.StatisticheBilanciCEViewModel.StatisticheMulti) ||
                e.PropertyName == nameof(ViewModels.StatisticheBilanciCEViewModel.PeriodiColonne))
            {
                GeneraColonneDinamiche();
            }
        }

        private void GeneraColonneDinamiche()
        {
            if (_viewModel == null || _viewModel.StatisticheMulti.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"GeneraColonneDinamiche SKIP: ViewModel={_viewModel != null}, Count={_viewModel?.StatisticheMulti.Count ?? 0}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"GeneraColonneDinamiche START: Count={_viewModel.StatisticheMulti.Count}, Periodi={_viewModel.PeriodiColonne.Count}");

            StatisticheDataGrid.Columns.Clear();

            // Colonna Codice
            StatisticheDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Codice",
                Binding = new Binding("Codice"),
                Width = new DataGridLength(80),
                ElementStyle = CreateTextStyle(isBold: true, foreground: "#2196F3")
            });

            // Colonna Descrizione
            StatisticheDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Descrizione",
                Binding = new Binding("Descrizione"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                ElementStyle = CreateTextStyle(isWrapped: true)
            });

            // Colonne dinamiche per ogni periodo
            foreach (var periodoKey in _viewModel.PeriodiColonne)
            {
                // Trova periodo display dal viewmodel
                var bilancio = _viewModel.BilanciSelezionati.FirstOrDefault(b => 
                    $"{b.Mese:D2}_{b.Anno}" == periodoKey);
                var periodoDisplay = bilancio?.PeriodoDisplay ?? periodoKey;

                // Colonna Importo per questo periodo
                StatisticheDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = $"€ {periodoDisplay}",
                    Binding = new Binding($"DatiPerPeriodo[{periodoKey}].ImportoFormatted"),
                    Width = new DataGridLength(120),
                    ElementStyle = CreateTextStyle(isRight: true, isSemiBold: true)
                });

                // Colonna % per questo periodo
                StatisticheDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = $"% {periodoDisplay}",
                    Binding = new Binding($"DatiPerPeriodo[{periodoKey}].PercentualeDisplay"),
                    Width = new DataGridLength(90),
                    ElementStyle = CreateTextStyle(isRight: true, foreground: "#4CAF50")
                });
            }

            // ⭐ Colonne TOTALE (solo se MostraTotale è true)
            if (_viewModel.MostraTotale)
            {
                // Colonna TOTALE (evidenziata)
                StatisticheDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "💰 TOTALE",
                    Binding = new Binding("ImportoTotaleFormatted"),
                    Width = new DataGridLength(130),
                    ElementStyle = CreateTextStyle(isRight: true, isBold: true, foreground: "#FF5722")
                });

                // Colonna % TOTALE (evidenziata)
                StatisticheDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = "% TOTALE",
                    Binding = new Binding("PercentualeTotaleDisplay"),
                    Width = new DataGridLength(100),
                    ElementStyle = CreateTextStyle(isRight: true, isBold: true, foreground: "#4CAF50")
                });
            }

            // Colonna Associazioni (Button)
            var associazioniColumn = new DataGridTemplateColumn
            {
                Header = "Dettagli",
                Width = new DataGridLength(120)
            };

            var cellTemplate = new DataTemplate();
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetBinding(Button.ContentProperty, new Binding("AssociazioniDisplay"));
            buttonFactory.SetBinding(Button.CommandProperty, 
                new Binding("DataContext.MostraDettaglioAssociazioniCommand") 
                { 
                    RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1) 
                });
            buttonFactory.SetBinding(Button.CommandParameterProperty, new Binding());
            buttonFactory.SetBinding(Button.IsEnabledProperty, new Binding("HasContiAssociati"));
            buttonFactory.SetValue(Button.PaddingProperty, new Thickness(8, 4, 8, 4));
            buttonFactory.SetValue(Button.MarginProperty, new Thickness(5));
            buttonFactory.SetValue(Button.FontSizeProperty, 11.0);
            
            cellTemplate.VisualTree = buttonFactory;
            associazioniColumn.CellTemplate = cellTemplate;
            
            StatisticheDataGrid.Columns.Add(associazioniColumn);
        }

        private Style CreateTextStyle(bool isBold = false, bool isSemiBold = false, 
            bool isRight = false, bool isWrapped = false, string? foreground = null)
        {
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(10, 0, 10, 0)));
            style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
            
            if (isBold)
                style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.Bold));
            else if (isSemiBold)
                style.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));

            if (isRight)
                style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Right));

            if (isWrapped)
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));

            if (!string.IsNullOrEmpty(foreground))
                style.Setters.Add(new Setter(TextBlock.ForegroundProperty, 
                    new System.Windows.Media.BrushConverter().ConvertFromString(foreground)));

            return style;
        }
    }
}

