# Script per convertire logo.png in logo.ico
# Usa System.Drawing di .NET

Write-Host "Conversione logo.png in logo.ico..." -ForegroundColor Cyan

$inputPath = "src\CGEasy.App\Images\logo.png"
$outputPath = "src\CGEasy.App\Images\logo.ico"

if (-Not (Test-Path $inputPath)) {
    Write-Host "ERRORE: File logo.png non trovato in $inputPath" -ForegroundColor Red
    exit 1
}

try {
    # Carica System.Drawing
    Add-Type -AssemblyName System.Drawing
    
    # Carica l'immagine PNG
    $image = [System.Drawing.Image]::FromFile((Resolve-Path $inputPath))
    
    # Crea bitmap ridimensionato a 256x256 (dimensione icona)
    $bitmap = New-Object System.Drawing.Bitmap 256, 256
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $graphics.DrawImage($image, 0, 0, 256, 256)
    
    # Salva come ICO
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $fileStream = [System.IO.FileStream]::new($outputPath, [System.IO.FileMode]::Create)
    $icon.Save($fileStream)
    $fileStream.Close()
    
    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
    $image.Dispose()
    
    Write-Host "✅ Conversione completata!" -ForegroundColor Green
    Write-Host "File creato: $outputPath" -ForegroundColor White
    
} catch {
    Write-Host "❌ ERRORE durante la conversione: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}























