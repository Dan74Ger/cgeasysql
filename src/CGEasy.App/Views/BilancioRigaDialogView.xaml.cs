using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using CGEasy.Core.Data;
using CGEasy.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioRigaDialogView : Window
{
    private readonly LiteDbContext _context;
    private readonly BilancioContabileRepository _repository;
    private readonly int _rigaId;
    private bool _isSaving = false;  // Flag per evitare doppio salvataggio

    public string Codice { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty;
    public string Importo { get; set; } = string.Empty;

    public BilancioRigaDialogView(int rigaId)
    {
        InitializeComponent();
        
        _context = ((App)Application.Current).Services!.GetRequiredService<LiteDbContext>();
        _repository = new BilancioContabileRepository(_context);
        _rigaId = rigaId;

        LoadData();
        DataContext = this;  // DataContext DOPO aver caricato i dati
    }

    private void LoadData()
    {
        var riga = _repository.GetById(_rigaId);
        if (riga != null)
        {
            Codice = riga.CodiceMastrino;
            Descrizione = riga.DescrizioneMastrino;
            Importo = riga.Importo.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Previeni doppio click
        if (_isSaving) return;
        _isSaving = true;

        // Leggi i valori direttamente dai TextBox
        var codice = CodiceTextBox.Text;
        var descrizione = DescrizioneTextBox.Text;
        var importoStr = ImportoTextBox.Text;

        // Validazione
        if (string.IsNullOrWhiteSpace(codice))
        {
            _isSaving = false;
            MessageBox.Show("Il codice è obbligatorio", "Validazione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(descrizione))
        {
            _isSaving = false;
            MessageBox.Show("La descrizione è obbligatoria", "Validazione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!decimal.TryParse(importoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal importoDecimal))
        {
            // Prova con cultura italiana
            if (!decimal.TryParse(importoStr, NumberStyles.Any, CultureInfo.GetCultureInfo("it-IT"), out importoDecimal))
            {
                _isSaving = false;
                MessageBox.Show("Importo non valido", "Validazione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        try
        {
            var riga = _repository.GetById(_rigaId);
            if (riga != null)
            {
                // Verifica se il codice modificato esiste già in un'altra riga
                var altreRighe = _repository.GetByClienteAndPeriodo(riga.ClienteId, riga.Mese, riga.Anno)
                    .Where(b => b.Id != _rigaId) // Escludi la riga corrente
                    .ToList();
                
                var codiceEsistente = altreRighe.FirstOrDefault(b => 
                    b.CodiceMastrino.Equals(codice.Trim(), StringComparison.OrdinalIgnoreCase));
                
                if (codiceEsistente != null)
                {
                    _isSaving = false;
                    MessageBox.Show($"❌ Codice '{codice.Trim()}' già esistente!\n\nIl codice è già utilizzato per:\n{codiceEsistente.DescrizioneMastrino}", 
                        "Codice Duplicato", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    return;
                }
                
                riga.CodiceMastrino = codice.Trim();
                riga.DescrizioneMastrino = descrizione.Trim();
                riga.Importo = importoDecimal;

                if (_repository.Update(riga))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    _isSaving = false;
                    MessageBox.Show("Errore durante l'aggiornamento", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _isSaving = false;
                MessageBox.Show("Riga non trovata nel database", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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


