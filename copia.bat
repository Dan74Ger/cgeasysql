@echo off
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
    echo [1/4] Creazione cartella backup principale...
    mkdir "%BACKUP_DIR%"
    echo       FATTO: %BACKUP_DIR%
) else (
    echo [1/4] Cartella backup gia' esistente: OK
)

echo.
echo [2/4] Creazione cartella backup con timestamp...
mkdir "%BACKUP_FOLDER%"
echo       FATTO: %BACKUP_FOLDER%

echo.
echo [3/4] Copia progetto in corso...
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
echo [4/4] Verifica backup...

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

REM Note: Database SQL Server non viene copiato (gestito da backup SQL Server separato)
echo.
echo [INFO] Database SQL Server: Backup gestito separatamente dal server
echo        Repository: https://github.com/Dan74Ger/cgeasysql

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
