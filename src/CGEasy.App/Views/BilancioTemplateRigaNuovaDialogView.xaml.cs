using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioTemplateRigaNuovaDialogView : Window
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioTemplateRepository _repository;
    private readonly int _clienteId;
    private readonly int _mese;
    private readonly int _anno;
    private bool _isSaving = false;

    public BilancioTemplateRigaNuovaDialogView(int clienteId, int mese, int anno)
    {
        InitializeComponent();
        
        _context = ((App)Application.Current).Services!.GetRequiredService<CGEasyDbContext>();
        _repository = new BilancioTemplateRepository(_context);
        _clienteId = clienteId;
        _mese = mese;
        _anno = anno;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isSaving) return;
        _isSaving = true;

        // Validazione
        if (string.IsNullOrWhiteSpace(CodiceTextBox.Text))
        {
            _isSaving = false;
            MessageBox.Show("Il codice è obbligatorio", "Validazione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(DescrizioneTextBox.Text))
        {
            _isSaving = false;
            MessageBox.Show("La descrizione è obbligatoria", "Validazione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(ImportoTextBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal importoDecimal))
        {
            if (!decimal.TryParse(ImportoTextBox.Text, NumberStyles.Any, CultureInfo.GetCultureInfo("it-IT"), out importoDecimal))
            {
                _isSaving = false;
                MessageBox.Show("Importo non valido", "Validazione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        try
        {
            // Verifica se esiste già una riga con questo codice
            var templateEsistenti = _repository.GetByClienteAndPeriodo(_clienteId, _mese, _anno).ToList();
            var codiceEsistente = templateEsistenti.FirstOrDefault(b => 
                b.CodiceMastrino.Equals(CodiceTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            
            if (codiceEsistente != null)
            {
                _isSaving = false;
                MessageBox.Show($"❌ Codice '{CodiceTextBox.Text.Trim()}' già esistente!\n\nIl codice è già utilizzato per:\n{codiceEsistente.DescrizioneMastrino}", 
                    "Codice Duplicato", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }
            
            var primoTemplate = templateEsistenti.FirstOrDefault();

            var nuovaRiga = new BilancioTemplate
            {
                ClienteId = _clienteId,
                ClienteNome = primoTemplate?.ClienteNome ?? "",
                Mese = _mese,
                Anno = _anno,
                DescrizioneBilancio = primoTemplate?.DescrizioneBilancio,
                CodiceMastrino = CodiceTextBox.Text.Trim(),
                DescrizioneMastrino = DescrizioneTextBox.Text.Trim(),
                Importo = importoDecimal,
                DataImport = DateTime.Now,
                ImportedBy = SessionManager.CurrentUser?.Id ?? 0,
                ImportedByName = SessionManager.CurrentUser?.NomeCompleto ?? "Sistema"
            };

            _repository.Insert(nuovaRiga);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            _isSaving = false;
            MessageBox.Show($"Errore:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

