using CGEasy.Core.Models;
using ClosedXML.Excel;
using System.Globalization;

namespace CGEasy.Core.Services;

public class ExcelBilancioService
{
    /// <summary>
    /// Importa mastrini da file Excel
    /// Formato: Codice | Descrizione | Importo
    /// Prima riga = intestazioni (saltata)
    /// </summary>
    public static List<BilancioContabile> ImportFromExcel(
        string filePath,
        int clienteId,
        string clienteNome,
        string? descrizioneBilancio,
        int mese,
        int anno,
        int importedBy,
        string importedByName)
    {
        var bilanci = new List<BilancioContabile>();

        System.Diagnostics.Debug.WriteLine($"[EXCEL IMPORT] Inizio lettura file: {filePath}");
        
        using var workbook = new XLWorkbook(filePath);
        var worksheet = workbook.Worksheet(1); // Primo foglio

        // Trova ultima riga con dati
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        
        System.Diagnostics.Debug.WriteLine($"[EXCEL IMPORT] Ultima riga Excel: {lastRow}");
        System.Diagnostics.Debug.WriteLine($"[EXCEL IMPORT] Inizio lettura da riga 2 a riga {lastRow}");

        // Salta prima riga (intestazioni), parte da riga 2
        for (int row = 2; row <= lastRow; row++)
        {
            var codice = worksheet.Cell(row, 1).GetString().Trim();
            var descrizione = worksheet.Cell(row, 2).GetString().Trim();
            
            // Salta righe vuote
            if (string.IsNullOrWhiteSpace(codice) && 
                string.IsNullOrWhiteSpace(descrizione) && 
                worksheet.Cell(row, 3).IsEmpty())
                continue;

            // Valida campi obbligatori
            if (string.IsNullOrWhiteSpace(codice))
                throw new Exception($"Riga {row}: Codice Mastrino obbligatorio");

            if (string.IsNullOrWhiteSpace(descrizione))
                throw new Exception($"Riga {row}: Descrizione obbligatoria");

            // Parse importo - CORRETTO: legge direttamente come numero dalla cella Excel
            decimal importo;
            var cellImporto = worksheet.Cell(row, 3);
            
            if (cellImporto.IsEmpty())
            {
                throw new Exception($"Riga {row}: Importo obbligatorio");
            }
            
            // Prova a leggere come numero diretto (funziona sempre con celle numeri formattate)
            if (cellImporto.TryGetValue(out double importoDouble))
            {
                importo = (decimal)importoDouble;
            }
            // Altrimenti prova a fare parsing della stringa
            else
            {
                var importoStr = cellImporto.GetString().Trim();
                
                // Prova prima con cultura italiana (59.350,29)
                if (decimal.TryParse(importoStr, NumberStyles.Any, CultureInfo.GetCultureInfo("it-IT"), out importo))
                {
                    // OK, parsato correttamente
                }
                // Poi prova con cultura invariante (59350.29 o 59,350.29)
                else if (decimal.TryParse(importoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out importo))
                {
                    // OK, parsato correttamente
                }
                else
                {
                    throw new Exception($"Riga {row}: Importo '{importoStr}' non valido. Usa formato italiano (es: 59.350,29) o americano (es: 59350.29)");
                }
            }

            bilanci.Add(new BilancioContabile
            {
                ClienteId = clienteId,
                ClienteNome = clienteNome,
                Mese = mese,
                Anno = anno,
                DescrizioneBilancio = descrizioneBilancio,
                CodiceMastrino = codice,
                DescrizioneMastrino = descrizione,
                Importo = importo,
                DataImport = DateTime.Now,
                ImportedBy = importedBy,
                ImportedByName = importedByName
            });
        }

        if (bilanci.Count == 0)
            throw new Exception("Nessuna riga valida trovata nel file Excel");

        return bilanci;
    }

    /// <summary>
    /// Esporta bilanci in Excel
    /// </summary>
    public static void ExportToExcel(List<BilancioContabile> bilanci, string filePath)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Bilancio Contabile");

        // Intestazioni
        worksheet.Cell(1, 1).Value = "Cliente";
        worksheet.Cell(1, 2).Value = "Periodo";
        worksheet.Cell(1, 3).Value = "Mese";
        worksheet.Cell(1, 4).Value = "Anno";
        worksheet.Cell(1, 5).Value = "Codice Mastrino";
        worksheet.Cell(1, 6).Value = "Descrizione";
        worksheet.Cell(1, 7).Value = "Importo";
        worksheet.Cell(1, 8).Value = "Note";
        worksheet.Cell(1, 9).Value = "Data Import";
        worksheet.Cell(1, 10).Value = "Importato Da";

        // Formatta intestazioni
        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Dati
        int row = 2;
        foreach (var bilancio in bilanci)
        {
            worksheet.Cell(row, 1).Value = bilancio.ClienteNome;
            worksheet.Cell(row, 2).Value = bilancio.PeriodoCompleto;
            worksheet.Cell(row, 3).Value = bilancio.Mese;
            worksheet.Cell(row, 4).Value = bilancio.Anno;
            worksheet.Cell(row, 5).Value = bilancio.CodiceMastrino;
            worksheet.Cell(row, 6).Value = bilancio.DescrizioneMastrino;
            worksheet.Cell(row, 7).Value = bilancio.Importo;
            worksheet.Cell(row, 8).Value = bilancio.Note ?? "";
            worksheet.Cell(row, 9).Value = bilancio.DataImport.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 10).Value = bilancio.ImportedByName;

            // Formatta importo come valuta
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "€#,##0.00;[Red]-€#,##0.00";

            row++;
        }

        // Auto-fit colonne
        worksheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);
    }
}

