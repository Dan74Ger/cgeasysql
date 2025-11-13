# Script per verificare duplicati nel database
Write-Host "=== VERIFICA DUPLICATI BILANCIO LORENZINI ===" -ForegroundColor Cyan
Write-Host ""

$dbPath = "C:\db_CGEASY\cgeasy.db"

if (!(Test-Path $dbPath)) {
    Write-Host "❌ Database non trovato in: $dbPath" -ForegroundColor Red
    exit
}

Write-Host "✅ Database trovato" -ForegroundColor Green
Write-Host "📊 Query bilanci Lorenzini settembre 2025..." -ForegroundColor Yellow
Write-Host ""

# Usa dotnet per eseguire una query C#
$query = @"
using System;
using System.Linq;
using LiteDB;

var connStr = new ConnectionString
{
    Filename = @"$dbPath",
    Connection = ConnectionType.Shared
};

using (var db = new LiteDatabase(connStr))
{
    var bilanci = db.GetCollection<dynamic>("bilancio_contabile")
        .Find(x => x["Mese"] == 9 && x["Anno"] == 2025)
        .ToList();
    
    Console.WriteLine("Totale righe trovate: " + bilanci.Count);
    Console.WriteLine("");
    
    var grouped = bilanci
        .GroupBy(b => b["CodiceMastrino"].AsString)
        .Where(g => g.Count() > 1)
        .ToList();
    
    if (grouped.Count > 0)
    {
        Console.WriteLine("❌ TROVATI DUPLICATI:");
        foreach (var group in grouped)
        {
            Console.WriteLine("  Codice: " + group.Key + " -> " + group.Count() + " righe");
            foreach (var item in group)
            {
                Console.WriteLine("    ID: " + item["_id"] + 
                    ", Descrizione: '" + item["DescrizioneBilancio"]?.AsString + 
                    "', Importo: " + item["Importo"]);
            }
        }
    }
    else
    {
        Console.WriteLine("✅ Nessun duplicato trovato");
    }
}
"@

# Salva query in file temporaneo
$tempFile = [System.IO.Path]::GetTempFileName() + ".csx"
$query | Out-File -FilePath $tempFile -Encoding UTF8

Write-Host "Esecuzione query con dotnet-script..." -ForegroundColor Yellow
dotnet script $tempFile

Remove-Item $tempFile -Force

