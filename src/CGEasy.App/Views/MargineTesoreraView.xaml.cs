using CGEasy.App.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Linq;

namespace CGEasy.App.Views
{
    public partial class MargineTesoreraView : UserControl
    {
        public MargineTesoreraView()
        {
            InitializeComponent();
            DataContext = new RiepilogoBancheViewModel();
            DataContextChanged += MargineTesoreraView_DataContextChanged;
            Loaded += MargineTesoreraView_Loaded;
        }

        private void MargineTesoreraView_Loaded(object sender, RoutedEventArgs e)
        {
            // Carica i dati quando la view viene caricata
            System.Diagnostics.Debug.WriteLine("MargineTesoreraView_Loaded: START");
            
            // Usa Dispatcher per assicurarsi che l'UI sia pronta
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (DataContext is RiepilogoBancheViewModel vm)
                {
                    System.Diagnostics.Debug.WriteLine("MargineTesoreraView_Loaded: DataContext OK, carico dati...");
                    
                    // Mostra subito il messaggio di caricamento
                    NoDataBorder.Visibility = Visibility.Visible;
                    PivotScrollViewer.Visibility = Visibility.Collapsed;
                    
                    vm.LoadMargineTesoreria();
                    
                    System.Diagnostics.Debug.WriteLine($"MargineTesoreraView_Loaded: MargineTesoreria è null? {vm.MargineTesoreria == null}");
                    if (vm.MargineTesoreria != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"MargineTesoreraView_Loaded: Numero mesi: {vm.MargineTesoreria.Mesi.Count}");
                    }
                    
                    // Forza il popolamento anche se già caricato
                    PopulateMarginePivotGrid();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"MargineTesoreraView_Loaded: DataContext è {DataContext?.GetType().Name ?? "NULL"}!");
                    NoDataBorder.Visibility = Visibility.Visible;
                    PivotScrollViewer.Visibility = Visibility.Collapsed;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void MargineTesoreraView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is RiepilogoBancheViewModel vm)
            {
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(RiepilogoBancheViewModel.MargineTesoreria))
                    {
                        PopulateMarginePivotGrid();
                    }
                };

                // Popola subito se i dati sono già disponibili
                if (vm.MargineTesoreria != null)
                {
                    PopulateMarginePivotGrid();
                }
            }
        }

        private void PopulateMarginePivotGrid()
        {
            System.Diagnostics.Debug.WriteLine("PopulateMarginePivotGrid: Inizio popolamento...");
            
            if (DataContext is not RiepilogoBancheViewModel vm || vm.MargineTesoreria == null)
            {
                System.Diagnostics.Debug.WriteLine("PopulateMarginePivotGrid: ViewModel o MargineTesoreria è null");
                NoDataBorder.Visibility = Visibility.Visible;
                PivotScrollViewer.Visibility = Visibility.Collapsed;
                return;
            }

            var margine = vm.MargineTesoreria;
            var grid = MarginePivotGrid;

            System.Diagnostics.Debug.WriteLine($"PopulateMarginePivotGrid: Numero mesi: {margine.Mesi.Count}");

            // Pulisci grid esistente
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Children.Clear();

            if (!margine.Mesi.Any())
            {
                System.Diagnostics.Debug.WriteLine("PopulateMarginePivotGrid: Nessun mese disponibile");
                NoDataBorder.Visibility = Visibility.Visible;
                PivotScrollViewer.Visibility = Visibility.Collapsed;
                return;
            }

            // Nascondi il messaggio e mostra la grid
            NoDataBorder.Visibility = Visibility.Collapsed;
            PivotScrollViewer.Visibility = Visibility.Visible;

            System.Diagnostics.Debug.WriteLine($"PopulateMarginePivotGrid: Creazione {margine.Mesi.Count + 1} colonne...");

            // Aggiungi colonne: Categoria + Mesi
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Categoria
            foreach (var mese in margine.Mesi)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            }

            // Aggiungi righe
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: Fatturato Anticipato
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1: Residuo Anticipabile
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2: Header (Categoria + Mesi)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 3: Saldo Corrente
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 4: Incassi (con accordion)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 5: Pagamenti (con accordion)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 6: Saldo Disponibile
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 7: Utilizzo Fido C/C

            // Fatturato Anticipato (con accordion per banche)
            AddMarginePivotRowWithBancheBreakdown(0, margine.FatturatoAnticipato, margine.Banche, margine.Mesi, "#9B59B6", false);

            // Residuo Anticipabile (con accordion per banche)
            AddMarginePivotRowWithBancheBreakdown(1, margine.ResiduoAnticipabile, margine.Banche, margine.Mesi, "#E67E22", false);

            // Header (Categoria + Mesi)
            var headerCategoriaCell = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E")),
                Padding = new Thickness(10)
            };
            var headerCategoriaText = new TextBlock
            {
                Text = "Categoria",
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            headerCategoriaCell.Child = headerCategoriaText;
            Grid.SetRow(headerCategoriaCell, 2);
            Grid.SetColumn(headerCategoriaCell, 0);
            MarginePivotGrid.Children.Add(headerCategoriaCell);

            for (int i = 0; i < margine.Mesi.Count; i++)
            {
                var headerMeseCell = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E")),
                    Padding = new Thickness(10)
                };
                var headerMeseText = new TextBlock
                {
                    Text = margine.Mesi[i],
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                headerMeseCell.Child = headerMeseText;
                Grid.SetRow(headerMeseCell, 2);
                Grid.SetColumn(headerMeseCell, i + 1);
                MarginePivotGrid.Children.Add(headerMeseCell);
            }

            // Saldo Corrente (con accordion per banche)
            AddMarginePivotRowWithBancheBreakdown(3, margine.SaldoCorrente, margine.Banche, margine.Mesi, "#95A5A6", false);

            // Incassi (con accordion)
            AddMarginePivotRowWithAccordion(4, margine.Incassi, margine.Mesi, "#27AE60", false);

            // Pagamenti (con accordion)
            AddMarginePivotRowWithAccordion(5, margine.Pagamenti, margine.Mesi, "#E74C3C", false);

            // Saldo Disponibile (con accordion per banche)
            AddMarginePivotRowWithBancheBreakdown(6, margine.SaldoDisponibile, margine.Banche, margine.Mesi, "#3498DB", true);

            // Utilizzo Fido C/C (solo se ci sono banche in negativo)
            AddMarginePivotRowFidoCC(7, margine, margine.Banche, margine.Mesi);

            System.Diagnostics.Debug.WriteLine($"PopulateMarginePivotGrid: Completato! Grid ha {grid.Children.Count} elementi.");
        }

        private void AddMarginePivotRow(int rowIndex, SaldoPivotRigaConsolidato riga, System.Collections.Generic.List<string> mesi, string colorHex, bool isSaldoFinale)
        {
            // Cella categoria
            var categoriaCell = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                Padding = new Thickness(10)
            };
            var categoriaText = new TextBlock
            {
                Text = riga.Categoria,
                FontWeight = isSaldoFinale ? FontWeights.Bold : FontWeights.SemiBold,
                Foreground = Brushes.White,
                FontSize = isSaldoFinale ? 14 : 12
            };
            categoriaCell.Child = categoriaText;
            Grid.SetRow(categoriaCell, rowIndex);
            Grid.SetColumn(categoriaCell, 0);
            MarginePivotGrid.Children.Add(categoriaCell);

            // Celle valori
            for (int i = 0; i < mesi.Count; i++)
            {
                var mese = mesi[i];
                var valore = riga.ValoriMensili.ContainsKey(mese) ? riga.ValoriMensili[mese] : 0;

                var valoreCell = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                    Padding = new Thickness(10)
                };
                var valoreText = new TextBlock
                {
                    Text = valore.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €",
                    FontWeight = isSaldoFinale ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                valoreCell.Child = valoreText;
                Grid.SetRow(valoreCell, rowIndex);
                Grid.SetColumn(valoreCell, i + 1);
                MarginePivotGrid.Children.Add(valoreCell);
            }
        }

        private void AddMarginePivotRowWithAccordion(int rowIndex, SaldoPivotRigaConsolidato riga, System.Collections.Generic.List<string> mesi, string colorHex, bool isSaldoFinale)
        {
            // Cella categoria
            var categoriaCell = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                Padding = new Thickness(10)
            };
            var categoriaText = new TextBlock
            {
                Text = riga.Categoria,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 13
            };
            categoriaCell.Child = categoriaText;
            Grid.SetRow(categoriaCell, rowIndex);
            Grid.SetColumn(categoriaCell, 0);
            MarginePivotGrid.Children.Add(categoriaCell);

            // Celle valori con accordion
            for (int i = 0; i < mesi.Count; i++)
            {
                var mese = mesi[i];
                var valore = riga.ValoriMensili.ContainsKey(mese) ? riga.ValoriMensili[mese] : 0;
                var dettagli = riga.DettagliMensili.ContainsKey(mese) ? riga.DettagliMensili[mese] : new System.Collections.Generic.List<MovimentoMensileDettaglioConsolidato>();

                var valoreCell = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                    Padding = new Thickness(8)
                };

                var containerStack = new StackPanel();

                // Totale + pulsante expand
                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var totaleText = new TextBlock
                {
                    Text = valore.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €",
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(totaleText, 0);
                headerGrid.Children.Add(totaleText);

                if (dettagli.Count > 0)
                {
                    var expandButton = new Button
                    {
                        Content = "▼",
                        Background = Brushes.Transparent,
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        FontSize = 10,
                        Padding = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(expandButton, 1);
                    headerGrid.Children.Add(expandButton);

                    var detailsStackPanel = new StackPanel
                    {
                        Visibility = Visibility.Collapsed,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    foreach (var dettaglio in dettagli.OrderBy(d => d.NomeBanca).ThenBy(d => d.DataScadenza))
                    {
                        var detailBorder = new Border
                        {
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, 2, 0, 0)
                        };

                        // Colore di sfondo per anticipi e storni
                        if (dettaglio.Descrizione.StartsWith("Anticipo "))
                        {
                            detailBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4EDDA"));
                        }
                        else if (dettaglio.Descrizione.StartsWith("Storno anticipo"))
                        {
                            detailBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE4B5"));
                        }
                        else
                        {
                            detailBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA"));
                        }

                        var detailStack = new StackPanel();
                        
                        var bancaText = new TextBlock
                        {
                            Text = $"🏦 {dettaglio.NomeBanca}",
                            FontWeight = FontWeights.Bold,
                            FontSize = 9,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"))
                        };
                        detailStack.Children.Add(bancaText);

                        var descrizioneText = new TextBlock
                        {
                            Text = dettaglio.Descrizione,
                            FontSize = 10,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E"))
                        };
                        detailStack.Children.Add(descrizioneText);

                        var importoText = new TextBlock
                        {
                            Text = dettaglio.Importo.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €",
                            FontWeight = FontWeights.Bold,
                            FontSize = 11,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                        };
                        detailStack.Children.Add(importoText);

                        if (!string.IsNullOrEmpty(dettaglio.NumeroFattura))
                        {
                            var fatturaText = new TextBlock
                            {
                                Text = $"Fatt. {dettaglio.NumeroFattura}" + 
                                       (dettaglio.DataFattura.HasValue ? $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}" : ""),
                                FontSize = 9,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"))
                            };
                            detailStack.Children.Add(fatturaText);
                        }

                        var scadenzaText = new TextBlock
                        {
                            Text = $"Scad: {dettaglio.DataScadenza:dd/MM/yyyy}",
                            FontSize = 9,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"))
                        };
                        detailStack.Children.Add(scadenzaText);

                        detailBorder.Child = detailStack;
                        detailsStackPanel.Children.Add(detailBorder);
                    }

                    expandButton.Click += (s, e) =>
                    {
                        if (detailsStackPanel.Visibility == Visibility.Collapsed)
                        {
                            detailsStackPanel.Visibility = Visibility.Visible;
                            expandButton.Content = "▲";
                        }
                        else
                        {
                            detailsStackPanel.Visibility = Visibility.Collapsed;
                            expandButton.Content = "▼";
                        }
                    };

                    containerStack.Children.Add(headerGrid);
                    containerStack.Children.Add(detailsStackPanel);
                }
                else
                {
                    containerStack.Children.Add(headerGrid);
                }

                valoreCell.Child = containerStack;
                Grid.SetRow(valoreCell, rowIndex);
                Grid.SetColumn(valoreCell, i + 1);
                MarginePivotGrid.Children.Add(valoreCell);
            }
        }

        private void AddMarginePivotRowWithBancheBreakdown(int rowIndex, SaldoPivotRigaConsolidato riga, System.Collections.Generic.List<string> banche, System.Collections.Generic.List<string> mesi, string colorHex, bool isSaldoFinale)
        {
            // Cella categoria
            var categoriaCell = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                Padding = new Thickness(10)
            };
            var categoriaText = new TextBlock
            {
                Text = riga.Categoria,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = isSaldoFinale ? 14 : 13
            };
            categoriaCell.Child = categoriaText;
            Grid.SetRow(categoriaCell, rowIndex);
            Grid.SetColumn(categoriaCell, 0);
            MarginePivotGrid.Children.Add(categoriaCell);

            // Celle valori con accordion per banche
            for (int i = 0; i < mesi.Count; i++)
            {
                var mese = mesi[i];
                var valoreTotale = riga.ValoriMensili.ContainsKey(mese) ? riga.ValoriMensili[mese] : 0;
                var valoriPerBanca = riga.ValoriPerBanca.ContainsKey(mese) ? riga.ValoriPerBanca[mese] : new Dictionary<string, decimal>();

                var valoreCell = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                    Padding = new Thickness(8)
                };

                var containerStack = new StackPanel();

                // Totale + pulsante expand
                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var totaleText = new TextBlock
                {
                    Text = valoreTotale.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €",
                    FontWeight = isSaldoFinale ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = isSaldoFinale ? 12 : 11
                };
                Grid.SetColumn(totaleText, 0);
                headerGrid.Children.Add(totaleText);

                if (valoriPerBanca.Any())
                {
                    var expandButton = new Button
                    {
                        Content = "▼",
                        Background = Brushes.Transparent,
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        FontSize = 10,
                        Padding = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(expandButton, 1);
                    headerGrid.Children.Add(expandButton);

                    var detailsStackPanel = new StackPanel
                    {
                        Visibility = Visibility.Collapsed,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    foreach (var kvp in valoriPerBanca.OrderBy(x => x.Key))
                    {
                        var detailBorder = new Border
                        {
                            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, 2, 0, 0),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8F9FA"))
                        };

                        var detailStack = new StackPanel
                        {
                            Orientation = Orientation.Vertical
                        };
                        
                        var bancaText = new TextBlock
                        {
                            Text = $"🏦 {kvp.Key}",
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 9,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50")),
                            Margin = new Thickness(0, 0, 0, 2)
                        };
                        detailStack.Children.Add(bancaText);

                        var importoText = new TextBlock
                        {
                            Text = kvp.Value.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €",
                            FontWeight = FontWeights.Bold,
                            FontSize = 11,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60")),
                            HorizontalAlignment = HorizontalAlignment.Right
                        };
                        detailStack.Children.Add(importoText);

                        detailBorder.Child = detailStack;
                        detailsStackPanel.Children.Add(detailBorder);
                    }

                    expandButton.Click += (s, e) =>
                    {
                        if (detailsStackPanel.Visibility == Visibility.Collapsed)
                        {
                            detailsStackPanel.Visibility = Visibility.Visible;
                            expandButton.Content = "▲";
                        }
                        else
                        {
                            detailsStackPanel.Visibility = Visibility.Collapsed;
                            expandButton.Content = "▼";
                        }
                    };

                    containerStack.Children.Add(headerGrid);
                    containerStack.Children.Add(detailsStackPanel);
                }
                else
                {
                    containerStack.Children.Add(headerGrid);
                }

                valoreCell.Child = containerStack;
                Grid.SetRow(valoreCell, rowIndex);
                Grid.SetColumn(valoreCell, i + 1);
                MarginePivotGrid.Children.Add(valoreCell);
            }
        }

        private void AddMarginePivotRowFidoCC(int rowIndex, MargineTesoreraData margine, System.Collections.Generic.List<string> banche, System.Collections.Generic.List<string> mesi)
        {
            // Verifica se c'è almeno una banca in negativo in almeno un mese
            bool hasFidoUtilizzato = mesi.Any(m => margine.UtilizzoFidoCC.ValoriMensili[m] > 0);
            
            if (!hasFidoUtilizzato)
            {
                // Nessuna banca in negativo, non mostrare la riga
                return;
            }

            string colorHex = "#FF6B6B"; // Rosso/Arancione per alert

            // Cella categoria
            var categoriaCell = new Border
            {
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                Padding = new Thickness(10)
            };
            var categoriaText = new TextBlock
            {
                Text = "⚠️ Utilizzo Fido C/C",
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 13
            };
            categoriaCell.Child = categoriaText;
            Grid.SetRow(categoriaCell, rowIndex);
            Grid.SetColumn(categoriaCell, 0);
            MarginePivotGrid.Children.Add(categoriaCell);

            // Celle valori con dettaglio Fido
            for (int i = 0; i < mesi.Count; i++)
            {
                var mese = mesi[i];
                var utilizzoTotale = margine.UtilizzoFidoCC.ValoriMensili[mese];
                
                var valoreCell = new Border
                {
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(utilizzoTotale > 0 ? colorHex : "#F8F9FA")),
                    Padding = new Thickness(8)
                };

                if (utilizzoTotale == 0)
                {
                    // Nessun utilizzo in questo mese
                    var nessunUtilizzoText = new TextBlock
                    {
                        Text = "-",
                        FontWeight = FontWeights.Normal,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6")),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    valoreCell.Child = nessunUtilizzoText;
                }
                else
                {
                    // C'è utilizzo del fido
                    var containerStack = new StackPanel();

                    // Totale + pulsante expand
                    var headerGrid = new Grid();
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var totaleText = new TextBlock
                    {
                        Text = utilizzoTotale.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €",
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(totaleText, 0);
                    headerGrid.Children.Add(totaleText);

                    var expandButton = new Button
                    {
                        Content = "▼",
                        Background = Brushes.Transparent,
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        FontSize = 10,
                        Padding = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetColumn(expandButton, 1);
                    headerGrid.Children.Add(expandButton);

                    var detailsStackPanel = new StackPanel
                    {
                        Visibility = Visibility.Collapsed,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    // Aggiungi dettagli per ogni banca in negativo
                    foreach (var nomeBanca in banche)
                    {
                        var utilizzo = margine.UtilizzoFidoCC.ValoriPerBanca[mese][nomeBanca];
                        
                        if (utilizzo > 0)
                        {
                            var fidoCC = margine.FidoCCPerBanca[nomeBanca];
                            var fidoResiduo = fidoCC - utilizzo;

                            var detailBorder = new Border
                            {
                                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                                BorderThickness = new Thickness(1),
                                Padding = new Thickness(8),
                                Margin = new Thickness(0, 2, 0, 0),
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3CD"))
                            };

                            var detailStack = new StackPanel();
                            
                            var bancaText = new TextBlock
                            {
                                Text = $"⚠️ {nomeBanca}",
                                FontWeight = FontWeights.Bold,
                                FontSize = 10,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#856404")),
                                Margin = new Thickness(0, 0, 0, 5)
                            };
                            detailStack.Children.Add(bancaText);

                            var fidoDisponibileText = new TextBlock
                            {
                                Text = $"Fido C/C Disponibile: {fidoCC.ToString("N2", CultureInfo.GetCultureInfo("it-IT"))} €",
                                FontSize = 9,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D"))
                            };
                            detailStack.Children.Add(fidoDisponibileText);

                            var utilizzoText = new TextBlock
                            {
                                Text = $"Utilizzo Fido: {utilizzo.ToString("N2", CultureInfo.GetCultureInfo("it-IT"))} €",
                                FontWeight = FontWeights.Bold,
                                FontSize = 10,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545")),
                                Margin = new Thickness(0, 2, 0, 2)
                            };
                            detailStack.Children.Add(utilizzoText);

                            var residuoText = new TextBlock
                            {
                                Text = $"Fido Residuo: {fidoResiduo.ToString("N2", CultureInfo.GetCultureInfo("it-IT"))} €",
                                FontSize = 9,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fidoResiduo > 0 ? "#28A745" : "#DC3545")),
                                FontWeight = fidoResiduo <= 0 ? FontWeights.Bold : FontWeights.Normal
                            };
                            detailStack.Children.Add(residuoText);

                            detailBorder.Child = detailStack;
                            detailsStackPanel.Children.Add(detailBorder);
                        }
                    }

                    // Aggiungi totali consolidati
                    var totaleBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BDC3C7")),
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(8),
                        Margin = new Thickness(0, 5, 0, 0),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E9ECEF"))
                    };

                    var totaleStack = new StackPanel();
                    
                    var totaleHeaderText = new TextBlock
                    {
                        Text = "📊 TOTALI CONSOLIDATI",
                        FontWeight = FontWeights.Bold,
                        FontSize = 9,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#495057")),
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    totaleStack.Children.Add(totaleHeaderText);

                    decimal fidoTotale = margine.FidoCCPerBanca.Values.Sum();
                    decimal utilizzoTotaleConsolidato = utilizzoTotale;
                    decimal fidoResiduoTotale = fidoTotale - utilizzoTotaleConsolidato;

                    var totaleFidoText = new TextBlock
                    {
                        Text = $"Fido C/C Totale: {fidoTotale.ToString("N2", CultureInfo.GetCultureInfo("it-IT"))} €",
                        FontSize = 9,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C757D"))
                    };
                    totaleStack.Children.Add(totaleFidoText);

                    var totaleUtilizzoText = new TextBlock
                    {
                        Text = $"Utilizzo Totale: {utilizzoTotaleConsolidato.ToString("N2", CultureInfo.GetCultureInfo("it-IT"))} €",
                        FontWeight = FontWeights.Bold,
                        FontSize = 9,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC3545")),
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    totaleStack.Children.Add(totaleUtilizzoText);

                    var totaleResiduoText = new TextBlock
                    {
                        Text = $"Fido Residuo Totale: {fidoResiduoTotale.ToString("N2", CultureInfo.GetCultureInfo("it-IT"))} €",
                        FontSize = 9,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fidoResiduoTotale > 0 ? "#28A745" : "#DC3545")),
                        FontWeight = fidoResiduoTotale <= 0 ? FontWeights.Bold : FontWeights.Normal
                    };
                    totaleStack.Children.Add(totaleResiduoText);

                    totaleBorder.Child = totaleStack;
                    detailsStackPanel.Children.Add(totaleBorder);

                    expandButton.Click += (s, e) =>
                    {
                        if (detailsStackPanel.Visibility == Visibility.Collapsed)
                        {
                            detailsStackPanel.Visibility = Visibility.Visible;
                            expandButton.Content = "▲";
                        }
                        else
                        {
                            detailsStackPanel.Visibility = Visibility.Collapsed;
                            expandButton.Content = "▼";
                        }
                    };

                    containerStack.Children.Add(headerGrid);
                    containerStack.Children.Add(detailsStackPanel);

                    valoreCell.Child = containerStack;
                }

                Grid.SetRow(valoreCell, rowIndex);
                Grid.SetColumn(valoreCell, i + 1);
                MarginePivotGrid.Children.Add(valoreCell);
            }
        }
    }
}

