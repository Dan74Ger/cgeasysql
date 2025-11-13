using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CGEasy.Core.Models;

public class BilancioGruppo : INotifyPropertyChanged
{
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int Mese { get; set; }
    public int Anno { get; set; }
    public string? Descrizione { get; set; }
    public DateTime DataImport { get; set; }
    public string ImportedByName { get; set; } = string.Empty;
    public int NumeroRighe { get; set; }
    public decimal TotaleImporti { get; set; }
    
    // Proprietà per checkbox selezione
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }
    
    // Proprietà calcolate
    public string PeriodoDisplay => $"{MeseNome} {Anno}";
    public string PeriodoConDescrizioneDisplay => string.IsNullOrWhiteSpace(Descrizione) 
        ? $"{MeseNome} {Anno}" 
        : $"{MeseNome} {Anno} - {Descrizione}";
    public string MeseNome => Mese switch
    {
        1 => "Gen",
        2 => "Feb",
        3 => "Mar",
        4 => "Apr",
        5 => "Mag",
        6 => "Giu",
        7 => "Lug",
        8 => "Ago",
        9 => "Set",
        10 => "Ott",
        11 => "Nov",
        12 => "Dic",
        _ => ""
    };
    public string TotaleFormatted => TotaleImporti.ToString("C2");

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}





