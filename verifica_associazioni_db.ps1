# Script per verificare associazioni mastrini nel database
$dbPath = "$env:USERPROFILE\Documents\CGEasy\cgeasy.db"

if (-not (Test-Path $dbPath)) {
    $dbPath = "C:\Users\Public\Documents\CGEasy\cgeasy.db"
}

Write-Host "=== VERIFICA ASSOCIAZIONI MASTRINI ===" -ForegroundColor Cyan
Write-Host "Database: $dbPath" -ForegroundColor Yellow
Write-Host ""

# Carica assembly LiteDB
$liteDbPath = ".\src\CGEasy.App\bin\Debug\net8.0-windows\LiteDB.dll"
if (-not (Test-Path $liteDbPath)) {
    Write-Host "ERRORE: LiteDB.dll non trovato. Compila prima il progetto." -ForegroundColor Red
    exit
}

Add-Type -Path $liteDbPath

try {
    # Usa password master hardcoded (come nel codice C#)
    $password = "Woodstockac@74"
    $connectionString = "Filename=$dbPath;Connection=direct;Password=$password"
    Write-Host "Apertura database con password master..." -ForegroundColor Green
    
    $db = New-Object LiteDB.LiteDatabase($connectionString)
    
    # Ottieni collezione AssociazioniMastrini
    $collectionAssociazioni = $db.GetCollection("associazioni_mastrini")
    $collectionDettagli = $db.GetCollection("associazioni_mastrini_dettagli")
    
    Write-Host "=== ASSOCIAZIONI (Testata) ===" -ForegroundColor Green
    $associazioni = $collectionAssociazioni.FindAll()
    $count = 0
    foreach ($ass in $associazioni) {
        $count++
        Write-Host "`n--- Associazione #$count ---" -ForegroundColor Yellow
        Write-Host "ID: $($ass['_id'])"
        Write-Host "ClienteId: $($ass['cliente_id'])"
        Write-Host "ClienteNome: $($ass['cliente_nome'])"
        Write-Host "Mese: $($ass['mese'])"
        Write-Host "Anno: $($ass['anno'])"
        Write-Host "TemplateId: $($ass['template_id'])" -ForegroundColor Cyan
        Write-Host "TemplatNome: $($ass['template_nome'])" -ForegroundColor Cyan
        Write-Host "Descrizione: $($ass['descrizione'])"
        Write-Host "NumeroMappature: $($ass['numero_mappature'])"
        Write-Host "DataCreazione: $($ass['data_creazione'])"
        
        # Ottieni dettagli per questa associazione
        Write-Host "`n  === DETTAGLI (Mappature) ===" -ForegroundColor Magenta
        $dettagli = $collectionDettagli.Find([LiteDB.Query]::EQ("associazione_id", $ass['_id']))
        $detCount = 0
        foreach ($det in $dettagli) {
            $detCount++
            Write-Host "  [$detCount] Mastrino: $($det['codice_mastrino']) - $($det['descrizione_mastrino'])"
            Write-Host "      Importo: $($det['importo'])"
            Write-Host "      TemplateVoceId: $($det['template_voce_id'])" -ForegroundColor Cyan
            Write-Host "      TemplateCodice: $($det['template_codice'])" -ForegroundColor Cyan
            Write-Host "      TemplateDescrizione: $($det['template_descrizione'])" -ForegroundColor Cyan
            Write-Host "      TemplateSegno: $($det['template_segno'])" -ForegroundColor Cyan
        }
        Write-Host "  Totale dettagli trovati: $detCount"
    }
    
    Write-Host "`n=== RIEPILOGO ===" -ForegroundColor Green
    Write-Host "Totale associazioni: $count"
    
    $db.Dispose()
    Write-Host "`nVerifica completata!" -ForegroundColor Green
}
catch {
    Write-Host "ERRORE: $_" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

