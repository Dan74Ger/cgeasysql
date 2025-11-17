using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.Core.Helpers;
using Microsoft.Win32;
using ClosedXML.Excel;

namespace CGEasy.App.ViewModels;

public class BilancioTemplateDettaglioViewModel : INotifyPropertyChanged
{
    private readonly LiteDbContext _context;
    private readonly BilancioTemplateRepository _repository;
    private readonly AuditLogService _auditService;
    private int _clienteId;
    private int _mese;
    private int _anno;
    private string _nomeFile = string.Empty;
    private ObservableCollection<BilancioTemplate> _righe = new();

    public BilancioTemplateDettaglioViewModel(
        LiteDbContext context,
        BilancioTemplateRepository repository,
        AuditLogService auditService)
    {
        _context = context;
        _repository = repository;
        _auditService = auditService;

        // Commands
        TornaIndietroCommand = new RelayCommand(TornaIndietro);
        NuovaRigaCommand = new RelayCommand(NuovaRiga);
        ModificaRigaCommand = new RelayCommand(ModificaRiga, () => RigaSelezionata != null);
        EliminaRigaCommand = new RelayCommand(EliminaRiga, () => RigaSelezionata != null);
        SalvaCommand = new RelayCommand(Salva);
        ExportExcelCommand = new RelayCommand(ExportExcel);
    }

    // Properties
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ObservableCollection<BilancioTemplate> Righe
    {
        get => _righe;
        set { _righe = value; OnPropertyChanged(); OnPropertyChanged(nameof(NumeroRighe)); }
    }

    private BilancioTemplate? _rigaSelezionata;
    public BilancioTemplate? RigaSelezionata
    {
        get => _rigaSelezionata;
        set
        {
            _rigaSelezionata = value;
            OnPropertyChanged();
            ((RelayCommand)ModificaRigaCommand).RaiseCanExecuteChanged();
            ((RelayCommand)EliminaRigaCommand).RaiseCanExecuteChanged();
        }
    }

    public string HeaderInfo => $"📁 {_nomeFile} • 📅 {_mese:00}/{_anno}";
    public int NumeroRighe => Righe?.Count ?? 0;

    // Commands
    public ICommand TornaIndietroCommand { get; }
    public ICommand NuovaRigaCommand { get; }
    public ICommand ModificaRigaCommand { get; }
    public ICommand EliminaRigaCommand { get; }
    public ICommand SalvaCommand { get; }
    public ICommand ExportExcelCommand { get; }

    // Event per navigazione
    public event EventHandler? RichiestaChiusura;

