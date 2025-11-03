# 📦 CGEasy - Guida Creazione Installer

## 🎯 REQUISITI

1. **Inno Setup 6.x**
   - Download: https://jrsoftware.org/isdl.php
   - Installa versione completa (con preprocessore e tools)

2. **.NET 8.0 Desktop Runtime**
   - Il programma richiede .NET 8.0 per funzionare
   - L'installer verificherà se è presente e guiderà l'utente all'installazione

## 📋 PASSI PER CREARE L'INSTALLER

### **STEP 1: Compila il progetto in Release**

Prima di creare l'installer, compila il progetto in modalità **Release** per avere file ottimizzati:

```powershell
dotnet publish src\CGEasy.App\CGEasy.App.csproj -c Release -r win-x64 --self-contained false
```

**NOTA:** Se vuoi includere .NET nel pacchetto (self-contained), usa `--self-contained true` (il file sarà più grande).

### **STEP 2: Modifica lo script (se necessario)**

Apri `CGEasy_Installer.iss` e modifica se necessario:

- **Versione:** Cambia `MyAppVersion` (riga 5)
- **Percorso:** Se usi Release, cambia `Debug` in `Release` nelle righe `Source:`
- **Icona:** Se hai un file `.ico`, decommentare la riga `SetupIconFile`

### **STEP 3: Compila lo script con Inno Setup**

#### **Metodo 1: Interfaccia Grafica**

1. Apri **Inno Setup Compiler** (dal menu Start)
2. File → Open → Seleziona `CGEasy_Installer.iss`
3. Build → Compile (o premi F9)
4. Attendi la compilazione
5. Il file `CGEasy_Setup_v1.0.0.exe` verrà creato nella cartella `installer_output`

#### **Metodo 2: Riga di comando**

```powershell
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" CGEasy_Installer.iss
```

### **STEP 4: Testa l'installer**

1. Vai nella cartella `installer_output`
2. Esegui `CGEasy_Setup_v1.0.0.exe`
3. Segui il wizard di installazione
4. Verifica che il programma si avvii correttamente

## 📂 STRUTTURA INSTALLAZIONE

Dopo l'installazione, i file saranno così organizzati:

```
C:\Program Files\CGEasy\          → File del programma
  ├─ CGEasy.App.exe               → Eseguibile principale
  ├─ CGEasy.Core.dll              → DLL core
  ├─ CGEasy.*.dll                 → Altri moduli
  ├─ *.dll                        → Librerie di terze parti
  └─ [cartelle lingue]            → Risorse localizzate

C:\Users\[user]\AppData\Local\CGEasy\  → Dati utente
  ├─ cgeasy.db                    → Database principale
  ├─ DatabaseBackup\              → Backup automatici
  └─ logs\                        → File di log
```

## 🔧 PERSONALIZZAZIONI AVANZATE

### **Includere il database preconfigurato**

Se vuoi includere un database con dati iniziali (es: utente admin, template preimpostati):

1. Crea un file `cgeasy_template.db` con i dati iniziali
2. Aggiungi questa riga in `[Files]`:
   ```
   Source: "cgeasy_template.db"; DestDir: "{localappdata}\CGEasy"; DestName: "cgeasy.db"; Flags: onlyifdoesntexist
   ```

### **Includere documentazione**

Aggiungi file PDF o HTML:

```
Source: "docs\manuale.pdf"; DestDir: "{app}\docs"; Flags: ignoreversion
```

E crea un'icona nel menu Start:

```
Name: "{group}\Manuale Utente"; Filename: "{app}\docs\manuale.pdf"
```

### **Includere .NET Runtime nell'installer**

Per creare un pacchetto che include .NET (più grande ma autonomo):

1. Usa `--self-contained true` nella build
2. Cambia le righe `Source:` per includere tutti i file runtime

## ⚠️ NOTE IMPORTANTI

1. **Database:**
   - Il database viene creato in `%LOCALAPPDATA%\CGEasy`
   - Ogni utente Windows ha il proprio database
   - Per database condiviso, serve una versione server-client

2. **Permessi:**
   - L'installer richiede privilegi admin (per installare in Program Files)
   - L'app può girare senza admin (dati in AppData)

3. **Disinstallazione:**
   - L'installer crea automaticamente un uninstaller
   - I dati utente (database) NON vengono eliminati alla disinstallazione
   - Per eliminare anche i dati, l'utente deve cancellare manualmente `%LOCALAPPDATA%\CGEasy`

4. **Aggiornamenti:**
   - Per aggiornare, basta eseguire il nuovo installer
   - Inno Setup sovrascrive i file del programma
   - Il database viene preservato

## 🚀 DISTRIBUZIONE

Una volta creato `CGEasy_Setup_v1.0.0.exe`:

- **Dimensione:** ~50-100 MB (senza .NET runtime incluso)
- **Compatibilità:** Windows 10/11 (x64)
- **Requisiti:** .NET 8.0 Desktop Runtime

Puoi distribuire questo singolo file `.exe` tramite:
- Email
- Chiavetta USB
- Download da sito web
- Rete aziendale

## 🔐 FIRMA DIGITALE (Opzionale)

Per firmare digitalmente l'installer (consigliato per distribuzione professionale):

1. Ottieni un certificato di firma codice
2. Aggiungi in `[Setup]`:
   ```
   SignTool=signtool sign /f "path\to\certificate.pfx" /p "password" /t http://timestamp.digicert.com $f
   ```

## 📞 SUPPORTO

Per problemi o domande, contatta: support@cggroup.it










