using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;

namespace CGEasy.App.Views;

public partial class LicenseKeyEditDialogView : Window
{
    private readonly LicenseKey _licenseKey;
    private readonly LicenseRepository _repository;
    private readonly string _clienteName;

    public LicenseKeyEditDialogView(LicenseKey licenseKey, string clienteName)
    {
        InitializeComponent();
        
        _licenseKey = licenseKey;
        _clienteName = clienteName;
        
        var context = ((App)Application.Current).Services!.GetRequiredService<LiteDbContext>();
        _repository = new LicenseRepository(context);
        
        LoadData();
    }

    private void LoadData()
    {
        // Info licenza
        ModuloTextBlock.Text = _licenseKey.ModuleName;
        ClienteTextBlock.Text = _clienteName;
        DataGenerazioneTextBlock.Text = _licenseKey.DataGenerazione.ToString("dd/MM/yyyy HH:mm");

        // Imposta tipo durata
        if (_licenseKey.DataScadenza.HasValue)
        {
            RadioDurataLimitata.IsChecked = true;
            
            // Imposta DatePicker con la data di scadenza
            ScadenzaDatePicker.SelectedDate = _licenseKey.DataScadenza.Value;
            
            // Calcola anni dalla data di generazione alla scadenza (per info)
            var durata = _licenseKey.DataScadenza.Value - _licenseKey.DataGenerazione;
            var anni = (int)(durata.TotalDays / 365);
            
            AnniTextBox.Text = anni.ToString();
        }
        else
        {
            RadioPerpetua.IsChecked = true;
            DurataPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void RadioDurataLimitata_Checked(object sender, RoutedEventArgs e)
    {
        if (DurataPanel != null)
        {
            DurataPanel.Visibility = Visibility.Visible;
            UpdateScadenzaCalcolata();
        }
    }

    private void RadioPerpetua_Checked(object sender, RoutedEventArgs e)
    {
        if (DurataPanel != null)
        {
            DurataPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateScadenzaCalcolata()
    {
        if (int.TryParse(AnniTextBox.Text, out int anni) && anni >= 0)
        {
            if (anni == 0)
            {
                // Test immediato
                var testScadenza = DateTime.Now.AddSeconds(10);
                ScadenzaDatePicker.SelectedDate = testScadenza;
                ScadenzaCalcolataTextBlock.Text = $"🧪 TEST: Scadenza tra 10 secondi → {testScadenza:dd/MM/yyyy HH:mm:ss}";
            }
            else
            {
                var scadenza = _licenseKey.DataGenerazione.AddYears(anni);
                ScadenzaDatePicker.SelectedDate = scadenza;
                ScadenzaCalcolataTextBlock.Text = $"✅ Scadenza calcolata: {scadenza:dd/MM/yyyy}";
            }
        }
        else
        {
            ScadenzaCalcolataTextBlock.Text = "";
        }
    }

    private void ScadenzaDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Quando l'utente cambia manualmente la data, aggiorna il campo anni per coerenza
        if (ScadenzaDatePicker.SelectedDate.HasValue && _licenseKey != null)
        {
            var durata = ScadenzaDatePicker.SelectedDate.Value - _licenseKey.DataGenerazione;
            var anni = (int)(durata.TotalDays / 365);
            
            // Non aggiornare AnniTextBox se l'utente sta digitando (evita loop)
            if (!AnniTextBox.IsFocused)
            {
                AnniTextBox.Text = anni.ToString();
            }
            
            ScadenzaCalcolataTextBlock.Text = $"📅 Scadenza impostata: {ScadenzaDatePicker.SelectedDate.Value:dd/MM/yyyy}";
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Verifica tipo durata
            if (RadioDurataLimitata.IsChecked == true)
            {
                // Durata limitata - usa la data dal DatePicker
                if (!ScadenzaDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show(
                        "Seleziona una data di scadenza.\n\n" +
                        "💡 Puoi selezionarla dal calendario oppure inserire il numero di anni nel campo sottostante.",
                        "Validazione",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    ScadenzaDatePicker.Focus();
                    return;
                }

                var scadenzaSelezionata = ScadenzaDatePicker.SelectedDate.Value;

                // Verifica se è una licenza di test (scade entro 1 minuto)
                var durataSecondi = (scadenzaSelezionata - DateTime.Now).TotalSeconds;
                
                if (durataSecondi < 60 && durataSecondi > 0)
                {
                    // Licenza di test
                    var result = MessageBox.Show(
                        "⚠️ ATTENZIONE - LICENZA DI TEST\n\n" +
                        $"Stai per creare una licenza che scadrà tra {(int)durataSecondi} secondi.\n" +
                        "Questo è utile per testare il comportamento delle licenze scadute.\n\n" +
                        "Vuoi continuare?",
                        "Conferma Test Scadenza",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result != MessageBoxResult.Yes)
                        return;
                }
                else if (scadenzaSelezionata <= DateTime.Now)
                {
                    // Data già passata
                    var result = MessageBox.Show(
                        "⚠️ ATTENZIONE - DATA GIÀ SCADUTA\n\n" +
                        $"La data selezionata ({scadenzaSelezionata:dd/MM/yyyy HH:mm}) è già passata.\n" +
                        "La licenza risulterà immediatamente scaduta.\n\n" +
                        "Vuoi continuare comunque?",
                        "Conferma Data Passata",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    
                    if (result != MessageBoxResult.Yes)
                        return;
                }

                // Imposta la scadenza dalla data selezionata
                _licenseKey.DataScadenza = scadenzaSelezionata;
                
                if (durataSecondi < 60 && durataSecondi > 0)
                {
                    MessageBox.Show(
                        "🧪 Licenza di test creata!\n\n" +
                        $"Scadenza: {_licenseKey.DataScadenza:dd/MM/yyyy HH:mm:ss}\n\n" +
                        $"La licenza scadrà tra {(int)durataSecondi} secondi.\n" +
                        "Ricarica la pagina o riavvia l'app per vedere la licenza scaduta.",
                        "Test Scadenza",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                // Licenza perpetua
                _licenseKey.DataScadenza = null;
            }

            // Salva nel database
            _repository.UpdateKey(_licenseKey);

            // 🔥 RICARICA LICENZE per aggiornare subito lo stato senza riavviare l'app
            LicenseService.ReloadLicenses();

            MessageBox.Show(
                "✅ Durata licenza aggiornata con successo!",
                "Successo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante il salvataggio:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        
        // Auto-aggiorna scadenza calcolata quando l'utente digita
        AnniTextBox.TextChanged += (s, args) => UpdateScadenzaCalcolata();
    }
}

