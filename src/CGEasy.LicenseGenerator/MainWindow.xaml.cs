using System.Windows;
using CGEasy.Core.Services;

namespace CGEasy.LicenseGenerator;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void GeneraTodoStudio_Click(object sender, RoutedEventArgs e)
    {
        var key = LicenseService.GenerateLicenseKey("TODO-STUDIO");
        ChiaveTodoStudio.Text = key;
        MessageBox.Show(
            $"✅ Chiave generata con successo!\n\n{key}\n\nLa chiave è stata copiata nel campo qui sotto.",
            "TODO Studio - Chiave Generata",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CopiaTodoStudio_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ChiaveTodoStudio.Text))
        {
            Clipboard.SetText(ChiaveTodoStudio.Text);
            MessageBox.Show("✅ Chiave copiata negli appunti!", "Copiato", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void GeneraBilanci_Click(object sender, RoutedEventArgs e)
    {
        var key = LicenseService.GenerateLicenseKey("BILANCI");
        ChiaveBilanci.Text = key;
        MessageBox.Show(
            $"✅ Chiave generata con successo!\n\n{key}\n\nLa chiave è stata copiata nel campo qui sotto.",
            "Bilanci - Chiave Generata",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CopiaBilanci_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ChiaveBilanci.Text))
        {
            Clipboard.SetText(ChiaveBilanci.Text);
            MessageBox.Show("✅ Chiave copiata negli appunti!", "Copiato", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void GeneraCircolari_Click(object sender, RoutedEventArgs e)
    {
        var key = LicenseService.GenerateLicenseKey("CIRCOLARI");
        ChiaveCircolari.Text = key;
        MessageBox.Show(
            $"✅ Chiave generata con successo!\n\n{key}\n\nLa chiave è stata copiata nel campo qui sotto.",
            "Circolari - Chiave Generata",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CopiaCircolari_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ChiaveCircolari.Text))
        {
            Clipboard.SetText(ChiaveCircolari.Text);
            MessageBox.Show("✅ Chiave copiata negli appunti!", "Copiato", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void GeneraControlloGestione_Click(object sender, RoutedEventArgs e)
    {
        var key = LicenseService.GenerateLicenseKey("CONTROLLO-GESTIONE");
        ChiaveControlloGestione.Text = key;
        MessageBox.Show(
            $"✅ Chiave generata con successo!\n\n{key}\n\nLa chiave è stata copiata nel campo qui sotto.",
            "Controllo Gestione - Chiave Generata",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void CopiaControlloGestione_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ChiaveControlloGestione.Text))
        {
            Clipboard.SetText(ChiaveControlloGestione.Text);
            MessageBox.Show("✅ Chiave copiata negli appunti!", "Copiato", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
