# 💾 Guida al Database CGEasy

## 🔍 **IL FILE NON È CRIPTATO!**

Il file `cgeasy.db` sembra "criptato" quando lo apri con Notepad perché usa **formato binario LiteDB**, non testo.

È come aprire un file `.exe` o `.zip` con Notepad - vedi caratteri strani ma non è criptato!

## 📂 **Dove Si Trova il Database**

```
C:\Users\Public\Documents\CGEasy\cgeasy.db
```

**Dimensione attuale**: ~248 KB

## 🔓 **Password Database**

**NESSUNA PASSWORD**  
Il database è completamente aperto, accessibile da qualsiasi applicazione che usi LiteDB.

## 📊 **Come Visualizzare i Dati**

### ✅ **Metodo 1: Usa l'Applicazione CGEasy** (Consigliato)

L'applicazione stessa ti mostra tutti i dati in modo chiaro e professionale:

1. Avvia CGEasy.App
2. Vai su "👥 Clienti" per vedere l'anagrafica clienti
3. Vai su "👤 Utenti" per vedere gli utenti
4. Vai su "🔧 Sistema" per vedere statistiche database

**Vantaggi**:
- Interface grafica completa
- Modifica, crea, elimina record
- Ricerca e filtri
- Permessi e audit log

---

### 🛠️ **Metodo 2: LiteDB Studio** (Tool Grafico)

LiteDB Studio è un visualizzatore gratuito come "SQL Server Management Studio" ma per LiteDB.

**Download**:
https://github.com/mbdavid/LiteDB.Studio/releases

**Come usare**:
1. Scarica `LiteDB.Studio.exe`
2. Esegui il programma
3. File → Open
4. Seleziona: `C:\Users\Public\Documents\CGEasy\cgeasy.db`
5. Esplora le collections (tabelle):
   - `clienti` - Anagrafica clienti
   - `utenti` - Utenti sistema
   - `professionisti` - Anagrafica professionisti
   - `tipo_pratiche` - Tipologie pratiche
   - `user_permissions` - Permessi utenti
   - `audit_logs` - Log operazioni

**Screenshot ideale**:
```
┌─────────────────────────────────────┐
│ Collections         │  Viewer       │
├─────────────────────┼───────────────┤
│ • clienti (25)      │  {            │
│ • utenti (3)        │    Id: 1      │
│ • professionisti    │    Nome: ...  │
│ • tipo_pratiche     │    Email: ... │
│ • user_permissions  │  }            │
│ • audit_logs        │               │
└─────────────────────┴───────────────┘
```

---

### 💻 **Metodo 3: Script PowerShell** (Programmatori)

**PREREQUISITO**: Chiudi CGEasy.App prima di usare lo script!

```powershell
# Chiudi l'app
taskkill /F /IM CGEasy.App.exe

# Leggi i clienti
.\leggi_database.ps1 -Collection clienti -Limit 10

# Leggi gli utenti
.\leggi_database.ps1 -Collection utenti -Limit 5
```

---

### 🔬 **Metodo 4: Codice C#** (Sviluppatori)

```csharp
using LiteDB;

using var db = new LiteDatabase(@"C:\Users\Public\Documents\CGEasy\cgeasy.db");
var clienti = db.GetCollection("clienti");

foreach (var cliente in clienti.FindAll())
{
    Console.WriteLine($"Cliente: {cliente["nome_cliente"]}");
}
```

---

## 📋 **Struttura Database**

### Collection: `clienti`

```json
{
  "_id": 1,
  "nome_cliente": "Azienda SpA",
  "mail_cliente": "info@azienda.it",
  "cf_cliente": "RSSMRA80A01H501U",
  "piva_cliente": "12345678901",
  "indirizzo": "Via Roma 1",
  "citta": "Milano",
  "provincia": "MI",
  "cap": "20100",
  "attivo": true,
  "data_attivazione": "2025-10-17T10:30:00Z",
  "created_at": "2025-10-17T10:30:00Z",
  "updated_at": "2025-10-17T10:30:00Z"
}
```

### Collection: `utenti`

```json
{
  "_id": 1,
  "username": "admin",
  "email": "admin@cgeasy.local",
  "password_hash": "$2a$11$...",
  "nome": "Mario",
  "cognome": "Rossi",
  "ruolo": 1,
  "attivo": true,
  "data_creazione": "2025-10-17T10:00:00Z"
}
```

### Collection: `audit_logs`

```json
{
  "_id": 1,
  "id_utente": 1,
  "entita": "Cliente",
  "id_entita": "1",
  "operazione": "Create",
  "descrizione": "Creato cliente: Azienda SpA",
  "timestamp": "2025-10-17T10:30:00Z"
}
```

