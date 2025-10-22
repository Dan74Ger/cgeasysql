# Script per controllare doppioni nel bilancio contabile
$ErrorActionPreference = "Stop"

Write-Host "=== Controllo Doppioni Bilancio Contabile ===" -ForegroundColor Cyan
Write-Host ""

# Crea un semplice programma C# temporaneo
$tempCs = Join-Path $env:TEMP "check_db.cs"
$tempExe = Join-Path $env:TEMP "check_db.exe"

$code = @'
using System;
using System.Linq;
using LiteDB;

class Program
{
    static void Main()
    {
        var dbPath = @"C:\Users\Public\Documents\CGEasy\cgeasy.db";
        var password = "Woodstockac@74";
        
        using (var db = new LiteDatabase($"Filename={dbPath};Password={password}"))
        {
            var col = db.GetCollection("bilancio_contabile");
            var all = col.FindAll().ToList();
            
            Console.WriteLine($"TOTALE record: {all.Count}");
            Console.WriteLine();
            
            var ciaoRecords = all.Where(d => 
                d.ContainsKey("ClienteNome") && 
                d["ClienteNome"].AsString.Contains("CIAO")).ToList();
            
            Console.WriteLine($"Record cliente CIAO: {ciaoRecords.Count}");
            Console.WriteLine();
            
            if (ciaoRecords.Count > 0)
            {
                foreach (var doc in ciaoRecords)
                {
                    Console.WriteLine($"ID: {doc["_id"]} | Codice: {doc["CodiceMastrino"]} | Desc: {doc["DescrizioneMastrino"]} | Importo: {doc["Importo"]}");
                }
            }
        }
    }
}
'@

$code | Out-File -FilePath $tempCs -Encoding UTF8

# Trova LiteDB.dll
$liteDbDll = Get-ChildItem -Path ".\src\CGEasy.Core\bin\Debug\net8.0" -Filter "LiteDB.dll" -Recurse | Select-Object -First 1

if (-not $liteDbDll) {
    Write-Host "Compilando CGEasy.Core..." -ForegroundColor Yellow
    dotnet build ".\src\CGEasy.Core\CGEasy.Core.csproj" -c Debug -v quiet
    $liteDbDll = Get-ChildItem -Path ".\src\CGEasy.Core\bin\Debug\net8.0" -Filter "LiteDB.dll" -Recurse | Select-Object -First 1
}

Write-Host "Compilando checker..." -ForegroundColor Yellow
csc /out:$tempExe /r:$liteDbDll.FullName $tempCs 2>&1 | Out-Null

if (Test-Path $tempExe) {
    & $tempExe
} else {
    Write-Host "Errore compilazione" -ForegroundColor Red
}

Write-Host ""
Write-Host "Completato!" -ForegroundColor Green


