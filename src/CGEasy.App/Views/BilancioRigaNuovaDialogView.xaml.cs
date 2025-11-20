using System;
using System.Globalization;
using System.Windows;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioRigaNuovaDialogView : Window
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioContabileRepository _repository;
    private readonly int _clienteId;
    private readonly int _mese;
    private readonly int _anno;
    private bool _isSaving = false;  // Flag per evitare doppio salvataggio

    public BilancioRigaNuovaDialogView(int clienteId, int mese, int anno)
    {
        InitializeComponent();
        
        _context = ((App)Application.Current).Services!.GetRequiredService<CGEasyDbContext>();
        _repository = new BilancioContabileRepository(_context);
        _clienteId = clienteId;
        _mese = mese;
        _anno = anno;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Previeni doppio click
        if (_isSaving)
        {
            return;
        }
        
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
            // Prova con cultura italiana
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
            var bilanciEsistenti = _repository.GetByClienteAndPeriodo(_clienteId, _mese, _anno).ToList();
            var codiceEsistente = bilanciEsistenti.FirstOrDefault(b => 
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
            
            var primoBilancio = bilanciEsistenti.FirstOrDefault();

            var nuovaRiga = new BilancioContabile
            {
                ClienteId = _clienteId,
                ClienteNome = primoBilancio?.ClienteNome ?? "",
                Mese = _mese,
                Anno = _anno,
                DescrizioneBilancio = primoBilancio?.DescrizioneBilancio,
                CodiceMastrino = CodiceTextBox.Text.Trim(),
                DescrizioneMastrino = DescrizioneTextBox.Text.Trim(),
                Importo = importoDecimal,
                DataImport = DateTime.Now,
                ImportedBy = SessionManager.CurrentUser?.Id ?? 0,
                ImportedByName = SessionManager.CurrentUser?.NomeCompleto ?? "Sistema"
            };

            var newId = _repository.Insert(nuovaRiga);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            _isSaving = false;  // Reset flag in caso di errore
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


