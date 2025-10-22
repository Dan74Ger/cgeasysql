# Script per leggere dati dal database CGEasy
param(
    [string]$Collection = "clienti",
    [int]$Limit = 10
)

$dbPath = "C:\Users\Public\Documents\CGEasy\cgeasy.db"
$liteDllPath = "src\CGEasy.Core\bin\Debug\net8.0\LiteDB.dll"

# Se non trovo in net8.0, provo in net8.0-windows
if (!(Test-Path $liteDllPath)) {
    $liteDllPath = "src\CGEasy.App\bin\Debug\net8.0-windows\LiteDB.dll"
}

Write-Host "=== CGEasy Database Reader ===" -ForegroundColor Cyan
Write-Host ""

# Verifica database
if (!(Test-Path $dbPath)) {
    Write-Host "Database non trovato: $dbPath" -ForegroundColor Red
    exit
}

Write-Host "Database: $dbPath" -ForegroundColor Green
$dbInfo = Get-Item $dbPath
Write-Host ("  Dimensione: {0:N2} KB" -f ($dbInfo.Length / 1KB)) -ForegroundColor Gray
Write-Host ""

# Verifica LiteDB.dll
if (!(Test-Path $liteDllPath)) {
    Write-Host "LiteDB.dll non trovato. Compila prima il progetto:" -ForegroundColor Red
    Write-Host "  dotnet build src/CGEasy.App/CGEasy.App.csproj" -ForegroundColor Yellow
    exit
}

# Carica assembly
try {
    Add-Type -Path (Resolve-Path $liteDllPath).Path
    Write-Host "LiteDB.dll caricato" -ForegroundColor Green
} catch {
    Write-Host "Errore caricamento LiteDB.dll" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "=== Collections Disponibili ===" -ForegroundColor Cyan
Write-Host "- clienti"
Write-Host "- utenti"
Write-Host "- professionisti"
Write-Host "- tipo_pratiche"
Write-Host "- user_permissions"
Write-Host "- audit_logs"
Write-Host ""

$db = $null
try {
    # Apri database
    $db = New-Object LiteDB.LiteDatabase($dbPath)
    
    # Ottieni collection
    $col = $db.GetCollection($Collection)
    $count = $col.Count()
    
    Write-Host "=== Collection: $Collection ===" -ForegroundColor Cyan
    Write-Host "Totale record: $count" -ForegroundColor Green
    Write-Host ""
    
    if ($count -eq 0) {
        Write-Host "Nessun record trovato" -ForegroundColor Yellow
    } else {
        Write-Host "Primi $Limit record:" -ForegroundColor Yellow
        Write-Host ""
        
        # Leggi documenti
        $docs = $col.FindAll() | Select-Object -First $Limit
        
        $counter = 1
        foreach ($doc in $docs) {
            Write-Host "--- Record $counter ---" -ForegroundColor DarkGray
            $doc | Format-List
            $counter++
        }
        
        if ($count -gt $Limit) {
            Write-Host ""
            Write-Host "... e altri $(($count - $Limit)) record" -ForegroundColor Gray
        }
    }
    
} catch {
    Write-Host "Errore lettura database" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
} finally {
    if ($db -ne $null) { 
        $db.Dispose() 
    }
}

Write-Host ""
Write-Host "=== Uso ===" -ForegroundColor Cyan
Write-Host '.\leggi_database.ps1 -Collection clienti -Limit 10'
Write-Host '.\leggi_database.ps1 -Collection utenti -Limit 5'
Write-Host ""
