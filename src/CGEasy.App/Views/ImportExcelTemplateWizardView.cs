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

public partial class ImportExcelTemplateWizardView : Window
{
    private readonly CGEasyDbContext? _context;
    private readonly BilancioTemplateRepository? _repository;
    private readonly ClienteRepository? _clienteRepository;
    
    private string? _selectedFilePath;
    public List<BilancioTemplate>? BilanciImportati { get; private set; }

    public ImportExcelTemplateWizardView()
    {
        InitializeComponent();
        
        try
        {
            _context = ((App)Application.Current).Services!.GetRequiredService<CGEasyDbContext>();
            _repository = new BilancioTemplateRepository(_context);
            _clienteRepository = new ClienteRepository(_context);

            // Carica immediatamente i dati
            LoadClienti();
            
            // Precompila mese e anno corrente
            MeseComboBox.SelectedIndex = DateTime.Now.Month - 1;
            AnnoTextBox.Text = DateTime.Now.Year.ToString();
            
            // ✅ WORKAROUND: Riabilita tutti i controlli quando la finestra riottiene il focus
            this.Activated += (s, e) =>
            {
                ClienteComboBox.IsEnabled = true;
                MeseComboBox.IsEnabled = true;
                AnnoTextBox.IsEnabled = true;
                DescrizioneTextBox.IsEnabled = true;
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore inizializzazione:\n{ex.Message}", "Errore", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private void LoadClienti()
    {
        try
        {
            var clienti = _clienteRepository!.GetAll()
                .Where(c => c.Attivo)
                .OrderBy(c => c.NomeCliente)
                .ToList();
            
            ClienteComboBox.ItemsSource = clienti;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento clienti:\n{ex.Message}", "Errore", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SelectFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
            Title = "Seleziona file Excel Template"
        };

        var dialogResult = openDialog.ShowDialog();
        
        // ✅ FORZA riabilitazione DOPO che il dialog si chiude usando Dispatcher
        Dispatcher.BeginInvoke(new Action(() =>
        {
            ClienteComboBox.IsEnabled = true;
            MeseComboBox.IsEnabled = true;
            AnnoTextBox.IsEnabled = true;
            DescrizioneTextBox.IsEnabled = true;
            
            if (dialogResult == true)
            {
                _selectedFilePath = openDialog.FileName;
                FilePathTextBox.Text = _selectedFilePath;
                
                // Mostra info file
                var fileInfo = new FileInfo(_selectedFilePath);
                FileSizeTextBlock.Text = $"📏 Dimensione: {fileInfo.Length / 1024:N0} KB";
                FileSizeTextBlock.Visibility = Visibility.Visible;
            }
        }), System.Windows.Threading.DispatcherPriority.ContextIdle);
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        // Validazioni
        if (string.IsNullOrEmpty(_selectedFilePath))
        {
            MessageBox.Show("Seleziona un file Excel", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (ClienteComboBox.SelectedItem == null)
        {
            MessageBox.Show("Seleziona un cliente", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (MeseComboBox.SelectedIndex < 0)
        {
            MessageBox.Show("Seleziona un mese", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(AnnoTextBox.Text))
        {
            MessageBox.Show("Inserisci un anno", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(AnnoTextBox.Text, out int anno) || anno < 2000 || anno > 2100)
        {
            MessageBox.Show("Anno non valido (deve essere tra 2000 e 2100)", "Validazione", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var cliente = (Cliente)ClienteComboBox.SelectedItem;
            var descrizione = DescrizioneTextBox.Text?.Trim();
            var mese = MeseComboBox.SelectedIndex + 1;
            // anno è già dichiarato sopra nel TryParse
            
            // Leggi tipo bilancio
            var tipoBilancio = "CE"; // Default
            if (TipoBilancioTemplateComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                tipoBilancio = selectedItem.Tag?.ToString() ?? "CE";
            }
            
            var currentUser = SessionManager.CurrentUser;

            if (currentUser == null)
            {
                MessageBox.Show("Sessione scaduta", "Errore", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Import Excel (restituisce BilancioContabile)
            var bilanciContabili = ExcelBilancioService.ImportFromExcel(
                _selectedFilePath,
                cliente.Id,
                cliente.NomeCliente,
                descrizione,
                mese,
                anno,
                currentUser.Id,
                currentUser.NomeCompleto
            );

            if (bilanciContabili.Count == 0)
            {
                MessageBox.Show("Nessuna riga valida trovata nel file", "Attenzione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔥 CONVERTI BilancioContabile → BilancioTemplate
            var bilanciTemplate = bilanciContabili.Select(bc => new BilancioTemplate
            {
                // ⚠️ NON specificare Id - LiteDB lo assegna automaticamente con auto-increment
                ClienteId = bc.ClienteId,
                ClienteNome = bc.ClienteNome,
                Mese = bc.Mese,
                Anno = bc.Anno,
                CodiceMastrino = bc.CodiceMastrino,
                DescrizioneMastrino = bc.DescrizioneMastrino,
                Importo = bc.Importo,
                DescrizioneBilancio = bc.DescrizioneBilancio,
                TipoBilancio = tipoBilancio, // ⭐ Assegna tipo bilancio
                DataImport = bc.DataImport,
                ImportedBy = bc.ImportedBy,
                ImportedByName = bc.ImportedByName,
                Note = bc.Note
            }).ToList();
            
            // 🔍 DEBUG: Verifica che gli ID siano 0 (non assegnati)
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Righe da inserire: {bilanciTemplate.Count}");
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Primi 3 ID prima dell'insert: {string.Join(", ", bilanciTemplate.Take(3).Select(b => b.Id))}");

            // ✅ Controlla se esistono template con STESSA descrizione (non solo cliente/periodo)
            var existing = _repository!.GetByClienteAndPeriodoAndDescrizione(cliente.Id, mese, anno, descrizione);
            
            // 🔍 DEBUG: Log per capire cosa trova
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Cliente={cliente.NomeCliente}, Periodo={mese}/{anno}, Descrizione='{descrizione ?? "(vuota)"}'");
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Template esistenti trovati con STESSA descrizione: {existing.Count}");
            if (existing.Any())
            {
                foreach (var t in existing.Take(3))
                {
                    System.Diagnostics.Debug.WriteLine($"  - ID={t.Id}, Codice={t.CodiceMastrino}, Desc='{t.DescrizioneBilancio ?? "(vuota)"}'");
                }
            }
            
            if (existing.Count > 0)
            {
                var descDisplay = string.IsNullOrWhiteSpace(descrizione) ? "(nessuna descrizione)" : $"'{descrizione}'";
                var result = MessageBox.Show(
                    $"⚠️ Esiste già un template per:\n\n" +
                    $"Cliente: {cliente.NomeCliente}\n" +
                    $"Periodo: {mese:00}/{anno}\n" +
                    $"Descrizione: {descDisplay}\n" +
                    $"Righe esistenti: {existing.Count}\n\n" +
                    $"Vuoi ELIMINARE il template esistente e SOSTITUIRLO con il nuovo?\n\n" +
                    $"⚠️ Clicca 'No' per annullare l'importazione.",
                    "Conferma Sostituzione",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    // ❌ Utente ha scelto NO → Annulla importazione
                    System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Importazione ANNULLATA dall'utente");
                    return;
                }

                // ✅ Utente ha scelto YES → Elimina il vecchio
                System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Eliminazione template esistente...");
                var deleted = _repository.DeleteByClienteAndPeriodoAndDescrizione(cliente.Id, mese, anno, descrizione);
                System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Eliminate {deleted} righe");
            }

            // Salva nel database (collection bilancio_template)
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Inserimento {bilanciTemplate.Count} nuove righe...");
            _repository.InsertBulk(bilanciTemplate);
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] Primi 3 ID DOPO l'insert: {string.Join(", ", bilanciTemplate.Take(3).Select(b => b.Id))}");
            System.Diagnostics.Debug.WriteLine($"[IMPORT TEMPLATE] ✅ Import completato!");
            BilanciImportati = bilanciTemplate;

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante import:\n\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
