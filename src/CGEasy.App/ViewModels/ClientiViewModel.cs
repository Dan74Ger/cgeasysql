using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per gestione anagrafica Clienti
/// </summary>
public partial class ClientiViewModel : ObservableObject
{
    private readonly ClienteRepository _clienteRepository;

    [ObservableProperty]
    private ObservableCollection<Cliente> _clienti;

    [ObservableProperty]
    private Cliente? _selectedCliente;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyActive = true;

    [ObservableProperty]
    private int _totalClienti;

    [ObservableProperty]
    private int _clientiAttivi;

    [ObservableProperty]
    private int _clientiCessati;

    public ClientiViewModel(ClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
        _clienti = new ObservableCollection<Cliente>();
        
        LoadData();
    }

    /// <summary>
    /// Carica dati clienti
    /// </summary>
    private void LoadData()
    {
        try
        {
            var clienti = ShowOnlyActive 
                ? _clienteRepository.GetActive() 
                : _clienteRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                clienti = _clienteRepository.SearchByName(SearchText);
                if (ShowOnlyActive)
                    clienti = clienti.Where(c => c.Attivo);
            }

            Clienti = new ObservableCollection<Cliente>(clienti.OrderBy(c => c.NomeCliente));

            // Statistiche
            TotalClienti = _clienteRepository.Count();
            ClientiAttivi = _clienteRepository.Count(c => c.Attivo);
            ClientiCessati = TotalClienti - ClientiAttivi;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento clienti: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Ricarica lista
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }

