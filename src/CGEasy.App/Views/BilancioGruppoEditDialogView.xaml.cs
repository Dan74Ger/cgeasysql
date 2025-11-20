using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CGEasy.Core.Data;
using CGEasy.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CGEasy.App.Views;

public partial class BilancioGruppoEditDialogView : Window
{
    private readonly CGEasyDbContext _context;
    private readonly BilancioContabileRepository _repository;
    private readonly int _clienteId;
    private readonly int _meseOriginale;
    private readonly int _annoOriginale;

    public int NuovoMese { get; private set; }
    public int NuovoAnno { get; private set; }
    public string? NuovaDescrizione { get; private set; }
    public string NuovoTipoBilancio { get; private set; } = "CE";

    public BilancioGruppoEditDialogView(int clienteId, int mese, int anno, string clienteNome, string? descrizione)
    {
        InitializeComponent();

        _context = ((App)Application.Current).Services!.GetRequiredService<CGEasyDbContext>();
        _repository = new BilancioContabileRepository(_context);
        _clienteId = clienteId;
        _meseOriginale = mese;
        _annoOriginale = anno;
        NuovoMese = mese;
        NuovoAnno = anno;

        // Popola form
        ClienteTextBox.Text = clienteNome;
        DescrizioneTextBox.Text = descrizione ?? "";
        
        // Imposta valori correnti
        MeseComboBox.SelectedIndex = mese - 1;
        AnnoTextBox.Text = anno.ToString();
        
        // Carica tipo bilancio dalla prima riga del gruppo
        var primaRiga = _repository.GetByClienteAndPeriodo(clienteId, mese, anno).FirstOrDefault();
        if (primaRiga != null)
        {
            NuovoTipoBilancio = primaRiga.TipoBilancio ?? "CE";
            TipoBilancioComboBox.SelectedIndex = NuovoTipoBilancio == "SP" ? 1 : 0;
        }
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

            // Verifica se il periodo è cambiato e se esiste già un bilancio per quel periodo
            if ((NuovoMese != _meseOriginale || NuovoAnno != _annoOriginale))
            {
                var esistente = _repository.GetByClienteAndPeriodo(_clienteId, NuovoMese, NuovoAnno);
                if (esistente.Any())
                {
                    var result = MessageBox.Show(
                        $"⚠️ Esiste già un bilancio per {ClienteTextBox.Text} nel periodo {NuovoMese:00}/{NuovoAnno}!\n\n" +
                        $"Continuando, le righe verranno unite a quelle esistenti.\n\n" +
                        $"Vuoi procedere?",
                        "Conflitto Periodo",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result != MessageBoxResult.Yes)
                        return;
                }
            }

            // Aggiorna tutte le righe del gruppo
            var righe = _repository.GetByClienteAndPeriodo(_clienteId, _meseOriginale, _annoOriginale);
            
            foreach (var riga in righe)
            {
                riga.Mese = NuovoMese;
                riga.Anno = NuovoAnno;
                riga.DescrizioneBilancio = NuovaDescrizione;
                riga.TipoBilancio = NuovoTipoBilancio;
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

