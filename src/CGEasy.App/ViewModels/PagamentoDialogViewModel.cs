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
/// ViewModel per il dialog di creazione/modifica Pagamento
/// </summary>
public partial class PagamentoDialogViewModel : ObservableObject
{
    private readonly BancaPagamentoRepository _pagamentoRepo;
    private readonly AuditLogService _auditService;
    private readonly int _bancaId;
    private readonly BancaPagamento? _pagamentoEsistente;

    [ObservableProperty]
    private string _nomeFornitore = string.Empty;

    [ObservableProperty]
    private int _anno = DateTime.Now.Year;

    [ObservableProperty]
    private int _mese = DateTime.Now.Month;

    [ObservableProperty]
    private decimal _importo;

    [ObservableProperty]
    private DateTime _dataScadenza = DateTime.Now.AddDays(30);

    [ObservableProperty]
    private bool _pagato;

    [ObservableProperty]
    private DateTime? _dataPagamentoEffettivo;

    [ObservableProperty]
    private string? _numeroFatturaFornitore;

    [ObservableProperty]
    private DateTime? _dataFatturaFornitore;

    [ObservableProperty]
    private string _note = string.Empty;

    public bool DialogResult { get; private set; }

    public PagamentoDialogViewModel(CGEasyDbContext context, int bancaId, BancaPagamento? pagamentoEsistente = null)
    {
        _pagamentoRepo = new BancaPagamentoRepository(context);
        _auditService = new AuditLogService(context);
        _bancaId = bancaId;
        _pagamentoEsistente = pagamentoEsistente;

        // Se è una modifica, popola i campi
        if (_pagamentoEsistente != null)
        {
            NomeFornitore = _pagamentoEsistente.NomeFornitore;
            Anno = _pagamentoEsistente.Anno;
            Mese = _pagamentoEsistente.Mese;
            Importo = _pagamentoEsistente.Importo;
            DataScadenza = _pagamentoEsistente.DataScadenza;
            Pagato = _pagamentoEsistente.Pagato;
            DataPagamentoEffettivo = _pagamentoEsistente.DataPagamentoEffettivo;
            NumeroFatturaFornitore = _pagamentoEsistente.NumeroFatturaFornitore;
            DataFatturaFornitore = _pagamentoEsistente.DataFatturaFornitore;
            Note = _pagamentoEsistente.Note ?? string.Empty;
        }
    }

    [RelayCommand]
    private void Salva(Window window)
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(NomeFornitore))
        {
            MessageBox.Show("Il nome del fornitore è obbligatorio.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            if (_pagamentoEsistente == null)
            {
                // Nuovo pagamento
                var nuovoPagamento = new BancaPagamento
                {
                    BancaId = _bancaId,
                    NomeFornitore = NomeFornitore.Trim(),
                    Anno = Anno,
                    Mese = Mese,
                    Importo = Importo,
                    DataScadenza = DataScadenza,
                    Pagato = Pagato,
                    DataPagamentoEffettivo = DataPagamentoEffettivo,
                    NumeroFatturaFornitore = string.IsNullOrWhiteSpace(NumeroFatturaFornitore) ? null : NumeroFatturaFornitore.Trim(),
                    DataFatturaFornitore = DataFatturaFornitore,
                    Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()
                };

                var id = _pagamentoRepo.Insert(nuovoPagamento);

                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "CREATE",
                    "BancaPagamento",
                    id,
                    $"Creato pagamento: {NomeFornitore} - {CurrencyHelper.FormatEuro(Importo)}"
                );
            }
            else
            {
                // Modifica pagamento esistente
                _pagamentoEsistente.NomeFornitore = NomeFornitore.Trim();
                _pagamentoEsistente.Anno = Anno;
                _pagamentoEsistente.Mese = Mese;
                _pagamentoEsistente.Importo = Importo;
                _pagamentoEsistente.DataScadenza = DataScadenza;
                _pagamentoEsistente.Pagato = Pagato;
                _pagamentoEsistente.DataPagamentoEffettivo = DataPagamentoEffettivo;
                _pagamentoEsistente.NumeroFatturaFornitore = string.IsNullOrWhiteSpace(NumeroFatturaFornitore) ? null : NumeroFatturaFornitore.Trim();
                _pagamentoEsistente.DataFatturaFornitore = DataFatturaFornitore;
                _pagamentoEsistente.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();

                _pagamentoRepo.Update(_pagamentoEsistente);

                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "UPDATE",
                    "BancaPagamento",
                    _pagamentoEsistente.Id,
                    $"Modificato pagamento: {NomeFornitore} - {CurrencyHelper.FormatEuro(Importo)}"
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