    /// <summary>
    /// Ricerca clienti
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        LoadData();
    }

    /// <summary>
    /// Toggle mostra solo attivi
    /// </summary>
    partial void OnShowOnlyActiveChanged(bool value)
    {
        LoadData();
    }

    /// <summary>
    /// Nuovo cliente
    /// </summary>
    [RelayCommand]
    private void NewCliente()
    {
        // Verifica permessi
        if (!SessionManager.CanCreate("clienti"))
        {
            MessageBox.Show("Non hai i permessi per creare clienti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.ClienteDialogView(_clienteRepository);
            var result = dialog.ShowDialog();

            if (result == true && dialog.IsSaved)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Modifica cliente selezionato
    /// </summary>
    [RelayCommand]
    private void EditCliente()
    {
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleziona un cliente da modificare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica permessi
        if (!SessionManager.CanUpdate("clienti"))
        {
            MessageBox.Show("Non hai i permessi per modificare clienti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.ClienteDialogView(_clienteRepository, SelectedCliente);
            var result = dialog.ShowDialog();

            if (result == true && dialog.IsSaved)
            {
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Elimina cliente selezionato (soft delete)
    /// </summary>
    [RelayCommand]
    private void DeleteCliente()
    {
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleziona un cliente da disattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica permessi
        if (!SessionManager.CanDelete("clienti"))
        {
            MessageBox.Show("Non hai i permessi per disattivare clienti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler disattivare il cliente '{SelectedCliente.NomeCliente}'?\n\n" +
            "Il cliente non sarà eliminato ma contrassegnato come cessato.",
            "Conferma Disattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = _clienteRepository.Deactivate(SelectedCliente.Id);
                
                if (success)
                {
                    MessageBox.Show("Cliente disattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Errore durante la disattivazione del cliente.", 
                                  "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Riattiva cliente cessato
    /// </summary>
    [RelayCommand]
    private void ActivateCliente()
    {
        if (SelectedCliente == null || SelectedCliente.Attivo)
        {
            MessageBox.Show("Seleziona un cliente cessato da riattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica permessi
        if (!SessionManager.CanUpdate("clienti"))
        {
            MessageBox.Show("Non hai i permessi per riattivare clienti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler riattivare il cliente '{SelectedCliente.NomeCliente}'?",
            "Conferma Riattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = _clienteRepository.Activate(SelectedCliente.Id);
                
                if (success)
                {
                    MessageBox.Show("Cliente riattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Errore durante la riattivazione del cliente.", 
                                  "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Visualizza dettagli cliente
    /// </summary>
    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleziona un cliente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // TODO: Mostrare finestra dettagli cliente
        var details = $"Cliente: {SelectedCliente.NomeCliente}\n" +
                     $"P.IVA: {SelectedCliente.PivaCliente ?? "N/D"}\n" +
                     $"CF: {SelectedCliente.CfCliente ?? "N/D"}\n" +
                     $"Email: {SelectedCliente.MailCliente ?? "N/D"}\n" +
                     $"Indirizzo: {SelectedCliente.IndirizzoCompleto}\n" +
                     $"Stato: {SelectedCliente.StatoDescrizione}";

        MessageBox.Show(details, "Dettagli Cliente", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Elimina definitivamente cliente (hard delete) - Solo per clienti inattivi
    /// </summary>
    [RelayCommand]
    private void DeletePermanently()
    {
        if (SelectedCliente == null)
        {
            MessageBox.Show("Seleziona un cliente da eliminare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // VERIFICA: Solo clienti INATTIVI possono essere eliminati definitivamente
        if (SelectedCliente.Attivo)
        {
            MessageBox.Show("Impossibile eliminare un cliente ATTIVO.\n\n" +
                          "Prima disattiva il cliente, poi potrai eliminarlo definitivamente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica permessi
        if (!SessionManager.CanDelete("clienti"))
        {
            MessageBox.Show("Non hai i permessi per eliminare clienti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"⚠️ ATTENZIONE: ELIMINAZIONE PERMANENTE ⚠️\n\n" +
            $"Stai per eliminare DEFINITIVAMENTE il cliente:\n" +
            $"'{SelectedCliente.NomeCliente}'\n\n" +
            $"Questa operazione è IRREVERSIBILE!\n" +
            $"Tutti i dati del cliente saranno persi per sempre.\n\n" +
            $"Sei assolutamente sicuro di voler procedere?",
            "⚠️ CONFERMA ELIMINAZIONE DEFINITIVA", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var clienteId = SelectedCliente.Id;
                var clienteNome = SelectedCliente.NomeCliente;
                
                var success = _clienteRepository.Delete(clienteId);
                
                if (success)
                {
                    MessageBox.Show($"Cliente '{clienteNome}' eliminato definitivamente.", 
                                  "Eliminato", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Errore durante l'eliminazione del cliente.", 
                                  "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore: {ex.Message}", 
                              "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Export Excel con tutti i dati clienti - Usa ClosedXML (gratuito)
    /// </summary>
    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            // Ottieni tutti i clienti (senza filtri)
            var tuttiClienti = _clienteRepository.GetAll()
                .OrderBy(c => c.Attivo ? 0 : 1)  // Attivi prima
                .ThenBy(c => c.NomeCliente)
                .ToList();

            if (tuttiClienti.Count == 0)
            {
                MessageBox.Show("Nessun cliente da esportare.", 
                              "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dialog per salvare file
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Clienti_CGEasy_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Title = "Esporta Clienti in Excel",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            // Crea file Excel con ClosedXML
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            
            // === FOGLIO 1: CLIENTI ===
            var worksheet = workbook.Worksheets.Add("Clienti");

            // Intestazioni
            var headers = new[]
            {
                "ID", "Stato", "Nome Cliente", "P.IVA", "Codice Fiscale", "Codice ATECO",
                "Email", "Indirizzo", "CAP", "Città", "Provincia",
                "Legale Rappresentante", "CF Legale Rapp.",
                "Data Attivazione", "Data Cessazione", "Data Creazione", "Ultima Modifica"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(79, 129, 189);
                cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            }

            // Dati
            int row = 2;
            foreach (var cliente in tuttiClienti)
            {
                worksheet.Cell(row, 1).Value = cliente.Id;
                worksheet.Cell(row, 2).Value = cliente.Attivo ? "ATTIVO" : "CESSATO";
                worksheet.Cell(row, 3).Value = cliente.NomeCliente;
                worksheet.Cell(row, 4).Value = cliente.PivaCliente ?? "";
                worksheet.Cell(row, 5).Value = cliente.CfCliente ?? "";
                worksheet.Cell(row, 6).Value = cliente.CodiceAteco ?? "";
                worksheet.Cell(row, 7).Value = cliente.MailCliente ?? "";
                worksheet.Cell(row, 8).Value = cliente.Indirizzo ?? "";
                worksheet.Cell(row, 9).Value = cliente.Cap ?? "";
                worksheet.Cell(row, 10).Value = cliente.Citta ?? "";
                worksheet.Cell(row, 11).Value = cliente.Provincia ?? "";
                worksheet.Cell(row, 12).Value = cliente.LegaleRappresentante ?? "";
                worksheet.Cell(row, 13).Value = cliente.CfLegaleRappresentante ?? "";
                worksheet.Cell(row, 14).Value = cliente.DataAttivazione;
                worksheet.Cell(row, 15).Value = cliente.DataCessazione;
                worksheet.Cell(row, 16).Value = cliente.CreatedAt;
                worksheet.Cell(row, 17).Value = cliente.UpdatedAt;

                // Formattazione colonna Stato
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // Colora riga se cessato
                if (!cliente.Attivo)
                {
                    var rowRange = worksheet.Range(row, 1, row, headers.Length);
                    rowRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(255, 230, 230);
                }

                row++;
            }

            // Formattazione date
            worksheet.Column(14).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(15).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(16).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(17).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

            // Auto-fit colonne
            worksheet.Columns().AdjustToContents();

            // Freeze prima riga
            worksheet.SheetView.FreezeRows(1);

            // Aggiungi filtri
            var table = worksheet.Range(1, 1, row - 1, headers.Length);
            table.SetAutoFilter();

            // === FOGLIO 2: STATISTICHE ===
            var statsSheet = workbook.Worksheets.Add("Statistiche");

            statsSheet.Cell(1, 1).Value = "Statistiche Clienti CGEasy";
            statsSheet.Cell(1, 1).Style.Font.FontSize = 16;
            statsSheet.Cell(1, 1).Style.Font.Bold = true;

            statsSheet.Cell(3, 1).Value = "Totale Clienti:";
            statsSheet.Cell(3, 2).Value = tuttiClienti.Count;

            statsSheet.Cell(4, 1).Value = "Clienti Attivi:";
            statsSheet.Cell(4, 2).Value = tuttiClienti.Count(c => c.Attivo);
            statsSheet.Cell(4, 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
            statsSheet.Cell(4, 2).Style.Font.Bold = true;

            statsSheet.Cell(5, 1).Value = "Clienti Cessati:";
            statsSheet.Cell(5, 2).Value = tuttiClienti.Count(c => !c.Attivo);
            statsSheet.Cell(5, 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
            statsSheet.Cell(5, 2).Style.Font.Bold = true;

            statsSheet.Cell(7, 1).Value = "Data Export:";
            statsSheet.Cell(7, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            statsSheet.Cell(8, 1).Value = "Esportato da:";
            statsSheet.Cell(8, 2).Value = SessionManager.CurrentUser?.Username ?? "N/D";

            statsSheet.Columns().AdjustToContents();

            // Salva file
            workbook.SaveAs(saveDialog.FileName);

            // Successo!
            var risultato = MessageBox.Show(
                $"Export completato con successo!\n\n" +
                $"File: {saveDialog.FileName}\n" +
                $"Clienti esportati: {tuttiClienti.Count}\n" +
                $"- Attivi: {tuttiClienti.Count(c => c.Attivo)}\n" +
                $"- Cessati: {tuttiClienti.Count(c => !c.Attivo)}\n\n" +
                $"Vuoi aprire il file?",
                "Export Excel Completato", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Information);

            if (risultato == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'export Excel:\n{ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}



