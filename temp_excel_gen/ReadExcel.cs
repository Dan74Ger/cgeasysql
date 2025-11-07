using System;
using ClosedXML.Excel;

var excelPath = @"C:\devcg-group\bilanci verifiche\esempioIncassi.xlsx";

try
{
    using var workbook = new XLWorkbook(excelPath);
    var worksheet = workbook.Worksheets.First();
    
    Console.WriteLine("=== STRUTTURA FILE EXCEL ===");
    Console.WriteLine($"Nome Worksheet: {worksheet.Name}");
    Console.WriteLine($"Range usato: {worksheet.RangeUsed()?.RangeAddress}");
    Console.WriteLine();
    
    Console.WriteLine("=== PRIME 10 RIGHE ===");
    for (int row = 1; row <= 10; row++)
    {
        bool hasContent = false;
        var line = $"Riga {row}: ";
        
        for (int col = 1; col <= 12; col++)
        {
            var cell = worksheet.Cell(row, col);
            if (!cell.IsEmpty())
            {
                hasContent = true;
                line += $"[Col{col}={cell.Value}] ";
            }
        }
        
        if (hasContent)
        {
            Console.WriteLine(line);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERRORE: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}

