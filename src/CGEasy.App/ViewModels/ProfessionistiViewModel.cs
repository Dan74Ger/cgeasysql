using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using ClosedXML.Excel;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per gestione anagrafica Professionisti - COMPLETO
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

    public ProfessionistiViewModel(ProfessionistaRepository professionistaRepository)
    {
        _professionistaRepository = professionistaRepository;
        _professionisti = new ObservableCollection<Professionista>();
        
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            var professionisti = ShowOnlyActive 
                ? _professionistaRepository.GetActive() 
                : _professionistaRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                professionisti = _professionistaRepository.SearchByName(SearchText);
                if (ShowOnlyActive)
                    professionisti = professionisti.Where(p => p.Attivo);
            }

            Professionisti = new ObservableCollection<Professionista>(
                professionisti.OrderBy(p => p.Cognome).ThenBy(p => p.Nome));

            // Statistiche
            TotalProfessionisti = _professionistaRepository.Count();
            ProfessionistiAttivi = _professionistaRepository.Count(p => p.Attivo);
            ProfessionistiCessati = TotalProfessionisti - ProfessionistiAttivi;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento professionisti: {ex.Message}", 
                          "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadData();
    }

    partial void OnSearchTextChanged(string value)
    {
        LoadData();
    }

    partial void OnShowOnlyActiveChanged(bool value)
    {
        LoadData();
    }

    /// <summary>
    /// Nuovo professionista
    /// </summary>
    [RelayCommand]
    private void NewProfessionista()
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
    /// Modifica professionista selezionato
    /// </summary>
    [RelayCommand]
    private void EditProfessionista()
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
    /// Disattiva professionista (soft delete)
    /// </summary>
    [RelayCommand]
    private void DeleteProfessionista()
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
                var success = _professionistaRepository.Deactivate(SelectedProfessionista.Id);
                
                if (success)
                {
                    MessageBox.Show("Professionista disattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
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
    private void ActivateProfessionista()
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
                var success = _professionistaRepository.Activate(SelectedProfessionista.Id);
                
                if (success)
                {
                    MessageBox.Show("Professionista riattivato con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
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
    private void DeletePermanently()
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
                
                var success = _professionistaRepository.Delete(profId);
                
                if (success)
                {
                    MessageBox.Show($"Professionista '{profNome}' eliminato definitivamente.", 
                                  "Eliminato", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadData();
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
    private void ExportExcel()
    {
        try
        {
            // Ottieni tutti i professionisti
            var tuttiProfessionisti = _professionistaRepository.GetAll()
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
