@echo off
chcp 65001 > nul
setlocal EnableDelayedExpansion

echo ========================================
echo   🚀 GESTIONE TASK GIT - CGEasy Project
echo ========================================
echo.

REM Configurazione repository
set GIT_USER=dan74ger@gmail.com
set GIT_REPO=https://github.com/dan74ger/CGEasy.git

REM Vai alla directory del progetto
cd /d "C:\devcg-group\appcg_easy_project"

:MENU
echo.
echo ========================================
echo   📋 MENU OPERAZIONI
echo ========================================
echo.
echo   🚀 AZIONI RAPIDE:
echo   ----------------
echo   3. 📦 COMMIT + PUSH (Add + Commit + Push tutto insieme)
echo.
echo   📂 OPERAZIONI DETTAGLIATE:
echo   --------------------------
echo   1. 📊 Mostra stato repository (git status)
echo   2. 📝 Mostra modifiche (git diff)
echo   4. ➕ Aggiungi file (git add .)
echo   5. 💾 Solo commit
echo   6. ⬆️  Solo push
echo   7. 🔄 Pull da GitHub
echo   8. 📜 Log commit
echo   9. 🔧 Configura git user
echo   0. ❌ Esci
echo.
echo ========================================

set /p choice="Seleziona operazione [Premi 3 per commit rapido]: "

if "%choice%"=="" set choice=3
if "%choice%"=="1" goto STATUS
if "%choice%"=="2" goto DIFF
if "%choice%"=="3" goto QUICK_COMMIT_PUSH
if "%choice%"=="4" goto ADD
if "%choice%"=="5" goto COMMIT
if "%choice%"=="6" goto PUSH
if "%choice%"=="7" goto PULL
if "%choice%"=="8" goto LOG
if "%choice%"=="9" goto CONFIG
if "%choice%"=="0" goto END

echo ❌ Scelta non valida!
timeout /t 2 > nul
goto MENU

:STATUS
echo.
echo 📊 GIT STATUS:
echo ----------------------------------------
git status
echo ----------------------------------------
pause
goto MENU

:DIFF
echo.
echo 📝 GIT DIFF (modifiche non staged):
echo ----------------------------------------
git diff
echo ----------------------------------------
echo.
echo 📝 GIT DIFF --staged (modifiche staged):
echo ----------------------------------------
git diff --staged
echo ----------------------------------------
pause
goto MENU

:ADD
echo.
echo ➕ Aggiunta di tutti i file modificati...
git add .
echo ✅ File aggiunti con successo!
echo.
git status
pause
goto MENU

:COMMIT
echo.
echo 💾 COMMIT MODIFICHE
echo ----------------------------------------
set /p commit_msg="Inserisci il messaggio di commit: "
if "%commit_msg%"=="" (
    echo ❌ Messaggio di commit obbligatorio!
    pause
    goto MENU
)

git commit -m "%commit_msg%"
if errorlevel 1 (
    echo ❌ Errore durante il commit!
) else (
    echo ✅ Commit eseguito con successo!
)
pause
goto MENU

:PUSH
echo.
echo ⬆️  PUSH SU GITHUB
echo ----------------------------------------
echo ⚠️  IMPORTANTE: GitHub richiede un PERSONAL ACCESS TOKEN
echo     Non funziona più con username/password!
echo.
echo     Come ottenere il token:
echo     1. Vai su https://github.com/settings/tokens
echo     2. Click "Generate new token (classic)"
echo     3. Seleziona scope: repo (accesso completo)
echo     4. Copia il token generato
echo.
set /p confirm_push="Sei sicuro di voler fare push? (S/N): "
if /i not "%confirm_push%"=="S" (
    echo ❌ Push annullato
    pause
    goto MENU
)

echo.
echo 🔐 Username: %GIT_USER%
echo 🔐 Quando richiesto, inserisci il PERSONAL ACCESS TOKEN come password
echo.
git push origin master
if errorlevel 1 (
    echo.
    echo ❌ Errore durante il push!
    echo.
    echo 💡 Suggerimenti:
    echo    - Assicurati di usare un Personal Access Token valido
    echo    - Verifica la connessione internet
    echo    - Controlla il nome del branch (master/main)
) else (
    echo ✅ Push completato con successo!
)
pause
goto MENU

:PULL
echo.
echo ⬇️  PULL DA GITHUB
echo ----------------------------------------
git pull origin master
if errorlevel 1 (
    echo ❌ Errore durante il pull!
) else (
    echo ✅ Pull completato con successo!
)
pause
goto MENU

:LOG
echo.
echo 📜 ULTIMI 10 COMMIT:
echo ----------------------------------------
git log --oneline --graph --decorate -10
echo ----------------------------------------
pause
goto MENU

:CONFIG
echo.
echo 🔧 CONFIGURAZIONE GIT USER
echo ----------------------------------------
echo Configurazione corrente:
git config user.name
git config user.email
echo.
set /p new_name="Inserisci nome utente Git (invio per non modificare): "
set /p new_email="Inserisci email Git (invio per non modificare): "

if not "%new_name%"=="" (
    git config user.name "%new_name%"
    echo ✅ Nome utente impostato: %new_name%
)

if not "%new_email%"=="" (
    git config user.email "%new_email%"
    echo ✅ Email impostata: %new_email%
)

echo.
echo Nuova configurazione:
git config user.name
git config user.email
pause
goto MENU

:QUICK_COMMIT_PUSH
echo.
echo 📦 COMMIT + PUSH RAPIDO
echo ========================================
echo.
set /p quick_msg="Inserisci messaggio commit: "
if "%quick_msg%"=="" (
    echo ❌ Messaggio di commit obbligatorio!
    pause
    goto MENU
)

echo.
echo 1️⃣ Aggiunta file modificati...
git add .
echo ✅ File aggiunti

echo.
echo 2️⃣ Commit in corso...
git commit -m "%quick_msg%"
if errorlevel 1 (
    echo ❌ Errore durante il commit!
    pause
    goto MENU
)
echo ✅ Commit completato

echo.
echo 3️⃣ Push su GitHub...
echo 🔐 Inserisci il PERSONAL ACCESS TOKEN quando richiesto
git push origin master
if errorlevel 1 (
    echo ❌ Errore durante il push!
) else (
    echo ✅ Push completato con successo!
)
pause
goto MENU

:END
echo.
echo 👋 Uscita...
exit /b 0

