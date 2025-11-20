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
/// ViewModel per gestione Tipo Pratiche (EF Core async)
/// </summary>
public partial class TipoPraticaViewModel : ObservableObject
{
    private readonly TipoPraticaRepository _tipoPraticaRepository;

    [ObservableProperty]
    private ObservableCollection<TipoPratica> _pratiche;

    [ObservableProperty]
    private TipoPratica? _selectedPratica;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyActive = true;

    [ObservableProperty]
    private int _totalPratiche;

    [ObservableProperty]
    private int _praticheAttive;

    [ObservableProperty]
    private int _praticheInattive;

    [ObservableProperty]
    private bool _isLoading;

    public TipoPraticaViewModel(TipoPraticaRepository tipoPraticaRepository)
    {
        _tipoPraticaRepository = tipoPraticaRepository;
        _pratiche = new ObservableCollection<TipoPratica>();
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var pratiche = ShowOnlyActive 
                ? await _tipoPraticaRepository.GetActiveAsync() 
                : await _tipoPraticaRepository.GetAllAsync();

            // Filtro ricerca
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                pratiche = await _tipoPraticaRepository.SearchByNameAsync(SearchText);
                if (ShowOnlyActive)
                    pratiche = pratiche.Where(p => p.Attivo).ToList();
            }

            Pratiche = new ObservableCollection<TipoPratica>(
                pratiche.OrderBy(p => p.Ordine).ThenBy(p => p.NomePratica));

