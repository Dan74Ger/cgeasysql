using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.App.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// Rappresenta un movimento previsto (incasso, pagamento, anticipo) per il saldo previsto
/// </summary>
public class MovimentoPrevisto
{
    public DateTime DataScadenza { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Incasso", "Pagamento", "Anticipo Incasso", "Storno Anticipo"
    public string Descrizione { get; set; } = string.Empty;
    public decimal Importo { get; set; }
    public decimal SaldoProgressivo { get; set; }
    public bool Completato { get; set; }
    public bool UtilizzoFido { get; set; } // TRUE se il saldo progressivo è negativo (sto usando il fido)
    public string StatoFido { get; set; } = "✓ Liquidità"; // "⚠️ Uso Fido" o "✓ Liquidità"
    public string IconaTipo => Tipo switch
    {
        "Incasso" => "📥",
        "Pagamento" => "📤",
        "Anticipo Incasso" => "💰",
        "Storno Anticipo" => "🔄",
        "Anticipo" => "💳",
        _ => "📋"
    };
}

/// <summary>
/// Rappresenta un singolo movimento (incasso o pagamento) nel dettaglio mensile
/// </summary>
public class MovimentoMensileDettaglio
{
    public string Descrizione { get; set; } = string.Empty; // Cliente/Fornitore
    public decimal Importo { get; set; }
    public DateTime DataScadenza { get; set; }
    public string NumeroFattura { get; set; } = string.Empty;
    public DateTime? DataFattura { get; set; }
}

/// <summary>
/// Rappresenta una riga della pivot con i totali per ogni mese
/// </summary>
public class SaldoPivotRiga
{
    public string Categoria { get; set; } = string.Empty; // "Saldo Corrente", "Incassi", "Pagamenti", "Saldo Disponibile"
    public Dictionary<string, decimal> ValoriMensili { get; set; } = new Dictionary<string, decimal>();
    public Dictionary<string, string> DescrizioniMensili { get; set; } = new Dictionary<string, string>(); // Per descrizioni aggiuntive
    public Dictionary<string, List<MovimentoMensileDettaglio>> DettagliMensili { get; set; } = new Dictionary<string, List<MovimentoMensileDettaglio>>(); // Dettagli per accordion
}

/// <summary>
/// Rappresenta i dati della pivot con colonne mensili
/// </summary>
public class SaldoPivotData
{
    public List<string> Mesi { get; set; } = new List<string>(); // Es. "Gen 2025", "Feb 2025", ecc.
    public SaldoPivotRiga FatturatoAnticipato { get; set; } = new SaldoPivotRiga { Categoria = "Fatturato Anticipato" };
    public SaldoPivotRiga ResiduoAnticipabile { get; set; } = new SaldoPivotRiga { Categoria = "Residuo Anticipabile" };
    public SaldoPivotRiga SaldoCorrente { get; set; } = new SaldoPivotRiga { Categoria = "Saldo Corrente" };
    public SaldoPivotRiga Incassi { get; set; } = new SaldoPivotRiga { Categoria = "Incassi" };
    public SaldoPivotRiga Pagamenti { get; set; } = new SaldoPivotRiga { Categoria = "Pagamenti" };
    public SaldoPivotRiga SaldoDisponibile { get; set; } = new SaldoPivotRiga { Categoria = "Saldo Disponibile" };
    public string NomeBanca { get; set; } = string.Empty; // Nome della banca per la descrizione
    public decimal FidoMaxAnticipi { get; set; } = 0; // Fido massimo anticipi della banca
}

/// <summary>
/// ViewModel per il dettaglio di una singola banca
/// con gestione incassi, pagamenti, anticipi
/// </summary>
public partial class BancaDettaglioViewModel : ObservableObject
{
    private readonly BancaRepository _bancaRepo;
    private readonly BancaIncassoRepository _incassoRepo;
    private readonly BancaPagamentoRepository _pagamentoRepo;
    private readonly BancaUtilizzoAnticipoRepository _anticipoRepo;
    private readonly FinanziamentoImportRepository _finanziamentoRepo;
    private readonly BancaService _bancaService;
    private readonly AuditLogService _auditService;

    [ObservableProperty]
    private Banca? _banca;

    [ObservableProperty]
    private int _bancaId;

    // Tab Incassi
    [ObservableProperty]
    private ObservableCollection<BancaIncasso> _incassi = new();

    [ObservableProperty]
    private BancaIncasso? _incassoSelezionato;

