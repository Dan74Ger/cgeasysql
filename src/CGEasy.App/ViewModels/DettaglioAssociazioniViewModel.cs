using CommunityToolkit.Mvvm.ComponentModel;
using CGEasy.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class DettaglioAssociazioniViewModel : ObservableObject
{
    [ObservableProperty]
    private string _titolo = string.Empty;

    [ObservableProperty]
    private string _codiceVoce = string.Empty;

    [ObservableProperty]
    private string _descrizioneVoce = string.Empty;

    [ObservableProperty]
    private string _clienteNome = string.Empty;

    [ObservableProperty]
    private string _periodo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ContoContabileAssociato> _contiAssociati = new();

    [ObservableProperty]
    private ObservableCollection<ContoContabileAssociatoConPeriodo> _contiAssociatiConPeriodo = new();

    [ObservableProperty]
    private bool _isMultiPeriodo;

    [ObservableProperty]
    private int _totaleConti;

    [ObservableProperty]
    private decimal _sommaImporti;

    [ObservableProperty]
    private string _sommaImportiFormatted = string.Empty;

    public DettaglioAssociazioniViewModel(
        BilancioStatistica statistica,
        string clienteNome,
        string periodo)
    {
        IsMultiPeriodo = false;
        CodiceVoce = statistica.Codice;
        DescrizioneVoce = statistica.Descrizione;
        ClienteNome = clienteNome;
        Periodo = periodo;
        Titolo = $"Dettaglio Conti Contabili Associati - [{CodiceVoce}] {DescrizioneVoce}";

        // Carica conti
        foreach (var conto in statistica.ContiAssociati.OrderBy(c => c.CodiceConto))
        {
            ContiAssociati.Add(conto);
        }

        TotaleConti = ContiAssociati.Count;
        SommaImporti = ContiAssociati.Sum(c => c.Importo);
        SommaImportiFormatted = SommaImporti.ToString("N2") + " €";
    }

    public DettaglioAssociazioniViewModel(
        BilancioStatisticaMultiPeriodo statistica,
        string clienteNome)
    {
        IsMultiPeriodo = true;
        CodiceVoce = statistica.Codice;
        DescrizioneVoce = statistica.Descrizione;
        ClienteNome = clienteNome;
        Periodo = "Multi-Periodo";
        Titolo = $"Dettaglio Conti Contabili Associati - [{CodiceVoce}] {DescrizioneVoce}";

        // Carica conti multi-periodo
        foreach (var conto in statistica.ContiAssociatiTutti.OrderBy(c => c.Anno).ThenBy(c => c.Mese).ThenBy(c => c.CodiceConto))
        {
            ContiAssociatiConPeriodo.Add(conto);
        }

        TotaleConti = ContiAssociatiConPeriodo.Count;
        SommaImporti = ContiAssociatiConPeriodo.Sum(c => c.Importo);
        SommaImportiFormatted = SommaImporti.ToString("N2") + " €";
    }
}

