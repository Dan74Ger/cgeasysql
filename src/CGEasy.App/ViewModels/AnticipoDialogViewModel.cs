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
/// ViewModel per il dialog di creazione/modifica Anticipo Fatture
/// </summary>
public partial class AnticipoDialogViewModel : ObservableObject
{
    private readonly BancaUtilizzoAnticipoRepository _anticipoRepo;
    private readonly BancaService _bancaService;
    private readonly AuditLogService _auditService;
    private readonly int _bancaId;
    private readonly BancaUtilizzoAnticipo? _anticipoEsistente;

    [ObservableProperty]
    private decimal _fatturato;

    [ObservableProperty]
    private decimal _percentualeAnticipo = 80; // Default 80%

    [ObservableProperty]
    private decimal _importoUtilizzo;

    [ObservableProperty]
    private DateTime _dataInizioUtilizzo = DateTime.Now;

    [ObservableProperty]
    private DateTime _dataScadenzaUtilizzo = DateTime.Now.AddMonths(3);

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private decimal _anticipoResiduoDisponibile;

    public bool DialogResult { get; private set; }

    /// <summary>
    /// Metodo chiamato quando cambia il Fatturato - ricalcola l'importo
    /// </summary>
    partial void OnFatturatoChanged(decimal value)
    {
        RicalcolaImporto();
    }

    /// <summary>
    /// Metodo chiamato quando cambia la Percentuale - ricalcola l'importo
    /// </summary>
    partial void OnPercentualeAnticipoChanged(decimal value)
    {
        RicalcolaImporto();
    }

    /// <summary>
    /// Ricalcola l'importo anticipo basandosi su Fatturato e Percentuale
    /// </summary>
    private void RicalcolaImporto()
    {
        if (Fatturato > 0 && PercentualeAnticipo > 0 && PercentualeAnticipo <= 100)
        {
            ImportoUtilizzo = Fatturato * (PercentualeAnticipo / 100);
        }
    }

    public AnticipoDialogViewModel(CGEasyDbContext context, int bancaId, BancaUtilizzoAnticipo? anticipoEsistente = null)
    {
        _anticipoRepo = new BancaUtilizzoAnticipoRepository(context);
        _bancaService = new BancaService(context);
        _auditService = new AuditLogService(context);
        _bancaId = bancaId;
        _anticipoEsistente = anticipoEsistente;

        // Calcola anticipo residuo disponibile
        AnticipoResiduoDisponibile = _bancaService.GetAnticipoResiduoDisponibile(_bancaId);

        // Se è una modifica, popola i campi
        if (_anticipoEsistente != null)
        {
            Fatturato = _anticipoEsistente.Fatturato;
            PercentualeAnticipo = _anticipoEsistente.PercentualeAnticipo;
            ImportoUtilizzo = _anticipoEsistente.ImportoUtilizzo;
            DataInizioUtilizzo = _anticipoEsistente.DataInizioUtilizzo;
            DataScadenzaUtilizzo = _anticipoEsistente.DataScadenzaUtilizzo;
            Note = _anticipoEsistente.Note ?? string.Empty;
        }
    }

    [RelayCommand]
    private void Salva(Window window)
    {
        // Validazione
        if (ImportoUtilizzo <= 0)
        {
            MessageBox.Show("L'importo deve essere maggiore di zero.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (DataScadenzaUtilizzo <= DataInizioUtilizzo)
        {
            MessageBox.Show("La data di scadenza deve essere successiva alla data di inizio.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Verifica superamento massimale (solo per nuovo anticipo)
        if (_anticipoEsistente == null && !_bancaService.PuoUtilizzareAnticipo(_bancaId, ImportoUtilizzo))
        {
            MessageBox.Show(
                $"⚠️ ATTENZIONE!\n\n" +
                $"L'importo richiesto supera il massimale disponibile.\n\n" +
                $"Anticipo residuo: {CurrencyHelper.FormatEuro(AnticipoResiduoDisponibile)}\n" +
                $"Importo richiesto: {CurrencyHelper.FormatEuro(ImportoUtilizzo)}",
                "Superamento Massimale",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (_anticipoEsistente == null)
            {
                // Nuovo anticipo
                var nuovoAnticipo = new BancaUtilizzoAnticipo
                {
                    BancaId = _bancaId,
                    Fatturato = Fatturato,
                    PercentualeAnticipo = PercentualeAnticipo,
                    ImportoUtilizzo = ImportoUtilizzo,
                    DataInizioUtilizzo = DataInizioUtilizzo,
                    DataScadenzaUtilizzo = DataScadenzaUtilizzo,
                    Rimborsato = false,
                    Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()
                };

                // Calcola interessi
                nuovoAnticipo.InteressiMaturati = _bancaService.CalcolaInteressiAnticipo(
                    _bancaId,
                    ImportoUtilizzo,
                    DataInizioUtilizzo,
                    DataScadenzaUtilizzo
                );

                var id = _anticipoRepo.Insert(nuovoAnticipo);

                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "CREATE",
                    "BancaUtilizzoAnticipo",
                    id,
                    $"Creato anticipo: {CurrencyHelper.FormatEuro(ImportoUtilizzo)}"
                );
            }
            else
            {
                // Modifica anticipo esistente
                _anticipoEsistente.Fatturato = Fatturato;
                _anticipoEsistente.PercentualeAnticipo = PercentualeAnticipo;
                _anticipoEsistente.ImportoUtilizzo = ImportoUtilizzo;
                _anticipoEsistente.DataInizioUtilizzo = DataInizioUtilizzo;
                _anticipoEsistente.DataScadenzaUtilizzo = DataScadenzaUtilizzo;
                _anticipoEsistente.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();

                // Ricalcola interessi
                _anticipoEsistente.InteressiMaturati = _bancaService.CalcolaInteressiAnticipo(
                    _bancaId,
                    ImportoUtilizzo,
                    DataInizioUtilizzo,
                    _anticipoEsistente.Rimborsato && _anticipoEsistente.DataRimborsoEffettivo.HasValue
                        ? _anticipoEsistente.DataRimborsoEffettivo.Value
                        : DataScadenzaUtilizzo
                );

                _anticipoRepo.Update(_anticipoEsistente);

                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "UPDATE",
                    "BancaUtilizzoAnticipo",
                    _anticipoEsistente.Id,
                    $"Modificato anticipo: {CurrencyHelper.FormatEuro(ImportoUtilizzo)}"
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

