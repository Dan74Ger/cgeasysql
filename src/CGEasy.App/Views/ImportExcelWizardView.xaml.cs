using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class ImportExcelWizardView : Window
{
    private readonly CGEasyDbContext? _context;
    private readonly BilancioContabileRepository? _repository;
    private readonly ClienteRepository? _clienteRepository;
    
    private string? _selectedFilePath;
    private bool _isImporting = false; // ⚠️ FLAG PER PREVENIRE DOPPIO CLICK
    public List<BilancioContabile>? BilanciImportati { get; private set; }

    public ImportExcelWizardView()
    {
        InitializeComponent();
        
        try
        {
            _context = ((App)Application.Current).Services!.GetRequiredService<CGEasyDbContext>();
            _repository = new BilancioContabileRepository(_context);
            _clienteRepository = new ClienteRepository(_context);

            // ✅ Carica immediatamente i dati
            LoadClienti();
            
            // ✅ Precompila mese e anno corrente
            MeseComboBox.SelectedIndex = DateTime.Now.Month - 1;
            AnnoTextBox.Text = DateTime.Now.Year.ToString();
            
            // ✅ Pulisci campi
            _selectedFilePath = null;
            FilePathTextBox.Text = string.Empty;
            FileSizeTextBlock.Visibility = Visibility.Collapsed;
            DescrizioneTextBox.Text = string.Empty;
            
            // ✅ EVENTO LOADED: forza reset completo quando la finestra viene mostrata
            this.Loaded += (s, e) =>
            {
                ForzaAbilitazioneControlli();
            };
            
            // ✅ EVENTO ACTIVATED: forza reset quando la finestra prende focus
            this.Activated += (s, e) =>
            {
                ForzaAbilitazioneControlli();
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore inizializzazione:\n{ex.Message}", "Errore", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }
    
    /// <summary>
    /// ✅ Metodo centralizzato per forzare l'abilitazione di TUTTI i controlli
    /// Questo risolve il bug WPF dove OpenFileDialog lascia i controlli disabilitati
    /// </summary>
    private void ForzaAbilitazioneControlli()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                ClienteComboBox.IsEnabled = true;
                MeseComboBox.IsEnabled = true;
                AnnoTextBox.IsEnabled = true;
                DescrizioneTextBox.IsEnabled = true;
                SelectFileButton.IsEnabled = true;
                ImportButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                FilePathTextBox.IsEnabled = true;
                
                // Forza focus sul primo controllo
                if (ClienteComboBox.SelectedItem == null)
                {
                    ClienteComboBox.Focus();
                }
            }
            catch
            {
                // Ignora errori (es: controlli non ancora caricati)
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void LoadClienti()
    {
        try
        {
            var clienti = _clienteRepository?.GetAll()
                .Where(c => c.Attivo)
                .OrderBy(c => c.NomeCliente)
                .ToList();
            
            if (clienti != null)
            {
                ClienteComboBox.ItemsSource = clienti;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento clienti:\n{ex.Message}", "Errore", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Seleziona file Excel",
                CheckFileExists = true,
                CheckPathExists = true
            };

            var result = openDialog.ShowDialog(this);
            
            // ✅ USA il metodo centralizzato per riabilitare i controlli
            ForzaAbilitazioneControlli();
            
            if (result == true)
            {
                _selectedFilePath = openDialog.FileName;
                FilePathTextBox.Text = _selectedFilePath;
                
                // Mostra info file
                var fileInfo = new FileInfo(_selectedFilePath);
                FileSizeTextBlock.Text = $"📏 Dimensione: {fileInfo.Length / 1024:N0} KB";
                FileSizeTextBlock.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore selezione file:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        // ⚠️ PREVIENI DOPPIO CLICK - SE GIÀ IN IMPORT, IGNORA
        if (_isImporting)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ❌❌❌ DOPPIO CLICK RILEVATO! Import già in corso, IGNORO questo click!");
            return;
        }
        
        _isImporting = true;
        ImportButton.IsEnabled = false; // Disabilita il pulsante
        
        // 🐛 DEBUG: Traccia ogni click
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        System.Diagnostics.Debug.WriteLine($"[{timestamp}] ImportButton_Click - INIZIO (Flag _isImporting impostato a TRUE)");
        
        // Validazioni
        if (string.IsNullOrEmpty(_selectedFilePath))
        {
            MessageBox.Show("Seleziona un file Excel", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _isImporting = false;
            ImportButton.IsEnabled = true;
            return;
        }

        if (ClienteComboBox.SelectedItem == null)
        {
            MessageBox.Show("Seleziona un cliente", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _isImporting = false;
            ImportButton.IsEnabled = true;
            return;
        }

        if (MeseComboBox.SelectedIndex < 0)
        {
            MessageBox.Show("Seleziona un mese", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _isImporting = false;
            ImportButton.IsEnabled = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(AnnoTextBox.Text))
        {
            MessageBox.Show("Inserisci un anno", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _isImporting = false;
            ImportButton.IsEnabled = true;
            return;
        }

        if (!int.TryParse(AnnoTextBox.Text, out int anno) || anno < 2000 || anno > 2100)
        {
            MessageBox.Show("Anno non valido (deve essere tra 2000 e 2100)", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            _isImporting = false;
            ImportButton.IsEnabled = true;
            return;
        }

        try
        {
            var cliente = (Cliente)ClienteComboBox.SelectedItem;
            var descrizione = DescrizioneTextBox.Text?.Trim();
            var mese = MeseComboBox.SelectedIndex + 1;
            // anno è già dichiarato sopra nel TryParse
            
            // Leggi tipo bilancio (CE o SP)
            var tipoBilancio = "CE"; // Default
            if (TipoBilancioComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                tipoBilancio = selectedItem.Tag?.ToString() ?? "CE";
            }
            
            var currentUser = SessionManager.CurrentUser;

            if (currentUser == null)
            {
                MessageBox.Show("Sessione scaduta", "Errore", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _isImporting = false;
                ImportButton.IsEnabled = true;
                return;
            }

            // Import Excel
            var bilanci = ExcelBilancioService.ImportFromExcel(
                _selectedFilePath,
                cliente.Id,
                cliente.NomeCliente,
                descrizione,
                mese,
                anno,
                currentUser.Id,
                currentUser.NomeCompleto,
                tipoBilancio
            );

            if (bilanci.Count == 0)
            {
                MessageBox.Show("Nessuna riga valida trovata nel file", "Attenzione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                _isImporting = false;
                ImportButton.IsEnabled = true;
                return;
            }

            // 🎯 Controlla se esiste GIÀ un bilancio con STESSA descrizione
            var existing = _repository?.GetByClienteAndPeriodoAndDescrizione(cliente.Id, mese, anno, descrizione);
            
            // 🐛 DEBUG: Mostra cosa ha trovato
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] [IMPORT DEBUG] ========================================");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] Cliente: {cliente.NomeCliente} (ID: {cliente.Id})");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] Periodo: {mese}/{anno}");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] Descrizione: '{descrizione}' (Length: {descrizione?.Length ?? 0})");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] Trovate {existing?.Count ?? 0} righe esistenti con stessa descrizione");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] Righe da importare: {bilanci.Count}");
            
            if (existing != null && existing.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] Descrizioni esistenti nel DB:");
                foreach (var ex in existing.Take(3))
                {
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}]   - ID:{ex.Id}, Codice:{ex.CodiceMastrino}, Desc:'{ex.DescrizioneBilancio}', Length:{ex.DescrizioneBilancio?.Length ?? 0}");
                }
            }
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] ========================================");
            
            if (existing != null && existing.Count > 0)
            {
                // Trovato bilancio con descrizione identica → Chiedi se sostituire
                var result = MessageBox.Show(
                    $"Esiste già un bilancio per {cliente.NomeCliente} - {mese:00}/{anno}\n" +
                    $"con descrizione '{descrizione}' ({existing.Count} righe).\n\n" +
                    "Vuoi eliminarlo e sostituirlo con il nuovo?",
                    "Conferma Sostituzione",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.Yes && _repository != null)
                {
                    // Elimina usando il nuovo metodo che considera la descrizione
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}] ImportButton_Click - Inizio cancellazione bilancio esistente");
                    _repository.DeleteByClienteAndPeriodoAndDescrizione(cliente.Id, mese, anno, descrizione);
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}] ImportButton_Click - Cancellazione completata, attendo 300ms prima di importare");
                    
                    // ⚠️ IMPORTANTE: In modalità Shared, attendi che la cancellazione sia completata
                    System.Threading.Thread.Sleep(300);
                }
                else if (result == MessageBoxResult.No)
                {
                    // Annulla import
                    return;
                }
            }
            // Se descrizione diversa, procede con import senza chiedere nulla

            // Salva nel database
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] ImportButton_Click - Inserimento {bilanci.Count} righe nel database");
            if (_repository != null)
            {
                _repository.InsertBulk(bilanci);
            }
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] ImportButton_Click - Inserimento completato");
            
            // 🐛 DEBUG: Verifica duplicati SUBITO dopo l'inserimento
            if (_repository != null)
            {
                var duplicati = _repository.VerificaDuplicati(cliente.Id, mese, anno, descrizione);
                if (duplicati.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}] ❌ ATTENZIONE: Trovati {duplicati.Count} codici duplicati nel DB!");
                    // MessageBox rimosso temporaneamente per debug
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[{timestamp}] ✅ Nessun duplicato trovato nel DB");
                }
            }
            
            BilanciImportati = bilanci;

            // ✅ Reset completo prima di chiudere
            _selectedFilePath = null;
            FilePathTextBox.Text = string.Empty;
            FileSizeTextBlock.Visibility = Visibility.Collapsed;
            DescrizioneTextBox.Text = string.Empty;
            ClienteComboBox.SelectedItem = null;

            System.Diagnostics.Debug.WriteLine($"[{timestamp}] ImportButton_Click - FINE, chiudo dialog");
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ImportButton_Click - ERRORE: {ex.Message}");
            MessageBox.Show($"Errore durante import:\n\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // ⚠️ RESET FLAG E RIABILITA PULSANTE
            _isImporting = false;
            ImportButton.IsEnabled = true;
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ImportButton_Click - Flag _isImporting resettato a FALSE");
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