            // Statistiche
            TotalPratiche = await _tipoPraticaRepository.CountAsync();
            PraticheAttive = await _tipoPraticaRepository.CountAsync(p => p.Attivo);
            PraticheInattive = TotalPratiche - PraticheAttive;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento pratiche: {ex.Message}", 
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

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowOnlyActive = true;
    }

    /// <summary>
    /// Nuova pratica
    /// </summary>
    [RelayCommand]
    private async Task NewPraticaAsync()
    {
        if (!SessionManager.IsAdministrator)
        {
            MessageBox.Show("Solo gli amministratori possono creare pratiche.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.TipoPraticaDialogView(_tipoPraticaRepository);
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
    /// Modifica pratica selezionata
    /// </summary>
    [RelayCommand]
    private async Task EditPraticaAsync()
    {
        if (SelectedPratica == null)
        {
            MessageBox.Show("Seleziona una pratica da modificare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.IsAdministrator)
        {
            MessageBox.Show("Solo gli amministratori possono modificare pratiche.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.TipoPraticaDialogView(_tipoPraticaRepository, SelectedPratica);
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
    /// Disattiva pratica (soft delete)
    /// </summary>
    [RelayCommand]
    private async Task DeactivatePraticaAsync()
    {
        if (SelectedPratica == null)
        {
            MessageBox.Show("Seleziona una pratica da disattivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.IsAdministrator)
        {
            MessageBox.Show("Solo gli amministratori possono disattivare pratiche.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler disattivare la pratica '{SelectedPratica.NomePratica}'?\n\n" +
            "La pratica non sarà eliminata ma contrassegnata come inattiva.",
            "Conferma Disattivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _tipoPraticaRepository.DeactivateAsync(SelectedPratica.Id);
                
                if (success)
                {
                    MessageBox.Show("Pratica disattivata con successo.", 
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
    /// Attiva pratica
    /// </summary>
    [RelayCommand]
    private async Task ActivatePraticaAsync()
    {
        if (SelectedPratica == null || SelectedPratica.Attivo)
        {
            MessageBox.Show("Seleziona una pratica inattiva da attivare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.IsAdministrator)
        {
            MessageBox.Show("Solo gli amministratori possono attivare pratiche.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Confermi di voler attivare la pratica '{SelectedPratica.NomePratica}'?",
            "Conferma Attivazione", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _tipoPraticaRepository.ActivateAsync(SelectedPratica.Id);
                
                if (success)
                {
                    MessageBox.Show("Pratica attivata con successo.", 
                                  "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Errore durante l'attivazione.", 
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
    /// Elimina definitivamente pratica (hard delete) - Solo per inattive
    /// </summary>
    [RelayCommand]
    private async Task DeletePermanentlyAsync()
    {
        if (SelectedPratica == null)
        {
            MessageBox.Show("Seleziona una pratica da eliminare.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // VERIFICA: Solo pratiche INATTIVE possono essere eliminate definitivamente
        if (SelectedPratica.Attivo)
        {
            MessageBox.Show("Impossibile eliminare una pratica ATTIVA.\n\n" +
                          "Prima disattiva la pratica, poi potrai eliminarla definitivamente.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SessionManager.IsAdministrator)
        {
            MessageBox.Show("Solo gli amministratori possono eliminare pratiche.", 
                          "Accesso Negato", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"⚠️ ATTENZIONE: ELIMINAZIONE PERMANENTE ⚠️\n\n" +
            $"Stai per eliminare DEFINITIVAMENTE la pratica:\n" +
            $"'{SelectedPratica.NomePratica}'\n\n" +
            $"Questa operazione è IRREVERSIBILE!\n" +
            $"Tutti i dati della pratica saranno persi per sempre.\n\n" +
            $"Sei assolutamente sicuro di voler procedere?",
            "⚠️ CONFERMA ELIMINAZIONE DEFINITIVA", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var praticaId = SelectedPratica.Id;
                var praticaNome = SelectedPratica.NomePratica;
                
                var success = await _tipoPraticaRepository.DeleteAsync(praticaId);
                
                if (success)
                {
                    MessageBox.Show($"Pratica '{praticaNome}' eliminata definitivamente.", 
                                  "Eliminata", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Errore durante l'eliminazione della pratica.", 
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
    /// Visualizza dettagli pratica
    /// </summary>
    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedPratica == null)
        {
            MessageBox.Show("Seleziona una pratica.", 
                          "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var details = $"Pratica: {SelectedPratica.NomePratica}\n" +
                     $"Descrizione: {SelectedPratica.Descrizione ?? "N/D"}\n" +
                     $"Categoria: {SelectedPratica.CategoriaDescrizione} {SelectedPratica.IconaCategoria}\n" +
                     $"Priorità Default: {SelectedPratica.PrioritaDefault}\n" +
                     $"Ordine: {SelectedPratica.Ordine}\n" +
                     $"Attiva: {(SelectedPratica.Attivo ? "Sì" : "No")}";

        MessageBox.Show(details, "Dettagli Pratica", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Export Excel con tutti i dati pratiche - Usa ClosedXML
    /// </summary>
    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        try
        {
            // Ottieni tutte le pratiche
            var tuttePratiche = (await _tipoPraticaRepository.GetAllAsync())
                .OrderBy(p => p.Attivo ? 0 : 1)  // Attive prima
                .ThenBy(p => p.Ordine)
                .ThenBy(p => p.NomePratica)
                .ToList();

            if (tuttePratiche.Count == 0)
            {
                MessageBox.Show("Nessuna pratica da esportare.", 
                              "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dialog per salvare file
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Pratiche_CGEasy_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Title = "Esporta Pratiche in Excel",
                DefaultExt = "xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            // Crea file Excel con ClosedXML
            using var workbook = new XLWorkbook();
            
            // === FOGLIO 1: PRATICHE ===
            var worksheet = workbook.Worksheets.Add("Pratiche");

            // Intestazioni
            var headers = new[]
            {
                "ID", "Ordine", "Stato", "Nome Pratica", "Categoria", "Descrizione", 
                "Priorità", "Durata (gg)", "Data Creazione", "Ultima Modifica"
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
            foreach (var pratica in tuttePratiche)
            {
                worksheet.Cell(row, 1).Value = pratica.Id;
                worksheet.Cell(row, 2).Value = pratica.Ordine;
                worksheet.Cell(row, 3).Value = pratica.Attivo ? "ATTIVA" : "INATTIVA";
                worksheet.Cell(row, 4).Value = pratica.NomePratica;
                worksheet.Cell(row, 5).Value = pratica.CategoriaDescrizione;
                worksheet.Cell(row, 6).Value = pratica.Descrizione ?? "";
                worksheet.Cell(row, 7).Value = pratica.PrioritaDefault;
                worksheet.Cell(row, 8).Value = pratica.DurataStimataGiorni ?? 0;
                worksheet.Cell(row, 9).Value = pratica.CreatedAt;
                worksheet.Cell(row, 10).Value = pratica.UpdatedAt;

                // Formattazione colonna Stato
                worksheet.Cell(row, 3).Style.Font.Bold = true;
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Colora riga se inattiva
                if (!pratica.Attivo)
                {
                    var rowRange = worksheet.Range(row, 1, row, headers.Length);
                    rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 230);
                }

                row++;
            }

            // Formattazione date
            worksheet.Column(9).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";
            worksheet.Column(10).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

            // Auto-fit colonne
            worksheet.Columns().AdjustToContents();

            // Freeze prima riga
            worksheet.SheetView.FreezeRows(1);

            // Aggiungi filtri
            var table = worksheet.Range(1, 1, row - 1, headers.Length);
            table.SetAutoFilter();

            // === FOGLIO 2: STATISTICHE PER CATEGORIA ===
            var statsSheet = workbook.Worksheets.Add("Statistiche");

            statsSheet.Cell(1, 1).Value = "Statistiche Pratiche CGEasy";
            statsSheet.Cell(1, 1).Style.Font.FontSize = 16;
            statsSheet.Cell(1, 1).Style.Font.Bold = true;

            statsSheet.Cell(3, 1).Value = "Totale Pratiche:";
            statsSheet.Cell(3, 2).Value = tuttePratiche.Count;

            statsSheet.Cell(4, 1).Value = "Pratiche Attive:";
            statsSheet.Cell(4, 2).Value = tuttePratiche.Count(p => p.Attivo);
            statsSheet.Cell(4, 2).Style.Font.FontColor = XLColor.Green;
            statsSheet.Cell(4, 2).Style.Font.Bold = true;

            statsSheet.Cell(5, 1).Value = "Pratiche Inattive:";
            statsSheet.Cell(5, 2).Value = tuttePratiche.Count(p => !p.Attivo);
            statsSheet.Cell(5, 2).Style.Font.FontColor = XLColor.Red;
            statsSheet.Cell(5, 2).Style.Font.Bold = true;

            // Statistiche per categoria
            int statRow = 7;
            statsSheet.Cell(statRow, 1).Value = "PRATICHE PER CATEGORIA:";
            statsSheet.Cell(statRow, 1).Style.Font.Bold = true;
            statRow++;

            foreach (CategoriaPratica cat in Enum.GetValues(typeof(CategoriaPratica)))
            {
                var countCat = tuttePratiche.Count(p => p.Categoria == cat);
                if (countCat > 0)
                {
                    var firstPratica = tuttePratiche.FirstOrDefault(p => p.Categoria == cat);
                    statsSheet.Cell(statRow, 1).Value = $"{firstPratica?.IconaCategoria} {firstPratica?.CategoriaDescrizione}:";
                    statsSheet.Cell(statRow, 2).Value = countCat;
                    statRow++;
                }
            }

            statRow++;
            statsSheet.Cell(statRow, 1).Value = "Data Export:";
            statsSheet.Cell(statRow, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            statRow++;

            statsSheet.Cell(statRow, 1).Value = "Esportato da:";
            statsSheet.Cell(statRow, 2).Value = SessionManager.CurrentUser?.Username ?? "N/D";

            statsSheet.Columns().AdjustToContents();

            // Salva file
            workbook.SaveAs(saveDialog.FileName);

            // Successo!
            var risultato = MessageBox.Show(
                $"Export completato con successo!\n\n" +
                $"File: {saveDialog.FileName}\n" +
                $"Pratiche esportate: {tuttePratiche.Count}\n" +
                $"- Attive: {tuttePratiche.Count(p => p.Attivo)}\n" +
                $"- Inattive: {tuttePratiche.Count(p => !p.Attivo)}\n\n" +
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
