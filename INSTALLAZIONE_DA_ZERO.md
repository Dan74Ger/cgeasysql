# 🚀 Installazione CGEasy da Zero

## 📋 Panoramica

CGEasy utilizza **SQL Server** come database, configurabile per installazioni singole o multi-PC in rete.

---

## 🗄️ Configurazione Database

### ✅ **Singolo PC (Default)**

Il database SQL Server viene creato automaticamente su `localhost\SQLEXPRESS`.

**File configurazione**: `C:\db_CGEASY\sqlconfig.json`
```json
{
  "server": "localhost",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows"
}
```

**Primo avvio**:
1. L'applicazione crea automaticamente il database `CGEasy`
2. Applica le migrations per creare le 26 tabelle
3. Crea utente amministratore di default

---

## 🌐 Installazione Multi-PC (Rete Locale)

Per installazioni con database centralizzato su un server:

### **PC Server (primo PC con SQL Server)**

1. **Installa SQL Server** (Express o licenziato)
2. **Configura per accesso remoto**:
   - Abilita TCP/IP in SQL Server Configuration Manager
   - Riavvia servizio SQL Server
   - Apri porta 1433 nel firewall

3. **Configura CGEasy** con `sqlconfig.json`:
```json
{
  "server": "localhost",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows"
}
```

4. **Avvia CGEasy** per creare il database

### **PC Client (altri PC in rete)**

Su ogni PC client, configura `C:\db_CGEASY\sqlconfig.json`:

```json
{
  "server": "192.168.1.12",
  "instance": "SQLEXPRESS",
  "database": "CGEasy",
  "authentication_type": "Windows"
}
```

Sostituisci `192.168.1.12` con l'**IP del server**.

---

## 🔧 Configurazione Tramite Interfaccia

Puoi configurare SQL Server anche dall'applicazione:

1. Avvia CGEasy
2. Menu **Sistema** → **Configurazione SQL Server**
3. Inserisci:
   - Server: `192.168.1.12` (IP del server)
   - Istanza: `SQLEXPRESS`
   - Database: `CGEasy`
   - Tipo Autenticazione: `Windows` o `SQL`
4. Clicca **"TESTA CONNESSIONE"**
5. Se OK → **"SALVA CONFIGURAZIONE"**
6. **Riavvia l'applicazione**

---

## 📄 Documentazione Completa

Per tutti i dettagli sulla configurazione SQL Server, consulta:

**`CONFIGURAZIONE_SQL_SERVER.md`**

Include:
- Scenari di installazione (singolo PC, rete, SQL Server licenziato)
- Configurazione SQL Server per accesso remoto
- Autenticazione Windows e SQL
- Troubleshooting
- Best practices sicurezza

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