    /// <summary>
    /// Inizializza il dettaglio con i dati del template
    /// </summary>
    public void CaricaDati(int clienteId, int mese, int anno, string descrizione)
    {
        _clienteId = clienteId;
        _mese = mese;
        _anno = anno;
        _nomeFile = descrizione ?? ""; // Salva la descrizione

        IsLoading = true;

        try
        {
            // ⭐ IMPORTANTE: Carica SOLO le righe con la descrizione specifica
            var righe = _repository.GetByClienteAndPeriodoAndDescrizione(clienteId, mese, anno, descrizione);
            
            System.Diagnostics.Debug.WriteLine($"[CARICA DETTAGLIO TEMPLATE] Cliente={clienteId}, Periodo={mese}/{anno}, Desc='{descrizione ?? "(vuota)"}', Righe caricate={righe.Count}");

            Righe.Clear();
            foreach (var riga in righe.OrderByCodiceMastrinoNumerico(r => r.CodiceMastrino))
            {
                // Sottoscrivi agli eventi di PropertyChanged per ricalcolo automatico
                riga.PropertyChanged += Riga_PropertyChanged;
                Righe.Add(riga);
            }

            // Ricalcola tutti gli importi
            RicalcolaTuttiImporti();

            OnPropertyChanged(nameof(HeaderInfo));
            OnPropertyChanged(nameof(NumeroRighe));
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Gestisce le modifiche alle proprietà delle righe per ricalcolo automatico in tempo reale
    /// </summary>
    private void Riga_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BilancioTemplate.Importo) ||
            e.PropertyName == nameof(BilancioTemplate.Segno) ||
            e.PropertyName == nameof(BilancioTemplate.Formula))
        {
            // Ricalcola tutti gli importi quando cambia Importo, Segno o Formula
            RicalcolaTuttiImporti();
        }
        else if (e.PropertyName == nameof(BilancioTemplate.CodiceMastrino))
        {
            // Riordina la lista quando cambia il codice
            RiordinaRighe();
        }
    }

    /// <summary>
    /// Riordina le righe in base al CodiceMastrino
    /// </summary>
    private void RiordinaRighe()
    {
        var righeOrdinate = Righe.OrderByCodiceMastrinoNumerico(r => r.CodiceMastrino).ToList();
        
        // Ricostruisci la collezione mantenendo le sottoscrizioni
        Righe.Clear();
        foreach (var riga in righeOrdinate)
        {
            Righe.Add(riga);
        }
    }

    /// <summary>
    /// Ricalcola l'importo calcolato per tutte le righe
    /// </summary>
    public void RicalcolaTuttiImporti()
    {
        foreach (var riga in Righe)
        {
            RicalcolaImportoRiga(riga);
        }
    }

    /// <summary>
    /// Calcola l'importo di una singola riga in base alla formula
    /// </summary>
    private void RicalcolaImportoRiga(BilancioTemplate riga)
    {
        if (string.IsNullOrWhiteSpace(riga.Formula))
        {
            // Nessuna formula: usa importo originale CON segno applicato
            riga.ImportoCalcolato = riga.Importo * (riga.Segno == "-" ? -1 : 1);
        }
        else
        {
            // Ha formula: calcola somma/differenza e applica il segno della riga
            decimal risultato = CalcolaFormula(riga.Formula);
            riga.ImportoCalcolato = risultato * (riga.Segno == "-" ? -1 : 1);
        }

        OnPropertyChanged(nameof(Righe));
    }

    /// <summary>
    /// Calcola il risultato di una formula tipo "101+102-103"
    /// USA ImportoCalcolato (non Importo) per concatenare correttamente le formule
    /// </summary>
    private decimal CalcolaFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return 0;

        try
        {
            decimal risultato = 0;
            
            // Regex per trovare operazioni: opzionale +/- seguito da codice
            // Supporta: "101+102-103" o "101 + 102 - 103"
            var pattern = @"([+\-]?)(\d+)";
            var matches = Regex.Matches(formula.Replace(" ", ""), pattern);

            foreach (Match match in matches)
            {
                string operatore = match.Groups[1].Value;
                string codice = match.Groups[2].Value;

                // Trova la riga con questo codice
                var rigaTrovata = Righe.FirstOrDefault(r => r.CodiceMastrino == codice);

                if (rigaTrovata != null)
                {
                    // ✅ USA ImportoCalcolato invece di Importo!
                    // Questo permette di concatenare formule: se 105=101+102 e 126=105-125,
                    // la riga 126 userà il valore CALCOLATO di 105 (somma di 101+102)
                    decimal valore = rigaTrovata.ImportoCalcolato;

                    // Applica l'operatore della formula
                    if (operatore == "-")
                        risultato -= valore;
                    else
                        risultato += valore;
                }
            }

            return risultato;
        }
        catch
        {
            return 0;
        }
    }

    private void TornaIndietro()
    {
        RichiestaChiusura?.Invoke(this, EventArgs.Empty);
    }

    private void NuovaRiga()
    {
        var dialog = new Views.BilancioTemplateDialogView();
        var dialogVm = new BilancioTemplateDialogViewModel(_context, _auditService);
        
        // Passa dati per nuova riga
        dialogVm.Inizializza(null, _clienteId, _mese, _anno, _nomeFile);
        
        dialog.DataContext = dialogVm;
        // NON impostare Owner per permettere finestra indipendente
        // dialog.Owner = Application.Current.MainWindow;

        if (dialog.ShowDialog() == true && dialogVm.Riga != null)
        {
            // Sottoscrivi PropertyChanged per ricalcolo automatico
            dialogVm.Riga.PropertyChanged += Riga_PropertyChanged;
            
            // Aggiungi alla lista e salva
            Righe.Add(dialogVm.Riga);
            _repository.Insert(dialogVm.Riga);

            // Riordina le righe in base al codice
            RiordinaRighe();

            // Ricalcola tutti gli importi (la nuova riga potrebbe essere usata in formule)
            RicalcolaTuttiImporti();

            OnPropertyChanged(nameof(NumeroRighe));

            _auditService.Log(
                SessionManager.CurrentUser?.Id ?? 0,
                SessionManager.CurrentUsername,
                "TEMPLATE_RIGA_CREATE",
                "BilancioTemplate",
                null,
                $"Nuova riga template: {dialogVm.Riga.CodiceMastrino} - {dialogVm.Riga.DescrizioneMastrino}");

            MessageBox.Show("✅ Riga aggiunta con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ModificaRiga()
    {
        if (RigaSelezionata == null) return;

        var dialog = new Views.BilancioTemplateDialogView();
        var dialogVm = new BilancioTemplateDialogViewModel(_context, _auditService);
        
        // Passa riga da modificare
        dialogVm.Inizializza(RigaSelezionata, _clienteId, _mese, _anno, _nomeFile);
        
        dialog.DataContext = dialogVm;
        // NON impostare Owner per permettere finestra indipendente
        // dialog.Owner = Application.Current.MainWindow;

        if (dialog.ShowDialog() == true && dialogVm.Riga != null)
        {
            // Aggiorna nel database
            _repository.Update(dialogVm.Riga);

            // Ricalcola tutti gli importi (la riga modificata potrebbe essere usata in formule)
            RicalcolaTuttiImporti();

            _auditService.Log(
                SessionManager.CurrentUser?.Id ?? 0,
                SessionManager.CurrentUsername,
                "TEMPLATE_RIGA_UPDATE",
                "BilancioTemplate",
                null,
                $"Riga template modificata: {dialogVm.Riga.CodiceMastrino}");

            MessageBox.Show("✅ Riga modificata con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void EliminaRiga()
    {
        if (RigaSelezionata == null) return;

        // ✅ Verifica se la riga è utilizzata in qualche formula
        var codiceRiga = RigaSelezionata.CodiceMastrino;
        var righeConFormula = Righe.Where(r => 
            !string.IsNullOrWhiteSpace(r.Formula) && 
            r.Formula.Contains(codiceRiga) &&
            r.Id != RigaSelezionata.Id).ToList();

        if (righeConFormula.Any())
        {
            var righeElenco = string.Join("\n", righeConFormula.Select(r => 
                $"  • {r.CodiceMastrino} - {r.DescrizioneMastrino} (Formula: {r.Formula})"));

            MessageBox.Show(
                $"❌ IMPOSSIBILE ELIMINARE LA RIGA!\n\n" +
                $"La riga {codiceRiga} è utilizzata nelle seguenti formule:\n\n" +
                $"{righeElenco}\n\n" +
                $"Prima di eliminarla, devi rimuoverla dalle formule o eliminare le righe che la utilizzano.",
                "Eliminazione Non Consentita",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Vuoi eliminare la riga?\n\n{RigaSelezionata.CodiceMastrino} - {RigaSelezionata.DescrizioneMastrino}",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var codiceEliminato = RigaSelezionata.CodiceMastrino;
            
            _repository.Delete(RigaSelezionata.Id);
            Righe.Remove(RigaSelezionata);

            // Ricalcola tutti gli importi
            RicalcolaTuttiImporti();

            OnPropertyChanged(nameof(NumeroRighe));

            _auditService.Log(
                SessionManager.CurrentUser?.Id ?? 0,
                SessionManager.CurrentUsername,
                "TEMPLATE_RIGA_DELETE",
                "BilancioTemplate",
                null,
                $"Riga template eliminata: {codiceEliminato}");

            MessageBox.Show("✅ Riga eliminata con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Salva()
    {
        try
        {
            IsLoading = true;

            // Salva tutte le righe
            foreach (var riga in Righe)
            {
                _repository.Update(riga);
            }

            _auditService.Log(
                SessionManager.CurrentUser?.Id ?? 0,
                SessionManager.CurrentUsername,
                "TEMPLATE_SAVE",
                "BilancioTemplate",
                null,
                $"Template salvato: {_nomeFile} ({NumeroRighe} righe)");

            MessageBox.Show("✅ Template salvato con successo!", "Successo",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Errore durante il salvataggio:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExportExcel()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Template_{_nomeFile}_{_mese:00}_{_anno}.xlsx"
            };

            if (dialog.ShowDialog() == true)
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Template");

                // Headers
                worksheet.Cell(1, 1).Value = "Codice";
                worksheet.Cell(1, 2).Value = "Descrizione";
                worksheet.Cell(1, 3).Value = "Importo";
                worksheet.Cell(1, 4).Value = "Segno";
                worksheet.Cell(1, 5).Value = "Formula";
                worksheet.Cell(1, 6).Value = "Importo Calcolato";
                worksheet.Cell(1, 7).Value = "Differenza";

                // Formatta header
                var headerRange = worksheet.Range(1, 1, 1, 7);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2196F3");
                headerRange.Style.Font.FontColor = XLColor.White;

                // Dati
                int row = 2;
                foreach (var riga in Righe)
                {
                    worksheet.Cell(row, 1).Value = riga.CodiceMastrino;
                    worksheet.Cell(row, 2).Value = riga.DescrizioneMastrino;
                    worksheet.Cell(row, 3).Value = riga.Importo;
                    worksheet.Cell(row, 4).Value = riga.Segno;
                    worksheet.Cell(row, 5).Value = riga.Formula ?? "";
                    worksheet.Cell(row, 6).Value = riga.ImportoCalcolato;
                    worksheet.Cell(row, 7).Value = riga.Importo - Math.Abs(riga.ImportoCalcolato);

                    // Evidenzia righe con differenza
                    if (riga.HasDifferenza)
                    {
                        worksheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFEBEE");
                    }

                    row++;
                }

                // Autofit colonne
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(dialog.FileName);

                MessageBox.Show("✅ Export completato con successo!", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Errore durante l'export:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
