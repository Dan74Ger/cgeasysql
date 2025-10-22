namespace CGEasy.Core.Models;

public class BilancioContabile
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int Mese { get; set; } // 1-12
    public int Anno { get; set; }
    public string? DescrizioneBilancio { get; set; } // NUOVO: Descrizione del bilancio
    public string CodiceMastrino { get; set; } = string.Empty;
    public string DescrizioneMastrino { get; set; } = string.Empty;
    public decimal Importo { get; set; }
    public string? Note { get; set; }
    public DateTime DataImport { get; set; }
    public int ImportedBy { get; set; }
    public string ImportedByName { get; set; } = string.Empty;

    // Proprietà calcolate per UI
    public string PeriodoDisplay => $"{Mese:00}/{Anno}";
    public string ImportoFormatted => Importo.ToString("C2");
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
}

