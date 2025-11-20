using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace CGEasy.App.ViewModels;

/// <summary>
/// Rappresenta una voce di bilancio disponibile per la selezione
/// </summary>
public class VoceBilancioDisponibile
{
    public string Codice { get; set; } = "";
    public string Descrizione { get; set; } = "";
    public string Tipo { get; set; } = ""; // "CE" o "SP"
    
    /// <summary>
    /// Chiave univoca: CODICE|DESCRIZIONE
    /// Necessaria perché potrebbero esserci voci con stesso codice ma descrizione diversa
    /// </summary>
    public string ChiaveUnica => $"{Codice}|{Descrizione}";
    
    public override string ToString() => $"{Codice} - {Descrizione}";
}

/// <summary>
/// Rappresenta un moltiplicatore disponibile
/// </summary>
public class MoltiplicatoreItem
{
    public decimal Valore { get; set; }
    public string Display { get; set; } = "";
}

public partial class IndicePersonalizzatoDialogViewModel : ObservableObject
{
    private readonly CGEasyDbContext _context;
    private readonly IndicePersonalizzatoRepository _indiceRepo;
    private readonly Cliente _cliente;
    private readonly IndicePersonalizzato? _indiceEsistente;
    private Window? _ownerWindow;

    // Informazioni base
    [ObservableProperty]
    private string _nomeIndice = "";

    [ObservableProperty]
    private string _descrizione = "";

    [ObservableProperty]
    private ObservableCollection<string> _categorie = new()
    {
        "Liquidità",
        "Solidità",
        "Redditività",
        "Efficienza",
        "Personalizzato"
    };

    [ObservableProperty]
    private string _categoriaSelezionata = "Personalizzato";

    // Voci disponibili
    [ObservableProperty]
    private ObservableCollection<VoceBilancioDisponibile> _vociDisponibili = new();

    [ObservableProperty]
    private ObservableCollection<VoceBilancioDisponibile> _vociDisponibiliFiltrate = new();

    [ObservableProperty]
    private ObservableCollection<VoceBilancioDisponibile> _vociDisponibiliFiltrateDenominatore = new();

    // Numeratore
    [ObservableProperty]
    private string _filtroVociNumeratore = "";

    [ObservableProperty]
    private VoceBilancioDisponibile? _voceSelezionataNumeratore;

    [ObservableProperty]
    private ObservableCollection<VoceBilancioDisponibile> _vociNumeratoreSelezionate = new();

    [ObservableProperty]
    private VoceBilancioDisponibile? _voceNumeratoreDaRimuovere;

    // Denominatore
    [ObservableProperty]
    private string _filtroVociDenominatore = "";

    [ObservableProperty]
    private VoceBilancioDisponibile? _voceSelezionataDenominatore;

    [ObservableProperty]
    private ObservableCollection<VoceBilancioDisponibile> _vociDenominatoreSelezionate = new();

    [ObservableProperty]
    private VoceBilancioDisponibile? _voceDenominatoreDaRimuovere;

    // Operatori
    [ObservableProperty]
    private ObservableCollection<string> _operatori = new()
    {
        "Divisione",
        "Moltiplicazione",
        "Somma",
        "Sottrazione"
    };

    [ObservableProperty]
    private string _operatoreSelezionato = "Divisione";

    // Moltiplicatori
    [ObservableProperty]
    private ObservableCollection<MoltiplicatoreItem> _moltiplicatori = new()
    {
        new MoltiplicatoreItem { Valore = 1, Display = "x1 (valore assoluto)" },
        new MoltiplicatoreItem { Valore = 100, Display = "x100 (percentuale)" },
        new MoltiplicatoreItem { Valore = 365, Display = "x365 (giorni annuali)" },
        new MoltiplicatoreItem { Valore = 1000, Display = "x1000 (migliaia)" }
    };

    [ObservableProperty]
    private MoltiplicatoreItem? _moltiplicatoreSelezionato;

    // Unità di misura
    [ObservableProperty]
    private ObservableCollection<string> _unitaMisura = new()
    {
        "%",
        "€",
        "giorni",
        "volte",
        "ratio"
    };

    [ObservableProperty]
    private string _unitaMisuraSelezionata = "volte";

    // Anteprima
    [ObservableProperty]
    private string _formulaAnteprima = "Seleziona le voci per vedere l'anteprima della formula";

    public bool DialogResult { get; private set; }

    public IndicePersonalizzatoDialogViewModel(
        Cliente cliente,
        StatisticaCESalvata statisticaCE,
        StatisticaSPSalvata statisticaSP,
        IndicePersonalizzato? indiceEsistente = null)
    {
        _context = App.GetService<CGEasyDbContext>() ?? new CGEasyDbContext();
        // Singleton context - no special marking needed in EF Core

        _indiceRepo = new IndicePersonalizzatoRepository(_context);
        _cliente = cliente;
        _indiceEsistente = indiceEsistente;

        // Imposta moltiplicatore default
        MoltiplicatoreSelezionato = Moltiplicatori.First();

        // Carica voci disponibili dalle statistiche
        CaricaVociDisponibili(statisticaCE, statisticaSP);

        // Se è una modifica, carica i dati esistenti
        if (indiceEsistente != null)
        {
            CaricaIndiceEsistente(indiceEsistente);
        }

        // Inizializza filtri
        AggiornaFiltroNumeratore();
        AggiornaFiltroDenominatore();
        AggiornaFormulaAnteprima();
    }

