using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// ViewModel per la gestione dell'anagrafica Banche (lista + CRUD)
/// </summary>
public partial class GestioneBancheViewModel : ObservableObject
{
    private readonly BancaRepository _bancaRepo;
    private readonly BancaService _bancaService;
    private readonly AuditLogService _auditService;

    [ObservableProperty]
    private ObservableCollection<Banca> _banche = new();

    [ObservableProperty]
    private Banca? _bancaSelezionata;

    [ObservableProperty]
    private string _filtroRicerca = string.Empty;

    // Campi per nuova banca / modifica
    [ObservableProperty]
    private string _nomeBanca = string.Empty;

    [ObservableProperty]
    private string _codiceIdentificativo = string.Empty;

    [ObservableProperty]
    private string _iban = string.Empty;

    [ObservableProperty]
    private string _note = string.Empty;

    [ObservableProperty]
    private decimal _saldoDelGiorno;

    [ObservableProperty]
    private decimal _fidoCCAccordato;

    [ObservableProperty]
    private decimal _anticipoFattureMassimo;

    [ObservableProperty]
    private decimal _interesseAnticipoFatture;

    [ObservableProperty]
    private bool _isEditing;

    public GestioneBancheViewModel() : this(GetOrCreateContext())
    {
    }

    public GestioneBancheViewModel(CGEasyDbContext context)
    {
        _bancaRepo = new BancaRepository(context);
        _bancaService = new BancaService(context);
        _auditService = new AuditLogService(context);

        LoadBanche();
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
    /// Carica tutte le banche
    /// </summary>
    public void LoadBanche()
    {
        var banche = _bancaRepo.GetAll().OrderBy(b => b.NomeBanca).ToList();
        Banche = new ObservableCollection<Banca>(banche);
    }

    /// <summary>
    /// Filtra le banche per nome
    /// </summary>
    [RelayCommand]
    private void Filtra()
    {
        if (string.IsNullOrWhiteSpace(FiltroRicerca))
        {
            LoadBanche();
        }
        else
        {
            var bancheFiltered = _bancaRepo.SearchByNome(FiltroRicerca)
                .OrderBy(b => b.NomeBanca)
                .ToList();
            Banche = new ObservableCollection<Banca>(bancheFiltered);
        }
    }

    /// <summary>
    /// Crea una nuova banca
    /// </summary>
    [RelayCommand]
    private void NuovaBanca()
    {
        // Reset campi
        NomeBanca = string.Empty;
        CodiceIdentificativo = string.Empty;
        Iban = string.Empty;
        Note = string.Empty;
        SaldoDelGiorno = 0;
        FidoCCAccordato = 0;
        AnticipoFattureMassimo = 0;
        InteresseAnticipoFatture = 0;
        BancaSelezionata = null;
        IsEditing = true;
    }

    /// <summary>
    /// Modifica la banca selezionata
    /// </summary>
    [RelayCommand]
    private void ModificaBanca(Banca? banca = null)
    {
        var bancaDaModificare = banca ?? BancaSelezionata;
        if (bancaDaModificare == null)
        {
            MessageBox.Show("Seleziona una banca da modificare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Imposta la banca selezionata se arriva dal parametro
        if (banca != null)
        {
            BancaSelezionata = banca;
        }

        // Popola campi con dati banca selezionata
        NomeBanca = bancaDaModificare.NomeBanca;
        CodiceIdentificativo = bancaDaModificare.CodiceIdentificativo ?? string.Empty;
        Iban = bancaDaModificare.IBAN ?? string.Empty;
        Note = bancaDaModificare.Note ?? string.Empty;
        SaldoDelGiorno = bancaDaModificare.SaldoDelGiorno;
        FidoCCAccordato = bancaDaModificare.FidoCCAccordato;
        AnticipoFattureMassimo = bancaDaModificare.AnticipoFattureMassimo;
        InteresseAnticipoFatture = bancaDaModificare.InteresseAnticipoFatture;
        IsEditing = true;
    }

    /// <summary>
    /// Salva la banca (nuova o modificata)
    /// </summary>
    [RelayCommand]
    private void SalvaBanca()
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(NomeBanca))
        {
            MessageBox.Show("Il nome della banca è obbligatorio.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (BancaSelezionata == null)
            {
                // Nuova banca
                var nuovaBanca = new Banca
                {
                    NomeBanca = NomeBanca.Trim(),
                    CodiceIdentificativo = string.IsNullOrWhiteSpace(CodiceIdentificativo) ? null : CodiceIdentificativo.Trim(),
                    IBAN = string.IsNullOrWhiteSpace(Iban) ? null : Iban.Trim(),
                    Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                    SaldoDelGiorno = SaldoDelGiorno,
                    FidoCCAccordato = FidoCCAccordato,
                    AnticipoFattureMassimo = AnticipoFattureMassimo,
                    InteresseAnticipoFatture = InteresseAnticipoFatture
                };

                var id = _bancaRepo.Insert(nuovaBanca);

                // Audit log
                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "CREATE",
                    "Banca",
                    id,
                    $"Creata banca: {NomeBanca}"
                );

                MessageBox.Show("Banca creata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Modifica banca esistente
                BancaSelezionata.NomeBanca = NomeBanca.Trim();
                BancaSelezionata.CodiceIdentificativo = string.IsNullOrWhiteSpace(CodiceIdentificativo) ? null : CodiceIdentificativo.Trim();
                BancaSelezionata.IBAN = string.IsNullOrWhiteSpace(Iban) ? null : Iban.Trim();
                BancaSelezionata.Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim();
                BancaSelezionata.SaldoDelGiorno = SaldoDelGiorno;
                BancaSelezionata.FidoCCAccordato = FidoCCAccordato;
                BancaSelezionata.AnticipoFattureMassimo = AnticipoFattureMassimo;
                BancaSelezionata.InteresseAnticipoFatture = InteresseAnticipoFatture;

                _bancaRepo.Update(BancaSelezionata);

                // Audit log
                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "UPDATE",
                    "Banca",
                    BancaSelezionata.Id,
                    $"Modificata banca: {NomeBanca}"
                );

                MessageBox.Show("Banca modificata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Ricarica lista e chiudi form
            LoadBanche();
            AnnullaModifica();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Annulla la modifica/creazione
    /// </summary>
    [RelayCommand]
    private void AnnullaModifica()
    {
        IsEditing = false;
        BancaSelezionata = null;
        NomeBanca = string.Empty;
        CodiceIdentificativo = string.Empty;
        Iban = string.Empty;
        Note = string.Empty;
        SaldoDelGiorno = 0;
        FidoCCAccordato = 0;
        AnticipoFattureMassimo = 0;
        InteresseAnticipoFatture = 0;
    }

    /// <summary>
    /// Elimina la banca selezionata
    /// </summary>
    [RelayCommand]
    private void EliminaBanca(Banca? banca = null)
    {
        var bancaDaEliminare = banca ?? BancaSelezionata;
        if (bancaDaEliminare == null)
        {
            MessageBox.Show("Seleziona una banca da eliminare.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Sei sicuro di voler eliminare la banca '{bancaDaEliminare.NomeBanca}'?\n\n" +
            "⚠️ ATTENZIONE: Verranno eliminati anche tutti gli incassi, pagamenti e anticipi collegati!",
            "Conferma eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var nomeBanca = bancaDaEliminare.NomeBanca;
                var bancaId = bancaDaEliminare.Id;

                // Elimina dati collegati
                var incassoRepo = new BancaIncassoRepository(GetOrCreateContext());
                var pagamentoRepo = new BancaPagamentoRepository(GetOrCreateContext());
                var anticipoRepo = new BancaUtilizzoAnticipoRepository(GetOrCreateContext());
                var saldoRepo = new BancaSaldoGiornalieroRepository(GetOrCreateContext());

                incassoRepo.DeleteByBancaId(bancaId);
                pagamentoRepo.DeleteByBancaId(bancaId);
                anticipoRepo.DeleteByBancaId(bancaId);
                saldoRepo.DeleteByBancaId(bancaId);

                // Elimina banca
                _bancaRepo.Delete(bancaId);

                // Audit log
                _auditService.Log(
                    SessionManager.CurrentUser!.Id,
                    SessionManager.CurrentUser.Username,
                    "DELETE",
                    "Banca",
                    bancaId,
                    $"Eliminata banca: {nomeBanca}"
                );

                MessageBox.Show("Banca eliminata con successo!", "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Ricarica lista
                LoadBanche();
                BancaSelezionata = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'eliminazione: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

