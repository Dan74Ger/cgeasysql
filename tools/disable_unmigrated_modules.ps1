# Script per disabilitare temporaneamente i ViewModels non ancora migrati

$viewModelsToDisable = @(
    "UtentiViewModel",
    "LicenseManagerViewModel", 
    "IndiciDiBilancioViewModel",
    "ConfigurazioneIndiciViewModel",
    "IndicePersonalizzatoDialogViewModel",
    "GraficiViewModel",
    "AssociazioneMastrinoDialogViewModel",
    "StatisticheBilanciCEViewModel",
    "StatisticheBilanciSPViewModel",
    "BilancioTemplateDettaglioViewModel",
    "BilancioTemplateViewModel",
    "BilancioDettaglioViewModel",
    "BancaDettaglioViewModel",
    "BilancioContabileViewModel",
    "ImportBilancioViewModel",
    "RiepilogoBancheViewModel",
    "IncassoDialogViewModel",
    "PagamentoMensileDialogViewModel",
    "GestioneBancheViewModel",
    "AnticipoDialogViewModel",
    "PagamentoDialogViewModel",
    "RicercaCircolariViewModel",
    "ImportaCircolareViewModel",
    "ArgomentiViewModel",
    "ModificaCircolareDialogViewModel",
    "AssociazioniMastriniViewModel",
    "StatisticheBilanciViewModel",
    "BilancioTemplateDialogViewModel",
    "BilancioDialogViewModel",
    "TodoStudioViewModel"
)

Write-Host "⚠️ Questa operazione NON è necessaria." -ForegroundColor Yellow
Write-Host ""
Write-Host "STRATEGIA MIGLIORE:" -ForegroundColor Green
Write-Host "1. Lascia l'app così com'è" -ForegroundColor Cyan
Write-Host "2. Naviga SOLO nei moduli già migrati:" -ForegroundColor Cyan
Write-Host "   ✅ Dashboard" -ForegroundColor Green
Write-Host "   ✅ Sistema (ora funziona)" -ForegroundColor Green
Write-Host "   ✅ Gestione Utenti" -ForegroundColor Green
Write-Host ""
Write-Host "3. NON cliccare sugli altri moduli fino alla loro migrazione" -ForegroundColor Yellow
Write-Host ""
Write-Host "4. Quando serve un modulo, lo migrerai seguendo la guida:" -ForegroundColor Cyan
Write-Host "   📘 Vedi: pianosql.md per il processo completo" -ForegroundColor Cyan
Write-Host ""
Write-Host "Vuoi continuare comunque? (sconsigliato) [S/N]: " -NoNewline -ForegroundColor Red
$risposta = Read-Host

if ($risposta -ne "S") {
    Write-Host "✅ Operazione annullata. Usa l'app com'è!" -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Lista moduli da disabilitare:" -ForegroundColor Yellow
$viewModelsToDisable | ForEach-Object { Write-Host "  - $_" }
Write-Host ""
Write-Host "Totale: $($viewModelsToDisable.Count) moduli" -ForegroundColor Yellow

