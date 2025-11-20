using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using CGEasy.Core.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// Rappresenta un singolo movimento (incasso o pagamento) nel dettaglio mensile
/// </summary>
public class MovimentoMensileDettaglioConsolidato
{
    public string NomeBanca { get; set; } = string.Empty;
    public string Descrizione { get; set; } = string.Empty; // Cliente/Fornitore
    public decimal Importo { get; set; }
    public DateTime DataScadenza { get; set; }
    public string NumeroFattura { get; set; } = string.Empty;
    public DateTime? DataFattura { get; set; }
}

/// <summary>
/// Rappresenta una riga della pivot consolidata con breakdown per banca
/// </summary>
public class SaldoPivotRigaConsolidato
{
    public string Categoria { get; set; } = string.Empty;
    public Dictionary<string, decimal> ValoriMensili { get; set; } = new Dictionary<string, decimal>();
    public Dictionary<string, Dictionary<string, decimal>> ValoriPerBanca { get; set; } = new Dictionary<string, Dictionary<string, decimal>>(); // Mese -> Banca -> Valore
    public Dictionary<string, List<MovimentoMensileDettaglioConsolidato>> DettagliMensili { get; set; } = new Dictionary<string, List<MovimentoMensileDettaglioConsolidato>>();
}

/// <summary>
/// Rappresenta i dati della pivot consolidata
/// </summary>
public class MargineTesoreraData
{
    public List<string> Mesi { get; set; } = new List<string>();
    public List<string> Banche { get; set; } = new List<string>();
    public SaldoPivotRigaConsolidato FatturatoAnticipato { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "Fatturato Anticipato" };
    public SaldoPivotRigaConsolidato ResiduoAnticipabile { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "Residuo Anticipabile" };
    public SaldoPivotRigaConsolidato SaldoCorrente { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "Saldo Corrente" };
    public SaldoPivotRigaConsolidato Incassi { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "Incassi" };
    public SaldoPivotRigaConsolidato Pagamenti { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "Pagamenti" };
    public SaldoPivotRigaConsolidato SaldoDisponibile { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "Saldo Disponibile" };
    public SaldoPivotRigaConsolidato UtilizzoFidoCC { get; set; } = new SaldoPivotRigaConsolidato { Categoria = "⚠️ Utilizzo Fido C/C" };
    public decimal FidoTotaleAnticipi { get; set; } = 0;
    public Dictionary<string, decimal> FidoCCPerBanca { get; set; } = new Dictionary<string, decimal>(); // Nome Banca -> Fido C/C
}

/// <summary>
/// ViewModel per il riepilogo consolidato di tutte le banche
/// </summary>
public partial class RiepilogoBancheViewModel : ObservableObject
{
    private readonly BancaService _bancaService;
    private readonly CGEasyDbContext _context;
    private readonly BancaRepository _bancaRepo;
    private readonly BancaIncassoRepository _incassoRepo;
    private readonly BancaPagamentoRepository _pagamentoRepo;

    [ObservableProperty]
    private RiepilogoBanche _riepilogo = new();

    [ObservableProperty]
    private ObservableCollection<AlertBanca> _alertsTotali = new();

    [ObservableProperty]
    private ScadenziarioConsolidato _scadenziarioConsolidato = new();

    [ObservableProperty]
    private DateTime _dataInizioScadenziario = DateTime.Now;

    [ObservableProperty]
    private DateTime _dataFineScadenziario = DateTime.Now.AddMonths(3);

    [ObservableProperty]
    private MargineTesoreraData? _margineTesoreria;

    public RiepilogoBancheViewModel() : this(GetOrCreateContext())
    {
    }

    public RiepilogoBancheViewModel(CGEasyDbContext context)
    {
        _context = context;
        _bancaService = new BancaService(context);
        _bancaRepo = new BancaRepository(context);
        _incassoRepo = new BancaIncassoRepository(context);
        _pagamentoRepo = new BancaPagamentoRepository(context);
        LoadRiepilogo();
    }

