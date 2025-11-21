# 🗄️ CONFIGURAZIONE SQL SERVER - CGEasy

## 📋 Panoramica

CGEasy SQL utilizza un file di configurazione JSON per connettersi al database SQL Server. Questo permette di installare l'applicazione su più PC che si connettono allo stesso database centralizzato.

---

## 📁 File di Configurazione

**Percorso**: `C:\db_CGEASY\sqlconfig.json`

Il file viene creato automaticamente al primo avvio con configurazione di default (localhost\SQLEXPRESS).

---

## 🎯 SCENARI DI INSTALLAZIONE

### **🔹 IMPORTANTE: Database Locale vs Centralizzato**

CGEasy supporta **2 modalità di installazione**:

#### **A) Database Locale su Ogni PC** 💻
Ogni PC ha il **SUO database SQL Server separato**.
- ✅ Ogni utente lavora sui SUOI dati
- ✅ Nessuna configurazione di rete
- ✅ Più semplice da installare
- ❌ Nessuna condivisione dati tra PC

**Configurazione**: Su OGNI PC installi SQL Server + CGEasy, configurazione identica:
```json
{
  "server": "localhost",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows"
}
```

#### **B) Database Centralizzato su Server** 🌐
Un solo server con SQL Server, tutti i PC si connettono allo stesso database.
- ✅ Tutti gli utenti vedono gli STESSI dati
- ✅ Collaborazione in tempo reale
- ✅ Backup centralizzato
- ⚠️ Richiede configurazione rete e firewall

**Configurazione**: Solo il server ha SQL Server, i client puntano al server.

---

### **Scenario 1: PC Singolo con Database Locale (Default)**
Un solo PC con SQL Server Express installato localmente.

```json
{
  "server": "localhost",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows",
  "username": null,
  "password": null
}
```

✅ **Configurazione automatica** - Nessuna modifica necessaria!

---

### **Scenario 2: Multi-PC con Database Locale su Ogni PC**
Ogni PC ha SQL Server e il SUO database separato.

**Su OGNI PC** (configurazione identica):
```json
{
  "server": "localhost",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows"
}
```

**Procedura per ogni PC**:
1. Installa SQL Server Express
2. Installa CGEasy
3. Al primo avvio, CGEasy crea automaticamente il database locale
4. ✅ Fatto! Database separato per ogni PC

---

### **Scenario 3: Server Dedicato con Windows Authentication**
Più PC client che si connettono a un server SQL **centralizzato**, usando l'autenticazione Windows.

**Su TUTTI i PC client**, modifica `sqlconfig.json`:

```json
{
  "server": "192.168.1.100",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows",
  "username": null,
  "password": null
}
```

**Sostituisci**:
- `192.168.1.100` → IP del server SQL
- `SQLEXPRESS` → Nome istanza SQL sul server (o rimuovi per istanza default)

📌 **Requisiti**:
- Gli utenti Windows devono avere permessi sul database SQL Server
- SQL Server deve accettare connessioni remote (configurare firewall)

---

### **Scenario 3: Server Dedicato con SQL Authentication**
Più PC client che si connettono con username/password SQL.

**Su TUTTI i PC client**, modifica `sqlconfig.json`:

```json
{
  "server": "192.168.1.100",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "SQL",
  "username": "cgeasy_user",
  "password": "Password123!"
}
```

**Sostituisci**:
- `192.168.1.100` → IP del server SQL
- `cgeasy_user` → Username SQL creato sul server
- `Password123!` → Password SQL

⚠️ **ATTENZIONE**: La password è salvata in chiaro! Usa permessi di file adeguati.

📌 **Configurazione SQL Server**:
1. Abilita "SQL Server and Windows Authentication mode"
2. Crea login SQL con permessi db_owner su database CGEasy
3. Apri porta 1433 sul firewall del server

---

### **Scenario 4: Istanza SQL Nominata**
Server con istanza SQL personalizzata.

```json
{
  "server": "SERVER01",
  "instance": "ISTANZAPROD",
  "database": "CGEasy",
  "authentication_type": "Windows"
}
```

---

## 🖥️ CONFIGURAZIONE TRAMITE INTERFACCIA

CGEasy include un'interfaccia grafica per configurare SQL Server:

1. Avvia CGEasy
2. Menu **Sistema** → **Configurazione SQL Server**
3. Inserisci i dati del server
4. Clicca **TESTA CONNESSIONE** per verificare
5. Clicca **SALVA CONFIGURAZIONE**
6. **RIAVVIA L'APPLICAZIONE**

---

## ⚙️ PARAMETRI CONFIGURAZIONE

