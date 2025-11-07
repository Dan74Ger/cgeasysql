using CGEasy.App.ViewModels;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace CGEasy.App.Views
{
    public partial class BancaDettaglioView : UserControl
    {
        public BancaDettaglioView()
        {
            InitializeComponent();
            DataContextChanged += BancaDettaglioView_DataContextChanged;
        }

        public BancaDettaglioView(int bancaId)
        {
            InitializeComponent();
            DataContext = new BancaDettaglioViewModel(new Core.Data.LiteDbContext(), bancaId);
            DataContextChanged += BancaDettaglioView_DataContextChanged;
        }

        private void BancaDettaglioView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is BancaDettaglioViewModel vm)
            {
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(BancaDettaglioViewModel.SaldoPivot))
                    {
                        PopulatePivotGrid();
                    }
                };
                
                // Popola subito se i dati sono già disponibili
                if (vm.SaldoPivot != null)
                {
                    PopulatePivotGrid();
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Carica la pivot solo quando si apre il tab Saldo Previsto Pivot
            if (e.Source is System.Windows.Controls.TabControl && DataContext is BancaDettaglioViewModel vm)
            {
                if (e.AddedItems.Count > 0 && e.AddedItems[0] is System.Windows.Controls.TabItem tab)
                {
                    if (tab.Name == "TabSaldoPivot")
                    {
                        // Carica la pivot solo se non è già stata caricata o se è cambiato qualcosa
                        vm.LoadSaldoPivot();
                    }
                }
            }
        }

        private void PopulatePivotGrid()
        {
            if (DataContext is not BancaDettaglioViewModel vm || vm.SaldoPivot == null)
                return;

            var pivot = vm.SaldoPivot;
            
            // Pulisci il grid
            PivotGrid.Children.Clear();
            PivotGrid.ColumnDefinitions.Clear();
            
            // Definisci le colonne: prima colonna per le etichette, poi una colonna per ogni mese
            PivotGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) }); // Colonna etichette
            foreach (var mese in pivot.Mesi)
            {
                PivotGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            }
            
            // RIGA 0: FATTURATO ANTICIPATO (sopra header, viola chiaro)
            AddPivotRow(0, pivot.FatturatoAnticipato, pivot.Mesi, "#9B59B6", false);
            
            // RIGA 1: RESIDUO ANTICIPABILE (sopra header, arancione)
            AddPivotRow(1, pivot.ResiduoAnticipabile, pivot.Mesi, "#E67E22", false);
            
            // RIGA 2: HEADER
            var headerLabel = new TextBlock
            {
                Text = "Categoria",
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(10),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E")),
                Foreground = Brushes.White
            };
            Grid.SetRow(headerLabel, 2);
            Grid.SetColumn(headerLabel, 0);
            PivotGrid.Children.Add(headerLabel);
            
            // Header per ogni mese
            for (int i = 0; i < pivot.Mesi.Count; i++)
            {
                var meseHeader = new TextBlock
                {
                    Text = pivot.Mesi[i],
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Padding = new Thickness(10),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34495E")),
                    Foreground = Brushes.White
                };
                Grid.SetRow(meseHeader, 2);
                Grid.SetColumn(meseHeader, i + 1);
                PivotGrid.Children.Add(meseHeader);
            }
            
            // RIGA 3: SALDO CORRENTE
            AddPivotRowWithDescription(3, pivot.SaldoCorrente, pivot.Mesi, "#95A5A6", false, true);
            
            // RIGA 4: INCASSI (con accordion)
            AddPivotRowWithAccordion(4, pivot.Incassi, pivot.Mesi, "#27AE60", false);
            
            // RIGA 5: PAGAMENTI (con accordion)
            AddPivotRowWithAccordion(5, pivot.Pagamenti, pivot.Mesi, "#E74C3C", false);
            
            // RIGA 6: SALDO DISPONIBILE (in basso, in grassetto)
            AddPivotRow(6, pivot.SaldoDisponibile, pivot.Mesi, "#3498DB", true);
        }

        private void AddPivotRow(int rowIndex, SaldoPivotRiga riga, System.Collections.Generic.List<string> mesi, string colorHex, bool isSaldoFinale)
        {
            // Colonna etichetta
            var labelCell = new Border
            {
                Child = new TextBlock
                {
                    Text = riga.Categoria,
                    FontWeight = isSaldoFinale ? FontWeights.Bold : FontWeights.SemiBold,
                    Padding = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                },
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1)
            };
            ((TextBlock)labelCell.Child).Foreground = Brushes.White;
            
            Grid.SetRow(labelCell, rowIndex);
            Grid.SetColumn(labelCell, 0);
            PivotGrid.Children.Add(labelCell);
            
            // Colonne valori
            for (int i = 0; i < mesi.Count; i++)
            {
                string mese = mesi[i];
                decimal valore = riga.ValoriMensili.ContainsKey(mese) ? riga.ValoriMensili[mese] : 0;
                
                var valueCell = new Border
                {
                    Child = new TextBlock
                    {
                        Text = $"{valore:N2} €",
                        FontWeight = isSaldoFinale ? FontWeights.Bold : FontWeights.Normal,
                        TextAlignment = TextAlignment.Right,
                        Padding = new Thickness(10),
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Background = isSaldoFinale && valore < 0 
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")) // Arancione chiaro se negativo
                        : Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1)
                };
                
                // Colora il testo in base al valore
                if (isSaldoFinale)
                {
                    ((TextBlock)valueCell.Child).Foreground = valore < 0 ? Brushes.Red : Brushes.Green;
                }
                else
                {
                    ((TextBlock)valueCell.Child).Foreground = Brushes.Black;
                }
                
                Grid.SetRow(valueCell, rowIndex);
                Grid.SetColumn(valueCell, i + 1);
                PivotGrid.Children.Add(valueCell);
            }
        }

        private void AddPivotRowWithDescription(int rowIndex, SaldoPivotRiga riga, System.Collections.Generic.List<string> mesi, string colorHex, bool isSaldoFinale, bool isSaldoCorrente)
        {
            // Colonna etichetta
            var labelCell = new Border
            {
                Child = new TextBlock
                {
                    Text = riga.Categoria,
                    FontWeight = FontWeights.SemiBold,
                    Padding = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                },
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1)
            };
            ((TextBlock)labelCell.Child).Foreground = Brushes.White;
            
            Grid.SetRow(labelCell, rowIndex);
            Grid.SetColumn(labelCell, 0);
            PivotGrid.Children.Add(labelCell);
            
            // Colonne valori con descrizioni
            for (int i = 0; i < mesi.Count; i++)
            {
                string mese = mesi[i];
                decimal valore = riga.ValoriMensili.ContainsKey(mese) ? riga.ValoriMensili[mese] : 0;
                string descrizione = riga.DescrizioniMensili.ContainsKey(mese) ? riga.DescrizioniMensili[mese] : string.Empty;
                
                // StackPanel per valore e descrizione
                var stackPanel = new System.Windows.Controls.StackPanel();
                
                var valueText = new TextBlock
                {
                    Text = $"{valore:N2} €",
                    FontWeight = FontWeights.Normal,
                    TextAlignment = TextAlignment.Right,
                    Foreground = Brushes.Black
                };
                stackPanel.Children.Add(valueText);
                
                // Aggiungi descrizione se presente
                if (!string.IsNullOrWhiteSpace(descrizione))
                {
                    var descText = new TextBlock
                    {
                        Text = descrizione,
                        FontSize = 10,
                        FontStyle = FontStyles.Italic,
                        TextAlignment = TextAlignment.Right,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 2, 0, 0)
                    };
                    stackPanel.Children.Add(descText);
                }
                
                var valueCell = new Border
                {
                    Child = stackPanel,
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1)
                };
                
                Grid.SetRow(valueCell, rowIndex);
                Grid.SetColumn(valueCell, i + 1);
                PivotGrid.Children.Add(valueCell);
            }
        }

        private void AddPivotRowWithAccordion(int rowIndex, SaldoPivotRiga riga, System.Collections.Generic.List<string> mesi, string colorHex, bool isSaldoFinale)
        {
            // Colonna etichetta
            var labelCell = new Border
            {
                Child = new TextBlock
                {
                    Text = riga.Categoria,
                    FontWeight = FontWeights.SemiBold,
                    Padding = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center
                },
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1)
            };
            ((TextBlock)labelCell.Child).Foreground = Brushes.White;
            
            Grid.SetRow(labelCell, rowIndex);
            Grid.SetColumn(labelCell, 0);
            PivotGrid.Children.Add(labelCell);
            
            // Colonne valori con accordion
            for (int i = 0; i < mesi.Count; i++)
            {
                string mese = mesi[i];
                decimal valore = riga.ValoriMensili.ContainsKey(mese) ? riga.ValoriMensili[mese] : 0;
                var dettagli = riga.DettagliMensili.ContainsKey(mese) ? riga.DettagliMensili[mese] : new System.Collections.Generic.List<MovimentoMensileDettaglio>();
                
                // StackPanel principale
                var mainStackPanel = new System.Windows.Controls.StackPanel();
                
                // Riga con totale e pulsante expand/collapse
                var headerGrid = new System.Windows.Controls.Grid();
                headerGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = GridLength.Auto });
                
                var totalText = new TextBlock
                {
                    Text = $"{valore:N2} €",
                    FontWeight = FontWeights.Normal,
                    TextAlignment = TextAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                };
                System.Windows.Controls.Grid.SetColumn(totalText, 0);
                headerGrid.Children.Add(totalText);
                
                // Pulsante expand/collapse (solo se ci sono dettagli)
                if (dettagli.Count > 0)
                {
                    var expandButton = new System.Windows.Controls.Button
                    {
                        Content = "▼",
                        FontSize = 10,
                        Padding = new Thickness(5, 2, 5, 2),
                        Margin = new Thickness(5, 0, 0, 0),
                        Background = Brushes.LightGray,
                        BorderThickness = new Thickness(0),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        ToolTip = "Mostra dettagli"
                    };
                    System.Windows.Controls.Grid.SetColumn(expandButton, 1);
                    headerGrid.Children.Add(expandButton);
                    
                    // StackPanel per i dettagli (inizialmente nascosto)
                    var detailsStackPanel = new System.Windows.Controls.StackPanel
                    {
                        Margin = new Thickness(0, 5, 0, 0),
                        Visibility = Visibility.Collapsed
                    };
                    
                    // Raggruppa i dettagli per tipo
                    var anticipi = dettagli.Where(d => d.Descrizione.StartsWith("Anticipo ")).ToList();
                    var incassiNormali = dettagli.Where(d => !d.Descrizione.StartsWith("Anticipo ") && !d.Descrizione.StartsWith("Storno anticipo")).ToList();
                    var storni = dettagli.Where(d => d.Descrizione.StartsWith("Storno anticipo")).ToList();
                    var pagamentiNormali = dettagli.Where(d => !d.Descrizione.StartsWith("Anticipo ") && !d.Descrizione.StartsWith("Storno anticipo")).ToList();
                    
                    // Determina se siamo nella riga Incassi o Pagamenti
                    bool isIncassi = riga.Categoria == "Incassi";
                    
                    // SEZIONE ANTICIPI (solo per Incassi)
                    if (isIncassi && anticipi.Count > 0)
                    {
                        // Intestazione "ANTICIPI"
                        var anticipiHeader = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4EDDA")),
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, 2, 0, 0)
                        };
                        var anticipiHeaderText = new TextBlock
                        {
                            Text = $"📈 ANTICIPI ({anticipi.Count})",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                        };
                        anticipiHeader.Child = anticipiHeaderText;
                        detailsStackPanel.Children.Add(anticipiHeader);
                        
                        // Lista anticipi
                        foreach (var dettaglio in anticipi)
                        {
                            var detailBorder = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9")),
                                Padding = new Thickness(8, 5, 5, 5),
                                Margin = new Thickness(5, 2, 0, 0),
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(0, 1, 0, 0)
                            };
                            
                            var detailStack = new System.Windows.Controls.StackPanel();
                            
                            var descText = new TextBlock
                            {
                                Text = dettaglio.Descrizione,
                                FontSize = 11,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = Brushes.Black
                            };
                            detailStack.Children.Add(descText);
                            
                            var importoText = new TextBlock
                            {
                                Text = $"{dettaglio.Importo:N2} €",
                                FontSize = 11,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"))
                            };
                            detailStack.Children.Add(importoText);
                        
                        // Info fattura (se presente)
                        if (!string.IsNullOrWhiteSpace(dettaglio.NumeroFattura))
                        {
                            string fatturaInfo = $"Fatt. {dettaglio.NumeroFattura}";
                            if (dettaglio.DataFattura.HasValue)
                            {
                                fatturaInfo += $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}";
                            }
                            
                            var fatturaText = new TextBlock
                            {
                                Text = fatturaInfo,
                                FontSize = 10,
                                FontStyle = FontStyles.Italic,
                                Foreground = Brushes.Gray
                            };
                            detailStack.Children.Add(fatturaText);
                        }
                        
                        // Data scadenza
                        var scadenzaText = new TextBlock
                        {
                            Text = $"Scad. {dettaglio.DataScadenza:dd/MM/yyyy}",
                            FontSize = 10,
                            Foreground = Brushes.Gray
                        };
                        detailStack.Children.Add(scadenzaText);
                        
                            detailBorder.Child = detailStack;
                            detailsStackPanel.Children.Add(detailBorder);
                        }
                    }
                    
                    // SEZIONE INCASSI NORMALI (solo per Incassi)
                    if (isIncassi && incassiNormali.Count > 0)
                    {
                        // Intestazione "INCASSI"
                        var incassiHeader = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3E5FC")),
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, 5, 0, 0)
                        };
                        var incassiHeaderText = new TextBlock
                        {
                            Text = $"💰 INCASSI ({incassiNormali.Count})",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0277BD"))
                        };
                        incassiHeader.Child = incassiHeaderText;
                        detailsStackPanel.Children.Add(incassiHeader);
                        
                        // Lista incassi
                        foreach (var dettaglio in incassiNormali)
                        {
                            var detailBorder = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E1F5FE")),
                                Padding = new Thickness(8, 5, 5, 5),
                                Margin = new Thickness(5, 2, 0, 0),
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(0, 1, 0, 0)
                            };
                            
                            var detailStack = new System.Windows.Controls.StackPanel();
                            
                            var descText = new TextBlock
                            {
                                Text = dettaglio.Descrizione,
                                FontSize = 11,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = Brushes.Black
                            };
                            detailStack.Children.Add(descText);
                            
                            var importoText = new TextBlock
                            {
                                Text = $"{dettaglio.Importo:N2} €",
                                FontSize = 11,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0277BD"))
                            };
                            detailStack.Children.Add(importoText);
                        
                            if (!string.IsNullOrWhiteSpace(dettaglio.NumeroFattura))
                            {
                                string fatturaInfo = $"Fatt. {dettaglio.NumeroFattura}";
                                if (dettaglio.DataFattura.HasValue)
                                {
                                    fatturaInfo += $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}";
                                }
                                
                                var fatturaText = new TextBlock
                                {
                                    Text = fatturaInfo,
                                    FontSize = 10,
                                    FontStyle = FontStyles.Italic,
                                    Foreground = Brushes.Gray
                                };
                                detailStack.Children.Add(fatturaText);
                            }
                            
                            var scadenzaText = new TextBlock
                            {
                                Text = $"Scad. {dettaglio.DataScadenza:dd/MM/yyyy}",
                                FontSize = 10,
                                Foreground = Brushes.Gray
                            };
                            detailStack.Children.Add(scadenzaText);
                            
                            detailBorder.Child = detailStack;
                            detailsStackPanel.Children.Add(detailBorder);
                        }
                    }
                    
                    // SEZIONE STORNI ANTICIPO (solo per Pagamenti)
                    if (!isIncassi && storni.Count > 0)
                    {
                        // Intestazione "STORNI ANTICIPO"
                        var storniHeader = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE4B5")),
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, 2, 0, 0)
                        };
                        var storniHeaderText = new TextBlock
                        {
                            Text = $"🔄 STORNI ANTICIPO ({storni.Count})",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F57C00"))
                        };
                        storniHeader.Child = storniHeaderText;
                        detailsStackPanel.Children.Add(storniHeader);
                        
                        // Lista storni
                        foreach (var dettaglio in storni)
                        {
                            var detailBorder = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0")),
                                Padding = new Thickness(8, 5, 5, 5),
                                Margin = new Thickness(5, 2, 0, 0),
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(0, 1, 0, 0)
                            };
                            
                            var detailStack = new System.Windows.Controls.StackPanel();
                            
                            var descText = new TextBlock
                            {
                                Text = dettaglio.Descrizione,
                                FontSize = 11,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = Brushes.Black
                            };
                            detailStack.Children.Add(descText);
                            
                            var importoText = new TextBlock
                            {
                                Text = $"{dettaglio.Importo:N2} €",
                                FontSize = 11,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F57C00"))
                            };
                            detailStack.Children.Add(importoText);
                        
                            if (!string.IsNullOrWhiteSpace(dettaglio.NumeroFattura))
                            {
                                string fatturaInfo = $"Fatt. {dettaglio.NumeroFattura}";
                                if (dettaglio.DataFattura.HasValue)
                                {
                                    fatturaInfo += $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}";
                                }
                                
                                var fatturaText = new TextBlock
                                {
                                    Text = fatturaInfo,
                                    FontSize = 10,
                                    FontStyle = FontStyles.Italic,
                                    Foreground = Brushes.Gray
                                };
                                detailStack.Children.Add(fatturaText);
                            }
                            
                            var scadenzaText = new TextBlock
                            {
                                Text = $"Scad. {dettaglio.DataScadenza:dd/MM/yyyy}",
                                FontSize = 10,
                                Foreground = Brushes.Gray
                            };
                            detailStack.Children.Add(scadenzaText);
                            
                            detailBorder.Child = detailStack;
                            detailsStackPanel.Children.Add(detailBorder);
                        }
                    }
                    
                    // SEZIONE PAGAMENTI NORMALI (solo per Pagamenti)
                    if (!isIncassi && pagamentiNormali.Count > 0)
                    {
                        // Intestazione "PAGAMENTI"
                        var pagamentiHeader = new Border
                        {
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCDD2")),
                            Padding = new Thickness(5),
                            Margin = new Thickness(0, 5, 0, 0)
                        };
                        var pagamentiHeaderText = new TextBlock
                        {
                            Text = $"💳 PAGAMENTI ({pagamentiNormali.Count})",
                            FontSize = 11,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"))
                        };
                        pagamentiHeader.Child = pagamentiHeaderText;
                        detailsStackPanel.Children.Add(pagamentiHeader);
                        
                        // Lista pagamenti
                        foreach (var dettaglio in pagamentiNormali)
                        {
                            var detailBorder = new Border
                            {
                                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")),
                                Padding = new Thickness(8, 5, 5, 5),
                                Margin = new Thickness(5, 2, 0, 0),
                                BorderBrush = Brushes.LightGray,
                                BorderThickness = new Thickness(0, 1, 0, 0)
                            };
                            
                            var detailStack = new System.Windows.Controls.StackPanel();
                            
                            var descText = new TextBlock
                            {
                                Text = dettaglio.Descrizione,
                                FontSize = 11,
                                FontWeight = FontWeights.SemiBold,
                                Foreground = Brushes.Black
                            };
                            detailStack.Children.Add(descText);
                            
                            var importoText = new TextBlock
                            {
                                Text = $"{dettaglio.Importo:N2} €",
                                FontSize = 11,
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"))
                            };
                            detailStack.Children.Add(importoText);
                        
                            if (!string.IsNullOrWhiteSpace(dettaglio.NumeroFattura))
                            {
                                string fatturaInfo = $"Fatt. {dettaglio.NumeroFattura}";
                                if (dettaglio.DataFattura.HasValue)
                                {
                                    fatturaInfo += $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}";
                                }
                                
                                var fatturaText = new TextBlock
                                {
                                    Text = fatturaInfo,
                                    FontSize = 10,
                                    FontStyle = FontStyles.Italic,
                                    Foreground = Brushes.Gray
                                };
                                detailStack.Children.Add(fatturaText);
                            }
                            
                            var scadenzaText = new TextBlock
                            {
                                Text = $"Scad. {dettaglio.DataScadenza:dd/MM/yyyy}",
                                FontSize = 10,
                                Foreground = Brushes.Gray
                            };
                            detailStack.Children.Add(scadenzaText);
                            
                            detailBorder.Child = detailStack;
                            detailsStackPanel.Children.Add(detailBorder);
                        }
                    }
                    
                    mainStackPanel.Children.Add(headerGrid);
                    mainStackPanel.Children.Add(detailsStackPanel);
                    
                    // Event handler per expand/collapse
                    expandButton.Click += (s, e) =>
                    {
                        if (detailsStackPanel.Visibility == Visibility.Collapsed)
                        {
                            detailsStackPanel.Visibility = Visibility.Visible;
                            expandButton.Content = "▲";
                            expandButton.ToolTip = "Nascondi dettagli";
                        }
                        else
                        {
                            detailsStackPanel.Visibility = Visibility.Collapsed;
                            expandButton.Content = "▼";
                            expandButton.ToolTip = "Mostra dettagli";
                        }
                    };
                }
                else
                {
                    // Nessun dettaglio, mostra solo il totale
                    mainStackPanel.Children.Add(headerGrid);
                }
                
                var valueCell = new Border
                {
                    Child = mainStackPanel,
                    Padding = new Thickness(10),
                    Background = Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1)
                };
                
                System.Windows.Controls.Grid.SetRow(valueCell, rowIndex);
                System.Windows.Controls.Grid.SetColumn(valueCell, i + 1);
                PivotGrid.Children.Add(valueCell);
            }
        }

        private void IncassiGrid_CurrentCellChanged(object sender, System.EventArgs e)
        {
            // Entra automaticamente in modalità edit per le checkbox
            if (sender is DataGrid grid && grid.CurrentColumn != null)
            {
                var header = grid.CurrentColumn.Header?.ToString();
                if (header == "Incassato" || header == "OK C/C" || header == "OK C/C chiuso Ant")
                {
                    grid.BeginEdit();
                }
            }
        }

        private void IncassiGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // Salva l'incasso dopo che il binding è completato
                Dispatcher.InvokeAsync(async () =>
                {
                    // Aspetta che il binding sia completato
                    await System.Threading.Tasks.Task.Delay(100);
                    
                    if (DataContext is BancaDettaglioViewModel vm && e.Row.Item is Core.Models.BancaIncasso incasso)
                    {
                        System.Diagnostics.Debug.WriteLine($"Incasso modificato: {incasso.NomeCliente}, AnticipoGestito_CC: {incasso.AnticipoGestito_CC}, AnticipoChiuso_CC: {incasso.AnticipoChiuso_CC}");
                        
                        // Salva l'incasso modificato
                        var incassoRepo = new Core.Repositories.BancaIncassoRepository(App.GetService<Core.Data.LiteDbContext>()!);
                        incassoRepo.Update(incasso);
                        
                        // Ricarica i dati (il Saldo Previsto viene sempre aggiornato)
                        vm.LoadIncassi();
                        vm.LoadStatistiche();
                        vm.LoadAlerts();
                        vm.LoadMovimentiPrevisti();
                        
                        // Il Saldo Previsto Pivot si ricarica automaticamente quando l'utente apre quel tab
                        // grazie all'evento TabControl_SelectionChanged
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void PagamentiGrid_CurrentCellChanged(object sender, System.EventArgs e)
        {
            // Entra automaticamente in modalità edit per le checkbox
            if (sender is DataGrid grid && grid.CurrentColumn != null)
            {
                if (grid.CurrentColumn.Header?.ToString() == "Pagato")
                {
                    grid.BeginEdit();
                }
            }
        }

        private void PagamentiGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    // Aspetta che il binding sia completato
                    await System.Threading.Tasks.Task.Delay(100);
                    
                    if (DataContext is BancaDettaglioViewModel vm && e.Row.Item is Core.Models.BancaPagamento pagamento)
                    {
                        // Salva il pagamento modificato
                        var pagamentoRepo = new Core.Repositories.BancaPagamentoRepository(App.GetService<Core.Data.LiteDbContext>()!);
                        pagamentoRepo.Update(pagamento);
                        
                        // Ricarica i dati
                        vm.LoadPagamenti();
                        vm.LoadStatistiche();
                        vm.LoadAlerts();
                        vm.LoadMovimentiPrevisti();
                        // vm.LoadSaldoPivot(); // Caricato solo quando necessario
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}

