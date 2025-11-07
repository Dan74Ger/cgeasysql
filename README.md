# 📊 CGEasy - Software Gestionale per Studi Professionali

**CGEasy** è un software gestionale completo progettato per studi professionali, che include moduli per gestione clienti, bilanci, circolari, controllo di gestione bancario e molto altro.

---

## 🚀 Caratteristiche Principali

### 💼 **Gestione Clienti e Professionisti**
- Anagrafica completa clienti e professionisti
- Gestione documenti e allegati
- Storico completo delle operazioni

### 📊 **Bilanci e Riclassificazione**
- Importazione bilanci da Excel
- Riclassificazione automatica mastrini
- Creazione template personalizzati
- Report CEE standard

### 🏦 **Controllo di Gestione Bancario**
- Gestione conti correnti multipli
- Incassi e pagamenti con anticipi
- Saldo previsto con proiezioni mensili
- Pivot consolidato "Margine di Tesoreria"
- Calcolo automatico interessi su anticipi
- Gestione fidi C/C e anticipi fatture

### 📰 **Circolari Fiscali**
- Database circolari aggiornate
- Ricerca avanzata per argomento
- Gestione allegati PDF

### ✅ **TODO Studio**
- Task management per lo studio
- Assegnazione attività ai professionisti
- Tracking deadline e scadenze

### 👥 **Multi-Utente**
- Gestione utenti con ruoli (Administrator, UserSenior, User)
- Permessi granulari per modulo
- Audit log completo delle operazioni
- Database condiviso in rete locale

### 🔐 **Sicurezza**
- Database criptato (LiteDB + AES-256)
- Password master protetta
- Backup automatici
- Sistema di licensing integrato

---

## 📦 Installazione

### **Requisiti di Sistema**
- **Sistema Operativo**: Windows 10/11 (64-bit)
- **.NET Runtime**: 8.0 o superiore (incluso nell'installer)
- **Spazio su Disco**: 500 MB (minimo), 5 GB (consigliato per dati)
- **RAM**: 4 GB (minimo), 8 GB (consigliato)

### **Installazione Standard (Singolo PC)**

1. **Scarica l'installer**
   - `CGEasy_Setup_v1.0.0.exe`

2. **Esegui l'installer**
   - Fai doppio click sul file `.exe`
   - Segui il wizard di installazione
   - Accetta la licenza
   - Scegli la cartella di installazione (default: `C:\Program Files\CGEasy\`)

3. **Database**
   - Verrà creato automaticamente in `C:\db_CGEASY\`
   - Utenti predefiniti: `admin` (123456) e `admin1` (123123)

4. **Primo accesso**
   - Avvia CGEasy dal menu Start o dall'icona desktop
   - Login: `admin1` / `123123`
   - Cambia la password al primo accesso

### **Installazione Multi-PC (Server + Client)**

#### **Server (primo PC)**
1. Esegui l'installer come sopra
2. Durante l'installazione, scegli "Sì" per condivisione di rete
3. Il database sarà condiviso come: `\\NOME-SERVER\CGEasy\`

#### **Client (altri PC)**
1. Esegui l'installer su ogni client
2. Configura il percorso di rete al database condiviso
3. Tutti i PC accederanno allo stesso database

---

## 🎯 Avvio Rapido

### **1. Login**
```
Username: admin1
Password: 123123
```

### **2. Gestione Clienti**
- Vai su "👥 Gestione Clienti"
- Clicca "Nuovo Cliente"
- Compila i dati e salva

### **3. Gestione Banche**
- Vai su "🏦 Controllo Gestione" → "Gestione Banche"
- Aggiungi una banca con saldo corrente e fidi
- Vai su "Dettaglio Banca" per gestire incassi/pagamenti

### **4. Importazione Bilanci**
- Vai su "📊 Bilanci" → "Import Bilancio"
- Seleziona il file Excel con i mastrini
- Assegna cliente e anno
- Procedi con la riclassificazione

---

## 📂 Struttura Database

Il database si trova in:
```
C:\db_CGEASY\
  ├── cgeasy.db         # Database principale (LiteDB criptato)
  ├── db.key            # Chiave di crittografia
  ├── licenses.json     # Licenze attive
  ├── Backups\          # Backup automatici
  ├── Logs\             # Log applicazione
  └── Allegati\         # File allegati
```

**Password Master**: `Woodstockac@74` (solo per recupero amministratore)

---

## 🔧 Configurazione Avanzata

### **Condivisione di Rete Manuale**
Se non hai configurato la condivisione durante l'installazione:

```powershell
# Su Windows Server/PC principale
net share CGEasy=C:\db_CGEASY /grant:everyone,FULL
```

Sui client, modifica il percorso database:
```json
// In C:\Program Files\CGEasy\config.json (crea se non esiste)
{
  "DatabasePath": "\\\\SERVER-PC\\CGEasy\\cgeasy.db"
}
```

### **Backup Manuale**
```powershell
# Copia il database
Copy-Item "C:\db_CGEASY\cgeasy.db" "C:\Backup\cgeasy_backup_$(Get-Date -Format 'yyyyMMdd').db"
```

---

## 🆘 Supporto e Risoluzione Problemi

### **Errore: "Impossibile aprire il database"**
- Verifica che il file `C:\db_CGEASY\cgeasy.db` esista
- Verifica che il file `C:\db_CGEASY\db.key` sia presente
- Riavvia l'applicazione

### **Errore: "Accesso negato"**
- Esegui l'applicazione come Amministratore (tasto destro → Esegui come amministratore)
- Verifica i permessi sulla cartella `C:\db_CGEASY\`

### **Database in rete non accessibile**
- Verifica che il PC server sia acceso e connesso alla rete
- Ping al server: `ping NOME-SERVER`
- Verifica la condivisione: `net view \\NOME-SERVER`

### **Performance lente**
- Controlla la dimensione del database (>500 MB può rallentare)
- Considera di archiviare dati vecchi
- Verifica la velocità della rete (se multi-PC)

---

## 🔄 Aggiornamenti

Gli aggiornamenti futuri saranno disponibili tramite:
- GitHub Releases: https://github.com/Dan74Ger/CGEasy/releases
- Download diretto sul sito web (quando disponibile)

Per aggiornare:
1. **Backup del database** (importante!)
2. Disinstalla la versione precedente (il database non viene toccato)
3. Installa la nuova versione
4. Avvia e verifica che tutto funzioni

---

## 📄 Licenza

Copyright © 2025 Dott. Geron Daniele. Tutti i diritti riservati.

Vedere il file `LICENSE.txt` per i termini completi della licenza.

---

## 🤝 Contributi

Questo è un progetto privato per uso interno dello studio professionale.

---

## 📞 Contatti

**Sviluppatore**: Dott. Geron Daniele  
**Email**: [da configurare]  
**GitHub**: https://github.com/Dan74Ger/CGEasy  

---

## 📚 Documentazione Aggiuntiva

- `INSTALLAZIONE_DA_ZERO.md` - Guida installazione dettagliata
- `GUIDA_INSTALLER.md` - Come creare l'installer
- `GUIDA_DATABASE.md` - Gestione database LiteDB
- `CG_EASY_PROJECT_SPECS.md` - Specifiche tecniche complete

---

**Versione**: 1.0.0  
**Data Rilascio**: Novembre 2025  
**Build**: Release