    private static CGEasyDbContext GetOrCreateContext()
    {
        var context = App.GetService<CGEasyDbContext>();
        if (context == null)
        {
            context = new CGEasyDbContext();
            // Singleton context - no special marking needed in EF Core
        }
        return context;
    }

    /// <summary>
    /// Carica il riepilogo consolidato
    /// </summary>
    public void LoadRiepilogo()
    {
        Riepilogo = _bancaService.GetRiepilogoTotale();
        AlertsTotali = new ObservableCollection<AlertBanca>(_bancaService.GetAllAlerts());
        LoadScadenziario();
    }

    /// <summary>
    /// Carica lo scadenziario consolidato
    /// </summary>
    public void LoadScadenziario()
    {
        ScadenziarioConsolidato = _bancaService.GetScadenziarioConsolidato(DataInizioScadenziario, DataFineScadenziario);
    }

    /// <summary>
    /// Aggiorna i dati
    /// </summary>
    [RelayCommand]
    private void Aggiorna()
    {
        LoadRiepilogo();
    }

    /// <summary>
    /// Filtra scadenziario per periodo
    /// </summary>
    [RelayCommand]
    private void FiltraScadenziario()
    {
        LoadScadenziario();
    }

    /// <summary>
    /// Carica il Margine di Tesoreria consolidato (pivot di tutte le banche)
    /// </summary>
    public void LoadMargineTesoreria()
    {
        var banche = _bancaRepo.GetAll().ToList();
        System.Diagnostics.Debug.WriteLine($"LoadMargineTesoreria: Trovate {banche.Count} banche");
        
        if (!banche.Any())
        {
            MargineTesoreria = null;
            System.Diagnostics.Debug.WriteLine("LoadMargineTesoreria: Nessuna banca trovata");
            return;
        }

        var margine = new MargineTesoreraData();
        margine.Banche = banche.Select(b => b.NomeBanca).ToList();
        margine.FidoTotaleAnticipi = banche.Sum(b => b.AnticipoFattureMassimo);
        
        // Memorizza il Fido C/C per ogni banca
        foreach (var banca in banche)
        {
            margine.FidoCCPerBanca[banca.NomeBanca] = banca.FidoCCAccordato;
        }

        // Raccogli tutti gli incassi e pagamenti di tutte le banche
        var tuttiIncassi = new List<(BancaIncasso incasso, string nomeBanca)>();
        var tuttiPagamenti = new List<(BancaPagamento pagamento, string nomeBanca)>();

        foreach (var banca in banche)
        {
            var incassi = _incassoRepo.GetByBancaId(banca.Id);
            var pagamenti = _pagamentoRepo.GetByBancaId(banca.Id);
            
            System.Diagnostics.Debug.WriteLine($"LoadMargineTesoreria: Banca '{banca.NomeBanca}' - {incassi.Count()} incassi, {pagamenti.Count()} pagamenti");

            foreach (var inc in incassi)
            {
                tuttiIncassi.Add((inc, banca.NomeBanca));
            }

            foreach (var pag in pagamenti)
            {
                tuttiPagamenti.Add((pag, banca.NomeBanca));
            }
        }

        System.Diagnostics.Debug.WriteLine($"LoadMargineTesoreria: Totale {tuttiIncassi.Count} incassi e {tuttiPagamenti.Count} pagamenti");

        // Se non ci sono movimenti, restituisci dati vuoti
        if (!tuttiIncassi.Any() && !tuttiPagamenti.Any())
        {
            System.Diagnostics.Debug.WriteLine("LoadMargineTesoreria: Nessun movimento trovato");
            MargineTesoreria = margine;
            return;
        }

        // Determina il range di mesi da considerare
        var dataInizio = DateTime.Now;
        var dataFine = DateTime.Now.AddMonths(12);

        if (tuttiIncassi.Any() || tuttiPagamenti.Any())
        {
            var allDates = new List<DateTime>();
            if (tuttiIncassi.Any())
            {
                allDates.AddRange(tuttiIncassi.Select(t => t.incasso.DataScadenza));
                allDates.AddRange(tuttiIncassi.Where(t => t.incasso.DataInizioAnticipo.HasValue).Select(t => t.incasso.DataInizioAnticipo!.Value));
                allDates.AddRange(tuttiIncassi.Where(t => t.incasso.DataScadenzaAnticipo.HasValue).Select(t => t.incasso.DataScadenzaAnticipo!.Value));
            }
            if (tuttiPagamenti.Any())
            {
                allDates.AddRange(tuttiPagamenti.Select(t => t.pagamento.DataScadenza));
            }

            if (allDates.Any())
            {
                dataInizio = new DateTime(allDates.Min().Year, allDates.Min().Month, 1);
                dataFine = allDates.Max().AddMonths(1);
            }
        }

        // Genera lista mesi
        var mesiList = new List<string>();
        var currentDate = dataInizio;
        while (currentDate <= dataFine)
        {
            mesiList.Add(currentDate.ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT")));
            currentDate = currentDate.AddMonths(1);
        }
        margine.Mesi = mesiList;

        // Inizializza dizionari
        foreach (var mese in mesiList)
        {
            margine.FatturatoAnticipato.ValoriMensili[mese] = 0;
            margine.ResiduoAnticipabile.ValoriMensili[mese] = 0;
            margine.SaldoCorrente.ValoriMensili[mese] = 0;
            margine.Incassi.ValoriMensili[mese] = 0;
            margine.Pagamenti.ValoriMensili[mese] = 0;
            margine.SaldoDisponibile.ValoriMensili[mese] = 0;
            margine.UtilizzoFidoCC.ValoriMensili[mese] = 0;

            margine.FatturatoAnticipato.ValoriPerBanca[mese] = new Dictionary<string, decimal>();
            margine.ResiduoAnticipabile.ValoriPerBanca[mese] = new Dictionary<string, decimal>();
            margine.SaldoCorrente.ValoriPerBanca[mese] = new Dictionary<string, decimal>();
            margine.Incassi.ValoriPerBanca[mese] = new Dictionary<string, decimal>();
            margine.Pagamenti.ValoriPerBanca[mese] = new Dictionary<string, decimal>();
            margine.SaldoDisponibile.ValoriPerBanca[mese] = new Dictionary<string, decimal>();
            margine.UtilizzoFidoCC.ValoriPerBanca[mese] = new Dictionary<string, decimal>();

            margine.Incassi.DettagliMensili[mese] = new List<MovimentoMensileDettaglioConsolidato>();
            margine.Pagamenti.DettagliMensili[mese] = new List<MovimentoMensileDettaglioConsolidato>();

            foreach (var nomeBanca in margine.Banche)
            {
                margine.FatturatoAnticipato.ValoriPerBanca[mese][nomeBanca] = 0;
                margine.ResiduoAnticipabile.ValoriPerBanca[mese][nomeBanca] = 0;
                margine.SaldoCorrente.ValoriPerBanca[mese][nomeBanca] = 0;
                margine.Incassi.ValoriPerBanca[mese][nomeBanca] = 0;
                margine.Pagamenti.ValoriPerBanca[mese][nomeBanca] = 0;
                margine.SaldoDisponibile.ValoriPerBanca[mese][nomeBanca] = 0;
                margine.UtilizzoFidoCC.ValoriPerBanca[mese][nomeBanca] = 0;
            }
        }

        // Calcola incassi per mese e per banca
        foreach (var (incasso, nomeBanca) in tuttiIncassi)
        {
            // Se incassato, ignora i movimenti futuri
            if (incasso.Incassato && incasso.DataIncassoEffettivo.HasValue)
                continue;

            // Anticipo (se presente e non incassato e NON gestito in C/C)
            if (incasso.PercentualeAnticipo > 0 && incasso.DataInizioAnticipo.HasValue && !incasso.Incassato && !incasso.AnticipoGestito_CC)
            {
                var meseAnticipo = incasso.DataInizioAnticipo.Value.ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
                if (margine.Incassi.ValoriMensili.ContainsKey(meseAnticipo))
                {
                    margine.Incassi.ValoriMensili[meseAnticipo] += incasso.ImportoAnticipato;
                    margine.Incassi.ValoriPerBanca[meseAnticipo][nomeBanca] += incasso.ImportoAnticipato;

                    margine.Incassi.DettagliMensili[meseAnticipo].Add(new MovimentoMensileDettaglioConsolidato
                    {
                        NomeBanca = nomeBanca,
                        Descrizione = $"Anticipo {incasso.NomeCliente}",
                        Importo = incasso.ImportoAnticipato,
                        DataScadenza = incasso.DataInizioAnticipo.Value,
                        NumeroFattura = incasso.NumeroFatturaCliente ?? string.Empty,
                        DataFattura = incasso.DataFatturaCliente
                    });
                }
            }

            // Incasso a scadenza (totale fattura, non residuo)
            var meseScadenza = incasso.DataScadenza.ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
            if (margine.Incassi.ValoriMensili.ContainsKey(meseScadenza))
            {
                margine.Incassi.ValoriMensili[meseScadenza] += incasso.Importo; // Totale fattura
                margine.Incassi.ValoriPerBanca[meseScadenza][nomeBanca] += incasso.Importo;

                margine.Incassi.DettagliMensili[meseScadenza].Add(new MovimentoMensileDettaglioConsolidato
                {
                    NomeBanca = nomeBanca,
                    Descrizione = incasso.NomeCliente,
                    Importo = incasso.Importo,
                    DataScadenza = incasso.DataScadenza,
                    NumeroFattura = incasso.NumeroFatturaCliente ?? string.Empty,
                    DataFattura = incasso.DataFatturaCliente
                });
            }
        }

        // Calcola pagamenti (inclusi storni anticipo)
        foreach (var (pagamento, nomeBanca) in tuttiPagamenti)
        {
            if (pagamento.Pagato && pagamento.DataPagamentoEffettivo.HasValue)
                continue;

            var mesePagamento = pagamento.DataScadenza.ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
            if (margine.Pagamenti.ValoriMensili.ContainsKey(mesePagamento))
            {
                margine.Pagamenti.ValoriMensili[mesePagamento] += pagamento.Importo;
                margine.Pagamenti.ValoriPerBanca[mesePagamento][nomeBanca] += pagamento.Importo;

                margine.Pagamenti.DettagliMensili[mesePagamento].Add(new MovimentoMensileDettaglioConsolidato
                {
                    NomeBanca = nomeBanca,
                    Descrizione = pagamento.NomeFornitore,
                    Importo = pagamento.Importo,
                    DataScadenza = pagamento.DataScadenza,
                    NumeroFattura = pagamento.NumeroFatturaFornitore ?? string.Empty,
                    DataFattura = pagamento.DataFatturaFornitore
                });
            }
        }

        // Aggiungi storni anticipo nei pagamenti (SOLO SE non chiuso in C/C)
        foreach (var (incasso, nomeBanca) in tuttiIncassi)
        {
            if (incasso.PercentualeAnticipo > 0 && incasso.DataScadenzaAnticipo.HasValue && !incasso.Incassato && !incasso.AnticipoChiuso_CC)
            {
                var meseStorno = incasso.DataScadenzaAnticipo.Value.ToString("MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
                if (margine.Pagamenti.ValoriMensili.ContainsKey(meseStorno))
                {
                    margine.Pagamenti.ValoriMensili[meseStorno] += incasso.ImportoAnticipato;
                    margine.Pagamenti.ValoriPerBanca[meseStorno][nomeBanca] += incasso.ImportoAnticipato;

                    margine.Pagamenti.DettagliMensili[meseStorno].Add(new MovimentoMensileDettaglioConsolidato
                    {
                        NomeBanca = nomeBanca,
                        Descrizione = $"Storno anticipo {incasso.NomeCliente}",
                        Importo = incasso.ImportoAnticipato,
                        DataScadenza = incasso.DataScadenzaAnticipo.Value,
                        NumeroFattura = incasso.NumeroFatturaCliente ?? string.Empty,
                        DataFattura = incasso.DataFatturaCliente
                    });
                }
            }
        }

        // Calcola Fatturato Anticipato e Residuo Anticipabile per ogni mese
        foreach (var mese in mesiList)
        {
            var dataMese = DateTime.ParseExact(mese, "MMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("it-IT"));
            var primoGiornoMese = new DateTime(dataMese.Year, dataMese.Month, 1);
            var ultimoGiornoMese = primoGiornoMese.AddMonths(1).AddDays(-1);

            decimal fatturatoAnticipatoMese = 0;
            
            // Calcola per ogni banca
            foreach (var banca in banche)
            {
                decimal fatturatoAnticipatoBanca = 0;
                
                // Trova tutti gli incassi di questa banca
                var incassiBanca = tuttiIncassi.Where(t => t.nomeBanca == banca.NomeBanca);
                
                foreach (var (incasso, nomeBanca) in incassiBanca)
                {
                    if (incasso.Incassato)
                        continue;

                    // ESCLUDI gli anticipi gestiti in C/C
                    if (incasso.PercentualeAnticipo > 0 &&
                        incasso.DataInizioAnticipo.HasValue &&
                        incasso.DataInizioAnticipo.Value <= ultimoGiornoMese &&
                        !incasso.AnticipoGestito_CC)
                    {
                        if (!incasso.DataScadenzaAnticipo.HasValue || incasso.DataScadenzaAnticipo.Value >= primoGiornoMese)
                        {
                            fatturatoAnticipatoBanca += incasso.ImportoAnticipato;
                        }
                    }
                }
                
                margine.FatturatoAnticipato.ValoriPerBanca[mese][banca.NomeBanca] = fatturatoAnticipatoBanca;
                margine.ResiduoAnticipabile.ValoriPerBanca[mese][banca.NomeBanca] = banca.AnticipoFattureMassimo - fatturatoAnticipatoBanca;
                fatturatoAnticipatoMese += fatturatoAnticipatoBanca;
            }

            margine.FatturatoAnticipato.ValoriMensili[mese] = fatturatoAnticipatoMese;
            margine.ResiduoAnticipabile.ValoriMensili[mese] = margine.FidoTotaleAnticipi - fatturatoAnticipatoMese;
        }

        // Calcola saldi correnti e disponibili per banca e progressivi
        for (int i = 0; i < mesiList.Count; i++)
        {
            var mese = mesiList[i];

            foreach (var banca in banche)
            {
                if (i == 0)
                {
                    // Primo mese: usa il saldo corrente della banca
                    margine.SaldoCorrente.ValoriPerBanca[mese][banca.NomeBanca] = banca.SaldoDelGiorno;
                }
                else
                {
                    // Mesi successivi: riporta il saldo disponibile del mese precedente
                    var mesePrecedente = mesiList[i - 1];
                    margine.SaldoCorrente.ValoriPerBanca[mese][banca.NomeBanca] = margine.SaldoDisponibile.ValoriPerBanca[mesePrecedente][banca.NomeBanca];
                }

                // Calcola saldo disponibile per questa banca in questo mese
                var saldoIniziale = margine.SaldoCorrente.ValoriPerBanca[mese][banca.NomeBanca];
                var incassiMese = margine.Incassi.ValoriPerBanca[mese][banca.NomeBanca];
                var pagamentiMese = margine.Pagamenti.ValoriPerBanca[mese][banca.NomeBanca];
                margine.SaldoDisponibile.ValoriPerBanca[mese][banca.NomeBanca] = saldoIniziale + incassiMese - pagamentiMese;
            }

            // Calcola totali per il mese
            margine.SaldoCorrente.ValoriMensili[mese] = margine.SaldoCorrente.ValoriPerBanca[mese].Values.Sum();
            margine.SaldoDisponibile.ValoriMensili[mese] = margine.SaldoDisponibile.ValoriPerBanca[mese].Values.Sum();
            
            // Calcola utilizzo Fido C/C per banche in negativo
            decimal utilizzoFidoTotale = 0;
            foreach (var banca in banche)
            {
                var saldoDisponibileBanca = margine.SaldoDisponibile.ValoriPerBanca[mese][banca.NomeBanca];
                
                if (saldoDisponibileBanca < 0)
                {
                    // Banca in negativo: usa il Fido C/C
                    decimal utilizzoFido = Math.Abs(saldoDisponibileBanca);
                    margine.UtilizzoFidoCC.ValoriPerBanca[mese][banca.NomeBanca] = utilizzoFido;
                    utilizzoFidoTotale += utilizzoFido;
                }
                else
                {
                    margine.UtilizzoFidoCC.ValoriPerBanca[mese][banca.NomeBanca] = 0;
                }
            }
            
            margine.UtilizzoFidoCC.ValoriMensili[mese] = utilizzoFidoTotale;
        }

        MargineTesoreria = margine;
    }

    /// <summary>
    /// Esporta il Margine di Tesoreria in Excel
    /// </summary>
    [RelayCommand]
    private void ExportMargineTesoreriaExcel()
    {
        if (MargineTesoreria == null || !MargineTesoreria.Mesi.Any())
        {
            MessageBox.Show("Non ci sono dati da esportare.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"MargineTesoreria_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Margine Tesoreria");

            int currentRow = 1;
            int numMesi = MargineTesoreria.Mesi.Count;

            // Header principale
            worksheet.Cell(currentRow, 1).Value = "MARGINE DI TESORERIA CONSOLIDATO";
            worksheet.Range(currentRow, 1, currentRow, numMesi + 1).Merge();
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
            worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            currentRow += 2;

            // Fatturato Anticipato
            worksheet.Cell(currentRow, 1).Value = "Fatturato Anticipato";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#9B59B6");
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.FatturatoAnticipato.ValoriMensili[mese];
                worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#9B59B6");
                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            currentRow++;

            // Residuo Anticipabile
            worksheet.Cell(currentRow, 1).Value = "Residuo Anticipabile";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E67E22");
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.ResiduoAnticipabile.ValoriMensili[mese];
                worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E67E22");
                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            currentRow += 2;

            // Header mesi
            worksheet.Cell(currentRow, 1).Value = "Categoria";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkGray;
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            for (int i = 0; i < numMesi; i++)
            {
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkGray;
                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            currentRow++;

            // Saldo Corrente (per banca + totale)
            worksheet.Cell(currentRow, 1).Value = "Saldo Corrente";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            currentRow++;

            foreach (var nomeBanca in MargineTesoreria.Banche)
            {
                worksheet.Cell(currentRow, 1).Value = $"  {nomeBanca}";
                for (int i = 0; i < numMesi; i++)
                {
                    var mese = MargineTesoreria.Mesi[i];
                    worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.SaldoCorrente.ValoriPerBanca[mese][nomeBanca];
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                }
                currentRow++;
            }

            worksheet.Cell(currentRow, 1).Value = "TOTALE Saldo Corrente";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.SaldoCorrente.ValoriMensili[mese];
                worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
            }
            currentRow += 2;

            // Incassi (totale + dettagli per banca)
            worksheet.Cell(currentRow, 1).Value = "Incassi";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#27AE60");
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.Incassi.ValoriMensili[mese];
                worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#27AE60");
                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            currentRow++;

            // Dettagli incassi per ogni mese
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                var dettagli = MargineTesoreria.Incassi.DettagliMensili[mese];
                if (dettagli.Any())
                {
                    foreach (var dettaglio in dettagli.OrderBy(d => d.NomeBanca).ThenBy(d => d.DataScadenza))
                    {
                        string descrizione = $"[{dettaglio.NomeBanca}] {dettaglio.Descrizione}";
                        if (!string.IsNullOrEmpty(dettaglio.NumeroFattura))
                            descrizione += $" - Fatt. {dettaglio.NumeroFattura}";
                        if (dettaglio.DataFattura.HasValue)
                            descrizione += $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}";
                        descrizione += $" - Scad: {dettaglio.DataScadenza:dd/MM/yyyy}";

                        worksheet.Cell(currentRow, 1).Value = descrizione;
                        worksheet.Cell(currentRow, i + 2).Value = dettaglio.Importo;
                        worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                        
                        // Colore per anticipi e storni
                        if (dettaglio.Descrizione.StartsWith("Anticipo "))
                        {
                            worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#D4EDDA");
                        }
                        
                        currentRow++;
                    }
                }
            }
            currentRow++;

            // Pagamenti (totale + dettagli per banca)
            worksheet.Cell(currentRow, 1).Value = "Pagamenti";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E74C3C");
            worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.Pagamenti.ValoriMensili[mese];
                worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E74C3C");
                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            currentRow++;

            // Dettagli pagamenti per ogni mese
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                var dettagli = MargineTesoreria.Pagamenti.DettagliMensili[mese];
                if (dettagli.Any())
                {
                    foreach (var dettaglio in dettagli.OrderBy(d => d.NomeBanca).ThenBy(d => d.DataScadenza))
                    {
                        string descrizione = $"[{dettaglio.NomeBanca}] {dettaglio.Descrizione}";
                        if (!string.IsNullOrEmpty(dettaglio.NumeroFattura))
                            descrizione += $" - Fatt. {dettaglio.NumeroFattura}";
                        if (dettaglio.DataFattura.HasValue)
                            descrizione += $" del {dettaglio.DataFattura.Value:dd/MM/yyyy}";
                        descrizione += $" - Scad: {dettaglio.DataScadenza:dd/MM/yyyy}";

                        worksheet.Cell(currentRow, 1).Value = descrizione;
                        worksheet.Cell(currentRow, i + 2).Value = dettaglio.Importo;
                        worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                        
                        // Colore per storni anticipo
                        if (dettaglio.Descrizione.StartsWith("Storno anticipo"))
                        {
                            worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FFE4B5");
                        }
                        
                        currentRow++;
                    }
                }
            }
            currentRow += 2;

            // Saldo Disponibile (per banca + totale)
            worksheet.Cell(currentRow, 1).Value = "Saldo Disponibile";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
            currentRow++;

            foreach (var nomeBanca in MargineTesoreria.Banche)
            {
                worksheet.Cell(currentRow, 1).Value = $"  {nomeBanca}";
                for (int i = 0; i < numMesi; i++)
                {
                    var mese = MargineTesoreria.Mesi[i];
                    worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.SaldoDisponibile.ValoriPerBanca[mese][nomeBanca];
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                }
                currentRow++;
            }

            worksheet.Cell(currentRow, 1).Value = "TOTALE Saldo Disponibile";
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            for (int i = 0; i < numMesi; i++)
            {
                var mese = MargineTesoreria.Mesi[i];
                worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.SaldoDisponibile.ValoriMensili[mese];
                worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#3498DB");
                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            }
            currentRow += 2;

            // Utilizzo Fido C/C (solo se ci sono banche in negativo)
            bool hasFidoUtilizzato = MargineTesoreria.UtilizzoFidoCC.ValoriMensili.Any(kvp => kvp.Value > 0);
            
            if (hasFidoUtilizzato)
            {
                worksheet.Cell(currentRow, 1).Value = "⚠️ UTILIZZO FIDO C/C";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FF6B6B");
                worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                currentRow++;

                // Per ogni banca che ha utilizzato il fido
                foreach (var nomeBanca in MargineTesoreria.Banche)
                {
                    bool bancaHaUtilizzato = false;
                    foreach (var mese in MargineTesoreria.Mesi)
                    {
                        if (MargineTesoreria.UtilizzoFidoCC.ValoriPerBanca[mese][nomeBanca] > 0)
                        {
                            bancaHaUtilizzato = true;
                            break;
                        }
                    }

                    if (bancaHaUtilizzato)
                    {
                        // Intestazione banca
                        worksheet.Cell(currentRow, 1).Value = $"⚠️ {nomeBanca}";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FFF3CD");
                        currentRow++;

                        decimal fidoCC = MargineTesoreria.FidoCCPerBanca[nomeBanca];

                        // Fido C/C Disponibile
                        worksheet.Cell(currentRow, 1).Value = "  Fido C/C Disponibile";
                        for (int i = 0; i < numMesi; i++)
                        {
                            worksheet.Cell(currentRow, i + 2).Value = fidoCC;
                            worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                        }
                        currentRow++;

                        // Utilizzo Fido
                        worksheet.Cell(currentRow, 1).Value = "  Utilizzo Fido";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        for (int i = 0; i < numMesi; i++)
                        {
                            var mese = MargineTesoreria.Mesi[i];
                            var utilizzo = MargineTesoreria.UtilizzoFidoCC.ValoriPerBanca[mese][nomeBanca];
                            worksheet.Cell(currentRow, i + 2).Value = utilizzo;
                            worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                            if (utilizzo > 0)
                            {
                                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#DC3545");
                            }
                        }
                        currentRow++;

                        // Fido Residuo
                        worksheet.Cell(currentRow, 1).Value = "  Fido Residuo";
                        for (int i = 0; i < numMesi; i++)
                        {
                            var mese = MargineTesoreria.Mesi[i];
                            var utilizzo = MargineTesoreria.UtilizzoFidoCC.ValoriPerBanca[mese][nomeBanca];
                            var residuo = fidoCC - utilizzo;
                            worksheet.Cell(currentRow, i + 2).Value = residuo;
                            worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                            if (residuo <= 0)
                            {
                                worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#DC3545");
                            }
                            else
                            {
                                worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#28A745");
                            }
                        }
                        currentRow++;
                    }
                }

                // Totali consolidati
                worksheet.Cell(currentRow, 1).Value = "📊 TOTALI CONSOLIDATI";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                currentRow++;

                decimal fidoTotale = MargineTesoreria.FidoCCPerBanca.Values.Sum();

                // Fido C/C Totale
                worksheet.Cell(currentRow, 1).Value = "  Fido C/C Totale";
                for (int i = 0; i < numMesi; i++)
                {
                    worksheet.Cell(currentRow, i + 2).Value = fidoTotale;
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                }
                currentRow++;

                // Utilizzo Totale
                worksheet.Cell(currentRow, 1).Value = "  Utilizzo Totale";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                for (int i = 0; i < numMesi; i++)
                {
                    var mese = MargineTesoreria.Mesi[i];
                    worksheet.Cell(currentRow, i + 2).Value = MargineTesoreria.UtilizzoFidoCC.ValoriMensili[mese];
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                    worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#DC3545");
                }
                currentRow++;

                // Fido Residuo Totale
                worksheet.Cell(currentRow, 1).Value = "  Fido Residuo Totale";
                for (int i = 0; i < numMesi; i++)
                {
                    var mese = MargineTesoreria.Mesi[i];
                    var utilizzo = MargineTesoreria.UtilizzoFidoCC.ValoriMensili[mese];
                    var residuo = fidoTotale - utilizzo;
                    worksheet.Cell(currentRow, i + 2).Value = residuo;
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                    if (residuo <= 0)
                    {
                        worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#DC3545");
                    }
                    else
                    {
                        worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#28A745");
                    }
                }
            }

            // Formattazione finale
            worksheet.Columns().AdjustToContents();
            worksheet.Column(1).Width = 50;

            workbook.SaveAs(saveFileDialog.FileName);
            MessageBox.Show($"File Excel esportato con successo!\n\n{saveFileDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'esportazione: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Mostra il grafico andamento margine di tesoreria
    /// </summary>
    [RelayCommand]
    private void MostraGrafico()
    {
        if (MargineTesoreria == null || !MargineTesoreria.Mesi.Any())
        {
            MessageBox.Show("Non ci sono dati da visualizzare nel grafico.", "Informazione", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var viewModel = new GraficoMargineViewModel(MargineTesoreria);
            var window = new Views.GraficoMargineView(viewModel);
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'apertura del grafico: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

