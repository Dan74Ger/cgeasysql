using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioTemplateRigaEditDialogView : Window
{
    private readonly LiteDbContext _context;
    private readonly BilancioTemplateRepository _repository;
    private readonly BilancioTemplate _riga;
    private bool _isSaving = false;

    public BilancioTemplateRigaEditDialogView(BilancioTemplate riga)
    {
        InitializeComponent();
        
        _context = ((App)Application.Current).Services!.GetRequiredService<LiteDbContext>();
        _repository = new BilancioTemplateRepository(_context);
        _riga = riga;

        // Precompila i campi
        CodiceTextBox.Text = riga.CodiceMastrino;
        DescrizioneTextBox.Text = riga.DescrizioneMastrino;
        ImportoTextBox.Text = riga.Importo.ToString("F2", CultureInfo.InvariantCulture);
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
            // Verifica se il nuovo codice esiste già (escludendo la riga corrente)
            var templateEsistenti = _repository.GetByClienteAndPeriodo(_riga.ClienteId, _riga.Mese, _riga.Anno).ToList();
            var codiceEsistente = templateEsistenti.FirstOrDefault(b => 
                b.Id != _riga.Id && 
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

            // Aggiorna i campi
            _riga.CodiceMastrino = CodiceTextBox.Text.Trim();
            _riga.DescrizioneMastrino = DescrizioneTextBox.Text.Trim();
            _riga.Importo = importoDecimal;

            _repository.Update(_riga);
            
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

