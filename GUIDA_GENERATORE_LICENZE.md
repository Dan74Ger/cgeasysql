# 🔑 GUIDA GENERATORE LICENZE CGEASY

## 📋 Indice
1. [Cos'è il Generatore di Licenze](#cosè-il-generatore-di-licenze)
2. [Come Avviare il Generatore](#come-avviare-il-generatore)
3. [Come Generare una Chiave](#come-generare-una-chiave)
4. [Come Attivare un Modulo in CGEasy](#come-attivare-un-modulo-in-cgeasy)
5. [Moduli Disponibili](#moduli-disponibili)
6. [Chiavi Generate](#chiavi-generate)
7. [Risoluzione Problemi](#risoluzione-problemi)

---

## Cos'è il Generatore di Licenze

Il **Generatore di Licenze CGEasy** è un'applicazione standalone che permette di:
- ✅ Generare chiavi di attivazione per i 4 moduli dell'applicazione
- ✅ Copiare facilmente le chiavi generate
- ✅ Gestire le licenze in modo sicuro offline

**NON RICHIEDE CONNESSIONE INTERNET** - Tutto avviene in locale.

---

## Come Avviare il Generatore

### Metodo 1: File Batch (FACILE) ⭐
1. Fai doppio click su **`GENERA_LICENZE.bat`** nella cartella principale del progetto
2. Il generatore si avvierà automaticamente
3. Aspetta che la finestra si apra

### Metodo 2: Comando Manuale
1. Apri il Prompt dei Comandi (CMD) o PowerShell
2. Vai nella cartella del progetto:
   ```
   cd c:\devcg-group\appcg_easy_project
   ```
3. Esegui:
   ```
   dotnet run --project src\CGEasy.LicenseGenerator\CGEasy.LicenseGenerator.csproj
   ```

### Metodo 3: Compilato (RACCOMANDATO per Distribuzione)
1. Compila il generatore:
   ```
   dotnet publish src\CGEasy.LicenseGenerator\CGEasy.LicenseGenerator.csproj -c Release -o Generatore_Licenze
   ```
2. Vai nella cartella `Generatore_Licenze`
3. Esegui `CGEasy.LicenseGenerator.exe`
4. **Puoi copiare questa cartella su qualsiasi PC Windows**

---

## Come Generare una Chiave

### Passo 1: Scegli il Modulo
Nel generatore vedrai 4 sezioni, una per ogni modulo:
- 📝 **TODO STUDIO** - Gestione task e calendario
- 📊 **BILANCI** - Bilanci CEE
- 📢 **CIRCOLARI** - Comunicazioni
- 📈 **CONTROLLO GESTIONE** - Analisi gestionale

### Passo 2: Genera la Chiave
1. Click sul pulsante **"🔑 Genera Chiave"** del modulo desiderato
2. Apparirà un messaggio con la chiave generata
3. La chiave appare anche nel campo di testo sotto

### Passo 3: Copia la Chiave
1. Click sul pulsante **"📋 Copia Chiave"**
2. La chiave viene copiata negli appunti
3. Ora puoi incollarla dove serve

### Esempio di Chiave Generata:
```
TODO-STUDIO-3V2WDCVTWZYM
```

---

## Come Attivare un Modulo in CGEasy

### Passo 1: Avvia CGEasy
1. Apri l'applicazione CGEasy
2. Accedi con le tue credenziali

### Passo 2: Vai in Sistema
1. Click sul menu **"⚙️ Impostazioni"** o **"Sistema"**
2. Scorri fino alla sezione **"🔑 Licenze Moduli"**

### Passo 3: Inserisci la Chiave
Vedrai 4 card, una per ogni modulo. Per esempio, per TODO Studio:

```
┌────────────────────────────────────────┐
│ 📝 TODO STUDIO                         │
│ Stato: ❌ Non Attivato                 │
│                                        │
│ Chiave: [________________]             │
│                                        │
│ [🔑 Genera]  [✅ Attiva]  [❌ Disattiva] │
└────────────────────────────────────────┘
```

1. **Incolla la chiave** copiata dal generatore nel campo "Chiave"
2. Click su **"✅ Attiva Licenza"**
3. Vedrai il messaggio: "✅ MODULO TODO STUDIO ATTIVATO!"

### Passo 4: Riavvia CGEasy
1. **Chiudi completamente** l'applicazione CGEasy
2. **Riapri** CGEasy
3. Ora il modulo è attivo! 🎉

### Passo 5: Usa il Modulo
- Il pulsante del modulo nel menu principale è ora **cliccabile**
- Non vedrai più il messaggio "Modulo non attivato"

---

## Moduli Disponibili

### 1. 📝 TODO STUDIO
**Chiave Formato:** `TODO-STUDIO-XXXXXXXXXXXX`

**Funzionalità:**
- Calendario TODO interattivo
- 8 filtri real-time combinabili
- Drag & drop per spostare TODO
- Statistiche per fase
- Export Excel
- Gestione professionisti multipli

**Stato Implementazione:** ✅ COMPLETO E FUNZIONANTE

---

### 2. 📊 BILANCI
**Chiave Formato:** `BILANCI-XXXXXXXXXXXX`

**Funzionalità:**
- Gestione bilanci CEE
- Analisi e reportistica
- Import/Export dati

**Stato Implementazione:** 🚧 IN SVILUPPO (placeholder)

---

### 3. 📢 CIRCOLARI
**Chiave Formato:** `CIRCOLARI-XXXXXXXXXXXX`

**Funzionalità:**
- Gestione circolari
- Comunicazioni studio
- Archivio documentale

**Stato Implementazione:** 🚧 IN SVILUPPO (placeholder)

---

### 4. 📈 CONTROLLO GESTIONE
**Chiave Formato:** `CONTROLLO-GESTIONE-XXXXXXXXXXXX`

**Funzionalità:**
- Analisi costi e ricavi
- Budget e previsioni
- Reportistica avanzata

**Stato Implementazione:** 🚧 IN SVILUPPO (placeholder)

---

## Chiavi Generate

### Come Funzionano?
Le chiavi sono generate con:
- **Algoritmo:** SHA256 (sicuro e non reversibile)
- **Secret Key:** `CGEasy2025-TodoStudio-SecretKey-DottGeronDaniele`
- **Formato:** `MODULO-HASH12CARATTERI`

### Sono Sicure?
✅ **SÌ**
- Non possono essere reverse-engineered
- Ogni modulo ha un hash diverso
- Funzionano solo con CGEasy

### Scadono?
❌ **NO**
- Le chiavi sono **permanenti**
- Non hanno data di scadenza
- Una volta attivato, il modulo resta attivo

### Posso Rigenerarle?
✅ **SÌ**
- Puoi rigenerare le chiavi infinite volte
- **La chiave sarà sempre identica** per lo stesso modulo
- Esempio: Generando `TODO-STUDIO` 100 volte, avrai sempre `TODO-STUDIO-3V2WDCVTWZYM`

---

## Chiavi Pre-Generate (Per Comodità)

Puoi usare direttamente queste chiavi:

```
📝 TODO STUDIO:
TODO-STUDIO-3V2WDCVTWZYM

📊 BILANCI:
BILANCI-KVRFBFODYKST

📢 CIRCOLARI:
CIRCOLARI-OBHSXPEDUKMT

📈 CONTROLLO GESTIONE:
CONTROLLO-GESTIONE-BSKXQMFBONUB
```

---

## Risoluzione Problemi

### Problema: "Chiave non valida"
**Soluzione:**
1. Verifica di aver copiato la chiave completa (con il prefisso)
2. Assicurati che non ci siano spazi all'inizio o alla fine
3. La chiave è case-insensitive (maiuscole/minuscole non importano)
4. Formato corretto: `MODULO-XXXXXXXXXXXX`

### Problema: "Il generatore non si avvia"
**Soluzione:**
1. Verifica di aver installato .NET 8.0 SDK
2. Scarica da: https://dotnet.microsoft.com/download/dotnet/8.0
3. Riavvia il PC dopo l'installazione

### Problema: "Modulo non appare dopo attivazione"
**Soluzione:**
1. **RIAVVIA COMPLETAMENTE** CGEasy (chiudi e riapri)
2. Verifica che la chiave sia stata salvata (vai in Sistema > Licenze)
3. Lo stato deve dire "✅ Attivato"

### Problema: "La chiave è troppo lunga/corta"
**Soluzione:**
- La lunghezza è corretta: `MODULO-` + 12 caratteri
- Esempio: `TODO-STUDIO-3V2WDCVTWZYM` = 24 caratteri totali
- Se diversa, rigenera la chiave

### Problema: "Voglio disattivare un modulo"
**Soluzione:**
1. Vai in Sistema > Licenze Moduli
2. Click su "❌ Disattiva Licenza" per il modulo
3. Conferma
4. Il modulo sarà disattivato immediatamente

---

## Backup Chiavi

### Consiglio:
Salva le chiavi in un file di testo sicuro:

```
=== LICENZE CGEASY ===

TODO Studio: TODO-STUDIO-3V2WDCVTWZYM
Bilanci: BILANCI-KVRFBFODYKST
Circolari: CIRCOLARI-OBHSXPEDUKMT
Controllo Gestione: CONTROLLO-GESTIONE-BSKXQMFBONUB

Data: 20/10/2025
Note: Chiavi valide permanentemente
```

Conserva questo file in:
- 📁 Cloud sicuro (Google Drive, OneDrive)
- 💾 Chiavetta USB
- 📧 Email a te stesso

---

## Supporto

Per problemi o domande:
- **Developer:** Dott. Geron Daniele
- **Email:** info@cg-group.it
- **Versione:** CGEasy v2.0
- **Copyright:** © 2025 - Tutti i diritti riservati

---

## Quick Start (TL;DR) ⚡

1. Esegui `GENERA_LICENZE.bat`
2. Click su "🔑 Genera Chiave" per il modulo desiderato
3. Click su "📋 Copia Chiave"
4. Apri CGEasy > Sistema > Licenze Moduli
5. Incolla chiave e click "✅ Attiva"
6. Riavvia CGEasy
7. ✅ FATTO!

---

**Buon lavoro con CGEasy! 🚀**


