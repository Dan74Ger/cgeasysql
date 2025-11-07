# 🚀 Installazione CGEasy da Zero

## 📋 Panoramica

CGEasy è ora configurato per essere installato in modalità **PROFESSIONALE** con database condiviso multi-utente.

---

## 📂 Dove Viene Installato il Database

### ✅ **Installazione (Modalità Produzione)**
```
C:\ProgramData\CGEasy\
  ├── cgeasy.db         # Database principale
  ├── db.key            # Chiave crittografia
  ├── licenses.json     # Licenze
  ├── Backups\          # Backup automatici
  ├── Logs\             # Log operazioni
  └── Allegati\         # File allegati
```

**Perché `C:\ProgramData\`?**
- ✅ Accessibile da **tutti gli utenti** Windows
- ✅ **Condivisibile in rete** (es: `\\SERVER\CGEasy\`)
- ✅ **Non richiede permessi amministratore** per scrivere
- ✅ Standard per applicazioni multi-utente

### 🔧 **Sviluppo (Modalità Test)**
```
C:\devcg-group\dbtest_prova\
  ├── cgeasy.db         # Database di test
  ├── db.key
  ├── licenses.json
  └── Backups\
```

**Come funziona?**
- L'applicazione **rileva automaticamente** quale percorso usare:
  - Se esiste `C:\ProgramData\CGEasy\` → usa quello (PRODUZIONE)
  - Altrimenti → usa `C:\devcg-group\dbtest_prova\` (SVILUPPO)

---

## 🛠️ Procedura di Installazione

### **Step 1: Preparazione Ambiente**

1. **Esegui lo script di preparazione:**
   ```powershell
   .\prepara_installazione.ps1
   ```

   Lo script farà:
   - ✅ Crea cartella `C:\ProgramData\CGEasy\`
   - ✅ Crea sottocartelle (Backups, Logs, Allegati)
   - ✅ Copia database vuoto (se disponibile)
   - ✅ Crea `licenses.json` di default
   - ✅ Imposta permessi corretti
   - ✅ (Opzionale) Crea condivisione di rete

2. **Conferma opzioni:**
   - Condivisione di rete? → **Sì** (per multi-PC) / **No** (singolo PC)
   - Compilare ora? → **Sì** (consigliato)

---

### **Step 2: Compilazione Applicazione**

Se non l'hai già fatto nello script:

```powershell
cd C:\devcg-group\appcg_easy_project
dotnet build --configuration Release
```

---

### **Step 3: Primo Avvio**

```powershell
dotnet run --project src/CGEasy.App/CGEasy.App.csproj
```

**Al primo avvio:**
1. L'applicazione rileva che è in `C:\ProgramData\CGEasy\`
2. Crea automaticamente il database vuoto
3. Inizializza le collezioni (tabelle)
4. Crea utente **Administrator** di default
5. Richiede attivazione licenza

---

## 🔐 Crittografia Database

Il database è **sempre criptato** con password master.

### **Password Master**
```
Woodstockac@74
```

**Dove viene salvata?**
```
C:\ProgramData\CGEasy\db.key
```

**Gestione automatica:**
- ✅ Password salvata in modo sicuro (AES-256)
- ✅ Utenti **non** devono inserirla ad ogni avvio
- ✅ Recuperabile solo dal proprietario (Dott. Geron Daniele)

---

## 🌐 Configurazione Multi-PC (Rete Locale)

### **PC Server (primo PC)**

1. Esegui `prepara_installazione.ps1`
2. Quando chiede "Vuoi condividere la cartella in rete?" → **Sì**
3. La cartella sarà condivisa come: `\\NOME-PC\CGEasy\`

### **PC Client (altri PC)**

**Opzione A: Configurazione Manuale**
```json
// C:\Program Files\CGEasy\config.json
{
  "DatabasePath": "\\\\SERVER-PC\\CGEasy\\cgeasy.db"
}
```

**Opzione B: Variabile d'Ambiente**
```powershell
[System.Environment]::SetEnvironmentVariable("CGEASY_DB_PATH", "\\SERVER-PC\CGEasy\cgeasy.db", "Machine")
```

---

## 🔄 Migrazione da Database Esistente

Se hai già un database in `C:\devcg-group\dbtest_prova\`:

### **Opzione 1: Copia Manuale**
```powershell
Copy-Item "C:\devcg-group\dbtest_prova\cgeasy.db" "C:\ProgramData\CGEasy\cgeasy.db"
Copy-Item "C:\devcg-group\dbtest_prova\db.key" "C:\ProgramData\CGEasy\db.key"
Copy-Item "C:\devcg-group\dbtest_prova\licenses.json" "C:\ProgramData\CGEasy\licenses.json"
```

### **Opzione 2: Script Automatico**
```powershell
# Copia tutto mantenendo backup
$source = "C:\devcg-group\dbtest_prova"
$dest = "C:\ProgramData\CGEasy"

