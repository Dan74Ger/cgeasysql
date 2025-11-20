@echo off
chcp 65001 > nul
setlocal EnableDelayedExpansion

REM Script migliorato per commit e push automatico
REM Mostra chiaramente i file da committare e gestisce meglio gli errori

REM Configurazione repository
REM IMPORTANTE: Imposta la variabile GIT_TOKEN come variabile d'ambiente di sistema
REM oppure crea un file "git_token.txt" nella root del progetto con il token
if exist "%~dp0git_token.txt" (
    set /p GIT_TOKEN=<"%~dp0git_token.txt"
    echo Token caricato da git_token.txt
) else (
    if "%GIT_TOKEN%"=="" (
        echo ERRORE: Token GitHub non configurato!
        echo Crea un file "git_token.txt" nella root con il token oppure imposta GIT_TOKEN come variabile d'ambiente.
        pause
        exit /b 1
    )
)

REM Vai alla directory del progetto
cd /d "C:\CGEASY_sql\appcg_easy_projectsql"

REM Messaggio di commit automatico con data e ora
for /f "tokens=1-3 delims=/ " %%a in ('date /t') do (set mydate=%%a/%%b/%%c)
for /f "tokens=1-2 delims=: " %%a in ('time /t') do (set mytime=%%a:%%b)
set commit_msg=Update %mydate% %mytime%

echo ========================================
echo  COMMIT E PUSH AUTOMATICO - CGEasy SQL
echo ========================================
echo.

echo [1] Verifica stato repository...
git status --porcelain > nul 2>&1
echo OK
echo.

echo [2] Aggiunta di TUTTI i file (modificati e nuovi)...
echo    Attendere...
git add -A 2>nul
if errorlevel 1 (
    echo ERRORE durante git add!
    pause
    exit /b 1
)
echo OK - Tutti i file aggiunti
echo.

echo [3] Mostra file da committare:
git status --short
echo.

echo [4] Commit con messaggio: %commit_msg%
git commit -m "%commit_msg%" 2>nul
if errorlevel 1 (
    echo.
    echo ==========================================
    echo   NESSUNA MODIFICA DA COMMITTARE
    echo ==========================================
    echo   Tutti i file sono gia aggiornati!
    echo ==========================================
    echo.
    pause
    exit /b 0
)
echo OK - Commit creato!
echo.

echo [5] Push su GitHub...
git push https://%GIT_TOKEN%@github.com/Dan74Ger/cgeasysql.git main 2>nul
if errorlevel 1 (
    echo ERRORE durante il push!
    echo Verifica la connessione di rete o i permessi.
    pause
    exit /b 1
)
echo OK - Push completato!
echo.

echo ========================================
echo   COMPLETATO CON SUCCESSO!
echo ========================================
echo.
echo Commit: %commit_msg%
echo Repository: https://github.com/Dan74Ger/cgeasysql
echo.
echo File committati e pushati:
git diff --name-only HEAD~1 HEAD | findstr /v "^$"
echo.
echo ========================================
echo.
pause
exit /b 0
