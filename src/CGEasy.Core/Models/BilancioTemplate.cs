using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CGEasy.Core.Models;

public class BilancioTemplate : INotifyPropertyChanged
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int Mese { get; set; } // 1-12
    public int Anno { get; set; }
    public string? DescrizioneBilancio { get; set; }
    
    private string _codiceMastrino = string.Empty;
    public string CodiceMastrino
    {
        get => _codiceMastrino;
        set
        {
            if (_codiceMastrino != value)
            {
                _codiceMastrino = value;
                OnPropertyChanged();
            }
        }
    }
    
    public string DescrizioneMastrino { get; set; } = string.Empty;
    
    private decimal _importo;
    public decimal Importo
    {
        get => _importo;
        set
        {
            if (_importo != value)
            {
                _importo = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImportoFormatted));
                OnPropertyChanged(nameof(HasDifferenza));
            }
        }
    }
    
    public string? Note { get; set; }
    public DateTime DataImport { get; set; }
    public int ImportedBy { get; set; }
    public string ImportedByName { get; set; } = string.Empty;

    // 🆕 CAMPI PER RICLASSIFICAZIONE
    private string _segno = "+";
    public string Segno
    {
        get => _segno;
        set
        {
            if (_segno != value)
            {
                _segno = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTestoRosso));
            }
        }
    }
    
    private string? _formula;
    public string? Formula
    {
        get => _formula;
        set
        {
            if (_formula != value)
            {
                _formula = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFormula));
            }
        }
    }
    
    private decimal _importoCalcolato;
    public decimal ImportoCalcolato
    {
        get => _importoCalcolato;
        set
        {
            if (_importoCalcolato != value)
            {
                _importoCalcolato = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImportoCalcolatoFormatted));
                OnPropertyChanged(nameof(HasDifferenza));
                OnPropertyChanged(nameof(IsTestoRosso));
            }
        }
    }

    // Proprietà calcolate per UI
    public string PeriodoDisplay => $"{Mese:00}/{Anno}";
    public string ImportoFormatted => Importo.ToString("C2");
    public string ImportoCalcolatoFormatted => ImportoCalcolato.ToString("C2");
    
    // Indica se c'è differenza tra importo originale e calcolato
    public bool HasDifferenza => Math.Abs(Importo - Math.Abs(ImportoCalcolato)) > 0.01m;
    
    // Indica se la riga ha una formula
    public bool HasFormula => !string.IsNullOrWhiteSpace(Formula);
    
    // ✅ Indica se il testo della riga deve essere ROSSO
    // Rosso SOLO se ImportoCalcolato è negativo (< 0)
    public bool IsTestoRosso => ImportoCalcolato < 0;
    
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
    public string PeriodoCompleto => $"{MeseNome} {Anno}";

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

