using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using CGEasy.App.Helpers;
using System;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per il dialog di creazione/modifica Incasso
/// </summary>
public partial class IncassoDialogViewModel : ObservableObject
{
    private readonly BancaIncassoRepository _incassoRepo;
    private readonly AuditLogService _auditService;
    private readonly int _bancaId;
    private readonly BancaIncasso? _incassoEsistente;

    [ObservableProperty]
    private string _nomeCliente = string.Empty;

    [ObservableProperty]
    private int _anno = DateTime.Now.Year;

    [ObservableProperty]
    private int _mese = DateTime.Now.Month;

    [ObservableProperty]
    private decimal _importo;

    [ObservableProperty]
    private decimal _percentualeAnticipo;

    [ObservableProperty]
    private decimal _importoAnticipato;

    [ObservableProperty]
    private DateTime? _dataInizioAnticipo;

    [ObservableProperty]
    private bool _anticipoGestito_CC;

    [ObservableProperty]
    private DateTime? _dataScadenzaAnticipo;

    [ObservableProperty]
    private bool _anticipoChiuso_CC;

    [ObservableProperty]
    private decimal _importoFatturaScadenza;

    [ObservableProperty]
    private DateTime _dataScadenza = DateTime.Now.AddDays(30);

    [ObservableProperty]
    private bool _incassato;

    [ObservableProperty]
    private DateTime? _dataIncassoEffettivo;

    [ObservableProperty]
    private string? _numeroFatturaCliente;

    [ObservableProperty]
    private DateTime? _dataFatturaCliente;

    [ObservableProperty]
    private string _note = string.Empty;

    public bool DialogResult { get; private set; }

    public IncassoDialogViewModel(LiteDbContext context, int bancaId, BancaIncasso? incassoEsistente = null)
    {
        _incassoRepo = new BancaIncassoRepository(context);
        _auditService = new AuditLogService(context);
        _bancaId = bancaId;
        _incassoEsistente = incassoEsistente;

        // Se è una modifica, popola i campi
        if (_incassoEsistente != null)
        {
            NomeCliente = _incassoEsistente.NomeCliente;
            Anno = _incassoEsistente.Anno;
            Mese = _incassoEsistente.Mese;
            Importo = _incassoEsistente.Importo;
            PercentualeAnticipo = _incassoEsistente.PercentualeAnticipo;
            ImportoAnticipato = _incassoEsistente.ImportoAnticipato;
            DataInizioAnticipo = _incassoEsistente.DataInizioAnticipo;
            AnticipoGestito_CC = _incassoEsistente.AnticipoGestito_CC;
            DataScadenzaAnticipo = _incassoEsistente.DataScadenzaAnticipo;
            AnticipoChiuso_CC = _incassoEsistente.AnticipoChiuso_CC;
            ImportoFatturaScadenza = _incassoEsistente.ImportoFatturaScadenza;
            DataScadenza = _incassoEsistente.DataScadenza;
            Incassato = _incassoEsistente.Incassato;
            DataIncassoEffettivo = _incassoEsistente.DataIncassoEffettivo;
            NumeroFatturaCliente = _incassoEsistente.NumeroFatturaCliente;
            DataFatturaCliente = _incassoEsistente.DataFatturaCliente;
            Note = _incassoEsistente.Note ?? string.Empty;
        }
    }

    partial void OnImportoChanged(decimal value)
    {
        CalcolaImportoAnticipato();
    }

    partial void OnPercentualeAnticipoChanged(decimal value)
    {
        CalcolaImportoAnticipato();
    }

    private void CalcolaImportoAnticipato()
    {
        ImportoAnticipato = Importo * (PercentualeAnticipo / 100);
        ImportoFatturaScadenza = Importo - ImportoAnticipato;
    }

    [RelayCommand]
    private void Salva(Window window)
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(NomeCliente))
        {
            MessageBox.Show("Il nome del cliente è obbligatorio.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Importo <= 0)
        {
            MessageBox.Show("L'importo deve essere maggiore di zero.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Anno < 2000 || Anno > 2100)
        {
            MessageBox.Show("Anno non valido.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Mese < 1 || Mese > 12)
        {
            MessageBox.Show("Mese non valido (1-12).", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_incassoEsistente == null)
            {
                // Nuovo incasso
                var nuovoIncasso = new BancaIncasso
                {
                    BancaId = _bancaId,
                    NomeCliente = NomeCliente.Trim(),
                    Anno = Anno,
                    Mese = Mese,
                    Importo = Importo,
                    PercentualeAnticipo = PercentualeAnticipo,
                    DataInizioAnticipo = DataInizioAnticipo,
                    AnticipoGestito_CC = AnticipoGestito_CC,
                    DataScadenzaAnticipo = DataScadenzaAnticipo,
                    AnticipoChiuso_CC = AnticipoChiuso_CC,
                    DataScadenza = DataScadenza,
                    Incassato = Incassato,
                    DataIncassoEffettivo = DataIncassoEffettivo,
                    NumeroFatturaCliente = string.IsNullOrWhiteSpace(NumeroFatturaCliente) ? null : NumeroFatturaCliente.Trim(),
                    DataFatturaCliente = DataFatturaCliente,
                    Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()
                };

                var id = _incassoRepo.Insert(nuovoIncasso);

                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "CREATE",
                    "BancaIncasso",
                    id,
                    $"Creato incasso: {NomeCliente} - {CurrencyHelper.FormatEuro(Importo)}"
                );
            }
            else
            {
                // Modifica incasso esistente
                _incassoEsistente.NomeCliente = NomeCliente.Trim();
                _incassoEsistente.Anno = Anno;
                _incassoEsistente.Mese = Mese;
                _incassoEsistente.Importo = Importo;
                _incassoEsistente.PercentualeAnticipo = PercentualeAnticipo;
                _incassoEsistente.DataInizioAnticipo = DataInizioAnticipo;
                _incassoEsistente.AnticipoGestito_CC = AnticipoGestito_CC;
                _incassoEsistente.DataScadenzaAnticipo = DataScadenzaAnticipo;
                _incassoEsistente.AnticipoChiuso_CC = AnticipoChiuso_CC;
                _incassoEsistente.DataScadenza = DataScadenza;
                _incassoEsistente.Incassato = Incassato;
                _incassoEsistente.DataIncassoEffettivo = DataIncassoEffettivo;
                _incassoEsistente.NumeroFatturaCliente = string.IsNullOrWhiteSpace(NumeroFatturaCliente) ? null : NumeroFatturaCliente.Trim();
                _incassoEsistente.DataFatturaCliente = DataFatturaCliente;
                _incassoEsistente.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();

                _incassoRepo.Update(_incassoEsistente);

                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "UPDATE",
                    "BancaIncasso",
                    _incassoEsistente.Id,
                    $"Modificato incasso: {NomeCliente} - {CurrencyHelper.FormatEuro(Importo)}"
                );
            }

            DialogResult = true;
            window.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Annulla(Window window)
    {
        DialogResult = false;
        window.Close();
    }
}

