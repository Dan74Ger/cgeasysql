@echo off
setlocal enabledelayedexpansion
REM ========================================================
REM SCRIPT BACKUP PROGETTO CG EASY SQL SERVER
REM Copia progetto completo + database in cartella backup
REM ========================================================

echo.
echo ========================================
echo   BACKUP PROGETTO CG EASY SQL v2.1
echo ========================================
echo.

REM Imposta percorsi
set SOURCE_DIR=%~dp0
set BACKUP_DIR=C:\CGEASY_sql\backup
set TIMESTAMP=%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%
set BACKUP_FOLDER=%BACKUP_DIR%\cgeasy_backup_%TIMESTAMP%

echo SOURCE: %SOURCE_DIR%
echo DESTINATION: %BACKUP_FOLDER%
echo.

REM Crea cartella backup se non esiste
if not exist "%BACKUP_DIR%" (
    echo [1/5] Creazione cartella backup principale...
    mkdir "%BACKUP_DIR%"
    echo       FATTO: %BACKUP_DIR%
) else (
    echo [1/5] Cartella backup gia' esistente: OK
)

echo.
echo [2/5] Creazione cartella backup con timestamp...
mkdir "%BACKUP_FOLDER%"
echo       FATTO: %BACKUP_FOLDER%

echo.
echo [3/5] Copia progetto in corso...
echo       (Escludo: bin, obj, .vs, .git, node_modules, packages)
echo.

REM Copia tutto il progetto escludendo cartelle non necessarie
xcopy "%SOURCE_DIR%*.*" "%BACKUP_FOLDER%\" /E /I /H /Y /EXCLUDE:%~dp0xcopy_exclude.txt 2>nul

REM Se non esiste file exclude, usa robocopy con esclusioni
if errorlevel 1 (
    robocopy "%SOURCE_DIR%" "%BACKUP_FOLDER%" /E /XD bin obj .vs .git node_modules packages .nuget /XF *.user *.suo /R:3 /W:5 /MT:8 /NFL /NDL /NP
    if errorlevel 8 (
        echo       ERRORE durante la copia!
        pause
        exit /b 1
    )
)

echo.
echo [4/5] Verifica backup...

REM Conta file copiati
for /f %%A in ('dir /s /b "%BACKUP_FOLDER%\*.cs" 2^>nul ^| find /c /v ""') do set CS_COUNT=%%A
for /f %%A in ('dir /s /b "%BACKUP_FOLDER%\*.csproj" 2^>nul ^| find /c /v ""') do set PROJ_COUNT=%%A
for /f %%A in ('dir /s /b "%BACKUP_FOLDER%\*.xaml" 2^>nul ^| find /c /v ""') do set XAML_COUNT=%%A

echo       File .cs copiati: %CS_COUNT%
echo       File .csproj copiati: %PROJ_COUNT%
echo       File .xaml copiati: %XAML_COUNT%

REM Copia connection string SQL Server
set DB_CONFIG_PATH=C:\db_CGEASY
if exist "%DB_CONFIG_PATH%\connectionstring.txt" (
    echo.
    echo [+] Connection string SQL Server trovata...
    if not exist "%BACKUP_FOLDER%\Database\" mkdir "%BACKUP_FOLDER%\Database\"
    copy "%DB_CONFIG_PATH%\connectionstring.txt" "%BACKUP_FOLDER%\Database\connectionstring.txt" /Y >nul 2>&1
    echo       FATTO: Connection string copiata in \Database\
)

REM Copia migrations EF Core
if exist "%SOURCE_DIR%\src\CGEasy.Core\Migrations\" (
    echo [+] Migrations EF Core trovate...
    xcopy "%SOURCE_DIR%\src\CGEasy.Core\Migrations\*.*" "%BACKUP_FOLDER%\src\CGEasy.Core\Migrations\" /I /Y /S >nul 2>&1
    echo       FATTO: Migrations copiate
)

echo.
echo [5/5] Backup database SQL Server...
echo.

REM Crea cartella Database se non esiste
if not exist "%BACKUP_FOLDER%\Database\" mkdir "%BACKUP_FOLDER%\Database\"

REM Imposta parametri database
set DB_NAME=CGEasy
set DB_SERVER=localhost\SQLEXPRESS
set DB_BACKUP_FILE=%BACKUP_FOLDER%\Database\CGEasy_%TIMESTAMP%.bak

echo       Database: %DB_NAME%
echo       Server: %DB_SERVER%
echo       File: CGEasy_%TIMESTAMP%.bak
echo.

REM Verifica se sqlcmd e' disponibile
where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo [!] ATTENZIONE: sqlcmd non trovato - Backup database saltato
    echo     Installa SQL Server Command Line Utilities per abilitare backup automatico
    echo     Download: https://aka.ms/sqlcmd
    goto :skip_db_backup
)

REM Backup database con sqlcmd
echo       Backup in corso... (potrebbe richiedere alcuni minuti)
sqlcmd -S %DB_SERVER% -Q "BACKUP DATABASE [%DB_NAME%] TO DISK = N'%DB_BACKUP_FILE%' WITH NOFORMAT, NOINIT, NAME = N'CGEasy-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10, COMPRESSION" -b >nul 2>&1

if errorlevel 1 (
    echo [!] ATTENZIONE: Impossibile fare backup del database
    echo     Possibili cause:
    echo     - Database non in esecuzione
    echo     - Permessi insufficienti
    echo     - Nome server/database errato
    echo.
    echo     Tentativo backup alternativo con PowerShell...
    
    REM Tentativo alternativo con PowerShell
    powershell -Command "try { Invoke-Sqlcmd -ServerInstance '%DB_SERVER%' -Query \"BACKUP DATABASE [%DB_NAME%] TO DISK = N'%DB_BACKUP_FILE%' WITH COMPRESSION\" -ErrorAction Stop; exit 0 } catch { exit 1 }" >nul 2>&1
    
    if errorlevel 1 (
        echo [!] Backup database non riuscito
        echo     Il backup del codice e' comunque completo
    ) else (
        echo [OK] Backup database completato (via PowerShell)
        for %%A in ("%DB_BACKUP_FILE%") do set DB_SIZE=%%~zA
        set /a DB_SIZE_MB=!DB_SIZE! / 1048576
        echo       Dimensione: !DB_SIZE_MB! MB
    )
) else (
    echo [OK] Backup database completato con successo!
    for %%A in ("%DB_BACKUP_FILE%") do set DB_SIZE=%%~zA
    set /a DB_SIZE_MB=!DB_SIZE! / 1048576
    echo       Dimensione: !DB_SIZE_MB! MB
)

:skip_db_backup

echo.
echo [INFO] Repository: https://github.com/Dan74Ger/cgeasysql

echo.
echo ========================================
echo   BACKUP COMPLETATO CON SUCCESSO!
echo ========================================
echo.
echo Percorso backup: %BACKUP_FOLDER%
echo.

REM Chiedi se aprire cartella
set /p OPEN="Vuoi aprire la cartella di backup? (S/N): "
if /i "%OPEN%"=="S" (
    explorer "%BACKUP_FOLDER%"
)

echo.
echo Premi un tasto per chiudere...
pause >nul
