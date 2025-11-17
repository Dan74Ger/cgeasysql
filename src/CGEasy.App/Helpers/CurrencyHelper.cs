using System.Globalization;

namespace CGEasy.App.Helpers;

/// <summary>
/// Helper per la formattazione della valuta in formato europeo con Euro
/// </summary>
public static class CurrencyHelper
{
    /// <summary>
    /// Formatta un importo decimale in formato europeo con simbolo Euro
    /// Esempio: 1234.56 → "1.234,56 €"
    /// </summary>
    public static string FormatEuro(decimal amount)
    {
        return amount.ToString("N2", CultureInfo.GetCultureInfo("it-IT")) + " €";
    }
}

