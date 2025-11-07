$excelPath = "C:\devcg-group\bilanci verifiche\esempioIncassi.xlsx"

# Carica l'assembly ClosedXML
Add-Type -Path "C:\devcg-group\appcg_easy_project\src\CGEasy.App\bin\Debug\net8.0-windows\ClosedXML.dll"

try {
    $workbook = New-Object ClosedXML.Excel.XLWorkbook($excelPath)
    $worksheet = $workbook.Worksheets.Worksheet(1)
    
    Write-Host "=== STRUTTURA FILE EXCEL ===" -ForegroundColor Cyan
    Write-Host "Nome Worksheet: $($worksheet.Name)" -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "=== PRIME 10 RIGHE ===" -ForegroundColor Cyan
    for ($row = 1; $row -le 10; $row++) {
        $line = ""
        for ($col = 1; $col -le 12; $col++) {
            $cellValue = $worksheet.Cell($row, $col).Value
            if ($cellValue) {
                $line += "Col$col`: $cellValue | "
            }
        }
        if ($line) {
            Write-Host "Riga $row`: $line" -ForegroundColor White
        }
    }
    
    $workbook.Dispose()
} catch {
    Write-Host "ERRORE: $_" -ForegroundColor Red
}

