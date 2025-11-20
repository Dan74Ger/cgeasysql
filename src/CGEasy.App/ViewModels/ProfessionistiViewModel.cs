using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using ClosedXML.Excel;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per gestione anagrafica Professionisti (EF Core async)
/// </summary>
public partial class ProfessionistiViewModel : ObservableObject
{
    private readonly ProfessionistaRepository _professionistaRepository;

    [ObservableProperty]
    private ObservableCollection<Professionista> _professionisti;

    [ObservableProperty]
    private Professionista? _selectedProfessionista;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyActive = true;

    [ObservableProperty]
    private int _totalProfessionisti;

    [ObservableProperty]
    private int _professionistiAttivi;

    [ObservableProperty]
    private int _professionistiCessati;

    [ObservableProperty]
    private bool _isLoading;

    public ProfessionistiViewModel(ProfessionistaRepository professionistaRepository)
    {
        _professionistaRepository = professionistaRepository;
        _professionisti = new ObservableCollection<Professionista>();
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var professionisti = ShowOnlyActive 
                ? await _professionistaRepository.GetActiveAsync() 
                : await _professionistaRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                professionisti = await _professionistaRepository.SearchByNameAsync(SearchText);
                if (ShowOnlyActive)
                    professionisti = professionisti.Where(p => p.Attivo).ToList();
            }

            Professionisti = new ObservableCollection<Professionista>(
                professionisti.OrderBy(p => p.Cognome).ThenBy(p => p.Nome));

