using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Services;

namespace CGEasy.App.ViewModels;

public class BilancioTemplateDialogViewModel : INotifyPropertyChanged
{
    private readonly CGEasyDbContext _context;
    private readonly AuditLogService _auditService;
    private bool _isNuovaRiga;
    private int _clienteId;
    private int _mese;
    private int _anno;
    private string _nomeFile = string.Empty;

    public BilancioTemplateDialogViewModel(CGEasyDbContext context, AuditLogService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    // Properties per binding
    private string _codice = string.Empty;
    public string Codice
    {
        get => _codice;
        set { _codice = value; OnPropertyChanged(); }
    }

    private string _descrizione = string.Empty;
    public string Descrizione
    {
        get => _descrizione;
        set { _descrizione = value; OnPropertyChanged(); }
    }

    private decimal _importo;
    public decimal Importo
    {
        get => _importo;
        set { _importo = value; OnPropertyChanged(); }
    }

    private string _segno = "+";
    public string Segno
    {
        get => _segno;
        set { _segno = value; OnPropertyChanged(); }
    }

    private string? _formula;
    public string? Formula
    {
        get => _formula;
        set { _formula = value; OnPropertyChanged(); }
    }

    public bool IsCodiceReadonly => !_isNuovaRiga;
    public string TitoloDialog => _isNuovaRiga ? "📝 Nuova Riga Template" : "✏️ Modifica Riga Template";

    // Riga risultante
    public BilancioTemplate? Riga { get; private set; }

    /// <summary>
    /// Inizializza il dialog per nuova riga o modifica
    /// </summary>
    public void Inizializza(BilancioTemplate? rigaEsistente, int clienteId, int mese, int anno, string nomeFile)
    {
        _clienteId = clienteId;
        _mese = mese;
        _anno = anno;
        _nomeFile = nomeFile;

        if (rigaEsistente != null)
        {
            // Modifica riga esistente
            _isNuovaRiga = false;
            Riga = rigaEsistente;

            Codice = rigaEsistente.CodiceMastrino;
            Descrizione = rigaEsistente.DescrizioneMastrino;
            Importo = rigaEsistente.Importo;
            Segno = rigaEsistente.Segno;
            Formula = rigaEsistente.Formula;
        }
        else
        {
            // Nuova riga
            _isNuovaRiga = true;
            Codice = "";
            Descrizione = "";
            Importo = 0;
            Segno = "+";
            Formula = "";
        }

        OnPropertyChanged(nameof(IsCodiceReadonly));
        OnPropertyChanged(nameof(TitoloDialog));
    }

    /// <summary>
    /// Salva i dati inseriti nella riga
    /// </summary>
    public bool Salva()
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(Codice))
        {
            System.Windows.MessageBox.Show("⚠️ Il codice è obbligatorio!", "Validazione",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Descrizione))
        {
            System.Windows.MessageBox.Show("⚠️ La descrizione è obbligatoria!", "Validazione",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return false;
        }

        if (_isNuovaRiga)
        {
            // Crea nuova riga
            Riga = new BilancioTemplate
            {
                ClienteId = _clienteId,
                Mese = _mese,
                Anno = _anno,
                CodiceMastrino = Codice.Trim(),
                DescrizioneMastrino = Descrizione.Trim(),
                Importo = Importo,
                Segno = Segno,
                Formula = string.IsNullOrWhiteSpace(Formula) ? null : Formula.Trim(),
                DataImport = DateTime.Now,
                ImportedBy = SessionManager.CurrentUser?.Id ?? 0,
                ImportedByName = SessionManager.CurrentUsername,
                ClienteNome = ""  // Verrà impostato dal repository
            };

            // Calcola importo iniziale
            // TODO: ImportoCalcolato property non esiste in BilancioTemplate
            // Riga.ImportoCalcolato = Importo * (Segno == "-" ? -1 : 1);
        }
        else if (Riga != null)
        {
            // Aggiorna riga esistente
            Riga.DescrizioneMastrino = Descrizione.Trim();
            Riga.Importo = Importo;
            Riga.Segno = Segno;
            Riga.Formula = string.IsNullOrWhiteSpace(Formula) ? null : Formula.Trim();
            
            // Importo calcolato verrà ricalcolato nel ViewModel principale
        }

        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

