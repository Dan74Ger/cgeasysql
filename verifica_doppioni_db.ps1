# Script per verificare doppioni nel database LiteDB
# Legge direttamente dal database per vedere i record del cliente "CIAO"

$dbPath = "C:\Users\Public\Documents\CGEasy\cgeasy.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "❌ Database non trovato in: $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "📂 Database trovato: $dbPath" -ForegroundColor Green
Write-Host "📊 Dimensione: $((Get-Item $dbPath).Length / 1KB) KB" -ForegroundColor Cyan
Write-Host ""

# Compila ed esegue un programma C# per leggere il database
$code = @"
using System;
using System.Linq;
using LiteDB;

public class Program
{
    public static void Main(string[] args)
    {
        var dbPath = args[0];
        var password = args.Length > 1 ? args[1] : null;
        
        try
        {
            var connectionString = new ConnectionString
            {
                Filename = dbPath,
                Password = password
            };
            
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection("bilancio_contabile");
                var all = collection.FindAll().ToList();
                
                Console.WriteLine($"📊 TOTALE RECORD in bilancio_contabile: {all.Count}");
                Console.WriteLine("");
                
                // Cerca record per cliente "CIAO"
                var ciaoRecords = all.Where(doc => 
                {
                    var clienteNome = doc["ClienteNome"];
                    return clienteNome != null && clienteNome.AsString.Contains("CIAO");
                }).ToList();
                
                if (ciaoRecords.Count > 0)
                {
                    Console.WriteLine($"🔍 Trovati {ciaoRecords.Count} record per cliente contenente 'CIAO':");
                    Console.WriteLine("");
                    
                    foreach (var doc in ciaoRecords)
                    {
                        Console.WriteLine($"  ID: {doc["_id"]}");
                        Console.WriteLine($"  Cliente: {doc["ClienteNome"]}");
                        Console.WriteLine($"  ClienteId: {doc["ClienteId"]}");
                        Console.WriteLine($"  Codice: {doc["CodiceMastrino"]}");
                        Console.WriteLine($"  Descrizione: {doc["DescrizioneMastrino"]}");
                        Console.WriteLine($"  Importo: {doc["Importo"]}");
                        Console.WriteLine($"  DataImport: {doc["DataImport"]}");
                        Console.WriteLine($"  ImportedBy: {doc["ImportedByName"]}");
                        Console.WriteLine("  " + new string('-', 60));
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️  Nessun record trovato per cliente 'CIAO'");
                }
            }
        }
        catch (LiteException ex) when (ex.ErrorCode == LiteException.DATABASE_WRONG_PASSWORD)
        {
            Console.WriteLine("🔒 Database criptato - provo con password master...");
            
            var connectionString = new ConnectionString
            {
                Filename = dbPath,
                Password = "Woodstockac@74"
            };
            
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection("bilancio_contabile");
                var all = collection.FindAll().ToList();
                
                Console.WriteLine($"📊 TOTALE RECORD in bilancio_contabile: {all.Count}");
                Console.WriteLine("");
                
                var ciaoRecords = all.Where(doc => 
                {
                    var clienteNome = doc["ClienteNome"];
                    return clienteNome != null && clienteNome.AsString.Contains("CIAO");
                }).ToList();
                
                if (ciaoRecords.Count > 0)
                {
                    Console.WriteLine($"🔍 Trovati {ciaoRecords.Count} record per cliente contenente 'CIAO':");
                    Console.WriteLine("");
                    
                    foreach (var doc in ciaoRecords)
                    {
                        Console.WriteLine($"  ID: {doc["_id"]}");
                        Console.WriteLine($"  Cliente: {doc["ClienteNome"]}");
                        Console.WriteLine($"  ClienteId: {doc["ClienteId"]}");
                        Console.WriteLine($"  Codice: {doc["CodiceMastrino"]}");
                        Console.WriteLine($"  Descrizione: {doc["DescrizioneMastrino"]}");
                        Console.WriteLine($"  Importo: {doc["Importo"]}");
                        Console.WriteLine($"  DataImport: {doc["DataImport"]}");
                        Console.WriteLine($"  ImportedBy: {doc["ImportedByName"]}");
                        Console.WriteLine("  " + new string('-', 60));
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️  Nessun record trovato per cliente 'CIAO'");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Errore: {ex.Message}");
        }
    }
}
"@

# Trova il pacchetto LiteDB
$liteDbPackage = Get-ChildItem -Path "$PSScriptRoot\src\CGEasy.Core\bin\Debug\net8.0" -Filter "LiteDB.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if (-not $liteDbPackage) {
    Write-Host "⚠️  LiteDB.dll non trovato, compilo il progetto prima..." -ForegroundColor Yellow
    dotnet build "$PSScriptRoot\src\CGEasy.Core\CGEasy.Core.csproj" -c Debug | Out-Null
    $liteDbPackage = Get-ChildItem -Path "$PSScriptRoot\src\CGEasy.Core\bin\Debug\net8.0" -Filter "LiteDB.dll" -Recurse | Select-Object -First 1
}

$tempDir = Join-Path $env:TEMP "cgeasy_verify"
if (-not (Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir | Out-Null
}

$sourceFile = Join-Path $tempDir "Program.cs"
$code | Out-File -FilePath $sourceFile -Encoding UTF8

$liteDbPath = $liteDbPackage.FullName

# Compila
Write-Host "🔨 Compilazione..." -ForegroundColor Yellow
$exePath = Join-Path $tempDir "verify.exe"
csc /out:$exePath /r:$liteDbPath $sourceFile 2>&1 | Out-Null

if (Test-Path $exePath) {
    Write-Host "▶️  Esecuzione..." -ForegroundColor Yellow
    Write-Host ""
    & $exePath $dbPath
} else {
    Write-Host "❌ Compilazione fallita" -ForegroundColor Red
}

Write-Host ""
Write-Host "✅ Verifica completata" -ForegroundColor Green




