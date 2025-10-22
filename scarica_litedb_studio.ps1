# Script per scaricare e avviare LiteDB Studio
# Visualizzatore grafico per database LiteDB

$studioUrl = "https://github.com/mbdavid/LiteDB.Studio/releases/download/v1.0.3/LiteDB.Studio.exe"
$toolsDir = "$PSScriptRoot\tools"
$studioPath = "$toolsDir\LiteDB.Studio.exe"
$dbPath = "C:\Users\Public\Documents\CGEasy\cgeasy.db"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   LiteDB Studio Downloader" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verifica esistenza database
Write-Host "[1/4] Verifica database..." -ForegroundColor Yellow
if (Test-Path $dbPath) {
    Write-Host "      Database trovato!" -ForegroundColor Green
    $dbInfo = Get-Item $dbPath
    Write-Host "      Percorso: $dbPath" -ForegroundColor Gray
    Write-Host ("      Dimensione: {0:N2} KB" -f ($dbInfo.Length / 1KB)) -ForegroundColor Gray
    Write-Host "      Ultima modifica: $($dbInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "      Database non trovato!" -ForegroundColor Red
    Write-Host "      Il database verra creato al primo avvio di CGEasy.App" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Vuoi scaricare LiteDB Studio comunque? (S/N)"
    if ($continue -ne "S" -and $continue -ne "s") {
        exit
    }
}

Write-Host ""

# Crea cartella tools
Write-Host "[2/4] Preparazione cartella..." -ForegroundColor Yellow
if (!(Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
    Write-Host "      Cartella creata: $toolsDir" -ForegroundColor Green
} else {
    Write-Host "      Cartella gia esistente" -ForegroundColor Green
}

Write-Host ""

# Scarica o verifica LiteDB Studio
Write-Host "[3/4] Download LiteDB Studio..." -ForegroundColor Yellow
if (Test-Path $studioPath) {
    Write-Host "      LiteDB Studio gia presente!" -ForegroundColor Green
    $fileInfo = Get-Item $studioPath
    Write-Host ("      Dimensione: {0:N2} MB" -f ($fileInfo.Length / 1MB)) -ForegroundColor Gray
} else {
    Write-Host "      Download in corso da GitHub..." -ForegroundColor Cyan
    Write-Host "      URL: $studioUrl" -ForegroundColor Gray
    
    try {
        # Download con progress bar
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $studioUrl -OutFile $studioPath -UseBasicParsing
        $ProgressPreference = 'Continue'
        
        Write-Host "      Download completato!" -ForegroundColor Green
        $fileInfo = Get-Item $studioPath
        Write-Host ("      Dimensione: {0:N2} MB" -f ($fileInfo.Length / 1MB)) -ForegroundColor Gray
    } catch {
        Write-Host "      Errore durante il download!" -ForegroundColor Red
        Write-Host "      $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "Download manuale:" -ForegroundColor Yellow
        Write-Host $studioUrl
        Write-Host ""
        Write-Host "Salva il file in: $studioPath" -ForegroundColor Yellow
        exit
    }
}

Write-Host ""

# Avvio LiteDB Studio
Write-Host "[4/4] Avvio LiteDB Studio..." -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   ISTRUZIONI D'USO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Si aprira LiteDB Studio" -ForegroundColor White
Write-Host "2. Clicca sul pulsante 'Open' (icona cartella)" -ForegroundColor White
Write-Host "3. Naviga in: C:\Users\Public\Documents\CGEasy\" -ForegroundColor White
Write-Host "4. Seleziona il file: cgeasy.db" -ForegroundColor White
Write-Host "5. Clicca 'Apri'" -ForegroundColor White
Write-Host ""
Write-Host "Vedrai le COLLECTIONS (tabelle):" -ForegroundColor Yellow
Write-Host "  - clienti          (Anagrafica clienti)" -ForegroundColor Gray
Write-Host "  - utenti           (Utenti sistema)" -ForegroundColor Gray
Write-Host "  - professionisti   (Anagrafica professionisti)" -ForegroundColor Gray
Write-Host "  - tipo_pratiche    (Tipologie pratiche)" -ForegroundColor Gray
Write-Host "  - user_permissions (Permessi utenti)" -ForegroundColor Gray
Write-Host "  - audit_logs       (Log operazioni)" -ForegroundColor Gray
Write-Host ""
Write-Host "SUGGERIMENTI:" -ForegroundColor Cyan
Write-Host "  - Clicca su una collection per vedere i record" -ForegroundColor Gray
Write-Host "  - Usa la ricerca per filtrare i dati" -ForegroundColor Gray
Write-Host "  - Puoi eseguire query SQL-like" -ForegroundColor Gray
Write-Host "  - Non modificare i dati direttamente!" -ForegroundColor Red
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Copia percorso DB nella clipboard (se possibile)
try {
    Set-Clipboard -Value $dbPath
    Write-Host "Percorso DB copiato negli appunti!" -ForegroundColor Green
    Write-Host "Puoi incollarlo (Ctrl+V) nella finestra di apertura file" -ForegroundColor Gray
    Write-Host ""
} catch {
    # Clipboard non disponibile, ignora
}

Start-Sleep -Seconds 2

# Avvia LiteDB Studio
Write-Host "Apertura LiteDB Studio..." -ForegroundColor Cyan
Start-Process $studioPath

Write-Host ""
Write-Host "LiteDB Studio avviato!" -ForegroundColor Green
Write-Host ""
Write-Host "Per riaprire in futuro:" -ForegroundColor Yellow
Write-Host "  .\tools\LiteDB.Studio.exe" -ForegroundColor White
Write-Host ""