    // Filtri Incassi
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IncassiFiltrati))]
    private string _filtroCliente = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IncassiFiltrati))]
    private int? _filtroAnno;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IncassiFiltrati))]
    private int? _filtroMese;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IncassiFiltrati))]
    private string _filtroIncassato = "Tutti"; // "Tutti", "Si", "No"

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IncassiFiltrati))]
    private string _filtroConAnticipo = "Tutti"; // "Tutti", "Si", "No"

    // Proprietà calcolata per Incassi filtrati
    public ObservableCollection<BancaIncasso> IncassiFiltrati
    {
        get
        {
            var incassi = Incassi.ToList();

            // Applica filtri
            if (!string.IsNullOrWhiteSpace(FiltroCliente))
            {
                incassi = incassi.Where(i => i.NomeCliente.Contains(FiltroCliente, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (FiltroAnno.HasValue)
            {
                incassi = incassi.Where(i => i.Anno == FiltroAnno.Value).ToList();
            }

            if (FiltroMese.HasValue)
            {
                incassi = incassi.Where(i => i.Mese == FiltroMese.Value).ToList();
            }

            if (FiltroIncassato == "Si")
            {
                incassi = incassi.Where(i => i.Incassato).ToList();
            }
            else if (FiltroIncassato == "No")
            {
                incassi = incassi.Where(i => !i.Incassato).ToList();
            }

            if (FiltroConAnticipo == "Si")
            {
                incassi = incassi.Where(i => i.PercentualeAnticipo > 0).ToList();
            }
            else if (FiltroConAnticipo == "No")
            {
                incassi = incassi.Where(i => i.PercentualeAnticipo == 0).ToList();
            }

            return new ObservableCollection<BancaIncasso>(incassi);
        }
    }

    // Tab Pagamenti
    [ObservableProperty]
    private ObservableCollection<BancaPagamento> _pagamenti = new();

    [ObservableProperty]
    private BancaPagamento? _pagamentoSelezionato;

    // Filtri Pagamenti
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PagamentiFiltrati))]
    private string _filtroFornitore = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PagamentiFiltrati))]
    private int? _filtroAnnoPagamenti;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PagamentiFiltrati))]
    private int? _filtroMesePagamenti;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PagamentiFiltrati))]
    private string _filtroPagato = "Tutti"; // "Tutti", "Si", "No"

    // Proprietà calcolata per Pagamenti filtrati
    public ObservableCollection<BancaPagamento> PagamentiFiltrati
    {
        get
        {
            var pagamenti = Pagamenti.ToList();

            // Applica filtri
            if (!string.IsNullOrWhiteSpace(FiltroFornitore))
            {
                pagamenti = pagamenti.Where(p => p.NomeFornitore.Contains(FiltroFornitore, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (FiltroAnnoPagamenti.HasValue)
            {
                pagamenti = pagamenti.Where(p => p.Anno == FiltroAnnoPagamenti.Value).ToList();
            }

            if (FiltroMesePagamenti.HasValue)
            {
                pagamenti = pagamenti.Where(p => p.Mese == FiltroMesePagamenti.Value).ToList();
            }

            if (FiltroPagato == "Si")
            {
                pagamenti = pagamenti.Where(p => p.Pagato).ToList();
            }
            else if (FiltroPagato == "No")
            {
                pagamenti = pagamenti.Where(p => !p.Pagato).ToList();
            }

            return new ObservableCollection<BancaPagamento>(pagamenti);
        }
    }

    // Tab Anticipi
    [ObservableProperty]
    private ObservableCollection<BancaUtilizzoAnticipo> _anticipi = new();

    [ObservableProperty]
    private BancaUtilizzoAnticipo? _anticipoSelezionato;

    // Statistiche
    [ObservableProperty]
    private decimal _saldoCorrente;

    [ObservableProperty]
    private decimal _fidoResiduo;

    [ObservableProperty]
    private decimal _percentualeUtilizzoFido;

    [ObservableProperty]
    private decimal _anticipoResiduo;

    [ObservableProperty]
    private decimal _totaleInteressiAttivi;

    [ObservableProperty]
    private decimal _totaleFatturatoAnticipato;

    [ObservableProperty]
    private bool _isMassimaleAnticipoSuperato;

    [ObservableProperty]
    private ObservableCollection<AlertBanca> _alerts = new();

    // Tab Saldo Previsto
    [ObservableProperty]
    private ObservableCollection<MovimentoPrevisto> _movimentiPrevisti = new();

    // Tab Saldo Previsto Pivot
    [ObservableProperty]
    private SaldoPivotData? _saldoPivot;

    public BancaDettaglioViewModel() : this(GetOrCreateContext(), 0)
    {
    }

    public BancaDettaglioViewModel(CGEasyDbContext context, int bancaId)
    {
        _bancaRepo = new BancaRepository(context);
        _incassoRepo = new BancaIncassoRepository(context);
        _pagamentoRepo = new BancaPagamentoRepository(context);
        _anticipoRepo = new BancaUtilizzoAnticipoRepository(context);
        _finanziamentoRepo = new FinanziamentoImportRepository(context);
        _bancaService = new BancaService(context);
        _auditService = new AuditLogService(context);

        BancaId = bancaId;
        LoadBanca();
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
    /// Carica tutti i dati della banca
    /// </summary>
    public void LoadBanca()
    {
        if (BancaId <= 0) return;

        Banca = _bancaRepo.GetById(BancaId);
        if (Banca == null)
        {
            MessageBox.Show("Banca non trovata!", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        LoadIncassi();
        LoadPagamenti();
        LoadAnticipi();
        LoadStatistiche();
        LoadAlerts();
        LoadMovimentiPrevisti();
        // LoadSaldoPivot(); // Ottimizzazione: caricato solo quando si apre il tab Pivot
    }

    /// <summary>
    /// Carica gli incassi della banca
    /// </summary>
    public void LoadIncassi()
    {
        var incassi = _incassoRepo.GetByBancaId(BancaId).OrderBy(i => i.DataScadenza).ToList();
        Incassi = new ObservableCollection<BancaIncasso>(incassi);
        OnPropertyChanged(nameof(IncassiFiltrati)); // Forza l'aggiornamento della grid filtrata
    }

    [RelayCommand]
    private void PulisciFiltriIncassi()
    {
        FiltroCliente = string.Empty;
        FiltroAnno = null;
        FiltroMese = null;
        FiltroIncassato = "Tutti";
        FiltroConAnticipo = "Tutti";
    }

    /// <summary>
    /// Carica i pagamenti della banca
    /// </summary>
    public void LoadPagamenti()
    {
        var pagamenti = _pagamentoRepo.GetByBancaId(BancaId).OrderBy(p => p.DataScadenza).ToList();
        Pagamenti = new ObservableCollection<BancaPagamento>(pagamenti);
        OnPropertyChanged(nameof(PagamentiFiltrati)); // Forza l'aggiornamento della grid filtrata
    }

    [RelayCommand]
    private void PulisciFiltriPagamenti()
    {
        FiltroFornitore = string.Empty;
        FiltroAnnoPagamenti = null;
        FiltroMesePagamenti = null;
        FiltroPagato = "Tutti";
    }

    /// <summary>
    /// Carica gli anticipi della banca
    /// </summary>
    public void LoadAnticipi()
    {
        var anticipi = _anticipoRepo.GetByBancaId(BancaId).OrderBy(a => a.DataInizioUtilizzo).ToList();
        
        // Calcola interessi per ogni anticipo
        foreach (var anticipo in anticipi)
        {
            anticipo.InteressiMaturati = _bancaService.CalcolaInteressiUtilizzo(anticipo.Id);
        }
        
        Anticipi = new ObservableCollection<BancaUtilizzoAnticipo>(anticipi);
    }

    /// <summary>
    /// Carica le statistiche della banca
    /// </summary>
    public void LoadStatistiche()
    {
        SaldoCorrente = _bancaService.GetSaldoCorrente(BancaId);
        FidoResiduo = _bancaService.GetFidoResiduo(BancaId);
        
        // Calcola totale fatturato anticipato (somma di ImportoAnticipato degli incassi NON ancora incassati)
        // ESCLUDI gli anticipi gestiti in C/C (AnticipoGestito_CC = true)
        TotaleFatturatoAnticipato = _incassoRepo.GetAll()
            .Where(i => i.BancaId == BancaId && !i.Incassato && !i.AnticipoGestito_CC)
            .Sum(i => i.ImportoAnticipato);
        
        // Calcola anticipo residuo: Fido Max Anticipi - Totale ImportoAnticipato degli incassi NON incassati
        var banca = _bancaRepo.GetById(BancaId);
        if (banca != null)
        {
            AnticipoResiduo = banca.AnticipoFattureMassimo - TotaleFatturatoAnticipato;
        }
        else
        {
            AnticipoResiduo = 0;
        }
        
        // Calcola percentuale utilizzo anticipi basato sugli incassi NON incassati
        if (banca != null && banca.AnticipoFattureMassimo > 0)
        {
            PercentualeUtilizzoFido = (TotaleFatturatoAnticipato / banca.AnticipoFattureMassimo) * 100;
        }
        else
        {
            PercentualeUtilizzoFido = 0;
        }
        
        TotaleInteressiAttivi = _bancaService.GetTotaleInteressiAttivi(BancaId);
        
        // Verifica se il massimale anticipo è superato
        IsMassimaleAnticipoSuperato = AnticipoResiduo < 0;
    }

    /// <summary>
    /// Carica gli alert della banca
    /// </summary>
    public void LoadAlerts()
    {
        var alerts = _bancaService.GetAlerts(BancaId);
        Alerts = new ObservableCollection<AlertBanca>(alerts);
    }

    /// <summary>
    /// Carica i movimenti previsti (incassi, pagamenti) ordinati per scadenza
    /// con calcolo del saldo progressivo previsto
    /// </summary>
    public void LoadMovimentiPrevisti()
    {
        var movimenti = new List<MovimentoPrevisto>();

        // INCASSI NON ANCORA INCASSATI
        foreach (var incasso in _incassoRepo.GetAll().Where(i => i.BancaId == BancaId && !i.Incassato))
        {
            // Costruisci info fattura (Numero e Data)
            string infoFattura = string.Empty;
            if (!string.IsNullOrWhiteSpace(incasso.NumeroFatturaCliente))
            {
                infoFattura = $" - Fatt. {incasso.NumeroFatturaCliente}";
                if (incasso.DataFatturaCliente.HasValue)
                {
                    infoFattura += $" del {incasso.DataFatturaCliente.Value:dd/MM/yyyy}";
                }
            }

            // CASO 1: Incasso CON anticipo (ImportoAnticipato > 0)
            if (incasso.ImportoAnticipato > 0)
            {
                // RIGA 1: Anticipo (entrata immediata)
                // SOLO SE non è gestito in C/C
                if (incasso.DataInizioAnticipo.HasValue && !incasso.AnticipoGestito_CC)
                {
                    movimenti.Add(new MovimentoPrevisto
                    {
                        DataScadenza = incasso.DataInizioAnticipo.Value,
                        Tipo = "Anticipo Incasso",
                        Descrizione = $"Anticipo fattura {incasso.NomeCliente}{infoFattura}",
                        Importo = incasso.ImportoAnticipato, // POSITIVO
                        Completato = false
                    });
                }

                // RIGA 2: Storno Anticipo (uscita per chiudere anticipo)
                // SOLO SE non è chiuso in C/C
                if (incasso.DataScadenzaAnticipo.HasValue && !incasso.AnticipoChiuso_CC)
                {
                    movimenti.Add(new MovimentoPrevisto
                    {
                        DataScadenza = incasso.DataScadenzaAnticipo.Value,
                        Tipo = "Storno Anticipo",
                        Descrizione = $"Storno anticipo {incasso.NomeCliente}{infoFattura}",
                        Importo = -incasso.ImportoAnticipato, // NEGATIVO
                        Completato = false
                    });
                }

                // RIGA 3: Incasso Fattura Totale a Scadenza
                movimenti.Add(new MovimentoPrevisto
                {
                    DataScadenza = incasso.DataScadenza,
                    Tipo = "Incasso",
                    Descrizione = $"Incasso {incasso.NomeCliente} (fattura totale){infoFattura}",
                    Importo = incasso.Importo, // POSITIVO (importo totale della fattura)
                    Completato = false
                });
            }
            else
            {
                // CASO 2: Incasso SENZA anticipo (ImportoAnticipato = 0)
                // RIGA UNICA: Incasso totale
                movimenti.Add(new MovimentoPrevisto
                {
                    DataScadenza = incasso.DataScadenza,
                    Tipo = "Incasso",
                    Descrizione = $"Incasso {incasso.NomeCliente}{infoFattura}",
                    Importo = incasso.Importo, // POSITIVO (importo totale)
                    Completato = false
                });
            }
        }

        // PAGAMENTI NON ANCORA PAGATI
        foreach (var pagamento in _pagamentoRepo.GetAll().Where(p => p.BancaId == BancaId && !p.Pagato))
        {
            // Costruisci info fattura (Numero e Data)
            string infoFattura = string.Empty;
            if (!string.IsNullOrWhiteSpace(pagamento.NumeroFatturaFornitore))
            {
                infoFattura = $" - Fatt. {pagamento.NumeroFatturaFornitore}";
                if (pagamento.DataFatturaFornitore.HasValue)
                {
                    infoFattura += $" del {pagamento.DataFatturaFornitore.Value:dd/MM/yyyy}";
                }
            }

            movimenti.Add(new MovimentoPrevisto
            {
                DataScadenza = pagamento.DataScadenza,
                Tipo = "Pagamento",
                Descrizione = $"{pagamento.NomeFornitore} - {pagamento.Anno}/{pagamento.Mese:00}{infoFattura}",
                Importo = -pagamento.Importo, // Negativo perché è un'uscita
                Completato = false
            });
        }

        // Ordina per data scadenza
        movimenti = movimenti.OrderBy(m => m.DataScadenza).ToList();

        // Calcola saldo progressivo
        decimal saldoProgressivo = SaldoCorrente;
        foreach (var movimento in movimenti)
        {
            saldoProgressivo += movimento.Importo;
            movimento.SaldoProgressivo = saldoProgressivo;
            
            // Determina se si sta utilizzando il fido (saldo negativo)
            movimento.UtilizzoFido = saldoProgressivo < 0;
            movimento.StatoFido = movimento.UtilizzoFido ? "⚠️ Uso Fido" : "✓ Liquidità";
        }

        MovimentiPrevisti = new ObservableCollection<MovimentoPrevisto>(movimenti);
    }

    /// <summary>
    /// Carica i dati per la pivot mensile del saldo previsto
    /// </summary>
    public void LoadSaldoPivot()
    {
        // Usa gli stessi movimenti del saldo previsto
        var movimenti = MovimentiPrevisti.ToList();
        
        if (movimenti.Count == 0)
        {
            SaldoPivot = new SaldoPivotData();
            return;
        }

        var pivot = new SaldoPivotData();
        
        // Imposta il nome della banca e fido
        if (Banca != null)
        {
            pivot.NomeBanca = Banca.NomeBanca;
            pivot.FidoMaxAnticipi = Banca.AnticipoFattureMassimo;
        }
        
        // Determina l'intervallo di mesi da mostrare
        var dataInizio = movimenti.Min(m => m.DataScadenza);
        var dataFine = movimenti.Max(m => m.DataScadenza);
        
        // Genera i mesi da mostrare (dall'inizio alla fine)
        var meseCorrente = new DateTime(dataInizio.Year, dataInizio.Month, 1);
        var meseFinale = new DateTime(dataFine.Year, dataFine.Month, 1);
        
        var nomiMesi = new[] { "Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic" };
        
        while (meseCorrente <= meseFinale)
        {
            string nomeMese = $"{nomiMesi[meseCorrente.Month - 1]} {meseCorrente.Year}";
            pivot.Mesi.Add(nomeMese);
            
            // Inizializza i valori a zero
            pivot.FatturatoAnticipato.ValoriMensili[nomeMese] = 0;
            pivot.ResiduoAnticipabile.ValoriMensili[nomeMese] = 0;
            pivot.SaldoCorrente.ValoriMensili[nomeMese] = 0;
            pivot.SaldoCorrente.DescrizioniMensili[nomeMese] = string.Empty;
            pivot.Incassi.ValoriMensili[nomeMese] = 0;
            pivot.Incassi.DettagliMensili[nomeMese] = new List<MovimentoMensileDettaglio>();
            pivot.Pagamenti.ValoriMensili[nomeMese] = 0;
            pivot.Pagamenti.DettagliMensili[nomeMese] = new List<MovimentoMensileDettaglio>();
            pivot.SaldoDisponibile.ValoriMensili[nomeMese] = 0;
            
            meseCorrente = meseCorrente.AddMonths(1);
        }
        
        // Ottieni gli incassi e pagamenti NON completati per popolare i dettagli
        var incassiNonCompletati = _incassoRepo.GetAll().Where(i => i.BancaId == BancaId && !i.Incassato).ToList();
        var pagamentiNonCompletati = _pagamentoRepo.GetAll().Where(p => p.BancaId == BancaId && !p.Pagato).ToList();
        
        // Calcola gli aggregati per ogni mese
        foreach (var movimento in movimenti)
        {
            var meseMovimento = new DateTime(movimento.DataScadenza.Year, movimento.DataScadenza.Month, 1);
            string nomeMese = $"{nomiMesi[meseMovimento.Month - 1]} {meseMovimento.Year}";
            
            if (pivot.Mesi.Contains(nomeMese))
            {
                // Aggrega in base al tipo
                if (movimento.Tipo == "Incasso" || movimento.Tipo == "Anticipo Incasso")
                {
                    pivot.Incassi.ValoriMensili[nomeMese] += movimento.Importo;
                }
                else if (movimento.Tipo == "Pagamento" || movimento.Tipo == "Storno Anticipo")
                {
                    pivot.Pagamenti.ValoriMensili[nomeMese] += Math.Abs(movimento.Importo);
                }
            }
        }
        
        // Popola i dettagli degli incassi per ogni mese
        foreach (var incasso in incassiNonCompletati)
        {
            var meseIncasso = new DateTime(incasso.DataScadenza.Year, incasso.DataScadenza.Month, 1);
            string nomeMese = $"{nomiMesi[meseIncasso.Month - 1]} {meseIncasso.Year}";
            
            if (pivot.Mesi.Contains(nomeMese))
            {
                // Aggiungi i movimenti relativi a questo incasso
                
                // 1. Se ha anticipo, aggiungi l'anticipo (SOLO SE non gestito in C/C)
                if (incasso.ImportoAnticipato > 0 && incasso.DataInizioAnticipo.HasValue && !incasso.AnticipoGestito_CC)
                {
                    var meseAnticipo = new DateTime(incasso.DataInizioAnticipo.Value.Year, incasso.DataInizioAnticipo.Value.Month, 1);
                    string nomeMeseAnticipo = $"{nomiMesi[meseAnticipo.Month - 1]} {meseAnticipo.Year}";
                    
                    if (pivot.Mesi.Contains(nomeMeseAnticipo))
                    {
                        pivot.Incassi.DettagliMensili[nomeMeseAnticipo].Add(new MovimentoMensileDettaglio
                        {
                            Descrizione = $"Anticipo {incasso.NomeCliente}",
                            Importo = incasso.ImportoAnticipato,
                            DataScadenza = incasso.DataInizioAnticipo.Value,
                            NumeroFattura = incasso.NumeroFatturaCliente ?? "",
                            DataFattura = incasso.DataFatturaCliente
                        });
                    }
                }
                
                // 2. Aggiungi l'incasso totale alla scadenza
                pivot.Incassi.DettagliMensili[nomeMese].Add(new MovimentoMensileDettaglio
                {
                    Descrizione = $"{incasso.NomeCliente} (fattura totale)",
                    Importo = incasso.Importo, // Importo totale della fattura
                    DataScadenza = incasso.DataScadenza,
                    NumeroFattura = incasso.NumeroFatturaCliente ?? "",
                    DataFattura = incasso.DataFatturaCliente
                });
            }
        }
        
        // Popola i dettagli dei pagamenti per ogni mese
        foreach (var pagamento in pagamentiNonCompletati)
        {
            var mesePagamento = new DateTime(pagamento.DataScadenza.Year, pagamento.DataScadenza.Month, 1);
            string nomeMese = $"{nomiMesi[mesePagamento.Month - 1]} {mesePagamento.Year}";
            
            if (pivot.Mesi.Contains(nomeMese))
            {
                pivot.Pagamenti.DettagliMensili[nomeMese].Add(new MovimentoMensileDettaglio
                {
                    Descrizione = pagamento.NomeFornitore,
                    Importo = pagamento.Importo,
                    DataScadenza = pagamento.DataScadenza,
                    NumeroFattura = pagamento.NumeroFatturaFornitore ?? "",
                    DataFattura = pagamento.DataFatturaFornitore
                });
            }
        }
        
        // Popola gli storni anticipo nei dettagli dei PAGAMENTI (uscite)
        foreach (var incasso in incassiNonCompletati)
        {
            // Se ha anticipo, aggiungi lo storno nella data scadenza anticipo (SOLO SE non chiuso in C/C)
            if (incasso.ImportoAnticipato > 0 && incasso.DataScadenzaAnticipo.HasValue && !incasso.AnticipoChiuso_CC)
            {
                var meseStorno = new DateTime(incasso.DataScadenzaAnticipo.Value.Year, incasso.DataScadenzaAnticipo.Value.Month, 1);
                string nomeMeseStorno = $"{nomiMesi[meseStorno.Month - 1]} {meseStorno.Year}";
                
                if (pivot.Mesi.Contains(nomeMeseStorno))
                {
                    pivot.Pagamenti.DettagliMensili[nomeMeseStorno].Add(new MovimentoMensileDettaglio
                    {
                        Descrizione = $"Storno anticipo {incasso.NomeCliente}",
                        Importo = incasso.ImportoAnticipato,
                        DataScadenza = incasso.DataScadenzaAnticipo.Value,
                        NumeroFattura = incasso.NumeroFatturaCliente ?? "",
                        DataFattura = incasso.DataFatturaCliente
                    });
                }
            }
        }
        
        // Calcola il saldo progressivo per ogni mese con riporto
        decimal saldoProgressivo = SaldoCorrente;
        
        for (int i = 0; i < pivot.Mesi.Count; i++)
        {
            string mese = pivot.Mesi[i];
            var primoGiornoMese = new DateTime(
                int.Parse(mese.Split(' ')[1]),
                Array.IndexOf(nomiMesi, mese.Split(' ')[0]) + 1,
                1
            );
            var ultimoGiornoMese = primoGiornoMese.AddMonths(1).AddDays(-1);
            
            // Calcola il fatturato anticipato attivo in questo mese
            // (anticipi che sono iniziati e non ancora scaduti)
            // ESCLUDI gli anticipi gestiti in C/C
            decimal fatturatoAnticipatoMese = 0;
            foreach (var incasso in incassiNonCompletati.Where(i => i.ImportoAnticipato > 0 && !i.AnticipoGestito_CC))
            {
                // L'anticipo è attivo se:
                // - La data inizio anticipo è <= ultimo giorno del mese
                // - La data scadenza anticipo è >= primo giorno del mese (o non esiste ancora)
                if (incasso.DataInizioAnticipo.HasValue &&
                    incasso.DataInizioAnticipo.Value <= ultimoGiornoMese)
                {
                    if (!incasso.DataScadenzaAnticipo.HasValue ||
                        incasso.DataScadenzaAnticipo.Value >= primoGiornoMese)
                    {
                        fatturatoAnticipatoMese += incasso.ImportoAnticipato;
                    }
                }
            }
            
            pivot.FatturatoAnticipato.ValoriMensili[mese] = fatturatoAnticipatoMese;
            pivot.ResiduoAnticipabile.ValoriMensili[mese] = pivot.FidoMaxAnticipi - fatturatoAnticipatoMese;
            
            // Il Saldo Corrente all'inizio del mese è il saldo progressivo del mese precedente
            // (o il saldo corrente della banca per il primo mese)
            pivot.SaldoCorrente.ValoriMensili[mese] = saldoProgressivo;
            
            // Aggiungi descrizione solo per il primo mese
            if (i == 0)
            {
                pivot.SaldoCorrente.DescrizioniMensili[mese] = $"Saldo attuale {pivot.NomeBanca}";
            }
            else
            {
                pivot.SaldoCorrente.DescrizioniMensili[mese] = "Riporto mese precedente";
            }
            
            // Calcola il Saldo Disponibile a fine mese
            saldoProgressivo += pivot.Incassi.ValoriMensili[mese];
            saldoProgressivo -= pivot.Pagamenti.ValoriMensili[mese];
            
            // Salva il saldo disponibile a fine mese
            pivot.SaldoDisponibile.ValoriMensili[mese] = saldoProgressivo;
        }
        
        SaldoPivot = pivot;
    }

    #region COMANDI INCASSI

    [RelayCommand]
    private void NuovoIncasso()
    {
        var dialog = new Views.IncassoDialogView
        {
            DataContext = new IncassoDialogViewModel(GetOrCreateContext(), BancaId),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadIncassi();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot(); // Ottimizzazione: caricato solo quando necessario
        }
    }

    [RelayCommand]
    private void ModificaIncasso(BancaIncasso? incasso = null)
    {
        var incassoDaModificare = incasso ?? IncassoSelezionato;
        if (incassoDaModificare == null)
        {
            MessageBox.Show("Seleziona un incasso da modificare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Views.IncassoDialogView
        {
            DataContext = new IncassoDialogViewModel(GetOrCreateContext(), BancaId, incassoDaModificare),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadIncassi();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void EliminaIncasso(BancaIncasso? incasso = null)
    {
        var incassoDaEliminare = incasso ?? IncassoSelezionato;
        if (incassoDaEliminare == null)
        {
            MessageBox.Show("Seleziona un incasso da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Eliminare l'incasso di {CurrencyHelper.FormatEuro(incassoDaEliminare.Importo)} da {incassoDaEliminare.NomeCliente}?",
            "Conferma eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Yes)
        {
            _incassoRepo.Delete(incassoDaEliminare.Id);
            _auditService.Log(
                SessionManager.CurrentUser!.Id,
                SessionManager.CurrentUser.Username,
                "DELETE",
                "BancaIncasso",
                incassoDaEliminare.Id,
                $"Eliminato incasso: {incassoDaEliminare.NomeCliente} - {CurrencyHelper.FormatEuro(incassoDaEliminare.Importo)}"
            );
            LoadIncassi();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void SegnaIncassato()
    {
        if (IncassoSelezionato == null || IncassoSelezionato.Incassato)
        {
            MessageBox.Show("Seleziona un incasso non ancora incassato.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _incassoRepo.SegnaIncassato(IncassoSelezionato.Id, DateTime.Now);
        _auditService.Log(
            SessionManager.CurrentUser!.Id,
            SessionManager.CurrentUser.Username,
            "UPDATE",
            "BancaIncasso",
            IncassoSelezionato.Id,
            $"Segnato come incassato: {IncassoSelezionato.NomeCliente} - {CurrencyHelper.FormatEuro(IncassoSelezionato.Importo)}"
        );
        LoadIncassi();
        LoadStatistiche();
        LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        MessageBox.Show("Incasso segnato come incassato!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region COMANDI PAGAMENTI

    [RelayCommand]
    private void NuovoPagamento()
    {
        var dialog = new Views.PagamentoDialogView
        {
            DataContext = new PagamentoDialogViewModel(GetOrCreateContext(), BancaId),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadPagamenti();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void NuovoPagamentoMensile()
    {
        var dialog = new Views.PagamentoMensileDialogView
        {
            DataContext = new PagamentoMensileDialogViewModel(GetOrCreateContext(), BancaId),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadPagamenti();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void ModificaPagamento(BancaPagamento? pagamento = null)
    {
        var pagamentoDaModificare = pagamento ?? PagamentoSelezionato;
        if (pagamentoDaModificare == null)
        {
            MessageBox.Show("Seleziona un pagamento da modificare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Views.PagamentoDialogView
        {
            DataContext = new PagamentoDialogViewModel(GetOrCreateContext(), BancaId, pagamentoDaModificare),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadPagamenti();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void EliminaPagamento(BancaPagamento? pagamento = null)
    {
        var pagamentoDaEliminare = pagamento ?? PagamentoSelezionato;
        if (pagamentoDaEliminare == null)
        {
            MessageBox.Show("Seleziona un pagamento da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Eliminare il pagamento di {CurrencyHelper.FormatEuro(pagamentoDaEliminare.Importo)} a {pagamentoDaEliminare.NomeFornitore}?",
            "Conferma eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Yes)
        {
            _pagamentoRepo.Delete(pagamentoDaEliminare.Id);
            _auditService.Log(
                SessionManager.CurrentUser!.Id,
                SessionManager.CurrentUser.Username,
                "DELETE",
                "BancaPagamento",
                pagamentoDaEliminare.Id,
                $"Eliminato pagamento: {pagamentoDaEliminare.NomeFornitore} - {CurrencyHelper.FormatEuro(pagamentoDaEliminare.Importo)}"
            );
            LoadPagamenti();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void SegnaPagato()
    {
        if (PagamentoSelezionato == null || PagamentoSelezionato.Pagato)
        {
            MessageBox.Show("Seleziona un pagamento non ancora pagato.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _pagamentoRepo.SegnaPagato(PagamentoSelezionato.Id, DateTime.Now);
        _auditService.Log(
            SessionManager.CurrentUser!.Id,
            SessionManager.CurrentUser.Username,
            "UPDATE",
            "BancaPagamento",
            PagamentoSelezionato.Id,
            $"Segnato come pagato: {PagamentoSelezionato.NomeFornitore} - {CurrencyHelper.FormatEuro(PagamentoSelezionato.Importo)}"
        );
        LoadPagamenti();
        LoadStatistiche();
        LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        MessageBox.Show("Pagamento segnato come pagato!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region COMANDI ANTICIPI

    [RelayCommand]
    private void NuovoAnticipo()
    {
        var dialog = new Views.AnticipoDialogView
        {
            DataContext = new AnticipoDialogViewModel(GetOrCreateContext(), BancaId),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadAnticipi();
            LoadStatistiche();
            LoadAlerts();
        }
    }

    [RelayCommand]
    private void ModificaAnticipo(BancaUtilizzoAnticipo? anticipo = null)
    {
        var anticipoDaModificare = anticipo ?? AnticipoSelezionato;
        if (anticipoDaModificare == null)
        {
            MessageBox.Show("Seleziona un anticipo da modificare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Views.AnticipoDialogView
        {
            DataContext = new AnticipoDialogViewModel(GetOrCreateContext(), BancaId, anticipoDaModificare),
            Owner = Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            LoadAnticipi();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void EliminaAnticipo(BancaUtilizzoAnticipo? anticipo = null)
    {
        var anticipoDaEliminare = anticipo ?? AnticipoSelezionato;
        if (anticipoDaEliminare == null)
        {
            MessageBox.Show("Seleziona un anticipo da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Eliminare l'anticipo di {CurrencyHelper.FormatEuro(anticipoDaEliminare.ImportoUtilizzo)}?",
            "Conferma eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Yes)
        {
            _anticipoRepo.Delete(anticipoDaEliminare.Id);
            _auditService.Log(
                SessionManager.CurrentUser!.Id,
                SessionManager.CurrentUser.Username,
                "DELETE",
                "BancaUtilizzoAnticipo",
                anticipoDaEliminare.Id,
                $"Eliminato anticipo: {CurrencyHelper.FormatEuro(anticipoDaEliminare.ImportoUtilizzo)}"
            );
            LoadAnticipi();
            LoadStatistiche();
            LoadAlerts();
            LoadMovimentiPrevisti();
            // LoadSaldoPivot();
        }
    }

    [RelayCommand]
    private void SegnaRimborsato()
    {
        if (AnticipoSelezionato == null || AnticipoSelezionato.Rimborsato)
        {
            MessageBox.Show("Seleziona un anticipo non ancora rimborsato.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _anticipoRepo.SegnaRimborsato(AnticipoSelezionato.Id, DateTime.Now);
        _auditService.Log(
            SessionManager.CurrentUser!.Id,
            SessionManager.CurrentUser.Username,
            "UPDATE",
            "BancaUtilizzoAnticipo",
            AnticipoSelezionato.Id,
            $"Segnato come rimborsato: {CurrencyHelper.FormatEuro(AnticipoSelezionato.ImportoUtilizzo)}"
        );
        LoadAnticipi();
        LoadStatistiche();
        MessageBox.Show("Anticipo segnato come rimborsato!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region EXPORT EXCEL

    [RelayCommand]
    private void ExportIncassiExcel()
    {
        try
        {
            if (Banca == null) return;
            
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Incassi_{Banca.NomeBanca}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Incassi");

                // HEADER
                worksheet.Cell(1, 1).Value = "Cliente";
                worksheet.Cell(1, 2).Value = "Anno";
                worksheet.Cell(1, 3).Value = "Mese";
                worksheet.Cell(1, 4).Value = "Importo Fattura";
                worksheet.Cell(1, 5).Value = "% Anticipo";
                worksheet.Cell(1, 6).Value = "Imp. Anticipato";
                worksheet.Cell(1, 7).Value = "Imp. Fatt. a Scadenza";
                worksheet.Cell(1, 8).Value = "N. Fattura";
                worksheet.Cell(1, 9).Value = "Data Fattura";
                worksheet.Cell(1, 10).Value = "Incassato";
                worksheet.Cell(1, 11).Value = "Data Inizio Antic.";
                worksheet.Cell(1, 12).Value = "Data Scad. Antic.";
                worksheet.Cell(1, 13).Value = "Scadenza Fattura";
                worksheet.Cell(1, 14).Value = "Data Incasso";
                worksheet.Cell(1, 15).Value = "Note";

                // Stile header
                var headerRange = worksheet.Range(1, 1, 1, 15);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                // DATI
                int row = 2;
                foreach (var incasso in IncassiFiltrati)
                {
                    worksheet.Cell(row, 1).Value = incasso.NomeCliente;
                    worksheet.Cell(row, 2).Value = incasso.Anno;
                    worksheet.Cell(row, 3).Value = incasso.Mese;
                    worksheet.Cell(row, 4).Value = incasso.Importo;
                    worksheet.Cell(row, 5).Value = incasso.PercentualeAnticipo;
                    worksheet.Cell(row, 6).Value = incasso.ImportoAnticipato;
                    worksheet.Cell(row, 7).Value = incasso.ImportoFatturaScadenza;
                    worksheet.Cell(row, 8).Value = incasso.NumeroFatturaCliente ?? "";
                    worksheet.Cell(row, 9).Value = incasso.DataFatturaCliente?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 10).Value = incasso.Incassato ? "Si" : "No";
                    worksheet.Cell(row, 11).Value = incasso.DataInizioAnticipo?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 12).Value = incasso.DataScadenzaAnticipo?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 13).Value = incasso.DataScadenza.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 14).Value = incasso.DataIncassoEffettivo?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 15).Value = incasso.Note ?? "";
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(saveFileDialog.FileName);
            }

            MessageBox.Show($"Excel esportato con successo!\n{saveFileDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'esportazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportPagamentiExcel()
    {
        try
        {
            if (Banca == null) return;
            
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Pagamenti_{Banca.NomeBanca}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Pagamenti");

                // HEADER
                worksheet.Cell(1, 1).Value = "Fornitore";
                worksheet.Cell(1, 2).Value = "Anno";
                worksheet.Cell(1, 3).Value = "Mese";
                worksheet.Cell(1, 4).Value = "Importo Fattura";
                worksheet.Cell(1, 5).Value = "N. Fattura";
                worksheet.Cell(1, 6).Value = "Data Fattura";
                worksheet.Cell(1, 7).Value = "Pagato";
                worksheet.Cell(1, 8).Value = "Scadenza Fattura";
                worksheet.Cell(1, 9).Value = "Data Pagamento";
                worksheet.Cell(1, 10).Value = "Note";

                // Stile header
                var headerRange = worksheet.Range(1, 1, 1, 10);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightCoral;

                // DATI
                int row = 2;
                foreach (var pagamento in PagamentiFiltrati)
                {
                    worksheet.Cell(row, 1).Value = pagamento.NomeFornitore;
                    worksheet.Cell(row, 2).Value = pagamento.Anno;
                    worksheet.Cell(row, 3).Value = pagamento.Mese;
                    worksheet.Cell(row, 4).Value = pagamento.Importo;
                    worksheet.Cell(row, 5).Value = pagamento.NumeroFatturaFornitore ?? "";
                    worksheet.Cell(row, 6).Value = pagamento.DataFatturaFornitore?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 7).Value = pagamento.Pagato ? "Si" : "No";
                    worksheet.Cell(row, 8).Value = pagamento.DataScadenza.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 9).Value = pagamento.DataPagamentoEffettivo?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cell(row, 10).Value = pagamento.Note ?? "";
                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(saveFileDialog.FileName);
            }

            MessageBox.Show($"Excel esportato con successo!\n{saveFileDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'esportazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportSaldoPrevisoExcel()
    {
        try
        {
            if (Banca == null) return;
            
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"SaldoPrevisto_{Banca.NomeBanca}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Saldo Previsto");

                // HEADER
                worksheet.Cell(1, 1).Value = "Data";
                worksheet.Cell(1, 2).Value = "Tipo";
                worksheet.Cell(1, 3).Value = "Descrizione";
                worksheet.Cell(1, 4).Value = "Importo";
                worksheet.Cell(1, 5).Value = "Saldo Progressivo";
                worksheet.Cell(1, 6).Value = "Stato";

                // Stile header
                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;

                // DATI
                int row = 2;
                foreach (var movimento in MovimentiPrevisti)
                {
                    worksheet.Cell(row, 1).Value = movimento.DataScadenza.ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 2).Value = movimento.Tipo;
                    worksheet.Cell(row, 3).Value = movimento.Descrizione;
                    worksheet.Cell(row, 4).Value = movimento.Importo;
                    worksheet.Cell(row, 5).Value = movimento.SaldoProgressivo;
                    worksheet.Cell(row, 6).Value = movimento.StatoFido;

                    // Colora l'importo in base al tipo
                    if (movimento.Importo > 0)
                    {
                        worksheet.Cell(row, 4).Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
                    }
                    else if (movimento.Importo < 0)
                    {
                        worksheet.Cell(row, 4).Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                    }

                    // Evidenzia se sta usando il fido
                    if (movimento.UtilizzoFido)
                    {
                        worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Orange;
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(saveFileDialog.FileName);
            }

            MessageBox.Show($"Excel esportato con successo!\n{saveFileDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'esportazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ExportSaldoPivotExcel()
    {
        try
        {
            if (Banca == null) return;
            
            if (SaldoPivot == null || SaldoPivot.Mesi.Count == 0)
            {
                MessageBox.Show("Nessun dato da esportare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"SaldoPivot_{Banca.NomeBanca}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Saldo Pivot");

                int currentRow = 1;

                // HEADER PRINCIPALE
                worksheet.Cell(currentRow, 1).Value = "SALDO PREVISTO PIVOT - " + Banca.NomeBanca;
                worksheet.Range(currentRow, 1, currentRow, SaldoPivot.Mesi.Count + 1).Merge();
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.DarkBlue;
                worksheet.Cell(currentRow, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                currentRow += 2;

                // HEADER MESI
                worksheet.Cell(currentRow, 1).Value = "Categoria";
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Gray;
                
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    worksheet.Cell(currentRow, i + 2).Value = SaldoPivot.Mesi[i];
                    worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.Gray;
                    worksheet.Cell(currentRow, i + 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                }
                currentRow++;

                // RIGA: SALDO CORRENTE
                worksheet.Cell(currentRow, 1).Value = "Saldo Corrente";
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    string mese = SaldoPivot.Mesi[i];
                    decimal valore = SaldoPivot.SaldoCorrente.ValoriMensili[mese];
                    string descrizione = SaldoPivot.SaldoCorrente.DescrizioniMensili.ContainsKey(mese) 
                        ? SaldoPivot.SaldoCorrente.DescrizioniMensili[mese] 
                        : "";
                    
                    worksheet.Cell(currentRow, i + 2).Value = valore;
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                    
                    if (!string.IsNullOrWhiteSpace(descrizione))
                    {
                        worksheet.Cell(currentRow, i + 2).CreateComment().AddText(descrizione);
                    }
                }
                currentRow++;

                // RIGA: INCASSI (con dettagli espansi)
                worksheet.Cell(currentRow, 1).Value = "Incassi";
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                
                int incassiStartRow = currentRow;
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    string mese = SaldoPivot.Mesi[i];
                    decimal totale = SaldoPivot.Incassi.ValoriMensili[mese];
                    var dettagli = SaldoPivot.Incassi.DettagliMensili[mese];
                    
                    worksheet.Cell(currentRow, i + 2).Value = totale;
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                    worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkGreen;
                }
                currentRow++;

                // DETTAGLI INCASSI PER OGNI MESE
                int maxIncassiRows = 0;
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    string mese = SaldoPivot.Mesi[i];
                    var dettagli = SaldoPivot.Incassi.DettagliMensili[mese];
                    if (dettagli.Count > maxIncassiRows) maxIncassiRows = dettagli.Count;
                }

                for (int detailRow = 0; detailRow < maxIncassiRows; detailRow++)
                {
                    if (detailRow == 0)
                    {
                        worksheet.Cell(currentRow, 1).Value = "  • Dettagli:";
                        worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                    }
                    
                    for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                    {
                        string mese = SaldoPivot.Mesi[i];
                        var dettagli = SaldoPivot.Incassi.DettagliMensili[mese];
                        
                        if (detailRow < dettagli.Count)
                        {
                            var dettaglio = dettagli[detailRow];
                            string cellText = $"{dettaglio.Descrizione}\n{dettaglio.Importo:N2} €";
                            if (!string.IsNullOrWhiteSpace(dettaglio.NumeroFattura))
                            {
                                cellText += $"\nFatt. {dettaglio.NumeroFattura}";
                            }
                            cellText += $"\nScad. {dettaglio.DataScadenza:dd/MM/yyyy}";
                            
                            worksheet.Cell(currentRow, i + 2).Value = cellText;
                            worksheet.Cell(currentRow, i + 2).Style.Font.FontSize = 9;
                            worksheet.Cell(currentRow, i + 2).Style.Alignment.WrapText = true;
                            worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(240, 255, 240);
                        }
                    }
                    currentRow++;
                }

                // RIGA: PAGAMENTI (con dettagli espansi)
                worksheet.Cell(currentRow, 1).Value = "Pagamenti";
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightCoral;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    string mese = SaldoPivot.Mesi[i];
                    decimal totale = SaldoPivot.Pagamenti.ValoriMensili[mese];
                    
                    worksheet.Cell(currentRow, i + 2).Value = totale;
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                    worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkRed;
                }
                currentRow++;

                // DETTAGLI PAGAMENTI PER OGNI MESE
                int maxPagamentiRows = 0;
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    string mese = SaldoPivot.Mesi[i];
                    var dettagli = SaldoPivot.Pagamenti.DettagliMensili[mese];
                    if (dettagli.Count > maxPagamentiRows) maxPagamentiRows = dettagli.Count;
                }

                for (int detailRow = 0; detailRow < maxPagamentiRows; detailRow++)
                {
                    if (detailRow == 0)
                    {
                        worksheet.Cell(currentRow, 1).Value = "  • Dettagli:";
                        worksheet.Cell(currentRow, 1).Style.Font.Italic = true;
                    }
                    
                    for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                    {
                        string mese = SaldoPivot.Mesi[i];
                        var dettagli = SaldoPivot.Pagamenti.DettagliMensili[mese];
                        
                        if (detailRow < dettagli.Count)
                        {
                            var dettaglio = dettagli[detailRow];
                            string cellText = $"{dettaglio.Descrizione}\n{dettaglio.Importo:N2} €";
                            if (!string.IsNullOrWhiteSpace(dettaglio.NumeroFattura))
                            {
                                cellText += $"\nFatt. {dettaglio.NumeroFattura}";
                            }
                            cellText += $"\nScad. {dettaglio.DataScadenza:dd/MM/yyyy}";
                            
                            worksheet.Cell(currentRow, i + 2).Value = cellText;
                            worksheet.Cell(currentRow, i + 2).Style.Font.FontSize = 9;
                            worksheet.Cell(currentRow, i + 2).Style.Alignment.WrapText = true;
                            worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(255, 240, 240);
                        }
                    }
                    currentRow++;
                }

                // RIGA: SALDO DISPONIBILE
                worksheet.Cell(currentRow, 1).Value = "Saldo Disponibile";
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                
                for (int i = 0; i < SaldoPivot.Mesi.Count; i++)
                {
                    string mese = SaldoPivot.Mesi[i];
                    decimal valore = SaldoPivot.SaldoDisponibile.ValoriMensili[mese];
                    
                    worksheet.Cell(currentRow, i + 2).Value = valore;
                    worksheet.Cell(currentRow, i + 2).Style.NumberFormat.Format = "#,##0.00 €";
                    worksheet.Cell(currentRow, i + 2).Style.Font.Bold = true;
                    
                    if (valore < 0)
                    {
                        worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
                        worksheet.Cell(currentRow, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromArgb(255, 243, 224);
                    }
                    else
                    {
                        worksheet.Cell(currentRow, i + 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.DarkGreen;
                    }
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(saveFileDialog.FileName);
            }

            MessageBox.Show($"Excel esportato con successo!\n{saveFileDialog.FileName}", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'esportazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region IMPORT EXCEL

    [RelayCommand]
    private void ImportaIncassiExcel()
    {
        try
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Seleziona file Excel con Incassi"
            };

            if (openFileDialog.ShowDialog() != true) return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook(openFileDialog.FileName))
            {
                var worksheet = workbook.Worksheets.First();
                int importati = 0;
                int errori = 0;
                var messaggiErrore = new System.Text.StringBuilder();

                // Identifica la riga di partenza (cerca la prima riga con "Cliente" nella prima colonna)
                int startRow = 1;
                for (int r = 1; r <= 10; r++)
                {
                    var cellValue = worksheet.Cell(r, 1).GetString().ToLower();
                    if (cellValue.Contains("cliente"))
                    {
                        startRow = r + 1; // I dati partono dalla riga successiva all'header
                        break;
                    }
                }

                // Parte dalla riga successiva all'header
                int row = startRow;
                while (!worksheet.Cell(row, 1).IsEmpty())
                {
                    try
                    {
                        var cliente = worksheet.Cell(row, 1).GetString();
                        var numeroFattura = worksheet.Cell(row, 2).GetString();
                        
                        // Validazioni base
                        if (string.IsNullOrWhiteSpace(cliente) || string.IsNullOrWhiteSpace(numeroFattura))
                        {
                            errori++;
                            messaggiErrore.AppendLine($"Riga {row}: Cliente o Numero Fattura mancante");
                            row++;
                            continue;
                        }

                        // Conversione sicura importo
                        decimal importoFattura = 0;
                        var cell4 = worksheet.Cell(row, 4);
                        if (!cell4.IsEmpty())
                        {
                            try
                            {
                                importoFattura = cell4.GetValue<decimal>();
                            }
                            catch
                            {
                                try
                                {
                                    importoFattura = (decimal)cell4.GetValue<double>();
                                }
                                catch
                                {
                                    decimal.TryParse(cell4.GetString().Replace("€", "").Replace(".", "").Replace(",", ".").Trim(), out importoFattura);
                                }
                            }
                        }

                        // Conversione sicura anno
                        int anno = DateTime.Now.Year;
                        var cell12 = worksheet.Cell(row, 12);
                        if (!cell12.IsEmpty())
                        {
                            try
                            {
                                anno = cell12.GetValue<int>();
                            }
                            catch
                            {
                                try
                                {
                                    anno = (int)cell12.GetValue<double>();
                                }
                                catch
                                {
                                    int.TryParse(cell12.GetString(), out anno);
                                }
                            }
                        }

                        // Conversione sicura percentuale anticipo
                        decimal percentualeAnticipo = 0;
                        var cell5 = worksheet.Cell(row, 5);
                        if (!cell5.IsEmpty())
                        {
                            try
                            {
                                percentualeAnticipo = cell5.GetValue<decimal>();
                            }
                            catch
                            {
                                try
                                {
                                    percentualeAnticipo = (decimal)cell5.GetValue<double>();
                                }
                                catch
                                {
                                    decimal.TryParse(cell5.GetString(), out percentualeAnticipo);
                                }
                            }
                        }

                        // Conversione sicura date
                        DateTime? dataFattura = null;
                        try { if (!worksheet.Cell(row, 3).IsEmpty()) dataFattura = worksheet.Cell(row, 3).GetDateTime(); } catch { }

                        DateTime? dataInizioAnticipo = null;
                        try { if (!worksheet.Cell(row, 6).IsEmpty()) dataInizioAnticipo = worksheet.Cell(row, 6).GetDateTime(); } catch { }

                        DateTime? dataScadenzaAnticipo = null;
                        try { if (!worksheet.Cell(row, 7).IsEmpty()) dataScadenzaAnticipo = worksheet.Cell(row, 7).GetDateTime(); } catch { }

                        DateTime dataScadenza = DateTime.Now;
                        try { dataScadenza = worksheet.Cell(row, 8).GetDateTime(); } catch { dataScadenza = DateTime.Now.AddMonths(1); }

                        DateTime? dataIncasso = null;
                        try { if (!worksheet.Cell(row, 9).IsEmpty()) dataIncasso = worksheet.Cell(row, 9).GetDateTime(); } catch { }

                        var incasso = new BancaIncasso
                        {
                            BancaId = BancaId,
                            NomeCliente = cliente,
                            NumeroFatturaCliente = numeroFattura,
                            DataFatturaCliente = dataFattura,
                            Importo = importoFattura,
                            PercentualeAnticipo = percentualeAnticipo,
                            DataInizioAnticipo = dataInizioAnticipo,
                            DataScadenzaAnticipo = dataScadenzaAnticipo,
                            DataScadenza = dataScadenza,
                            DataIncassoEffettivo = dataIncasso,
                            Incassato = worksheet.Cell(row, 10).GetString().ToLower() == "si",
                            Note = worksheet.Cell(row, 11).GetString(),
                            Anno = anno,
                            Mese = dataScadenza.Month
                        };

                        _incassoRepo.Insert(incasso);
                        importati++;
                    }
                    catch (Exception ex)
                    {
                        errori++;
                        messaggiErrore.AppendLine($"Riga {row}: {ex.Message}");
                    }

                    row++;
                }

                try
                {
                    LoadIncassi();
                }
                catch (Exception loadEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore in LoadIncassi: {loadEx.Message}");
                }

                try
                {
                    LoadStatistiche();
                }
                catch (Exception statsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore in LoadStatistiche: {statsEx.Message}");
                }

                try
                {
                    LoadMovimentiPrevisti();
                }
                catch (Exception movEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore in LoadMovimentiPrevisti: {movEx.Message}");
                }

                string messaggio = $"Importazione completata!\n\nIncassi importati: {importati}";
                if (errori > 0)
                {
                    messaggio += $"\nErrori: {errori}\n\nDettagli errori:\n{messaggiErrore}";
                }

                MessageBox.Show(messaggio, "Importazione Excel", MessageBoxButton.OK, 
                    errori > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'importazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ImportaPagamentiExcel()
    {
        try
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Seleziona file Excel con Pagamenti"
            };

            if (openFileDialog.ShowDialog() != true) return;

            using (var workbook = new ClosedXML.Excel.XLWorkbook(openFileDialog.FileName))
            {
                var worksheet = workbook.Worksheets.First();
                int importati = 0;
                int errori = 0;
                var messaggiErrore = new System.Text.StringBuilder();

                // Identifica la riga di partenza (cerca la prima riga con "Fornitore" nella prima colonna)
                int startRow = 1;
                for (int r = 1; r <= 10; r++)
                {
                    var cellValue = worksheet.Cell(r, 1).GetString().ToLower();
                    if (cellValue.Contains("fornitore"))
                    {
                        startRow = r + 1; // I dati partono dalla riga successiva all'header
                        break;
                    }
                }

                // Parte dalla riga successiva all'header
                int row = startRow;
                while (!worksheet.Cell(row, 1).IsEmpty())
                {
                    try
                    {
                        var fornitore = worksheet.Cell(row, 1).GetString();
                        var numeroFattura = worksheet.Cell(row, 2).GetString();
                        
                        // Validazioni base
                        if (string.IsNullOrWhiteSpace(fornitore) || string.IsNullOrWhiteSpace(numeroFattura))
                        {
                            errori++;
                            messaggiErrore.AppendLine($"Riga {row}: Fornitore o Numero Fattura mancante");
                            row++;
                            continue;
                        }

                        // Conversione sicura importo
                        decimal importoFattura = 0;
                        var cell4 = worksheet.Cell(row, 4);
                        if (!cell4.IsEmpty())
                        {
                            try
                            {
                                importoFattura = cell4.GetValue<decimal>();
                            }
                            catch
                            {
                                try
                                {
                                    importoFattura = (decimal)cell4.GetValue<double>();
                                }
                                catch
                                {
                                    decimal.TryParse(cell4.GetString().Replace("€", "").Replace(".", "").Replace(",", ".").Trim(), out importoFattura);
                                }
                            }
                        }

                        // Conversione sicura anno
                        int anno = DateTime.Now.Year;
                        var cell9 = worksheet.Cell(row, 9);
                        if (!cell9.IsEmpty())
                        {
                            try
                            {
                                anno = cell9.GetValue<int>();
                            }
                            catch
                            {
                                try
                                {
                                    anno = (int)cell9.GetValue<double>();
                                }
                                catch
                                {
                                    int.TryParse(cell9.GetString(), out anno);
                                }
                            }
                        }

                        // Conversione sicura date
                        DateTime? dataFattura = null;
                        try { if (!worksheet.Cell(row, 3).IsEmpty()) dataFattura = worksheet.Cell(row, 3).GetDateTime(); } catch { }

                        DateTime dataScadenza = DateTime.Now;
                        try { dataScadenza = worksheet.Cell(row, 5).GetDateTime(); } catch { dataScadenza = DateTime.Now.AddMonths(1); }

                        DateTime? dataPagamento = null;
                        try { if (!worksheet.Cell(row, 6).IsEmpty()) dataPagamento = worksheet.Cell(row, 6).GetDateTime(); } catch { }

                        var pagamento = new BancaPagamento
                        {
                            BancaId = BancaId,
                            NomeFornitore = fornitore,
                            NumeroFatturaFornitore = numeroFattura,
                            DataFatturaFornitore = dataFattura,
                            Importo = importoFattura,
                            DataScadenza = dataScadenza,
                            DataPagamentoEffettivo = dataPagamento,
                            Pagato = worksheet.Cell(row, 7).GetString().ToLower() == "si",
                            Note = worksheet.Cell(row, 8).GetString(),
                            Anno = anno,
                            Mese = dataScadenza.Month
                        };

                        _pagamentoRepo.Insert(pagamento);
                        importati++;
                    }
                    catch (Exception ex)
                    {
                        errori++;
                        messaggiErrore.AppendLine($"Riga {row}: {ex.Message}");
                    }

                    row++;
                }

                try
                {
                    LoadPagamenti();
                }
                catch (Exception loadEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore in LoadPagamenti: {loadEx.Message}");
                }

                try
                {
                    LoadStatistiche();
                }
                catch (Exception statsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore in LoadStatistiche: {statsEx.Message}");
                }

                try
                {
                    LoadMovimentiPrevisti();
                }
                catch (Exception movEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Errore in LoadMovimentiPrevisti: {movEx.Message}");
                }

                string messaggio = $"Importazione completata!\n\nPagamenti importati: {importati}";
                if (errori > 0)
                {
                    messaggio += $"\nErrori: {errori}\n\nDettagli errori:\n{messaggiErrore}";
                }

                MessageBox.Show(messaggio, "Importazione Excel", MessageBoxButton.OK, 
                    errori > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante l'importazione:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region FINANZIAMENTO IMPORT

    /// <summary>
    /// Apre dialog per creare un nuovo finanziamento import
    /// </summary>
    [RelayCommand]
    private void ApriFinanziamentoImport()
    {
        if (Banca == null)
        {
            MessageBox.Show("Selezionare una banca prima di creare un finanziamento", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialogViewModel = new FinanziamentoImportDialogViewModel(
                _finanziamentoRepo,
                _incassoRepo,
                _pagamentoRepo,
                Banca.Id,
                Banca.NomeBanca);

            var dialogView = new Views.FinanziamentoImportDialogView(dialogViewModel);
            
            if (dialogView.ShowDialog() == true && dialogViewModel.DialogResult)
            {
                // Stesso comportamento di NuovoPagamento()
                LoadIncassi();
                LoadPagamenti();
                LoadStatistiche();
                LoadAlerts();
                LoadMovimentiPrevisti();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante la creazione del finanziamento:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    /// <summary>
    /// Aggiorna tutti i dati
    /// </summary>
    [RelayCommand]
    private void Aggiorna()
    {
        LoadBanca();
        MessageBox.Show("Dati aggiornati!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

