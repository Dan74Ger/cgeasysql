@echo off
REM Apre LiteDB Studio per visualizzare il database CGEasy

echo ========================================
echo    LiteDB Studio Launcher
echo ========================================
echo.

REM Verifica se LiteDB Studio esiste
if not exist ".\tools\LiteDB.Studio.exe" (
    echo [ERRORE] LiteDB Studio non trovato!
    echo.
    echo Esegui prima: scarica_litedb_studio.ps1
    echo.
    pause
    exit /b 1
)

echo Avvio LiteDB Studio...
echo.
echo ISTRUZIONI:
echo 1. Clicca su "Open" (icona cartella)
echo 2. Apri: C:\Users\Public\Documents\CGEasy\cgeasy.db
echo 3. Esplora le collections (clienti, utenti, etc.)
echo.

start "" ".\tools\LiteDB.Studio.exe"

echo LiteDB Studio avviato!
echo.

