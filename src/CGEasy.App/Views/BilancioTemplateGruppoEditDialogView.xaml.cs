using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CGEasy.Core.Data;
using CGEasy.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioTemplateGruppoEditDialogView : Window
{
    private readonly LiteDbContext _context;
    private readonly BilancioTemplateRepository _repository;
    private readonly int _clienteId;
    private readonly int _meseOriginale;
    private readonly int _annoOriginale;
    private readonly string? _descrizioneOriginale;

    public int NuovoMese { get; private set; }
    public int NuovoAnno { get; private set; }
    public string? NuovaDescrizione { get; private set; }
    public string NuovoTipoBilancio { get; private set; } = "CE"; // ⭐ Aggiunto

    public BilancioTemplateGruppoEditDialogView(int clienteId, int mese, int anno, string clienteNome, string? descrizione)
    {
        InitializeComponent();

        _context = ((App)Application.Current).Services!.GetRequiredService<LiteDbContext>();
        _repository = new BilancioTemplateRepository(_context);
        _clienteId = clienteId;
        _meseOriginale = mese;
        _annoOriginale = anno;
        _descrizioneOriginale = descrizione; // ⭐ Salva la descrizione originale
        NuovoMese = mese;
        NuovoAnno = anno;

        // Popola form
        ClienteTextBox.Text = clienteNome;
        DescrizioneTextBox.Text = descrizione ?? "";
        
        // Carica tipo bilancio dalla prima riga del gruppo
        var primaRiga = _repository.GetByClienteAndPeriodoAndDescrizione(clienteId, mese, anno, descrizione).FirstOrDefault();
        if (primaRiga != null)
        {
            NuovoTipoBilancio = primaRiga.TipoBilancio ?? "CE";
            TipoBilancioComboBox.SelectedIndex = NuovoTipoBilancio == "SP" ? 1 : 0;
        }
        
        // Imposta valori correnti
        MeseComboBox.SelectedIndex = mese - 1;
        AnnoTextBox.Text = anno.ToString();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validazione
        if (MeseComboBox.SelectedItem == null)
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

        if (!int.TryParse(AnnoTextBox.Text, out int annoInserito))
        {
            MessageBox.Show("L'anno deve essere un numero valido (es: 2024)", "Validazione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (annoInserito < 2000 || annoInserito > 2100)
        {
            MessageBox.Show("L'anno deve essere compreso tra 2000 e 2100", "Validazione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var selectedMeseItem = (ComboBoxItem)MeseComboBox.SelectedItem;
            NuovoMese = int.Parse(selectedMeseItem.Tag.ToString()!);
            NuovoAnno = annoInserito;
            NuovaDescrizione = string.IsNullOrWhiteSpace(DescrizioneTextBox.Text) 
                ? null 
                : DescrizioneTextBox.Text.Trim();
            
            // Leggi tipo bilancio
            if (TipoBilancioComboBox.SelectedItem is ComboBoxItem selectedTipoItem)
            {
                NuovoTipoBilancio = selectedTipoItem.Tag?.ToString() ?? "CE";
            }

            // Verifica se il periodo/descrizione è cambiato e se esiste già un template per quella combinazione
            bool periodoODescrizioneCambiati = (NuovoMese != _meseOriginale || 
                                                  NuovoAnno != _annoOriginale || 
                                                  NuovaDescrizione != _descrizioneOriginale);
            
            if (periodoODescrizioneCambiati)
            {
                var esistente = _repository.GetByClienteAndPeriodoAndDescrizione(
                    _clienteId, NuovoMese, NuovoAnno, NuovaDescrizione);
                    
                if (esistente.Any())
                {
                    // ⚠️ BLOCCA l'operazione - NON permettere di unire template
                    MessageBox.Show(
                        $"❌ ERRORE: Esiste già un template per {ClienteTextBox.Text} con:\n\n" +
                        $"Periodo: {NuovoMese:00}/{NuovoAnno}\n" +
                        $"Descrizione: {NuovaDescrizione ?? "(nessuna)"}\n\n" +
                        $"⚠️ Non è possibile unire template diversi!\n" +
                        $"Usa una descrizione differente oppure elimina prima il template esistente.",
                        "Conflitto Template",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            // ⭐ IMPORTANTE: Carica SOLO le righe con la descrizione ORIGINALE
            var righe = _repository.GetByClienteAndPeriodoAndDescrizione(
                _clienteId, _meseOriginale, _annoOriginale, _descrizioneOriginale);
            
            foreach (var riga in righe)
            {
                riga.Mese = NuovoMese;
                riga.Anno = NuovoAnno;
                riga.DescrizioneBilancio = NuovaDescrizione;
                riga.TipoBilancio = NuovoTipoBilancio; // ⭐ Aggiorna TipoBilancio
                _repository.Update(riga);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante il salvataggio:\n{ex.Message}", "Errore",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