    /// <summary>
    /// Estrae tutte le voci disponibili dalle statistiche CE e SP
    /// </summary>
    private void CaricaVociDisponibili(StatisticaCESalvata statisticaCE, StatisticaSPSalvata statisticaSP)
    {
        try
        {
            var voci = new List<VoceBilancioDisponibile>();

            // Estrai voci da CE
            if (!string.IsNullOrEmpty(statisticaCE.DatiStatisticheJson))
            {
                var datiCE = JsonSerializer.Deserialize<List<BilancioStatisticaMultiPeriodo>>(
                    statisticaCE.DatiStatisticheJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (datiCE != null)
                {
                    foreach (var voce in datiCE.Where(v => !string.IsNullOrWhiteSpace(v.Codice)))
                    {
                        voci.Add(new VoceBilancioDisponibile
                        {
                            Codice = voce.Codice,
                            Descrizione = voce.Descrizione,
                            Tipo = "CE"
                        });
                    }
                }
            }

            // Estrai voci da SP
            if (!string.IsNullOrEmpty(statisticaSP.DatiStatisticheJson))
            {
                var datiSP = JsonSerializer.Deserialize<List<BilancioStatisticaMultiPeriodo>>(
                    statisticaSP.DatiStatisticheJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (datiSP != null)
                {
                    foreach (var voce in datiSP.Where(v => !string.IsNullOrWhiteSpace(v.Codice)))
                    {
                        voci.Add(new VoceBilancioDisponibile
                        {
                            Codice = voce.Codice,
                            Descrizione = voce.Descrizione,
                            Tipo = "SP"
                        });
                    }
                }
            }

            // Ordina per tipo e poi per codice
            foreach (var voce in voci.OrderBy(v => v.Tipo).ThenBy(v => v.Codice))
            {
                VociDisponibili.Add(voce);
            }

            System.Diagnostics.Debug.WriteLine($"[INDICE DIALOG] Caricate {VociDisponibili.Count} voci disponibili");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento voci:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CaricaIndiceEsistente(IndicePersonalizzato indice)
    {
        NomeIndice = indice.NomeIndice;
        Descrizione = indice.Descrizione;
        OperatoreSelezionato = indice.Operatore.First().ToString().ToUpper() + indice.Operatore.Substring(1);
        UnitaMisuraSelezionata = indice.UnitaMisura;

        // Trova moltiplicatore
        var molt = Moltiplicatori.FirstOrDefault(m => m.Valore == indice.Moltiplicatore);
        if (molt != null) MoltiplicatoreSelezionato = molt;

        // Carica voci numeratore (cerca per ChiaveUnica: CODICE|DESCRIZIONE)
        foreach (var chiave in indice.CodiciNumeratore)
        {
            var voce = VociDisponibili.FirstOrDefault(v => v.ChiaveUnica == chiave);
            if (voce != null) VociNumeratoreSelezionate.Add(voce);
        }

        // Carica voci denominatore (cerca per ChiaveUnica: CODICE|DESCRIZIONE)
        foreach (var chiave in indice.CodiciDenominatore)
        {
            var voce = VociDisponibili.FirstOrDefault(v => v.ChiaveUnica == chiave);
            if (voce != null) VociDenominatoreSelezionate.Add(voce);
        }

        AggiornaFormulaAnteprima();
    }

    [RelayCommand]
    private void AggiungiVoceNumeratore()
    {
        if (VoceSelezionataNumeratore == null) return;

        if (!VociNumeratoreSelezionate.Any(v => v.ChiaveUnica == VoceSelezionataNumeratore.ChiaveUnica))
        {
            VociNumeratoreSelezionate.Add(VoceSelezionataNumeratore);
            AggiornaFormulaAnteprima();
        }
    }

    [RelayCommand]
    private void RimuoviVoceNumeratore()
    {
        if (VoceNumeratoreDaRimuovere != null)
        {
            VociNumeratoreSelezionate.Remove(VoceNumeratoreDaRimuovere);
            AggiornaFormulaAnteprima();
        }
    }

    [RelayCommand]
    private void AggiungiVoceDenominatore()
    {
        if (VoceSelezionataDenominatore == null) return;

        if (!VociDenominatoreSelezionate.Any(v => v.ChiaveUnica == VoceSelezionataDenominatore.ChiaveUnica))
        {
            VociDenominatoreSelezionate.Add(VoceSelezionataDenominatore);
            AggiornaFormulaAnteprima();
        }
    }

    [RelayCommand]
    private void RimuoviVoceDenominatore()
    {
        if (VoceDenominatoreDaRimuovere != null)
        {
            VociDenominatoreSelezionate.Remove(VoceDenominatoreDaRimuovere);
            AggiornaFormulaAnteprima();
        }
    }

    [RelayCommand]
    private void Salva()
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(NomeIndice))
        {
            MessageBox.Show("Inserisci il nome dell'indice", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (VociNumeratoreSelezionate.Count == 0)
        {
            MessageBox.Show("Seleziona almeno una voce per il numeratore", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (OperatoreSelezionato.ToLower() == "divisione" && VociDenominatoreSelezionate.Count == 0)
        {
            MessageBox.Show("Per la divisione devi selezionare almeno una voce per il denominatore", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IndicePersonalizzato indice;

            if (_indiceEsistente != null)
            {
                // Modifica esistente
                indice = _indiceEsistente;
            }
            else
            {
                // Nuovo indice
                indice = new IndicePersonalizzato
                {
                    ClienteId = _cliente.Id,
                    DataCreazione = DateTime.Now
                };
            }

            // Aggiorna dati
            indice.NomeIndice = NomeIndice.Trim();
            indice.Descrizione = Descrizione?.Trim() ?? "";
            indice.Operatore = OperatoreSelezionato.ToLower();
            indice.Moltiplicatore = MoltiplicatoreSelezionato?.Valore ?? 1;
            indice.UnitaMisura = UnitaMisuraSelezionata;
            // Salva CODICE|DESCRIZIONE per identificazione univoca
            indice.CodiciNumeratore = VociNumeratoreSelezionate.Select(v => v.ChiaveUnica).ToList();
            indice.CodiciDenominatore = VociDenominatoreSelezionate.Select(v => v.ChiaveUnica).ToList();
            indice.OperazioneNumeratore = "somma"; // Default: somma le voci
            indice.OperazioneDenominatore = "somma";
            indice.DataUltimaModifica = DateTime.Now;
            indice.Attivo = true;

            // Salva
            if (_indiceEsistente != null)
            {
                _indiceRepo.Update(indice);
            }
            else
            {
                _indiceRepo.Insert(indice);
            }

            DialogResult = true;
            
            // Chiudi la finestra
            if (_ownerWindow != null)
            {
                _ownerWindow.DialogResult = true;
                _ownerWindow.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore salvataggio indice:\n{ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void SetOwner(Window window)
    {
        _ownerWindow = window;
    }

    private void AggiornaFormulaAnteprima()
    {
        if (VociNumeratoreSelezionate.Count == 0)
        {
            FormulaAnteprima = "Seleziona le voci per il numeratore";
            return;
        }

        // Mostra CODICE + DESCRIZIONE per chiarezza
        var numeratore = string.Join(" + ", VociNumeratoreSelezionate.Select(v => $"{v.Codice} ({v.Descrizione})"));
        var denominatore = VociDenominatoreSelezionate.Count > 0
            ? string.Join(" + ", VociDenominatoreSelezionate.Select(v => $"{v.Codice} ({v.Descrizione})"))
            : "1";

        string simboloOperatore = OperatoreSelezionato.ToLower() switch
        {
            "divisione" => "÷",
            "moltiplicazione" => "×",
            "somma" => "+",
            "sottrazione" => "-",
            _ => "÷"
        };

        var molt = MoltiplicatoreSelezionato?.Valore ?? 1;
        var moltStr = molt != 1 ? $" × {molt}" : "";

        FormulaAnteprima = $"({numeratore}) {simboloOperatore} ({denominatore}){moltStr} {UnitaMisuraSelezionata}";
    }

    partial void OnFiltroVociNumeratoreChanged(string value)
    {
        AggiornaFiltroNumeratore();
    }

    partial void OnFiltroVociDenominatoreChanged(string value)
    {
        AggiornaFiltroDenominatore();
    }

    partial void OnOperatoreSelezionatoChanged(string value)
    {
        AggiornaFormulaAnteprima();
    }

    partial void OnMoltiplicatoreSelezionatoChanged(MoltiplicatoreItem? value)
    {
        AggiornaFormulaAnteprima();
    }

    partial void OnUnitaMisuraSelezionataChanged(string value)
    {
        AggiornaFormulaAnteprima();
    }

    private void AggiornaFiltroNumeratore()
    {
        VociDisponibiliFiltrate.Clear();

        var filtro = FiltroVociNumeratore?.ToLower() ?? "";
        var vociDaMostrare = string.IsNullOrWhiteSpace(filtro)
            ? VociDisponibili
            : VociDisponibili.Where(v =>
                v.Codice.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                v.Descrizione.Contains(filtro, StringComparison.OrdinalIgnoreCase)
            );

        foreach (var voce in vociDaMostrare)
        {
            VociDisponibiliFiltrate.Add(voce);
        }
    }

    private void AggiornaFiltroDenominatore()
    {
        VociDisponibiliFiltrateDenominatore.Clear();

        var filtro = FiltroVociDenominatore?.ToLower() ?? "";
        var vociDaMostrare = string.IsNullOrWhiteSpace(filtro)
            ? VociDisponibili
            : VociDisponibili.Where(v =>
                v.Codice.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                v.Descrizione.Contains(filtro, StringComparison.OrdinalIgnoreCase)
            );

        foreach (var voce in vociDaMostrare)
        {
            VociDisponibiliFiltrateDenominatore.Add(voce);
        }
    }
}