            // Statistiche
            TotalProfessionisti = await _professionistaRepository.CountAsync();
            ProfessionistiAttivi = await _professionistaRepository.CountAsync(p => p.Attivo);
            ProfessionistiCessati = TotalProfessionisti - ProfessionistiAttivi;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento professionisti: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadDataAsync();
    }

    partial void OnShowOnlyActiveChanged(bool value)
    {
        _ = LoadDataAsync();
    }

    /// <summary>
    /// Nuovo professionista
    /// </summary>
    [RelayCommand]
    private async Task NewProfessionistaAsync()
    {
        if (!SessionManager.CanCreate("professionisti"))
        {
            MessageBox.Show("Non hai i permessi per creare professionisti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.ProfessionistaDialogView(_professionistaRepository);
            var result = dialog.ShowDialog();

            if (result == true && dialog.IsSaved)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Modifica professionista selezionato
    /// </summary>
    [RelayCommand]
    private async Task EditProfessionistaAsync()
    {
        if (SelectedProfessionista == null)
        {
            MessageBox.Show("Seleziona un professionista da modificare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.CanUpdate("professionisti"))
        {
            MessageBox.Show("Non hai i permessi per modificare professionisti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.ProfessionistaDialogView(_professionistaRepository, SelectedProfessionista);
            var result = dialog.ShowDialog();

            if (result == true && dialog.IsSaved)
            {
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Disattiva professionista (soft delete)
    /// </summary>
    [RelayCommand]
    private async Task DeleteProfessionistaAsync()
    {
        if (SelectedProfessionista == null)
        {
            MessageBox.Show("Seleziona un professionista da disattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.CanDelete("professionisti"))
        {
            MessageBox.Show("Non hai i permessi per disattivare professionisti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler disattivare '{SelectedProfessionista.NomeCompleto}'?\n\n" +
            "Il professionista non sarà eliminato ma contrassegnato come cessato.",
            "Conferma Disattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _professionistaRepository.DeactivateAsync(SelectedProfessionista.Id);
                
                if (success)
                {
                    MessageBox.Show("Professionista disattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Errore durante la disattivazione.", 
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
    /// Riattiva professionista cessato
    /// </summary>
    [RelayCommand]
    private async Task ActivateProfessionistaAsync()
    {
        if (SelectedProfessionista == null || SelectedProfessionista.Attivo)
        {
            MessageBox.Show("Seleziona un professionista cessato da riattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.CanUpdate("professionisti"))
        {
            MessageBox.Show("Non hai i permessi per riattivare professionisti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler riattivare '{SelectedProfessionista.NomeCompleto}'?",
            "Conferma Riattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _professionistaRepository.ActivateAsync(SelectedProfessionista.Id);
                
                if (success)
                {
                    MessageBox.Show("Professionista riattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Errore durante la riattivazione.", 
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
    /// Elimina definitivamente professionista (hard delete) - Solo per inattivi
    /// </summary>
    [RelayCommand]
    private async Task DeletePermanentlyAsync()
    {
        if (SelectedProfessionista == null)
        {
            MessageBox.Show("Seleziona un professionista da eliminare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // VERIFICA: Solo professionisti INATTIVI possono essere eliminati definitivamente
        if (SelectedProfessionista.Attivo)
        {
            MessageBox.Show("Impossibile eliminare un professionista ATTIVO.\n\n" +
                          "Prima disattiva il professionista, poi potrai eliminarlo definitivamente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.CanDelete("professionisti"))
        {
            MessageBox.Show("Non hai i permessi per eliminare professionisti.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"⚠️ ATTENZIONE: ELIMINAZIONE PERMANENTE ⚠️\n\n" +
            $"Stai per eliminare DEFINITIVAMENTE il professionista:\n" +
            $"'{SelectedProfessionista.NomeCompleto}'\n\n" +
            $"Questa operazione è IRREVERSIBILE!\n" +
            $"Tutti i dati del professionista saranno persi per sempre.\n\n" +
            $"Sei assolutamente sicuro di voler procedere?",
            "⚠️ CONFERMA ELIMINAZIONE DEFINITIVA", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var profId = SelectedProfessionista.Id;
                var profNome = SelectedProfessionista.NomeCompleto;
                
                var success = await _professionistaRepository.DeleteAsync(profId);
                
                if (success)
                {
                    MessageBox.Show($"Professionista '{profNome}' eliminato definitivamente.", 
                                  "Eliminato", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Errore durante l'eliminazione del professionista.", 
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
    /// Visualizza dettagli professionista
    /// </summary>
    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedProfessionista == null)
        {
            MessageBox.Show("Seleziona un professionista.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var details = $"Professionista: {SelectedProfessionista.NomeCompleto}\n" +
                     $"Nome: {SelectedProfessionista.Nome}\n" +
                     $"Cognome: {SelectedProfessionista.Cognome}\n" +
                     $"Stato: {SelectedProfessionista.StatoDescrizione}\n" +
                     $"Data Attivazione: {SelectedProfessionista.DataAttivazione:dd/MM/yyyy}";

        if (SelectedProfessionista.DataCessazione.HasValue)
            details += $"\nData Cessazione: {SelectedProfessionista.DataCessazione:dd/MM/yyyy}";

        MessageBox.Show(details, "Dettagli Professionista", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Export Excel con tutti i dati professionisti - Usa ClosedXML
    /// </summary>
    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        try
        {
            // Ottieni tutti i professionisti
            var tuttiProfessionisti = (await _professionistaRepository.GetAllAsync())
                .OrderBy(p => p.Attivo ? 0 : 1)  // Attivi prima
                .ThenBy(p => p.Cognome)
                .ThenBy(p => p.Nome)
                .ToList();

            if (tuttiProfessionisti.Count == 0)
            {
                MessageBox.Show("Nessun professionista da esportare.", 
                              "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dialog per salvare file
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Professionisti_CGEasy_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Title = "Esporta Professionisti in Excel",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            // Crea file Excel con ClosedXML
            using var workbook = new XLWorkbook();
            
            // === FOGLIO 1: PROFESSIONISTI ===
            var worksheet = workbook.Worksheets.Add("Professionisti");

            // Intestazioni
            var headers = new[]
            {
                "ID", "Stato", "Cognome", "Nome", "Nome Completo",
                "Data Attivazione", "Data Cessazione", "Data Creazione", "Ultima Modifica"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 129, 189);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Dati
            int row = 2;
            foreach (var prof in tuttiProfessionisti)
            {
                worksheet.Cell(row, 1).Value = prof.Id;
                worksheet.Cell(row, 2).Value = prof.Attivo ? "ATTIVO" : "CESSATO";
                worksheet.Cell(row, 3).Value = prof.Cognome;
                worksheet.Cell(row, 4).Value = prof.Nome;
                worksheet.Cell(row, 5).Value = prof.NomeCompleto;
                worksheet.Cell(row, 6).Value = prof.DataAttivazione;
                worksheet.Cell(row, 7).Value = prof.DataCessazione;
                worksheet.Cell(row, 8).Value = prof.CreatedAt;
                worksheet.Cell(row, 9).Value = prof.UpdatedAt;

                // Formattazione colonna Stato
                worksheet.Cell(row, 2).Style.Font.Bold = true;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Colora riga se cessato
                if (!prof.Attivo)
                {
                    var rowRange = worksheet.Range(row, 1, row, headers.Length);
                    rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 230);
                }

                row++;
            }

            // Formattazione date
            worksheet.Column(6).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(7).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(8).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(9).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

            // Auto-fit colonne
            worksheet.Columns().AdjustToContents();

            // Freeze prima riga
            worksheet.SheetView.FreezeRows(1);

            // Aggiungi filtri
            var table = worksheet.Range(1, 1, row - 1, headers.Length);
            table.SetAutoFilter();

            // === FOGLIO 2: STATISTICHE ===
            var statsSheet = workbook.Worksheets.Add("Statistiche");

            statsSheet.Cell(1, 1).Value = "Statistiche Professionisti CGEasy";
            statsSheet.Cell(1, 1).Style.Font.FontSize = 16;
            statsSheet.Cell(1, 1).Style.Font.Bold = true;

            statsSheet.Cell(3, 1).Value = "Totale Professionisti:";
            statsSheet.Cell(3, 2).Value = tuttiProfessionisti.Count;

            statsSheet.Cell(4, 1).Value = "Professionisti Attivi:";
            statsSheet.Cell(4, 2).Value = tuttiProfessionisti.Count(p => p.Attivo);
            statsSheet.Cell(4, 2).Style.Font.FontColor = XLColor.Green;
            statsSheet.Cell(4, 2).Style.Font.Bold = true;

            statsSheet.Cell(5, 1).Value = "Professionisti Cessati:";
            statsSheet.Cell(5, 2).Value = tuttiProfessionisti.Count(p => !p.Attivo);
            statsSheet.Cell(5, 2).Style.Font.FontColor = XLColor.Red;
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
                $"Professionisti esportati: {tuttiProfessionisti.Count}\n" +
                $"- Attivi: {tuttiProfessionisti.Count(p => p.Attivo)}\n" +
                $"- Cessati: {tuttiProfessionisti.Count(p => !p.Attivo)}\n\n" +
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