| Parametro | Descrizione | Esempio |
|-----------|-------------|---------|
| `server` | Nome o IP del server SQL | `localhost`, `192.168.1.100`, `SERVER01` |
| `instance` | Istanza SQL Server | `SQLEXPRESS`, `MSSQLSERVER`, vuoto per default |
| `database` | Nome database | `CGEasy` |
| `authentication_type` | Tipo autenticazione | `Windows` o `SQL` |
| `username` | Username SQL (solo per SQL Auth) | `cgeasy_user` |
| `password` | Password SQL (solo per SQL Auth) | `Password123!` |
| `connection_timeout` | Timeout connessione (sec) | `30` |
| `multiple_active_result_sets` | MARS abilitato | `true` |
| `trust_server_certificate` | Trust certificato server | `true` |

---

## 🔧 RISOLUZIONE PROBLEMI

### ❌ **"Timeout expired"**
- **Causa**: Server SQL non raggiungibile
- **Soluzione**: 
  - Verifica IP/nome server
  - Controlla firewall (porta 1433)
  - Verifica che SQL Server sia avviato

### ❌ **"Login failed for user"**
- **Causa**: Credenziali errate o permessi insufficienti
- **Soluzione**:
  - Verifica username/password (se SQL Auth)
  - Verifica permessi utente Windows sul database
  - Controlla che SQL Server accetti il tipo di autenticazione usato

### ❌ **"Cannot open database 'CGEasy'"**
- **Causa**: Database non esiste sul server
- **Soluzione**:
  - Ripristina backup database sul server SQL
  - Esegui migrations EF Core per creare database

### ❌ **"Network path not found"**
- **Causa**: Nome server errato o servizio SQL Browser non avviato
- **Soluzione**:
  - Usa IP invece del nome server
  - Avvia servizio "SQL Server Browser" sul server

---

## 📦 INSTALLAZIONE MULTI-PC - PROCEDURA COMPLETA

### **1. Installa SQL Server sul Server Dedicato**
- Installa SQL Server Express (o versione licenziata)
- Durante installazione:
  - ✅ Abilita "Mixed Mode Authentication"
  - ✅ Crea password per utente `sa`
  - ✅ Configura istanza (es: SQLEXPRESS)

### **2. Configura SQL Server per Connessioni Remote**
```sql
-- SQL Server Configuration Manager
-- 1. Abilita TCP/IP in "SQL Server Network Configuration"
-- 2. Riavvia servizio SQL Server
-- 3. Apri porta 1433 su Windows Firewall
```

### **3. Crea Database CGEasy sul Server**
```sql
-- Opzione A: Ripristina backup
RESTORE DATABASE CGEasy FROM DISK = 'C:\backup\CGEasy.bak'

-- Opzione B: Crea database vuoto (l'app creerà le tabelle)
CREATE DATABASE CGEasy
```

### **4. Crea Utente SQL (se usi SQL Authentication)**
```sql
CREATE LOGIN cgeasy_user WITH PASSWORD = 'Password123!'
USE CGEasy
CREATE USER cgeasy_user FOR LOGIN cgeasy_user
ALTER ROLE db_owner ADD MEMBER cgeasy_user
```

### **5. Installa CGEasy su Ogni PC Client**
- Esegui installer CGEasy su ogni PC
- Non serve SQL Server sui client (solo sul server)

### **6. Configura Connessione su Ogni PC Client**
Modifica `C:\db_CGEASY\sqlconfig.json` su ogni PC con i dati del server:

```json
{
  "server": "192.168.1.100",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "SQL",
  "username": "cgeasy_user",
  "password": "Password123!"
}
```

### **7. Testa Connessione**
- Avvia CGEasy su un PC client
- Menu Sistema → Configurazione SQL Server
- Clicca **TESTA CONNESSIONE**
- Se funziona: ✅ Installazione completata!

---

## 🔐 SICUREZZA

### **Password in Chiaro**
⚠️ La password SQL è salvata in chiaro in `sqlconfig.json`. 

**Raccomandazioni**:
- Usa Windows Authentication quando possibile (nessuna password nel file)
- Se usi SQL Auth, imposta permessi file 600 (solo proprietario può leggere)
- Crea utente SQL dedicato con permessi minimi necessari
- NON usare account `sa`

### **Permessi File**
```cmd
REM Imposta permessi restrittivi su sqlconfig.json
icacls "C:\db_CGEASY\sqlconfig.json" /grant:r "%USERNAME%:F" /inheritance:r
```

---

## 📞 SUPPORTO

Per assistenza sulla configurazione SQL Server:
- Email: support@cgeasy.it
- Documentazione: https://docs.cgeasy.it/sql-configuration

---

## 🔄 MIGRAZIONE DA LiteDB

Se stai migrando da CGEasy LiteDB:

1. **Backup database LiteDB**: `C:\db_CGEASY\cgeasy.db`
2. **Installa SQL Server**
3. **Esegui script migrazione dati** (se disponibile)
4. **Configura** `sqlconfig.json` come sopra
5. **Avvia nuova versione SQL**

---

**Versione documento**: 1.0  
**Data**: Novembre 2025  
**Applicabile a**: CGEasy SQL v1.0+

