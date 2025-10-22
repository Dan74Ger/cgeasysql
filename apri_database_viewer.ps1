# Script per scaricare e aprire LiteDB Studio (Visualizzatore Database)
# LiteDB Studio è un tool grafico gratuito per esplorare database LiteDB

$dbPath = "C:\Users\Public\Documents\CGEasy\cgeasy.db"
$studioPath = "$PSScriptRoot\tools\LiteDB.Studio.exe"
$studioUrl = "https://github.com/mbdavid/LiteDB.Studio/releases/download/v1.0.3/LiteDB.Studio.exe"

Write-Host "=== CGEasy Database Viewer ===" -ForegroundColor Cyan
Write-Host ""

# Verifica esistenza database
if (Test-Path $dbPath) {
    Write-Host "✓ Database trovato: $dbPath" -ForegroundColor Green
    $dbInfo = Get-Item $dbPath
    Write-Host "  Dimensione: $([math]::Round($dbInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    Write-Host "  Ultima modifica: $($dbInfo.LastWriteTime)" -ForegroundColor Gray
} else {
    Write-Host "✗ Database non trovato in: $dbPath" -ForegroundColor Red
    Write-Host "  Il database verrà creato al primo avvio dell'applicazione." -ForegroundColor Yellow
    exit
}

Write-Host ""
Write-Host "=== LiteDB Studio ===" -ForegroundColor Cyan
Write-Host "LiteDB Studio è un visualizzatore grafico gratuito per database LiteDB"
Write-Host ""

# Crea cartella tools se non esiste
$toolsDir = "$PSScriptRoot\tools"
if (!(Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
}

# Scarica LiteDB Studio se non esiste
if (!(Test-Path $studioPath)) {
    Write-Host "⬇ Download LiteDB Studio..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Uri $studioUrl -OutFile $studioPath -UseBasicParsing
        Write-Host "✓ Download completato!" -ForegroundColor Green
    } catch {
        Write-Host "✗ Errore download: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Download manuale da:" -ForegroundColor Yellow
        Write-Host $studioUrl
        exit
    }
} else {
    Write-Host "✓ LiteDB Studio già installato" -ForegroundColor Green
}

Write-Host ""
Write-Host "🚀 Apertura LiteDB Studio..." -ForegroundColor Cyan
Write-Host ""
Write-Host "ISTRUZIONI:" -ForegroundColor Yellow
Write-Host "1. Clicca 'Open' nel menu"
Write-Host "2. Seleziona il file: $dbPath"
Write-Host "3. Esplora le collections: clienti, utenti, professionisti, etc."
Write-Host ""

# Apri LiteDB Studio
Start-Process $studioPath

