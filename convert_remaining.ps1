# Script per convertire TUTTI i repository e ViewModel rimanenti
# Eseguire dalla root del progetto

Write-Host "🚀 CONVERSIONE AUTOMATICA REPOSITORY E VIEWMODELS" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Lista repository da convertire
$repositories = @(
    "BancaRepository",
    "BancaIncassoRepository",
    "BancaPagamentoRepository",
    "BancaUtilizzoAnticipoRepository",
    "BancaSaldoGiornalieroRepository",
    "FinanziamentoImportRepository",
    "BilancioContabileRepository",
    "BilancioTemplateRepository",
    "AssociazioneMastrinoRepository",
    "AssociazioneMastrinoDettaglioRepository",
    "StatisticaSPSalvataRepository",
    "StatisticaCESalvataRepository",
    "IndicePersonalizzatoRepository",
    "TodoStudioRepository"
)

# Lista ViewModel da convertire
$viewmodels = @(
    "ImportaCircolareViewModel",
    "ModificaCircolareDialogViewModel",
    "GestioneBancheViewModel",
    "BancaDettaglioViewModel",
    "RiepilogoBancheViewModel",
    "IncassoDialogViewModel",
    "PagamentoDialogViewModel",
    "PagamentoMensileDialogViewModel",
    "AnticipoDialogViewModel",
    "BilancioContabileViewModel",
    "BilancioDettaglioViewModel",
    "BilancioDialogViewModel",
    "BilancioTemplateViewModel",
    "BilancioTemplateDettaglioViewModel",
    "ImportBilancioViewModel",
    "StatisticheBilanciViewModel",
    "StatisticheBilanciCEViewModel",
    "StatisticheBilanciSPViewModel",
    "IndiciDiBilancioViewModel",
    "ConfigurazioneIndiciViewModel",
    "IndicePersonalizzatoDialogViewModel",
    "AssociazioniMastriniViewModel",
    "AssociazioneMastrinoDialogViewModel",
    "TodoStudioViewModel",
    "TodoKanbanViewModel",
    "TodoCalendarioViewModel",
    "TodoDialogViewModel"
)

# FASE 1: Repository
Write-Host "`n📦 FASE 1: Conversione Repository ($($repositories.Count) files)" -ForegroundColor Yellow

$repoConverted = 0
foreach ($repo in $repositories) {
    $file = "src/CGEasy.Core/Repositories/$repo.cs"
    
    if (Test-Path $file) {
        Write-Host "  Converting $repo..." -ForegroundColor White
        
        try {
            $content = Get-Content $file -Raw
            
            # Backup
            Copy-Item $file "$file.bak" -Force
            
            # Conversioni base
            $content = $content -replace 'using LiteDB;', ''
            
            # Aggiungi using se mancano
            if ($content -notmatch 'using Microsoft.EntityFrameworkCore') {
                $insertPoint = $content.IndexOf('namespace')
                if ($insertPoint -gt 0) {
                    $content = $content.Insert($insertPoint, "using Microsoft.EntityFrameworkCore;`nusing System.Threading.Tasks;`n`n")
                }
            }
            
            # Converti metodi
            $content = $content -replace '\.FindAll\(\)', '.AsNoTracking().ToListAsync()'
            $content = $content -replace '\.FindById\(([^)]+)\)', '.FindAsync($1)'
            $content = $content -replace '\.Find\(', '.Where('
            $content = $content -replace '\.Query\(\)', '.AsQueryable()'
            $content = $content -replace '\.Insert\(', '.Add('
            $content = $content -replace '\.DeleteMany\(', '.Where('
            
            # Converti signature metodi
            $content = $content -replace 'public\s+List<(\w+)>\s+Get', 'public async Task<List<$1>> Get'
            $content = $content -replace 'public\s+(\w+\??)\s+Get(\w+)\(', 'public async Task<$1> Get$2('
            $content = $content -replace 'public\s+int\s+Insert', 'public async Task<int> Insert'
            $content = $content -replace 'public\s+bool\s+Update', 'public async Task<bool> Update'
            $content = $content -replace 'public\s+bool\s+Delete', 'public async Task<bool> Delete'
            $content = $content -replace 'public\s+int\s+Count', 'public async Task<int> Count'
            
            Set-Content $file $content -NoNewline
            $repoConverted++
            Write-Host "    ✅ $repo converted" -ForegroundColor Green
        }
        catch {
            Write-Host "    ❌ Error: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "    ⚠️ $repo not found" -ForegroundColor Yellow
    }
}

# FASE 2: ViewModels
Write-Host "`n🖥️ FASE 2: Conversione ViewModels ($($viewmodels.Count) files)" -ForegroundColor Yellow

$vmConverted = 0
foreach ($vm in $viewmodels) {
    $file = "src/CGEasy.App/ViewModels/$vm.cs"
    
    if (Test-Path $file) {
        Write-Host "  Converting $vm..." -ForegroundColor White
        
        try {
            $content = Get-Content $file -Raw
            
            # Backup
            Copy-Item $file "$file.bak" -Force
            
            # Aggiungi using
            if ($content -notmatch 'using System.Threading.Tasks') {
                $insertPoint = $content.IndexOf('namespace')
                if ($insertPoint -gt 0) {
                    $content = $content.Insert($insertPoint, "using System.Threading.Tasks;`n")
                }
            }
            
            # Aggiungi IsLoading se manca
            if ($content -notmatch '_isLoading') {
                $classPos = $content.IndexOf('public partial class')
                if ($classPos -gt 0) {
                    $bracePos = $content.IndexOf('{', $classPos) + 1
                    $newProperty = "`n    [ObservableProperty]`n    private bool _isLoading;`n"
                    $content = $content.Insert($bracePos, $newProperty)
                }
            }
            
            # Converti metodi Load
            $content = $content -replace 'private\s+void\s+Load(\w+)\(\)', 'private async Task Load$1Async()'
            $content = $content -replace 'LoadData\(\)', 'LoadDataAsync()'
            
            # Converti RelayCommand
            $content = $content -replace '\[RelayCommand\]\s+private\s+void\s+(\w+)\(', '[RelayCommand]$0private async Task $1Async('
            
            Set-Content $file $content -NoNewline
            $vmConverted++
            Write-Host "    ✅ $vm converted" -ForegroundColor Green
        }
        catch {
            Write-Host "    ❌ Error: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "    ⚠️ $vm not found" -ForegroundColor Yellow
    }
}

# RIEPILOGO
Write-Host "`n=============================================" -ForegroundColor Cyan
Write-Host "✅ CONVERSIONE COMPLETATA!" -ForegroundColor Green
Write-Host "  Repository convertiti: $repoConverted/$($repositories.Count)" -ForegroundColor White
Write-Host "  ViewModels convertiti: $vmConverted/$($viewmodels.Count)" -ForegroundColor White
Write-Host "`nPROSSIMI PASSI:" -ForegroundColor Yellow
Write-Host "  1. Compila: dotnet build" -ForegroundColor White
Write-Host "  2. Controlla errori: dotnet build 2>&1 | Select-String -Pattern 'error'" -ForegroundColor White
Write-Host "  3. Correggi manualmente gli errori" -ForegroundColor White
Write-Host "  4. Testa i moduli convertiti" -ForegroundColor White