---

## 🔐 **Sicurezza**

### ✅ **Punti di Forza**

- Database locale, non esposto su internet
- Password utenti hashate con BCrypt
- Audit log completo di tutte le operazioni
- Soft delete (i dati non vengono mai cancellati fisicamente)
- Sistema di permessi granulare

### ⚠️ **Considerazioni**

- Il database non ha password → Proteggi il file system!
- I dati sono in chiaro nel file (non criptati)
- Per criptare il database, considera di:
  - Usare BitLocker su Windows
  - Impostare password su LiteDB (modifica codice)
  - Usare un database server (SQL Server, PostgreSQL)

### 🔒 **Aggiungere Password al Database** (Opzionale)

Modifica `LiteDbContext.cs`:

```csharp
var connectionString = new ConnectionString
{
    Filename = databasePath,
    Password = "TuaPasswordSegreta123!",  // ← Aggiungi questa riga
    Connection = ConnectionType.Direct,
    Upgrade = true,
    ReadOnly = false
};
```

⚠️ **Attenzione**: Tutti gli utenti dovranno usare la stessa password!

---

## 📦 **Backup Database**

### Backup Manuale

```powershell
# Copia il file
Copy-Item "C:\Users\Public\Documents\CGEasy\cgeasy.db" `
          "C:\Backup\cgeasy_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
```

### Backup Automatico (Script)

```powershell
# backup_database.ps1
$source = "C:\Users\Public\Documents\CGEasy\cgeasy.db"
$backup = "C:\Backup\CGEasy"
$date = Get-Date -Format "yyyyMMdd_HHmmss"

if (!(Test-Path $backup)) {
    New-Item -ItemType Directory -Path $backup -Force
}

Copy-Item $source "$backup\cgeasy_$date.db"
Write-Host "Backup completato: $backup\cgeasy_$date.db"

# Mantieni solo ultimi 30 backup
Get-ChildItem $backup -Filter "cgeasy_*.db" | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -Skip 30 | 
    Remove-Item -Force
```

---

## 🔄 **Ripristino Database**

1. Chiudi CGEasy.App
2. Sostituisci il file:
   ```powershell
   Copy-Item "C:\Backup\cgeasy_BACKUP.db" `
             "C:\Users\Public\Documents\CGEasy\cgeasy.db" -Force
   ```
3. Riavvia CGEasy.App

---

## 🌐 **Condivisione in Rete**

Per usare un database condiviso su più PC:

1. **Metti il database su un server di rete**:
   ```
   \\SERVER\Condivisione\CGEasy\cgeasy.db
   ```

2. **Modifica `LiteDbContext.cs`**:
   ```csharp
   public static string DefaultDatabasePath => 
       @"\\SERVER\Condivisione\CGEasy\cgeasy.db";
   ```

3. **Tutti i PC useranno lo stesso database**

⚠️ **Limitazione LiteDB**: 
- Solo un'app alla volta può scrivere
- Considera SQL Server per vero multi-utente simultaneo

---

## 📊 **Statistiche Database (da CGEasy.App)**

Vai su "🔧 Sistema" → "Database Info" per vedere:

- Numero utenti
- Numero clienti attivi/cessati
- Numero professionisti
- Dimensione database
- Percorso file
- Ultima modifica

---

## ❓ **Domande Frequenti**

**Q: Il file è criptato?**  
A: NO! È solo formato binario LiteDB.

**Q: Come proteggo i dati?**  
A: Proteggi il file system, usa BitLocker, o aggiungi password al DB.

**Q: Posso usare SQL Server invece?**  
A: Sì, ma richiede riscrivere il `LiteDbContext` e i Repository.

**Q: Posso aprirlo con Excel/Access?**  
A: No, serve un tool specifico per LiteDB.

**Q: I dati sono al sicuro?**  
A: Sì, se proteggi l'accesso al file. Le password sono hashate.

**Q: Posso eliminare record?**  
A: L'app usa soft-delete. I record vengono disattivati, non eliminati.

---

## 🎯 **Riepilogo Veloce**

| Cosa vuoi fare | Come |
|----------------|------|
| **Vedere i dati** | Usa l'app CGEasy |
| **Esplorare il DB** | Scarica LiteDB Studio |
| **Backup** | Copia il file .db |
| **Condividere** | Metti su rete e modifica path |
| **Proteggere** | BitLocker o password DB |
| **Sviluppare** | Usa LiteDB.dll in C# |

---

**Database attuale**: 248 KB con utenti e clienti già presenti! ✅

