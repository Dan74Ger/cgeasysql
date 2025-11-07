# ========================================
# Script di Preparazione Installazione CGEasy
# ========================================
# Questo script prepara l'ambiente per un'installazione da zero

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PREPARAZIONE INSTALLAZIONE CGEASY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verifica percorso installazione
$installPath = "$env:ProgramData\CGEasy"
Write-Host "[1] Verifica cartella installazione: $installPath" -ForegroundColor Yellow

if (Test-Path $installPath) {
    Write-Host "    Cartella gia esistente!" -ForegroundColor Red
    $risposta = Read-Host "    Vuoi CANCELLARE tutto e ripartire da zero? (S/N)"
    
    if ($risposta -eq "S" -or $risposta -eq "s") {
        Write-Host "    Rimozione cartella esistente..." -ForegroundColor Red
        Remove-Item -Path $installPath -Recurse -Force
        Write-Host "    Cartella rimossa!" -ForegroundColor Green
    } else {
        Write-Host "    Operazione annullata." -ForegroundColor Yellow
        exit 0
    }
}

# 2. Crea struttura cartelle
Write-Host ""
Write-Host "[2] Creazione struttura cartelle..." -ForegroundColor Yellow

New-Item -ItemType Directory -Path "$installPath" -Force | Out-Null
New-Item -ItemType Directory -Path "$installPath\Backups" -Force | Out-Null
New-Item -ItemType Directory -Path "$installPath\Logs" -Force | Out-Null
New-Item -ItemType Directory -Path "$installPath\Allegati" -Force | Out-Null

Write-Host "    Struttura creata!" -ForegroundColor Green
Write-Host "       - $installPath\" -ForegroundColor Gray
Write-Host "       - $installPath\Backups\" -ForegroundColor Gray
Write-Host "       - $installPath\Logs\" -ForegroundColor Gray
Write-Host "       - $installPath\Allegati\" -ForegroundColor Gray

# 3. NON copiare database (verrà creato al primo avvio)
Write-Host ""
Write-Host "[3] Preparazione database..." -ForegroundColor Yellow
Write-Host "    Il database verra creato al primo avvio con solo admin e admin1" -ForegroundColor Cyan

# 4. Crea file licenses.json vuoto
Write-Host ""
Write-Host "[4] Creazione file licenses.json..." -ForegroundColor Yellow

$licensesJson = '{"LicenseInfo":{"ProductName":"CGEasy","Version":"1.0.0","LicenseType":"Trial","IsActivated":false},"Licenses":[]}'
Set-Content -Path "$installPath\licenses.json" -Value $licensesJson -Encoding UTF8
Write-Host "    File licenses.json creato!" -ForegroundColor Green

# 5. Imposta permessi
Write-Host ""
Write-Host "[5] Configurazione permessi..." -ForegroundColor Yellow

try {
    $acl = Get-Acl $installPath
    $permission = "BUILTIN\Users", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    Set-Acl $installPath $acl
    Write-Host "    Permessi configurati (Users: Full Control)" -ForegroundColor Green
} catch {
    Write-Host "    Impossibile impostare permessi: $_" -ForegroundColor Yellow
}

# 6. Condivisione di rete (opzionale)
Write-Host ""
Write-Host "[6] Condivisione di rete (opzionale)" -ForegroundColor Yellow
$rispostaRete = Read-Host "    Vuoi condividere la cartella in rete per multi-PC? (S/N)"

if ($rispostaRete -eq "S" -or $rispostaRete -eq "s") {
    $nomeCondivisione = "CGEasy"
    try {
        $existingShare = Get-SmbShare -Name $nomeCondivisione -ErrorAction SilentlyContinue
        if ($existingShare) {
            Remove-SmbShare -Name $nomeCondivisione -Force
        }
        
        New-SmbShare -Name $nomeCondivisione -Path $installPath -FullAccess "Everyone" -ErrorAction Stop | Out-Null
        Write-Host "    Condivisione creata: \\$env:COMPUTERNAME\$nomeCondivisione" -ForegroundColor Green
        Write-Host "       I client possono usare questo percorso!" -ForegroundColor Cyan
    } catch {
        Write-Host "    Errore creazione condivisione: $_" -ForegroundColor Red
        Write-Host "       (Esegui come Amministratore)" -ForegroundColor Yellow
    }
}

# 7. Riepilogo
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  INSTALLAZIONE PREPARATA!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Percorso: $installPath" -ForegroundColor Cyan
Write-Host "Database: Verra creato al primo avvio" -ForegroundColor Cyan
Write-Host "Utenti: admin (123456) e admin1 (123123)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Prossimi passi:" -ForegroundColor Yellow
Write-Host "   1. Compila: dotnet build" -ForegroundColor Gray
Write-Host "   2. Avvia: dotnet run --project src/CGEasy.App/CGEasy.App.csproj" -ForegroundColor Gray
Write-Host ""

# 8. Chiedi se vuoi compilare subito
$rispostaCompila = Read-Host "Vuoi compilare e avviare l'applicazione ora? (S/N)"
if ($rispostaCompila -eq "S" -or $rispostaCompila -eq "s") {
    Write-Host ""
    Write-Host "[Compilazione in corso...]" -ForegroundColor Yellow
    Set-Location "C:\devcg-group\appcg_easy_project"
    dotnet build
    Write-Host ""
    Write-Host "Compilazione completata!" -ForegroundColor Green
    Write-Host ""
    Write-Host "[Avvio applicazione...]" -ForegroundColor Yellow
    Write-Host "L'app si aprira e creera il database pulito con solo admin e admin1" -ForegroundColor Cyan
    Write-Host ""
    Start-Sleep -Seconds 2
    dotnet run --project src/CGEasy.App/CGEasy.App.csproj
}

Write-Host ""
Write-Host "Premi un tasto per uscire..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