# Backup attuale (se esiste)
if (Test-Path "$dest\cgeasy.db") {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    Copy-Item "$dest\cgeasy.db" "$dest\Backups\backup_before_migration_$timestamp.db"
}

# Copia file
Copy-Item "$source\cgeasy.db" "$dest\cgeasy.db" -Force
Copy-Item "$source\db.key" "$dest\db.key" -Force
Copy-Item "$source\licenses.json" "$dest\licenses.json" -Force

Write-Host "✅ Migrazione completata!"
```

---

## ✅ Verifica Installazione

### **Controllo 1: Cartella Creata**
```powershell
Test-Path "C:\ProgramData\CGEasy\cgeasy.db"
# Output: True
```

### **Controllo 2: Permessi**
```powershell
(Get-Acl "C:\ProgramData\CGEasy").Access | Where-Object { $_.IdentityReference -like "*Users*" }
# Output: FileSystemRights: FullControl
```

### **Controllo 3: Condivisione di Rete**
```powershell
Get-SmbShare -Name "CGEasy"
# Output: Name: CGEasy, Path: C:\ProgramData\CGEasy
```

### **Controllo 4: Avvio Applicazione**
```powershell
dotnet run --project src/CGEasy.App/CGEasy.App.csproj
# L'app si avvia senza errori
```

---

## 🐛 Risoluzione Problemi

### **Errore: "Impossibile aprire il database"**
- **Causa**: Password errata o `db.key` mancante
- **Soluzione**: 
  ```powershell
  Copy-Item "C:\devcg-group\dbtest_prova\db.key" "C:\ProgramData\CGEasy\db.key" -Force
  ```

### **Errore: "Accesso negato"**
- **Causa**: Permessi insufficienti
- **Soluzione**: Esegui `prepara_installazione.ps1` come **Amministratore**

### **Database non si vede in rete**
- **Causa**: Condivisione non creata
- **Soluzione**: 
  ```powershell
  New-SmbShare -Name "CGEasy" -Path "C:\ProgramData\CGEasy" -FullAccess "Everyone"
  ```

### **L'app usa ancora il database di sviluppo**
- **Causa**: `C:\ProgramData\CGEasy\` non esiste
- **Soluzione**: Esegui `prepara_installazione.ps1`

---

## 📦 Creazione Installer (Prossimo Step)

Per creare un installer `.exe` professionale:

### **Tool Consigliato: Inno Setup**
- Download: https://jrsoftware.org/isdl.php
- Gratuito, Open Source
- Supporta wizard di setup personalizzati

### **Script Inno Setup** (da creare)
```iss
[Setup]
AppName=CGEasy
AppVersion=1.0.0
DefaultDirName={pf}\CGEasy
DefaultGroupName=CGEasy
OutputBaseFilename=CGEasy_Setup_v1.0.0

[Files]
Source: "src\CGEasy.App\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: recursesubdirs

[Run]
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\prepara_installazione.ps1"""; \
    Description: "Prepara ambiente database"; Flags: postinstall runhidden

[Icons]
Name: "{group}\CGEasy"; Filename: "{app}\CGEasy.App.exe"
Name: "{commondesktop}\CGEasy"; Filename: "{app}\CGEasy.App.exe"
```

---

## 📝 Riepilogo File Modificati

### **1. `src/CGEasy.Core/Data/LiteDbContext.cs`**
- ✅ Aggiunto supporto per percorso di installazione `C:\ProgramData\CGEasy\`
- ✅ Fallback automatico su percorso di sviluppo

### **2. `src/CGEasy.Core/Services/DatabaseEncryptionService.cs`**
- ✅ Aggiornato per usare la stessa logica di `LiteDbContext`

### **3. `prepara_installazione.ps1`** (nuovo)
- ✅ Script automatico per preparazione ambiente
- ✅ Crea cartelle, imposta permessi, configura rete

---

## 🎯 Prossimi Passi

1. ✅ **Testare installazione** su un PC pulito
2. ⏳ **Creare installer Inno Setup** per distribuzione
3. ⏳ **Configurare auto-update** per aggiornamenti automatici
4. ⏳ **Documentare procedura** per utenti finali

---

## 📞 Supporto

Per problemi o domande:
- **Email**: [da configurare]
- **Telefono**: [da configurare]
- **GitHub**: https://github.com/Dan74Ger/CGEasy

---

**Versione documento**: 1.0  
**Data**: 07/11/2025  
**Autore**: AI Assistant per Dott. Geron Daniele

