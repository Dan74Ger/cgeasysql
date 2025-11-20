# Script per sistemare tutti i ViewModel che usano LiteDbContext

$files = @(
    "AnticipoDialogViewModel.cs",
    "ArgomentiViewModel.cs",
    "AssociazioneMastrinoDialogViewModel.cs",
    "AssociazioniMastriniViewModel.cs",
    "BancaDettaglioViewModel.cs",
    "BilancioContabileViewModel.cs",
    "BilancioDettaglioViewModel.cs",
    "BilancioDialogViewModel.cs",
    "BilancioTemplateDialogViewModel.cs",
    "BilancioTemplateViewModel.cs",
    "GestioneBancheViewModel.cs",
    "ImportaCircolareViewModel.cs",
    "ImportBilancioViewModel.cs",
    "IncassoDialogViewModel.cs",
    "LicenseManagerViewModel.cs",
    "ModificaCircolareDialogViewModel.cs",
    "PagamentoDialogViewModel.cs",
    "PagamentoMensileDialogViewModel.cs",
    "RicercaCircolariViewModel.cs",
    "RiepilogoBancheViewModel.cs",
    "StatisticheBilanciCEViewModel.cs",
    "StatisticheBilanciSPViewModel.cs",
    "StatisticheBilanciViewModel.cs",
    "TodoStudioViewModel.cs",
    "BilancioTemplateDettaglioViewModel.cs",
    "ConfigurazioneIndiciViewModel.cs",
    "IndiciDiBilancioViewModel.cs",
    "IndicePersonalizzatoDialogViewModel.cs",
    "GraficiViewModel.cs"
)

$basePath = "src\CGEasy.App\ViewModels"
$count = 0
$errors = 0

Write-Host "Inizio sostituzione LiteDbContext -> CGEasyDbContext..." -ForegroundColor Cyan
Write-Host ""

foreach ($file in $files) {
    $path = Join-Path $basePath $file
    
    if (Test-Path $path) {
        try {
            $content = Get-Content $path -Raw -Encoding UTF8
            $originalContent = $content
            
            # Sostituisci LiteDbContext con CGEasyDbContext
            $content = $content -replace 'private readonly LiteDbContext', 'private readonly CGEasyDbContext'
            $content = $content -replace 'LiteDbContext\s+_context', 'CGEasyDbContext _context'
            $content = $content -replace 'LiteDbContext\s+context\)', 'CGEasyDbContext context)'
            $content = $content -replace 'new LiteDbContext', 'new CGEasyDbContext'
            $content = $content -replace ': LiteDbContext', ': CGEasyDbContext'
            
            if ($content -ne $originalContent) {
                Set-Content $path -Value $content -NoNewline -Encoding UTF8
                Write-Host "✓ $file" -ForegroundColor Green
                $count++
            } else {
                Write-Host "- $file (già ok)" -ForegroundColor Gray
            }
        }
        catch {
            Write-Host "✗ $file - Errore: $_" -ForegroundColor Red
            $errors++
        }
    }
    else {
        Write-Host "? $file (non trovato)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Completato!" -ForegroundColor Green
Write-Host "   File modificati: $count" -ForegroundColor Green
Write-Host "   Errori: $errors" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
Write-Host "========================================" -ForegroundColor Cyan

