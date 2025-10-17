@echo off
REM ========================================================
REM BACKUP RAPIDO PROGETTO CG EASY (senza timestamp)
REM Sovrascrive backup precedente in c:\devcg-group\backup\cgeasy_latest
REM ========================================================

echo.
echo === BACKUP RAPIDO CG EASY v2.0 ===
echo.

set SOURCE_DIR=%~dp0
set BACKUP_DIR=c:\devcg-group\backup\cgeasy_latest

echo Backup in corso...
echo Da: %SOURCE_DIR%
echo A:  %BACKUP_DIR%
echo.

REM Crea cartella se non esiste
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

REM Usa robocopy per sincronizzazione veloce
robocopy "%SOURCE_DIR%" "%BACKUP_DIR%" /MIR /XD bin obj .vs .git node_modules packages .nuget /XF *.user *.suo /R:2 /W:3 /MT:8 /NFL /NDL /NJH /NJS

if errorlevel 8 (
    echo.
    echo ERRORE durante il backup!
    pause
    exit /b 1
) else (
    echo.
    echo === BACKUP COMPLETATO ===
    echo.
)

timeout /t 2 >nul

