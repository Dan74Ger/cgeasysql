using System;
using System.Linq;
using System.Collections.Generic;

namespace CGEasy.Core.Helpers;

/// <summary>
/// Helper per ordinamento numerico dei codici mastrini
/// </summary>
public static class CodiceMastrinoHelper
{
    /// <summary>
    /// Estrae il valore numerico dal codice mastrino per ordinamento corretto.
    /// Esempi:
    /// "10" -> 10
    /// "20" -> 20
    /// "100" -> 100
    /// "A10" -> 10
    /// "CODICE123" -> 123
    /// "XYZ" -> 0 (se non contiene numeri)
    /// </summary>
    public static int GetNumericValue(string? codiceMastrino)
    {
        if (string.IsNullOrWhiteSpace(codiceMastrino))
            return 0;

        // Estrai tutti i numeri dal codice
        var numeriStr = new string(codiceMastrino.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(numeriStr))
            return 0;

        // Prova a convertire in int (con limite per sicurezza)
        if (int.TryParse(numeriStr, out int numero))
            return numero;

        return 0;
    }

    /// <summary>
    /// Ordina una lista per CodiceMastrino in modo numerico corretto.
    /// Mantiene l'ordinamento numerico: 10, 20, 30, 100, 110, 120 (non 10, 100, 110, 120, 20, 30)
    /// </summary>
    public static IOrderedEnumerable<T> OrderByCodiceMastrinoNumerico<T>(this IEnumerable<T> source, Func<T, string?> codiceMastrinoSelector)
    {
        return source.OrderBy(item => GetNumericValue(codiceMastrinoSelector(item)))
                     .ThenBy(codiceMastrinoSelector); // Fallback alfabetico se stesso numero
    }
}

