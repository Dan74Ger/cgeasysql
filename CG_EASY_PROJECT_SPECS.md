# 🚀 CG EASY - Specifiche Progetto Desktop App

**Versione**: 2.0  
**Data**: 16 Ottobre 2025  
**Tipo**: Desktop Application Multi-Utente (WPF + LiteDB)  
**Target**: Studi Commercialisti (5-10 utenti)  
**Moduli**: 5 (TODO, Bilanci, Circolari, Controllo Gestione, Base)

---

## 📋 INDICE

1. [Overview Progetto](#overview-progetto)
2. [Scope Funzionale](#scope-funzionale)
3. [Architettura Tecnica](#architettura-tecnica)
4. [Database - LiteDB](#database-litedb)
5. [Sistema Multi-Utente](#sistema-multi-utente)
6. [Features Dettagliate](#features-dettagliate)
7. [UI/UX Design](#uiux-design)
8. [Deployment e Installazione](#deployment-e-installazione)
9. [Sistema Licensing e Attivazione](#sistema-licensing-e-attivazione)
10. [Architettura Modulare (Opzionale)](#architettura-modulare-opzionale)
11. [Tecnologie e Pacchetti](#tecnologie-e-pacchetti)
12. [Timeline e Stime](#timeline-e-stime)
13. [Roadmap Futura](#roadmap-futura)

---

## 🎯 OVERVIEW PROGETTO

### **Obiettivo**
Creare un'applicazione desktop Windows multi-utente per la gestione operativa completa di uno studio commercialista, focalizzata su:
- **Gestione TODO Studio** (task management con viste Lista/Kanban/Calendario)
- **Gestione Bilanci** (import, riclassificazione, export Excel, grafici)
- **Archiviazione Circolari e Documenti** (gestione documentale con ricerca full-text)
- **Controllo di Gestione** (analisi dati, KPI, budget, report)

### **Perché Desktop App?**
- ✅ **Performance**: Accesso diretto al database condiviso
- ✅ **Offline-first**: Funziona anche senza internet
- ✅ **Costo zero**: No server cloud, no licenze mensili
- ✅ **Controllo totale**: Database in-house
- ✅ **UI nativa**: Esperienza Windows professionale

### **Differenze con App Web Esistente**
La web app (`ConsultingGroup`) è completa ma **troppo complessa** per alcune situazioni. CG Easy è una versione:
- 🎯 **Focalizzata**: Solo gestione operativa (TODO + Bilanci)
- 🪶 **Leggera**: No AI, no Google Sync, no fatturazione
- 💾 **Standalone**: Database locale/rete condiviso
- 📦 **Distribuibile**: Singolo EXE installabile

---

## 📊 SCOPE FUNZIONALE

### ✅ **FEATURES INCLUSE**

#### **1. Sistema Multi-Utente**
- Login/Logout con credenziali
- 3 Ruoli: Administrator, UserSenior, User
- Permessi granulari per funzionalità
- Gestione utenti (CRUD)
- Audit log operazioni
- Session management

#### **2. TODO STUDIO (Task Management)**

##### **2.1 Viste TODO**
- **📋 Lista**: Tabella classica con ordinamento/filtri
- **📊 Kanban**: Board drag-and-drop (Da Fare → In Corso → Completata → Annullata)
- **📅 Calendario**: Vista mensile con drag-and-drop date

##### **2.2 Gestione TODO**
- CRUD completo (Create, Read, Update, Delete)
- Campi:
  - Titolo (personalizzato o da tipo pratica)
  - Descrizione
  - Categoria (Fiscale, Contabile, Amministrativo, Cliente, Altro)
  - Priorità (Alta, Media, Bassa)
  - Stato (Da Fare, In Corso, Completata, Annullata)
  - Data scadenza con notifiche
  - Cliente associato (opzionale)
  - Professionisti assegnati (multipli)
  - Note
  - Allegati file

##### **2.3 Funzionalità Avanzate**
- Notifiche scadenze (popup + badge)
- Filtri avanzati (per utente, cliente, priorità, stato, data)
- Ricerca full-text
- Export Excel TODO
- Colori visivi per priorità/stato
- Statistiche completamento

#### **3. Gestione Bilanci**

##### **3.1 Bilancio Contabile**
- Import da file Excel (mastrini contabili)
- Campi: Codice Mastrino, Descrizione, Dare, Avere
- Filtri per Cliente, Mese, Anno
- CRUD completo
- Vista riepilogativa per periodo
- Export Excel

##### **3.2 Template Riclassificazione**
- Creazione template personalizzati
- Voci gerarchiche (padre-figlio)
- Campi voce:
  - Codice voce
  - Descrizione
  - Livello (1, 2, 3...)
  - Formula di calcolo (es: `A + B - C`)
  - Tipo calcolo (Somma, Formula, Manuale, Percentuale)
  - Ordine visualizzazione
- Import/Export template
- Clona template esistenti

##### **3.3 Associazioni Mastrini**
- Mapping mastrini contabili → voci template
- Gestione segno contabile (Dare/Avere)
- Vista associazioni per cliente/template
- Batch associazioni (selezione multipla)
- Import/Export associazioni

##### **3.4 Bilancio Riclassificato Periodo**
- Generazione bilancio per singolo mese
- Calcolo automatico formule
- Percentuali su fatturato
- Vista albero gerarchica
- Export Excel con formattazione
- Salvataggio bilanci generati

##### **3.5 Bilancio Riclassificato Mensile**
- Generazione multi-mese (1-12 mesi)
- Confronto mesi affiancati
- Totali e medie
- Export Excel multi-colonna
- Vista grafica comparativa

##### **3.6 Grafici e Statistiche**
- Grafici interattivi (LiveCharts):
  - Grafico a barre (voci principali)
  - Grafico linee (trend mensile)
  - Grafico torta (percentuali)
  - Grafico area (cumulativo)
- Filtri periodo
- Export immagini grafici
- Dashboard riepilogativa

#### **4. Archiviazione Circolari e Documenti**

##### **4.1 Gestione Circolari**
- CRUD completo circolari
- Campi:
  - Numero/Protocollo circolare
  - Data emissione
  - Ente emittente (ADE, INPS, INAIL, Ministero, Altro)
  - Oggetto/Titolo
  - Categoria (Fiscale, Lavoro, Previdenziale, Societario, Altro)
  - Argomenti/Tags (multipli)
  - File PDF allegato
  - Note
  - Importanza (Alta, Media, Bassa)
  - Stato (Da leggere, Letta, Archiviata)
- Import massivo circolari (drag & drop PDF)
- Estrazione automatica metadati da PDF (numero, data, ente)
- Viewer PDF integrato

##### **4.2 Gestione Documenti**
- CRUD completo documenti generici
- Campi:
  - Titolo documento
  - Tipo documento (Circolare, Guida, Normativa, Modello, Altro)
  - Data documento
  - Categoria
  - Tags multipli
  - File allegato (PDF, Word, Excel)
  - Cliente associato (opzionale)
  - Note
- Upload file multipli
- Versioning documenti
- Preview documenti

##### **4.3 Ricerca Avanzata**
- Ricerca full-text nel contenuto PDF
- Filtri multipli:
  - Per ente emittente
  - Per categoria
  - Per data (range)
  - Per tags
  - Per cliente
  - Per importanza
  - Per stato
- Ricerca rapida (CTRL+F globale)
- Salva ricerche preferite
- Export risultati ricerca (Excel)

##### **4.4 Categorizzazione e Tags**
- Gestione categorie personalizzate
- Gestione tags (auto-suggest)
- Assegnazione massiva tags
- Vista per categoria
- Vista per tag
- Statistiche per categoria

##### **4.5 Notifiche e Scadenze**
- Notifica nuove circolari importanti
- Promemoria circolari da leggere
- Scadenze associate a circolari
- Badge contatore "da leggere"

##### **4.6 Export e Condivisione**
- Export PDF circolari selezionate
- Creazione ZIP per condivisione
- Stampa indice circolari
- Report circolari per periodo

#### **5. Controllo di Gestione**

##### **5.1 Dashboard KPI**
- Card KPI principali:
  - Fatturato (mese, trimestre, anno)
  - Margine operativo lordo (MOL)
  - EBITDA
  - Utile netto
  - Liquidità
  - Indice di redditività (ROE, ROI)
  - Punto di pareggio (break-even)
- Grafici trend mensili
- Confronto anno precedente
- Semafori (rosso/giallo/verde)
- Personalizzazione KPI visualizzati

##### **5.2 Analisi Bilanci**
- Selezione cliente e periodo
- Analisi verticale (% su fatturato)
- Analisi orizzontale (variazioni %)
- Indici di bilancio automatici:
  - Liquidità (current ratio, quick ratio)
  - Solidità (debt/equity, solvibilità)
  - Redditività (ROE, ROI, ROS, ROA)
  - Efficienza (rotazione crediti/debiti/magazzino)
- Confronto multi-periodo (2-5 anni)
- Grafici comparativi

##### **5.3 Budget e Previsioni**
- Creazione budget annuale per cliente
- Import da Excel
- Definizione obiettivi per voce
- Confronto Budget vs Consuntivo:
  - Tabella scostamenti (€ e %)
  - Grafici scostamenti
  - Analisi degli scostamenti
- Forecast (previsioni anno in corso)
- Scenario analysis (best/worst/realistic)

##### **5.4 Report Controllo Gestione**
- Report standard:
  - Situazione patrimoniale
  - Conto economico riclassificato
  - Rendiconto finanziario
  - Cash flow
  - Analisi per indici
- Report personalizzabili
- Export PDF/Excel con grafici
- Template report personalizzati
- Stampa report

##### **5.5 Grafici e Visualizzazioni**
- Grafici interattivi (LiveCharts):
  - Trend fatturato multi-anno
  - Composizione costi (waterfall)
  - Margini per categoria
  - Cash flow mensile
  - Break-even analysis
  - Scatter plot indici
- Dashboard multi-cliente (confronto)
- Heat map performance
- Export grafici PNG/PDF

##### **5.6 Alert e Soglie**
- Definizione soglie alert per:
  - Liquidità sotto soglia
  - Margine negativo
  - Scostamento budget >X%
  - Indici fuori range
- Notifiche automatiche
- Report alert mensile

#### **6. Anagrafiche**

##### **6.1 Clienti**
- CRUD completo
- Campi: Nome, Email, Telefono, Attivo
- Ricerca e filtri
- Lista clienti attivi
- Collegamento TODO e Bilanci

##### **4.2 Professionisti**
- CRUD completo
- Campi: Nome, Cognome, Email, Attivo
- Assegnazione TODO
- Statistiche workload

##### **4.3 Tipo Pratica**
- Tipologie predefinite per TODO
- CRUD completo
- Usate come template TODO veloci

#### **7. Dashboard**
- Riepilogo TODO in scadenza
- Bilanci in lavorazione
- KPI controllo gestione (card principali)
- Circolari da leggere (badge contatore)
- Alert e notifiche
- Statistiche giornaliere
- Quick actions:
  - Nuovo TODO
  - Import Bilancio
  - Nuova Circolare
  - Nuovo Documento
- Attività recenti
- Grafici riepilogativi

#### **8. Sistema**
- Backup/Restore database
- Export/Import dati
- Impostazioni applicazione
- Info versione e about
- Log operazioni

---

### ❌ **FEATURES ESCLUSE** (vs App Web)

Funzionalità della web app **non incluse** in CG Easy:

- ❌ Sistema Fatturazione (Proforma, Mandati, Fatture Cloud)
- ❌ Gestione Attività Fiscali (730, 740, 750, 760, 770, etc.)
- ❌ Anni Fiscali e Fatturazione
- ❌ AI Assistant (Ollama, Mistral)
- ❌ Google Calendar Sync
- ❌ Sistema Documenti e Mandati
- ❌ Gestione Spese Studio
- ❌ Contabilità Interna (Trimestrale/Mensile)
- ❌ Ripartizione Incassi
- ❌ Ore Accessi Esterni
- ❌ Cassetto Fiscale / Entratel
- ❌ Fatturazione Elettronica
- ❌ Conservazione Elettronica
- ❌ Firma Digitale
- ❌ Registri (IVA, Cespiti, Giornale)
- ❌ MOD Intrastat
- ❌ Titolare Effettivo
- ❌ Dati Utenza Extra
- ❌ Report complessi

**Nota**: Queste features rimangono nella web app principale. CG Easy è un'app **complementare** focalizzata.

---

## 🏗️ ARCHITETTURA TECNICA

### **Stack Tecnologico**

```
┌─────────────────────────────────────────────┐
│  PRESENTATION LAYER (WPF)                   │
│  ├── Views (XAML)                           │
│  ├── ViewModels (MVVM)                      │
│  └── UI Components (ModernWPF)              │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│  BUSINESS LOGIC LAYER                       │
│  ├── Services                               │
│  │   ├── TodoService                        │
│  │   ├── BilancioService                    │
│  │   ├── RiclassificazioneService          │
│  │   ├── ExcelService                       │
│  │   └── AuthService                        │
│  └── Helpers & Utilities                    │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│  DATA ACCESS LAYER                          │
│  ├── LiteDbContext                          │
│  ├── Repositories                           │
│  │   ├── UtenteRepository                   │
│  │   ├── TodoRepository                     │
│  │   ├── ClienteRepository                  │
│  │   └── BilancioRepository                 │
│  └── Models                                  │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│  DATABASE (LiteDB)                          │
│  📂 \\SERVER\CGEasy\database.db            │
│  (File condiviso multi-utente)              │
└─────────────────────────────────────────────┘
```

### **Pattern Architetturali**

1. **MVVM (Model-View-ViewModel)**
   - Separazione logica/presentazione
   - Data binding WPF
   - Testabilità elevata

2. **Repository Pattern**
   - Astrazione accesso dati
   - Indipendenza da DB specifico
   - Facilitates unit testing

3. **Dependency Injection**
   - IoC Container (Microsoft.Extensions.DependencyInjection)
   - Loose coupling
   - Configurazione centralizzata

4. **Service Layer**
   - Business logic isolata
   - Riusabilità tra ViewModels
   - Validazione centralizzata

---

## 💾 DATABASE - LITEDB

### **Perché LiteDB?**
- ✅ **Serverless**: No SQL Server da configurare
- ✅ **File singolo**: Facile backup/deploy
- ✅ **Multi-utente**: Shared connection mode
- ✅ **Performance**: 10.000+ ops/sec
- ✅ **LINQ support**: Query familiari
- ✅ **Thread-safe**: Sicuro per desktop app
- ✅ **Gratuito**: MIT License

### **Modalità Operativa**

```csharp
// Connection String Multi-Utente
var db = new LiteDatabase(new ConnectionString
{
    Filename = @"\\SERVER\CGEasy\database.db",
    Connection = ConnectionType.Shared,  // Multi-reader + Single-writer
    Timeout = TimeSpan.FromSeconds(60)
});
```

### **Collections (Tabelle)**

#### **1. Anagrafiche (5 collections)**

```csharp
// 1. Utenti
{
  Id: int (PK),
  Username: string,
  PasswordHash: string,  // SHA256
  NomeCompleto: string,
  Email: string,
  Ruolo: enum (Administrator, UserSenior, User),
  Attivo: bool,
  UltimoAccesso: DateTime,
  CreatedAt: DateTime,
  UpdatedAt: DateTime
}

// 2. Professionisti
{
  IdProfessionista: int (PK),
  Nome: string,
  Cognome: string,
  Email: string,
  Telefono: string,
  Attivo: bool,
  CreatedAt: DateTime,
  UpdatedAt: DateTime
}

// 3. Clienti
{
  IdCliente: int (PK),
  NomeCliente: string,
  Email: string,
  Telefono: string,
  CodiceAteco: string,
  Attivo: bool,
  CreatedAt: DateTime,
  UpdatedAt: DateTime
}

// 4. TipoPratica
{
  IdTipoPratica: int (PK),
  NomePratica: string,
  Descrizione: string,
  Categoria: enum,
  Attivo: bool
}

// 5. Permessi
{
  IdPermesso: int (PK),
  IdUtente: int (FK),
  CanAccessBilanci: bool,
  CanImportBilanci: bool,
  CanExportBilanci: bool,
  CanManageTemplates: bool,
  CanCreateTodo: bool,
  CanAssignTodo: bool,
  CanDeleteTodo: bool,
  CanManageUtenti: bool
}
```

#### **2. TODO System (3 collections)**

```csharp
// 6. TodoStudio
{
  IdTodo: int (PK),
  IdTipoPratica: int (FK nullable),
  TitoloPersonalizzato: string,
  Descrizione: string,
  Categoria: enum (Fiscale, Contabile, Amministrativo, Cliente, Altro),
  Priorita: enum (Alta, Media, Bassa),
  Stato: enum (DaFare, InCorso, Completata, Annullata),
  DataScadenza: DateTime nullable,
  IdCliente: int (FK nullable),
  IdCreatore: int (FK),
  Note: string,
  CreatedAt: DateTime,
  UpdatedAt: DateTime
}

// 7. TodoProfessionistaAssegnato (Many-to-Many)
{
  Id: int (PK),
  IdTodo: int (FK),
  IdProfessionista: int (FK),
  DataAssegnazione: DateTime
}

// 8. TodoAllegato
{
  Id: int (PK),
  IdTodo: int (FK),
  NomeFile: string,
  PathFile: string,
  DimensioneBytes: long,
  UploadedAt: DateTime,
  UploadedBy: int (FK utente)
}
```

#### **3. Bilanci (7 collections)**

```csharp
// 9. BilancioContabile
{
  IdBilancio: int (PK),
  ClienteId: int (FK),
  Mese: int (1-12),
  Anno: int,
  CodiceMastrino: string,
  DescrizioneMastrino: string,
  ImportoDare: decimal,
  ImportoAvere: decimal,
  Note: string,
  DataImport: DateTime,
  ImportedBy: int (FK utente)
}

// 10. BilancioTemplate
{
  Id: int (PK),
  NomeTemplate: string,
  Descrizione: string,
  IsDefault: bool,
  CreatedBy: int (FK utente),
  DataCreazione: DateTime,
  DataModifica: DateTime
}

// 11. BilancioTemplateItem (Voci Template)
{
  Id: int (PK),
  TemplateId: int (FK),
  CodiceVoce: string,
  DescrizioneVoce: string,
  Livello: int (1, 2, 3...),
  VocePadreId: int nullable (FK self),
  Formula: string nullable,  // es: "A + B - C"
  TipoCalcolo: enum (Somma, Formula, Manuale, Percentuale),
  OrdineVisualizzazione: int,
  IsVisible: bool
}

// 12. BilancioAssociazione (Mapping)
{
  Id: int (PK),
  ClienteId: int (FK),
  TemplateId: int (FK),
  CodiceMastrino: string,
  CodiceVoceTemplate: string,
  SegnoContabile: enum (Dare, Avere, Entrambi),
  Moltiplicatore: decimal (default 1),
  CreatedBy: int (FK utente),
  CreatedAt: DateTime
}

// 13. BilancioRiclassificato (Salvataggio singolo periodo)
{
  Id: int (PK),
  ClienteId: int (FK),
  TemplateId: int (FK),
  Mese: int,
  Anno: int,
  JsonDati: string,  // JSON con risultati calcoli
  TotaleFatturato: decimal,
  DataGenerazione: DateTime,
  GeneratedBy: int (FK utente)
}

// 14. BilancioRiclassificatoMensile (Multi-periodo)
{
  Id: int (PK),
  ClienteId: int (FK),
  TemplateId: int (FK),
  Anno: int,
  MesiInclusi: string,  // es: "1,2,3,4,5,6"
  NomeMesi: string,  // es: "Gen-Giu 2025"
  JsonDati: string,  // JSON multi-colonna
  TotaleFatturato: decimal,
  DataGenerazione: DateTime,
  GeneratedBy: int (FK utente)
}

// 15. AuditLog
{
  Id: int (PK),
  IdUtente: int (FK),
  Azione: string,
  Entita: string,  // "Todo", "Bilancio", "Cliente", "Circolare", "Budget"...
  IdEntita: int,
  DescrizioneBreve: string,
  Timestamp: DateTime,
  IpAddress: string nullable
}
```

#### **4. Circolari e Documenti (6 collections)**

```csharp
// 16. Circolari
{
  Id: int (PK),
  NumeroProtocollo: string,
  DataEmissione: DateTime,
  EnteEmittente: enum (ADE, INPS, INAIL, Ministero, Regione, Altro),
  Oggetto: string,
  Categoria: enum (Fiscale, Lavoro, Previdenziale, Societario, Amministrativo, Altro),
  Importanza: enum (Alta, Media, Bassa),
  Stato: enum (DaLeggere, Letta, Archiviata),
  PathFilePDF: string,
  NomeFile: string,
  DimensioneBytes: long,
  TestoEstrattoOCR: string,  // Per ricerca full-text
  Note: string,
  DataInserimento: DateTime,
  InseritoDa: int (FK utente),
  DataUltimaModifica: DateTime,
  ModificatoDa: int (FK utente)
}

// 17. CircolareTag (Many-to-Many)
{
  Id: int (PK),
  IdCircolare: int (FK),
  IdTag: int (FK),
  DataAssegnazione: DateTime
}

// 18. Tags
{
  Id: int (PK),
  NomeTag: string,
  Categoria: string nullable,
  Colore: string nullable,  // HEX color
  NumeroUtilizzi: int,
  CreatedAt: DateTime
}

// 19. Documenti
{
  Id: int (PK),
  Titolo: string,
  TipoDocumento: enum (Circolare, Guida, Normativa, Modello, Contratto, Altro),
  DataDocumento: DateTime,
  Categoria: string,
  IdCliente: int nullable (FK),
  PathFile: string,
  NomeFile: string,
  EstensioneFile: string,  // pdf, docx, xlsx...
  DimensioneBytes: long,
  Versione: int,
  IdDocumentoPadre: int nullable (FK self - per versioning),
  Note: string,
  DataInserimento: DateTime,
  InseritoDa: int (FK utente),
  DataUltimaModifica: DateTime
}

// 20. DocumentoTag (Many-to-Many)
{
  Id: int (PK),
  IdDocumento: int (FK),
  IdTag: int (FK),
  DataAssegnazione: DateTime
}

// 21. RicercheSalvate
{
  Id: int (PK),
  IdUtente: int (FK),
  NomeRicerca: string,
  Tipo: enum (Circolari, Documenti, Entrambi),
  FiltriJSON: string,  // Serializzazione filtri
  IsPreferita: bool,
  NumeroUtilizzi: int,
  CreatedAt: DateTime,
  UpdatedAt: DateTime
}
```

#### **5. Controllo di Gestione (5 collections)**

```csharp
// 22. Budget
{
  Id: int (PK),
  IdCliente: int (FK),
  Anno: int,
  Mese: int nullable,  // null = budget annuale
  Descrizione: string,
  JsonDati: string,  // JSON struttura voci budget
  TotaleFatturato: decimal,
  TotaleCosti: decimal,
  UtilePrevisto: decimal,
  DataCreazione: DateTime,
  CreatoDa: int (FK utente),
  DataUltimaModifica: DateTime,
  ModificatoDa: int (FK utente),
  IsApprovato: bool,
  DataApprovazione: DateTime nullable
}

// 23. BudgetVoce (Dettaglio voci budget)
{
  Id: int (PK),
  IdBudget: int (FK),
  CodiceVoce: string,
  DescrizioneVoce: string,
  ImportoPrevisto: decimal,
  Note: string
}

// 24. AnalisiControllo (Salva analisi generate)
{
  Id: int (PK),
  IdCliente: int (FK),
  TipoAnalisi: enum (KPI, Indici, BudgetVsConsuntivo, Forecast, Scenario),
  Anno: int,
  PeriodoDa: DateTime,
  PeriodoA: DateTime,
  JsonRisultati: string,  // Risultati analisi
  JsonParametri: string,  // Parametri usati
  DataGenerazione: DateTime,
  GeneratoDa: int (FK utente),
  Note: string
}

// 25. SoglieAlert
{
  Id: int (PK),
  IdCliente: int nullable,  // null = globale
  TipoIndicatore: enum (Liquidita, Margine, Scostamento, IndiceROE, IndiceROI, Altro),
  DescrizioneAlert: string,
  ValoreSogliaMin: decimal nullable,
  ValoreSogliaMax: decimal nullable,
  IsAttivo: bool,
  InviaEmail: bool,
  EmailDestinatari: string,  // CSV emails
  CreatedAt: DateTime,
  CreatedBy: int (FK utente)
}

// 26. StoricoAlert
{
  Id: int (PK),
  IdSoglia: int (FK),
  IdCliente: int (FK),
  DataAlert: DateTime,
  ValoreRilevato: decimal,
  Messaggio: string,
  IsLetto: bool,
  LettoD a: int nullable (FK utente),
  DataLettura: DateTime nullable
}
```

### **Performance Multi-Utente**

| Utenti Simultanei | Read Ops/sec | Write Ops/sec | Performance |
|-------------------|--------------|---------------|-------------|
| 1-3               | 5000+        | 1000+         | 🟢 Ottima   |
| 4-7               | 3000-5000    | 500-1000      | 🟢 Buona    |
| 8-12              | 1000-3000    | 200-500       | 🟡 Accettabile |

**Conclusione**: Per **5-10 utenti** è ideale ✅

---

## 🔐 SISTEMA MULTI-UTENTE

### **Autenticazione**

#### **Login Flow**
```
1. Utente inserisce Username/Password
2. Hash password con SHA256
3. Confronto con PasswordHash in DB
4. Se OK → Crea sessione utente
5. Carica permessi utente
6. Redirect a Dashboard
```

#### **Gestione Sessione**
```csharp
public class SessionManager
{
    public static Utente UtenteCorrente { get; set; }
    public static Permessi PermessiUtente { get; set; }
    public static DateTime LoginTime { get; set; }
    
    public static bool IsAuthenticated => UtenteCorrente != null;
    public static bool IsAdministrator => UtenteCorrente?.Ruolo == RuoloUtente.Administrator;
}
```

### **Autorizzazione (Permessi Granulari)**

```csharp
// Check permessi in ViewModel
public bool CanImportBilancio => 
    SessionManager.PermessiUtente?.CanImportBilanci == true;

// Nasconde UI se no permessi
<Button Visibility="{Binding CanImportBilancio, 
        Converter={StaticResource BoolToVisibility}}">
    Importa Bilancio
</Button>
```

### **Ruoli Predefiniti**

#### **1. Administrator**
- ✅ Gestione utenti (create/update/delete)
- ✅ Gestione permessi
- ✅ Tutte le funzionalità TODO
- ✅ Tutte le funzionalità Bilanci
- ✅ Backup/Restore database
- ✅ Visualizza audit log
- ✅ Configurazione sistema

#### **2. UserSenior**
- ✅ Tutte le funzionalità TODO (proprie + assegnate)
- ✅ Tutte le funzionalità Bilanci
- ✅ Visualizza TODO altri utenti (read-only)
- ❌ Gestione utenti
- ❌ Configurazione sistema

#### **3. User**
- ✅ TODO proprie + assegnate a lui
- ✅ Bilanci in visualizzazione
- ❌ Import/Delete bilanci
- ❌ Gestione utenti
- ❌ Visualizza TODO altri

### **Audit Log**

Traccia tutte le operazioni importanti:
```
[2025-10-16 10:30:15] Mario Rossi - TODO_CREATE - ID:123 - "Bilancio ACME SRL"
[2025-10-16 10:35:22] Sara Bianchi - BILANCIO_IMPORT - Cliente:45 - "Import Gen 2025"
[2025-10-16 11:00:00] Luca Verdi - TODO_COMPLETE - ID:120 - "730 Cliente X"
```

Visualizzabile da Administrator in sezione Audit Log.

---

## 🎨 UI/UX DESIGN

### **Framework UI: WPF + ModernWPF**

- **ModernWPF**: Tema moderno Windows 11-like
- **MaterialDesignThemes**: Icons e componenti Material
- **HandyControl** (opzionale): Componenti avanzati

### **Tema Colori**

```
Primary:   #0078D4 (Blu Microsoft)
Secondary: #107C10 (Verde)
Success:   #10893E (Verde scuro)
Warning:   #FFB900 (Arancione)
Danger:    #D13438 (Rosso)
Dark:      #212529 (Nero)
Light:     #F3F4F6 (Grigio chiaro)
```

### **Main Window Layout**

```
┌──────────────────────────────────────────────────────────────┐
│  CG Easy                      [_][□][X]  👤 Mario Rossi (Admin)│
├──────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌────────────────┐  ┌──────────────────────────────────┐  │
│  │  SIDEBAR MENU  │  │  CONTENUTO PRINCIPALE            │  │
│  │                │  │                                   │  │
│  │  🏠 Dashboard  │  │                                   │  │
│  │  ✅ TODO       │  │   [Contenuto dinamico per        │  │
│  │  📊 Bilanci    │  │    pagina selezionata]           │  │
│  │  👥 Clienti    │  │                                   │  │
│  │  👤 Profess.   │  │                                   │  │
│  │  ──────────    │  │                                   │  │
│  │  ⚙️ Impostaz.  │  │                                   │  │
│  │  📊 Audit Log  │  │                                   │  │
│  │  📤 Backup     │  │                                   │  │
│  │  ℹ️ Info       │  │                                   │  │
│  │  🚪 Logout     │  │                                   │  │
│  └────────────────┘  └──────────────────────────────────┘  │
│                                                               │
├──────────────────────────────────────────────────────────────┤
│  Status: 🟢 Connesso | DB: \\SERVER\CGEasy\database.db | v1.0│
└──────────────────────────────────────────────────────────────┘
```

### **Schermate Principali**

#### **1. Login**
- Username/Password fields
- Remember me checkbox
- Database status indicator
- Versione applicazione

#### **2. Dashboard**
- Cards con statistiche (TODO, Bilanci, Clienti)
- Lista TODO in scadenza
- Bilanci recenti
- Quick actions
- Notifiche

#### **3. TODO - Lista**
- DataGrid con tutte le colonne
- Filtri avanzati (top bar)
- Pulsanti azioni (Nuovo, Modifica, Elimina)
- Export Excel
- Ricerca full-text

#### **4. TODO - Kanban**
- 4 colonne (Da Fare, In Corso, Completata, Annullata)
- Drag & drop tra colonne
- Card visive con priorità/scadenza
- Quick edit inline

#### **5. TODO - Calendario**
- Vista mensile
- Drag & drop date
- Colori per priorità
- Click su giorno = nuovo TODO

#### **6. Bilancio Contabile**
- Import wizard Excel
- DataGrid dati importati
- Filtri Cliente/Periodo
- Delete selezionati
- Export Excel

#### **7. Template**
- Lista template (card layout)
- Dettaglio voci (tree view)
- Editor formula
- Clone template

#### **8. Associazioni**
- Dual list: Mastrini <-> Voci Template
- Drag & drop associazione
- Segno contabile dropdown
- Batch operations

#### **9. Riclassificato**
- Form generazione (Cliente, Template, Periodo)
- Tree view risultati
- Export Excel
- Grafici quick

#### **10. Grafici Bilanci**
- Chart selector (barre, linee, torta)
- Filtri periodo
- Interattivi (hover, zoom)
- Export PNG

#### **11. Circolari - Lista**
- DataGrid circolari
- Filtri (ente, categoria, data, tag, stato)
- Ricerca full-text
- Badge "da leggere"
- Quick actions (Nuova, Modifica, Elimina)
- Viewer PDF integrato (split view)

#### **12. Circolari - Dettaglio**
- Form completo circolare
- Upload/Replace PDF
- Gestione tags (autocomplete)
- Preview PDF fullscreen
- Versioning

#### **13. Documenti - Lista**
- DataGrid documenti
- Filtri multipli
- Ricerca avanzata
- Preview documento
- Versioning history

#### **14. Controllo Gestione - Dashboard**
- Cards KPI principali (4-8)
- Grafici trend
- Semafori performance
- Alert attivi
- Quick filters cliente

#### **15. Controllo Gestione - Analisi**
- Selezione cliente + periodo
- Tabella indici calcolati
- Grafici comparativi
- Export report PDF

#### **16. Controllo Gestione - Budget**
- Form creazione budget
- Tabella voci budget
- Import da Excel
- Confronto budget vs consuntivo
- Grafici scostamenti

---

## 📦 DEPLOYMENT E INSTALLAZIONE

### **Installer Unico Intelligente**

**File**: `CGEasy_Setup_v1.0.exe` (~120 MB)

**Tool**: Inno Setup (gratuito)

### **Wizard Setup**

#### **Step 1: Tipo Installazione**
```
Opzioni:
  ⚪ INSTALLAZIONE SERVER (primo PC)
  ⚪ INSTALLAZIONE CLIENT (altri PC)
```

#### **Step 2A: Setup Server**
```
- Cartella database: C:\ProgramData\CGEasy
- Crea condivisione rete
- Configura permessi
- Crea utente Administrator iniziale
```

#### **Step 3B: Setup Client**
```
- Percorso database: \\SERVER\CGEasy\database.db
- Test connessione
- Verifica accesso
```

#### **Step 3: Installazione**
```
- Copia file: C:\Program Files\CGEasy\
- Crea shortcut desktop
- Registra file associati
- Configura startup (opzionale)
```

#### **Step 4: Completamento**
```
- Mostra credenziali admin
- Info database path
- Avvia applicazione
```

### **Struttura Post-Installazione**

#### **Server:**
```
C:\Program Files\CGEasy\
  ├── CGEasy.exe
  ├── LiteDB.dll
  ├── *.dll (dependencies)
  └── config.json

C:\ProgramData\CGEasy\  (Condiviso come \\SERVER\CGEasy)
  ├── database.db
  ├── Logs\
  ├── Backups\
  └── Allegati\
```

#### **Client:**
```
C:\Program Files\CGEasy\
  ├── CGEasy.exe
  ├── LiteDB.dll
  ├── *.dll (dependencies)
  └── config.json
        {
          "DatabasePath": "\\\\SERVER\\CGEasy\\database.db"
        }
```

### **Aggiornamenti**

#### **Auto-Update**
```csharp
// Check update al startup
if (await UpdateService.CheckForUpdates())
{
    var result = MessageBox.Show(
        "Nuova versione disponibile! Vuoi aggiornare?",
        "Aggiornamento",
        MessageBoxButton.YesNo);
        
    if (result == MessageBoxResult.Yes)
    {
        await UpdateService.DownloadAndInstall();
        Application.Current.Shutdown();
    }
}
```

#### **Update Process**
1. Backup database automatico
2. Download nuova versione
3. Chiudi applicazione
4. Esegui installer update
5. Riavvia applicazione

---

## 🔐 SISTEMA LICENSING E ATTIVAZIONE

### **Panoramica**

CG Easy utilizza un **sistema di licensing offline basato su codici univoci** che permette di:
- ✅ Attivare l'applicazione senza connessione internet
- ✅ Abilitare moduli specifici (TODO, Bilanci, Full)
- ✅ Supportare trial di 30 giorni
- ✅ Generare codici univoci per ogni cliente
- ✅ Tracciare vendite e licenze

**Nessuna attivazione online richiesta** - Il sistema funziona completamente offline per garantire massima privacy e indipendenza.

---

### **🎯 Architettura Sistema**

```
┌─────────────────────────────────────────────────────────┐
│  VENDITORE (TU)                                         │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  1. Cliente acquista licenza                            │
│  2. Apri "CGEasy License Generator.exe"                 │
│  3. Inserisci dati cliente e tipo licenza               │
│  4. Genera codice univoco (es: CG4F2-5K9M3-...)        │
│  5. Invio codice via email                              │
│  6. Salva in tracking database (Excel/SQLite)           │
│                                                          │
└─────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────┐
│  CLIENTE                                                 │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  1. Scarica CGEasy_Setup.exe                            │
│  2. Esegue installazione                                │
│  3. Inserisce codice ricevuto                           │
│  4. App verifica codice OFFLINE (algoritmo SHA256)      │
│  5. Se valido → Installa moduli abilitati               │
│  6. Salva licenza in file criptato locale               │
│  7. App funziona senza internet                         │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

### **🔑 Formato License Key**

#### **Struttura Codice**
```
XXXXX-XXXXX-XXXXX-XXXXX-XXXXX

Esempio:
CG4F2-5K9M3-7H8L4-2N6P1-9R3S0
│││││
│││││
││││└─ Tipo Licenza: F=Full, T=TODO, B=Bilanci, X=Trial
│││└── Anno: 2 (2025)
││└─── Versione/Padding
│└──── Prefisso fisso "CG"
└───── Checksum/Random data
```

#### **Tipi di Licenza**

| Codice Tipo | Licenza | Moduli Abilitati | Scadenza | Prezzo Suggerito |
|-------------|---------|------------------|----------|------------------|
| **F** | Full Complete | Tutti i 5 moduli ✅ | Mai | €650 |
| **P** | Professional | TODO + Bilanci + Circolari ✅ | Mai | €450 |
| **B** | Business | TODO + Bilanci + Controllo ✅ | Mai | €500 |
| **T** | TODO Only | Solo TODO ✅ | Mai | €150 |
| **D** | Document | TODO + Circolari ✅ | Mai | €300 |
| **X** | Trial | Tutti i 5 moduli ✅ | 30 giorni | Gratuito |

**Legenda Moduli:**
- Modulo Base: Login, Dashboard, Anagrafiche (sempre incluso)
- Modulo TODO: Gestione TODO Studio
- Modulo Bilanci: Import, Riclassificazione, Grafici
- Modulo Circolari: Archiviazione documenti, Ricerca full-text
- Modulo Controllo: KPI, Budget, Analisi

---

### **⚙️ Algoritmo Generazione/Verifica**

#### **Processo di Generazione (Lato Venditore)**

```
INPUT:
  - Tipo licenza: "FULL" / "TODO" / "BILA" / "TRIAL"
  - Anno/Mese corrente: 2025-10
  - Stringa casuale: "K9M3H8L4..."
  - SEGRETO: "CGEasy_Secret_2025_XyZ123" (solo venditore)

STEP 1: Combina dati
  Payload = "F" + "2510" + "K9M3H8L4" + SEGRETO
         = "F2510K9M3H8L4CGEasy_Secret_2025_XyZ123"

STEP 2: Calcola HASH (SHA256)
  Hash = SHA256(Payload)
      = "8f7d3a2b9e1c4f5a6d8b7e3a9c2f1d4e..."

STEP 3: Estrai checksum (primi 2 char)
  Checksum = "8F"

STEP 4: Costruisci codice finale
  RawKey = "CG" + "4" + "F" + "25" + "K9M3" + "7H8L4" + "2N6P1" + "9R3S" + "8F"
         = "CG4F25K9M37H8L42N6P19R3S8F"

STEP 5: Formatta con trattini (gruppi di 5)
  FinalKey = "CG4F2-5K9M3-7H8L4-2N6P1-9R3S8"

OUTPUT: CG4F2-5K9M3-7H8L4-2N6P1-9R3S8
```

#### **Processo di Verifica (Lato Cliente)**

```
INPUT: Codice inserito = "CG4F2-5K9M3-7H8L4-2N6P1-9R3S8"

STEP 1: Rimuovi trattini
  CleanKey = "CG4F25K9M37H8L42N6P19R3S8"

STEP 2: Estrai componenti
  - Prefisso: "CG" (verifica corretta)
  - Versione: "4"
  - Tipo: "F" (Full)
  - Anno: "25" (2025)
  - Random data: "5K9M37H8L42N6P19R3S"
  - Checksum fornito: "8" (ultimi char)

STEP 3: Rigenera hash con STESSO segreto
  Payload = "F" + "2510" + "K9M3H8L4" + "CGEasy_Secret_2025_XyZ123"
  Hash = SHA256(Payload)
  Checksum calcolato = primi 2 char hash

STEP 4: Confronta checksum
  Se checksum_fornito == checksum_calcolato:
    ✅ CODICE VALIDO
    
    Determina moduli:
    - Tipo "F" → TODO ✅, Bilanci ✅, Scadenza: Mai
    - Tipo "T" → TODO ✅, Bilanci ❌, Scadenza: Mai
    - Tipo "B" → TODO ❌, Bilanci ✅, Scadenza: Mai
    - Tipo "X" → TODO ✅, Bilanci ✅, Scadenza: Oggi + 30gg
  Altrimenti:
    ❌ CODICE NON VALIDO
```

---

### **🛠️ Tool "License Generator" (Per Venditore)**

#### **Funzionalità**

Un'applicazione WPF desktop che solo il venditore usa per generare i codici:

**Features:**
- Form inserimento dati cliente (Nome, Email)
- Selezione tipo licenza (Full, TODO, Bilanci, Trial)
- Generazione codice univoco con un click
- Copia negli appunti automatica
- Database tracking licenze (Excel o SQLite)
- Export lista vendite
- Opzionale: Invio email automatico

#### **UI Schematica**

```
┌────────────────────────────────────────────────┐
│  CG Easy - License Generator            [_][X] │
├────────────────────────────────────────────────┤
│                                                 │
│  📋 GENERA NUOVA LICENZA                       │
│  ────────────────────────────────────────────  │
│                                                 │
│  Cliente:     [Mario Rossi                  ]  │
│  Email:       [mario@studio.it              ]  │
│                                                 │
│  Tipo Licenza:                                  │
│  ⚪ TODO Only (€150)                           │
│  ⚪ Bilanci Only (€200)                        │
│  ⚫ Full (€350)                                 │
│  ⚪ Trial 30gg (Gratis)                        │
│                                                 │
│  Note: [Cliente nuovo - Pagato 16/10/2025  ]  │
│                                                 │
│  [ 🔑 GENERA LICENSE KEY ]                     │
│                                                 │
│  ✅ LICENZA GENERATA:                          │
│  ┌──────────────────────────────────────────┐ │
│  │  CG4F2-5K9M3-7H8L4-2N6P1-9R3S0          │ │
│  └──────────────────────────────────────────┘ │
│                                                 │
│  📋 Moduli: TODO ✅, Bilanci ✅                │
│  📅 Scadenza: Mai (Perpetua)                   │
│                                                 │
│  [ 📋 Copia ] [ 📧 Invia Email ] [ 💾 Salva ] │
│                                                 │
│  ────────────────────────────────────────────  │
│  📊 LICENZE RECENTI                            │
│  Data    | Cliente      | Tipo  | Codice      │
│  ────────┼──────────────┼───────┼─────────    │
│  16/10   | Mario Rossi  | Full  | CG4F2-...   │
│  15/10   | Sara Bianchi | TODO  | CG4T2-...   │
│                                                 │
└────────────────────────────────────────────────┘
```

#### **Database Tracking Semplice**

Può essere un semplice file Excel:

| Data | Cliente | Email | Codice | Tipo | Prezzo | Note |
|------|---------|-------|--------|------|--------|------|
| 16/10/2025 | Studio ABC | info@abc.it | CG4F2-5K9M3-... | Full | €350 | Pagamento PayPal |
| 15/10/2025 | Sara Bianchi | sara@... | CG4T2-8N3K7-... | TODO | €150 | Cliente nuovo |
| 14/10/2025 | Demo Trial | trial@... | CG4X2-5K9M3-... | Trial | €0 | Demo 30gg |

**Vantaggi:**
- ✅ Tracking semplice vendite
- ✅ Cerca codice per cliente
- ✅ Reinvio codice se perso
- ✅ Report fatturato mensile

---

### **💻 Integrazione in CGEasy App**

#### **Durante Installazione (Inno Setup)**

```pascal
[Code]
var
  LicenseKeyPage: TInputQueryWizardPage;
  LicenseValid: Boolean;
  HasTodo, HasBilanci: Boolean;

procedure InitializeWizard;
begin
  // Crea pagina inserimento licenza
  LicenseKeyPage := CreateInputQueryPage(wpWelcome,
    'Attivazione Licenza', 
    'Inserisci la tua chiave di licenza',
    'La chiave è stata inviata via email dopo acquisto');
  
  LicenseKeyPage.Add('License Key:', False);
  LicenseKeyPage.Values[0] := '';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = LicenseKeyPage.ID then
  begin
    // Verifica licenza con algoritmo
    LicenseValid := VerifyLicenseKey(LicenseKeyPage.Values[0]);
    
    if LicenseValid then
      Result := True
    else
    begin
      MsgBox('Licenza non valida. Verifica il codice.', mbError, MB_OK);
      Result := False;
    end;
  end
  else
    Result := True;
end;
```

#### **Al Primo Avvio App**

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Verifica licenza salvata
        var licenseService = new LicenseService();
        var licenseInfo = licenseService.LoadLicense();
        
        if (!licenseInfo.IsValid)
        {
            // Mostra dialog attivazione
            var activationWindow = new ActivationWindow();
            if (activationWindow.ShowDialog() != true)
            {
                Shutdown();
                return;
            }
            
            licenseInfo = licenseService.LoadLicense();
        }
        
        // Verifica scadenza trial
        if (licenseInfo.IsTrial && licenseInfo.ExpiryDate < DateTime.Now)
        {
            MessageBox.Show(
                "Periodo di prova scaduto.\n" +
                "Acquista una licenza completa su www.cg-group.it",
                "Trial Scaduto",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Shutdown();
            return;
        }
        
        // Salva licenza in contesto globale
        AppContext.CurrentLicense = licenseInfo;
        
        // Avvia applicazione
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
```

#### **Controllo Accesso Moduli**

```csharp
// Service per controllo feature
public static class FeatureGuard
{
    public static bool HasTodoModule => 
        AppContext.CurrentLicense?.HasTodoModule == true;
    
    public static bool HasBilanciModule => 
        AppContext.CurrentLicense?.HasBilanciModule == true;
    
    public static void RequireTodoModule()
    {
        if (!HasTodoModule)
        {
            throw new UnauthorizedAccessException(
                "Modulo TODO non abilitato nella tua licenza.\n" +
                "Contatta il venditore per upgrade.");
        }
    }
    
    public static void RequireBilanciModule()
    {
        if (!HasBilanciModule)
        {
            throw new UnauthorizedAccessException(
                "Modulo Bilanci non abilitato nella tua licenza.\n" +
                "Contatta il venditore per upgrade.");
        }
    }
}

// Uso nei ViewModels
public class TodoListViewModel : ViewModelBase
{
    public TodoListViewModel()
    {
        // Verifica accesso modulo
        FeatureGuard.RequireTodoModule();
        
        // Carica TODO...
    }
}

// Uso in XAML (visibilità menu)
<MenuItem Header="TODO Studio" 
          Command="{Binding OpenTodoCommand}"
          Visibility="{Binding HasTodoModule, 
                       Converter={StaticResource BoolToVisibility}}" />

<MenuItem Header="Bilanci" 
          Command="{Binding OpenBilanciCommand}"
          Visibility="{Binding HasBilanciModule, 
                       Converter={StaticResource BoolToVisibility}}" />
```

#### **Salvataggio Licenza Locale**

```csharp
public class LicenseService
{
    private const string LICENSE_FILE = "license.dat";
    private const string ENCRYPTION_KEY = "CGEasy_Encryption_2025";
    
    public void SaveLicense(LicenseInfo license)
    {
        // Serializza
        var json = JsonConvert.SerializeObject(license);
        
        // Cripta
        var encrypted = AES_Encrypt(json, ENCRYPTION_KEY);
        
        // Salva in ProgramData
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "CGEasy",
            LICENSE_FILE);
        
        File.WriteAllText(path, encrypted);
    }
    
    public LicenseInfo LoadLicense()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CGEasy",
                LICENSE_FILE);
            
            if (!File.Exists(path))
                return new LicenseInfo { IsValid = false };
            
            // Leggi e decripta
            var encrypted = File.ReadAllText(path);
            var json = AES_Decrypt(encrypted, ENCRYPTION_KEY);
            
            // Deserializza
            var license = JsonConvert.DeserializeObject<LicenseInfo>(json);
            
            // Verifica integrità (opzionale: ri-verifica checksum)
            return license;
        }
        catch
        {
            return new LicenseInfo { IsValid = false };
        }
    }
}
```

---

### **📊 Workflow Completo Vendita**

#### **Scenario: Cliente Acquista "CG Easy Full"**

**STEP 1 - Venditore (TU):**
1. Cliente contatta per acquisto
2. Ricevi pagamento (€350)
3. Apri "License Generator.exe"
4. Inserisci:
   - Cliente: "Studio Commercialista ABC"
   - Email: "info@studioabc.it"
   - Tipo: "Full"
5. Click "Genera Licenza"
6. Codice generato: `CG4F2-5K9M3-7H8L4-2N6P1-9R3S0`
7. Click "Copia" → Codice negli appunti
8. Invii email:

```
Oggetto: CG Easy - Codice Licenza

Gentile Cliente,

Grazie per aver acquistato CG Easy Full!

Il tuo codice di attivazione è:
CG4F2-5K9M3-7H8L4-2N6P1-9R3S0

ISTRUZIONI:
1. Scarica l'installer da: www.cg-group.it/download
2. Esegui CGEasy_Setup.exe
3. Inserisci questo codice quando richiesto
4. L'installazione configurerà automaticamente i moduli

Moduli inclusi nella tua licenza:
✅ TODO Studio
✅ Gestione Bilanci

Licenza: Perpetua (senza scadenza)
Supporto: support@cg-group.it

Cordiali saluti
```

9. Salvi in Excel tracking (data, cliente, codice, tipo)

**STEP 2 - Cliente:**
1. Riceve email
2. Scarica `CGEasy_Setup.exe`
3. Esegue installazione
4. Wizard chiede: "Inserisci License Key"
5. Copia-incolla: `CG4F2-5K9M3-7H8L4-2N6P1-9R3S0`
6. Click "Verifica"
7. Installer: ✅ "Licenza Full valida! Installazione in corso..."
8. Installazione completa
9. App si avvia con TODO + Bilanci abilitati

**STEP 3 - Utilizzo Continuativo:**
- App legge `license.dat` ad ogni avvio
- Se trial: controlla data scadenza ogni giorno
- Se scaduto: mostra messaggio acquisto

---

### **🔒 Sicurezza Sistema**

#### **Punti di Forza**

✅ **Offline Funzionante**
- Cliente non serve internet per attivare
- Privacy totale (no telemetria)

✅ **Checksum SHA256**
- Impossibile generare codici validi senza segreto
- Modifica 1 carattere = codice invalido

✅ **Segreto Hard-Coded**
- Presente nell'app compilata
- Difficile da estrarre per utente medio

✅ **Criptazione Locale**
- License file criptato con AES
- Difficile modificare tipo licenza localmente

#### **Vulnerabilità (e Mitigazioni)**

⚠️ **Condivisione Codice**
- **Problema**: Cliente può dare codice ad amico
- **Mitigazione**: 
  - Policy: "1 licenza = 1 studio (illimitati PC)"
  - Accettabile per target B2B
  - Opzionale: Aggiungere Hardware Lock (complesso)

⚠️ **Decompilazione App**
- **Problema**: Hacker esperto può estrarre segreto da .exe
- **Mitigazione**:
  - Code Obfuscation (Dotfuscator, ConfuserEx)
  - Cambiare segreto ogni major release
  - Per 99% utenti è impossibile

⚠️ **Crack Locale**
- **Problema**: Utente modifica `license.dat` manualmente
- **Mitigazione**:
  - File criptato con AES
  - Checksum integrale al caricamento
  - Verifica firma digitale (opzionale)

#### **Livello Sicurezza: MEDIO-ALTO**

**Sufficiente per:**
- ✅ Studi commercialisti (professionisti onesti)
- ✅ Software B2B di nicchia
- ✅ Prezzo licenza ragionevole (€150-350)
- ✅ Target non tech-savvy

**Esempi software famosi con stesso sistema:**
- Sublime Text (Editor)
- WinRAR (Compressione)
- Molti plugin Adobe/VSCode
- Software vertical B2B

---

### **💰 Pricing Suggerito**

| Licenza | Moduli | Target | Prezzo | Risparmio |
|---------|--------|--------|--------|-----------|
| **Trial 30gg** | Tutti i 5 moduli | Demo / Test | **GRATIS** | - |
| **TODO Only** | Solo TODO | Micro studi (1-2 pers.) | **€150** | - |
| **Document** | TODO + Circolari | Studi piccoli | **€300** | - |
| **Professional** | TODO + Bilanci + Circolari | Studi medi | **€450** | €100 vs singoli |
| **Business** | TODO + Bilanci + Controllo | Consulenti avanzati | **€500** | €150 vs singoli |
| **Full Complete** | Tutti i 5 moduli | Studi completi (5-10 pers.) | **€650** | €250 vs singoli |

**Calcolo Valore Moduli Singoli:**
- TODO: €150
- Bilanci: €250
- Circolari: €200
- Controllo Gestione: €300
- **TOTALE se acquistati singolarmente: €900**
- **Full Complete: €650** = **Risparmio 28%**

**Considerazioni:**
- No costi ricorrenti server
- No fee Gumroad/Stripe
- Margine 100% dopo costi sviluppo
- Upselling: Trial → Professional → Business → Full
- Volume discount: 5+ licenze -15%, 10+ licenze -20%
- Manutenzione annuale: 15% del prezzo licenza (opzionale)
- Update major version: 30% del prezzo licenza (opzionale)

---

### **🔧 Miglioramenti Futuri (v1.1+)**

#### **Opzione A: Hardware Lock**
```
- Lega licenza a 1-3 PC specifici
- Hardware ID (CPU + MAC + Disk serial)
- Transfer licenza via supporto
- PRO: Anti-pirateria migliore
- CONTRO: Più supporto (cambio PC)
```

#### **Opzione B: Attivazione Online (Opzionale)**
```
- Server verifica codice
- Limita attivazioni (max 5 PC)
- Telemetria uso anonima
- PRO: Controllo totale
- CONTRO: Serve server + costi
```

#### **Opzione C: Subscription (Annuale)**
```
- Licenza con scadenza 1 anno
- Rinnovo automatico
- Update inclusi
- PRO: Revenue ricorrente
- CONTRO: Cliente preferisce perpetua
```

---

### **📋 Checklist Implementazione Licensing**

#### **Sviluppo**
- [ ] LicenseKeyGenerator service (generazione codici)
- [ ] LicenseKeyValidator service (verifica codici)
- [ ] LicenseService (save/load locale)
- [ ] FeatureGuard (controllo accessi moduli)
- [ ] Models: LicenseInfo, LicenseType
- [ ] Encryption helpers (AES per license.dat)
- [ ] Inno Setup: License input page
- [ ] ActivationWindow.xaml (dialog attivazione)

#### **Tool Generator**
- [ ] WPF app "License Generator"
- [ ] Form dati cliente
- [ ] Radio button tipi licenza
- [ ] Bottone genera codice
- [ ] Clipboard copy automatico
- [ ] Database tracking (Excel o SQLite)
- [ ] Export vendite Excel
- [ ] Email template

#### **Testing**
- [ ] Genera 10+ codici di ogni tipo
- [ ] Verifica tutti i codici generati
- [ ] Test codice invalido (modificato)
- [ ] Test trial scadenza
- [ ] Test file license.dat criptato
- [ ] Test upgrade trial → full
- [ ] Test reinstallazione stessa licenza
- [ ] Test su PC diversi (stesso codice)

#### **Documentazione**
- [ ] Manuale venditore (uso Generator)
- [ ] Email template per clienti
- [ ] FAQ attivazione
- [ ] Troubleshooting guide
- [ ] Policy licenze (1 studio = ?)

---

### **🎯 Riepilogo Decisione Licensing**

**Sistema Scelto: License Key Offline (Opzione 2)**

**Motivi:**
- ✅ Perfetto per desktop app
- ✅ Nessun server da gestire
- ✅ Cliente non serve internet
- ✅ Bilanciamento sicurezza/complessità
- ✅ Generazione illimitata codici
- ✅ Tracking semplice con Excel
- ✅ Supporta moduli e trial

**Implementazione:**
- Tool Generator per venditore (WPF app)
- Verifica in-app con SHA256 + checksum
- Salvataggio criptato `license.dat`
- FeatureGuard per accesso moduli

**Effort Stimato:**
- Generator tool: 2-3 giorni
- Integrazione in CGEasy: 3-4 giorni
- Testing: 1-2 giorni
- **TOTALE: ~1 settimana**

(Già incluso nelle 8 settimane di sviluppo)

---

## 🧩 ARCHITETTURA MODULARE (OPZIONALE)

### **Panoramica**

CG Easy può essere sviluppato con **architettura modulare** per permettere installazioni selettive:
- ✅ **Installazione Completa**: TODO + Bilanci
- ✅ **Solo TODO Studio**: Per piccoli studi
- ✅ **Solo Bilanci**: Per consulenti senior

**Decisione Consigliata: v1.0 Monolitica → v2.0 Modulare**

Per la prima release (v1.0) si consiglia un'app **monolitica** (tutto incluso) per:
- 🚀 Time-to-market più veloce (8 settimane vs 10)
- 🎯 Focus su features, non infrastruttura
- 🧪 Validazione mercato prima di over-engineering
- 💰 Pricing semplice (€350 flat)

Se il prodotto ha successo, v2.0 (6-8 mesi dopo) può introdurre modularità.

---

### **Architettura Modulare (Se Implementata)**

```
CGEasy.sln
├── CGEasy.Core (Obbligatorio)
│   └── Login, Dashboard, Anagrafiche
├── CGEasy.TodoModule (Opzionale)
│   └── TODO Lista/Kanban/Calendario
└── CGEasy.BilanciModule (Opzionale)
    └── Import, Riclassificazione, Grafici
```

#### **Module Loader**
```csharp
public class ModuleLoader
{
    public static void LoadModules(IServiceCollection services)
    {
        // Core sempre caricato
        services.AddSingleton<AuthService>();
        
        // TODO se abilitato da licenza
        if (AppContext.CurrentLicense.HasTodoModule)
        {
            services.AddTransient<TodoService>();
            services.AddTransient<TodoListViewModel>();
        }
        
        // Bilanci se abilitato
        if (AppContext.CurrentLicense.HasBilanciModule)
        {
            services.AddTransient<BilancioService>();
            services.AddTransient<BilancioImportViewModel>();
        }
    }
}
```

#### **Menu Dinamico**
```csharp
private void BuildDynamicMenu()
{
    MenuItems.Add(new MenuItem("🏠", "Dashboard"));
    
    if (FeatureGuard.HasTodoModule)
        MenuItems.Add(new MenuItem("✅", "TODO Studio"));
    else
        MenuItems.Add(new MenuItem("✅", "TODO (Non abilitato)", 
            () => ShowUpgradeDialog()));
    
    if (FeatureGuard.HasBilanciModule)
        MenuItems.Add(new MenuItem("📊", "Bilanci"));
    else
        MenuItems.Add(new MenuItem("📊", "Bilanci (Non abilitato)", 
            () => ShowUpgradeDialog()));
}
```

**Vantaggi Modulare:**
- Pricing flessibile (€150 TODO, €200 Bilanci, €350 Full)
- App più leggera se serve solo 1 modulo
- Upselling clienti (Trial → TODO → Full)

**Svantaggi:**
- +20-30% tempo sviluppo
- Complessità architettura
- Testing inter-moduli

**Conclusione: Rimandiamo a v2.0 se necessario**

---

## 🛠️ TECNOLOGIE E PACCHETTI

### **Framework Base**
- **.NET 8.0** (LTS)
- **WPF** (Windows Presentation Foundation)
- **C# 12**

### **NuGet Packages**

```xml
<!-- Database -->
<PackageReference Include="LiteDB" Version="5.0.21" />

<!-- WPF UI Framework -->
<PackageReference Include="ModernWpfUI" Version="0.9.6" />
<PackageReference Include="MaterialDesignThemes" Version="5.1.0" />
<PackageReference Include="MaterialDesignColors" Version="3.0.0" />

<!-- MVVM -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />

<!-- Excel Import/Export -->
<PackageReference Include="EPPlus" Version="7.0.0" />

<!-- Grafici -->
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc2" />

<!-- Security (Password Hashing) -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />

<!-- Logging -->
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />

<!-- Notifiche System Tray -->
<PackageReference Include="Hardcodet.NotifyIcon.Wpf" Version="1.1.0" />

<!-- Calendario (scelta da fare) -->
<!-- Opzione A: Syncfusion (commerciale con trial) -->
<PackageReference Include="Syncfusion.SfScheduler.WPF" Version="24.1.41" />
<!-- Opzione B: Custom o open-source alternativa -->

<!-- JSON -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />

<!-- Validator -->
<PackageReference Include="FluentValidation" Version="11.9.0" />

<!-- PDF Viewer/Handler -->
<PackageReference Include="PdfiumViewer" Version="2.13.0" />
<!-- Opzione alternativa: -->
<!-- <PackageReference Include="Syncfusion.PdfViewer.WPF" Version="24.1.41" /> -->

<!-- PDF Text Extraction (OCR/Full-text search) -->
<PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.4.6" />
<!-- O alternativa: -->
<!-- <PackageReference Include="PdfSharp" Version="6.0.0" /> -->

<!-- Full-Text Search Engine -->
<PackageReference Include="Lucene.Net" Version="4.8.0-beta00016" />
<PackageReference Include="Lucene.Net.Analysis.Common" Version="4.8.0-beta00016" />

<!-- Report Generation (PDF Export) -->
<PackageReference Include="QuestPDF" Version="2024.3.0" />
<!-- O alternativa: -->
<!-- <PackageReference Include="Syncfusion.Pdf.WPF" Version="24.1.41" /> -->
```

### **Development Tools**
- **Visual Studio 2022** (Community/Professional)
- **Inno Setup** (installer)
- **Git** (version control)
- **NuGet Package Manager**

---

## ⏱️ TIMELINE E STIME

### **Durata Totale: 12 Settimane (3 mesi)**

**NOTA**: Con i nuovi moduli Circolari e Controllo Gestione, il tempo di sviluppo aumenta da 8 a 12 settimane.

#### **Settimana 1: Setup e Autenticazione**
- ✅ Setup progetto WPF + MVVM + DI
- ✅ Configurazione LiteDB
- ✅ Models base (Utente, Permessi, Cliente, Professionista)
- ✅ Sistema Login/Logout
- ✅ SessionManager
- ✅ Main Window + Sidebar
- ✅ Dashboard base

**Deliverable**: App con login funzionante

---

#### **Settimana 2: Anagrafiche e Fondamenta**
- ✅ CRUD Clienti (View + ViewModel + Repository)
- ✅ CRUD Professionisti
- ✅ CRUD Tipo Pratica
- ✅ Gestione Utenti (solo Admin)
- ✅ Gestione Permessi
- ✅ Audit Log service
- ✅ Repository pattern per tutte le entità

**Deliverable**: Anagrafiche complete

---

#### **Settimana 3: TODO Studio - Base**
- ✅ Models TODO (TodoStudio, Assegnazioni, Allegati)
- ✅ TodoRepository + TodoService
- ✅ Vista LISTA TODO
  - DataGrid con filtri
  - CRUD completo
  - Ricerca
  - Dialog crea/modifica TODO
- ✅ Dialog assegnazione professionisti
- ✅ Upload/Download allegati

**Deliverable**: TODO Lista funzionante

---

#### **Settimana 4: TODO Studio - Viste Avanzate**
- ✅ Vista KANBAN
  - 4 colonne drag & drop
  - Card visive
  - Quick edit
  - Cambio stato con drag
- ✅ Vista CALENDARIO
  - Calendario mensile
  - Drag & drop date
  - Colori priorità
  - Click crea TODO
- ✅ Notifiche scadenze
- ✅ Badge contatori
- ✅ Statistiche TODO

**Deliverable**: TODO completo (3 viste + notifiche)

---

#### **Settimana 5: Bilanci - Import e Base**
- ✅ Models Bilanci (BilancioContabile, Template, Items, Associazioni)
- ✅ BilancioRepository + BilancioService
- ✅ Excel Import Service (EPPlus)
- ✅ Vista Bilancio Contabile
  - Import wizard
  - DataGrid dati
  - Filtri periodo
  - Delete
  - Export Excel
- ✅ Vista Template
  - Lista template
  - CRUD template base

**Deliverable**: Import bilanci Excel funzionante

---

#### **Settimana 6: Bilanci - Riclassificazione**
- ✅ Vista Voci Template (tree view editor)
- ✅ Formula engine (calcolo espressioni)
- ✅ Vista Associazioni
  - Dual list
  - Drag & drop
  - Segno contabile
- ✅ RiclassificazioneService
- ✅ Vista Riclassificato Periodo
  - Form generazione
  - Tree risultati
  - Calcolo formule
  - Percentuali
  - Export Excel

**Deliverable**: Riclassificazione funzionante

---

#### **Settimana 7: Bilanci - Multi-Periodo e Grafici**
- ✅ Vista Riclassificato Mensile
  - Selezione multi-mese
  - Tabella multi-colonna
  - Confronto mesi
  - Export Excel avanzato
- ✅ ChartService (LiveCharts wrapper)
- ✅ Vista Grafici
  - Grafico barre
  - Grafico linee
  - Grafico torta
  - Grafico area
  - Filtri periodo
  - Export PNG
- ✅ Dashboard con widget grafici

**Deliverable**: Bilanci completi + Grafici

---

#### **Settimana 8: Bilanci - Testing e Refinement**
- ✅ Testing completo tutti i moduli bilanci
- ✅ Bug fixing bilanci
- ✅ Performance optimization riclassificazione
- ✅ UI/UX refinement grafici
- ✅ Validazioni e error handling

**Deliverable**: Moduli TODO + Bilanci completi e testati

---

#### **Settimana 9: Circolari - Base**
- ✅ Models Circolari (Circolare, Tags, CircolareTag, RicercheSalvate)
- ✅ CircolareRepository + CircolareService
- ✅ TagService
- ✅ Vista Lista Circolari
  - DataGrid con filtri
  - CRUD completo
  - Gestione tags
  - Upload PDF
- ✅ PDF Viewer integrato (split view)
- ✅ Dialog crea/modifica circolare

**Deliverable**: Gestione circolari base funzionante

---

#### **Settimana 10: Circolari - Ricerca e Documenti**
- ✅ Lucene.Net integration (full-text search)
- ✅ PDF text extraction service (iTextSharp)
- ✅ Ricerca avanzata con filtri multipli
- ✅ Ricerche salvate e preferite
- ✅ Models Documenti
- ✅ Vista Gestione Documenti
  - CRUD documenti
  - Upload multipli
  - Versioning
  - Preview documenti
- ✅ Notifiche circolari da leggere
- ✅ Export e condivisione

**Deliverable**: Modulo Circolari completo con ricerca full-text

---

#### **Settimana 11: Controllo di Gestione**
- ✅ Models Controllo Gestione (Budget, BudgetVoce, AnalisiControllo, SoglieAlert, StoricoAlert)
- ✅ BudgetRepository + BudgetService
- ✅ AnalisiService (calcolo KPI e indici)
- ✅ Dashboard KPI
  - Cards principali
  - Grafici trend
  - Semafori
- ✅ Vista Analisi Bilanci
  - Calcolo indici automatico
  - Confronto multi-periodo
  - Grafici comparativi
- ✅ Vista Budget
  - Form creazione budget
  - Import Excel
  - CRUD voci budget
- ✅ Vista Budget vs Consuntivo
  - Tabella scostamenti
  - Grafici scostamenti
- ✅ Sistema Alert e Soglie

**Deliverable**: Modulo Controllo Gestione funzionante

---

#### **Settimana 12: Testing Finale, Polish e Installer**
- ✅ Testing completo tutti i 5 moduli
- ✅ Testing multi-utente (5-7 PC)
- ✅ Bug fixing generale
- ✅ Performance optimization
- ✅ UI/UX refinement
- ✅ Inno Setup script con licensing
- ✅ Wizard installazione completo
- ✅ License Generator tool
- ✅ Documentazione utente (PDF/Online)
- ✅ Video tutorial setup (10 min)
- ✅ Video tutorial moduli (20 min)
- ✅ README + Changelog
- ✅ FAQ e troubleshooting
- ✅ Firma digitale installer (opzionale)

**Deliverable**: `CGEasy_Setup_v2.0.exe` pronto per deploy

---

### **Effort Totale**
- **Sviluppo**: ~440-480 ore (40h/settimana × 12 settimane)
- **Testing**: ~60 ore
- **Documentazione**: ~30 ore
- **Licensing system**: ~40 ore (incluso License Generator)
- **TOTALE**: ~530-610 ore

---

## 🚀 ROADMAP FUTURA

### **v1.1 (Post-Release - 1 mese)**
- ⭐ Export PDF bilanci
- ⭐ Stampa TODO
- ⭐ Filtri salvati (preferiti)
- ⭐ Temi dark/light mode
- ⭐ Notifiche email scadenze
- ⭐ Shortcuts tastiera avanzati
- ⭐ Ricerca globale (CTRL+K)

### **v1.2 (3 mesi dopo release)**
- 🚀 Google Calendar Sync (opzionale)
- 🚀 Mobile companion app (read-only)
- 🚀 Cloud backup automatico
- 🚀 API REST per integrazioni
- 🚀 Plugin system

### **v2.0 (6+ mesi)**
- 💡 Dashboard avanzata con KPI
- 💡 Report builder
- 💡 Fatturazione semplificata
- 💡 Gestione scadenze automatica
- 💡 Integrazione PEC
- 💡 AI Assistant (analisi bilanci)

---

## 📋 CHECKLIST PRE-RELEASE

### **Sviluppo**
- [ ] Tutte le features core implementate
- [ ] Unit test copertura >70%
- [ ] Integration test multi-utente
- [ ] Performance test (10 utenti simultanei)
- [ ] Memory leak test (app aperta 8h+)
- [ ] Zero critical bugs
- [ ] Zero warning compilazione

### **Database**
- [ ] Migrazioni database testate
- [ ] Backup/Restore testato
- [ ] Concurrent access testato
- [ ] Data integrity verificata
- [ ] Rollback transactions OK

### **UI/UX**
- [ ] Responsive su risoluzioni diverse (1920x1080, 1366x768)
- [ ] Tutti i form validati
- [ ] Feedback visuale per azioni lunghe
- [ ] Error handling graceful
- [ ] Messaggi utente chiari
- [ ] Shortcuts tastiera documentati
- [ ] Accessibilità base (screen reader)

### **Security**
- [ ] Password hashing (BCrypt)
- [ ] Session timeout configurabile
- [ ] Permessi verificati lato server
- [ ] Audit log completo
- [ ] SQL injection impossibile (LiteDB NoSQL)
- [ ] File upload validato

### **Installer**
- [ ] Wizard testato su Windows 10
- [ ] Wizard testato su Windows 11
- [ ] Setup Server funziona
- [ ] Setup Client funziona
- [ ] Upgrade da versione precedente OK
- [ ] Disinstallazione pulita
- [ ] Shortcut creati correttamente
- [ ] Firma digitale applicata (opzionale)

### **Documentazione**
- [ ] README.md completo
- [ ] Manuale utente (PDF)
- [ ] Video tutorial setup (YouTube)
- [ ] Changelog dettagliato
- [ ] FAQ comuni
- [ ] Guida troubleshooting

### **Deploy**
- [ ] File installer < 200 MB
- [ ] Antivirus whitelist richiesta (Microsoft Defender)
- [ ] Testato su 3+ PC diversi
- [ ] Backup database pre-produzione
- [ ] Rollback plan pronto

---

## 📞 SUPPORTO E CONTATTI

### **Repository**
- GitHub: (da definire)
- Issue tracking: GitHub Issues
- Releases: GitHub Releases

### **Documentazione**
- Wiki: GitHub Wiki
- API Docs: (se applicabile)
- User Guide: PDF + Online

### **Licenza**
- Software: Proprietaria / MIT (da decidere)
- LiteDB: MIT License
- Altri componenti: Vedi NuGet packages

---

## 🎯 CONCLUSIONI

### **Vantaggi CG Easy**

#### **vs App Web Esistente**
- ✅ Più veloce (native desktop)
- ✅ Più semplice (solo TODO + Bilanci)
- ✅ Nessun server web
- ✅ Offline-first
- ✅ Costo zero cloud

#### **vs Altri Software Commerciali**
- ✅ Nessuna licenza mensile
- ✅ Database in-house (privacy)
- ✅ Personalizzabile
- ✅ Nessun limite utenti (5-10 OK)
- ✅ Nessun vendor lock-in

### **Target Ideale**
- 👥 Studi piccoli/medi (2-10 persone)
- 💰 Budget limitato (no licenze cloud)
- 🔒 Dati sensibili (preferenza in-house)
- ⚡ Performance importanti (no latency web)
- 🛠️ Personalizzazioni future

### **Rischi e Mitigazioni**

| Rischio | Probabilità | Impatto | Mitigazione |
|---------|-------------|---------|-------------|
| LiteDB lento con molti utenti | Media | Alto | Test performance, fallback SQL Server |
| Corruzione database | Bassa | Alto | Backup automatici ogni ora |
| Rete lenta/instabile | Media | Medio | Cache locale, timeout configurabili |
| Utenti non accettano desktop app | Bassa | Alto | Training + documentazione |
| Bug critici post-release | Media | Alto | Testing approfondito, hotfix rapidi |

### **Success Metrics**

**Post 3 mesi di utilizzo:**
- ✅ Zero downtime > 1 minuto
- ✅ 95% utenti soddisfatti
- ✅ < 10 bug critici riportati
- ✅ Tempo medio operazione < 2 secondi
- ✅ Database < 500 MB con 1 anno dati

---

## 📅 NEXT STEPS

### **Immediate (se approvato):**
1. ✅ Setup repository Git
2. ✅ Crea progetto Visual Studio 2022
3. ✅ Configura structure (Views, ViewModels, Services, Models)
4. ✅ Install NuGet packages
5. ✅ Crea database iniziale con LiteDB
6. ✅ Implementa Login + Main Window base

### **Prima Milestone (Week 1):**
- App avviabile con login
- Database funzionante
- Main Window con sidebar
- Dashboard placeholder

**Poi procediamo settimana per settimana come da timeline!** 🚀

---

---

## 📊 RIEPILOGO COMPLETO PROGETTO v2.0

### **🎯 Caratteristiche Principali**

**5 Moduli Integrati:**
1. ✅ **TODO Studio** - Task management (Lista/Kanban/Calendario)
2. ✅ **Bilanci** - Import, Riclassificazione, Grafici
3. ✅ **Circolari** - Archiviazione, Ricerca full-text, PDF Viewer
4. ✅ **Controllo Gestione** - KPI, Budget, Analisi, Alert
5. ✅ **Base** - Login, Dashboard, Anagrafiche (sempre incluso)

**Tecnologia:**
- Desktop App Windows (WPF + .NET 8.0)
- Database LiteDB (file singolo condiviso)
- Multi-utente (5-10 utenti)
- Offline-first
- Licensing offline

**Database:**
- 26 collections LiteDB
- File singolo condiviso (rete)
- Performance ottimizzata per 5-10 utenti
- Backup automatico

**UI/UX:**
- Tema moderno (ModernWPF + Material Design)
- 16 schermate principali
- Dashboard personalizzabile
- Grafici interattivi (LiveCharts)
- PDF Viewer integrato
- Ricerca full-text (Lucene.Net)

**Sicurezza:**
- Sistema licensing offline (SHA256)
- 3 ruoli utente (Admin, UserSenior, User)
- Permessi granulari
- Audit log completo
- File criptati (AES)

---

### **📈 Stime Progetto**

| Aspetto | Valore |
|---------|--------|
| **Durata Sviluppo** | 12 settimane (3 mesi) |
| **Effort Totale** | 530-610 ore |
| **Effort settimanale** | ~45 ore/settimana |
| **Numero Moduli** | 5 moduli principali |
| **Collections DB** | 26 collections |
| **Schermate UI** | 16 schermate |
| **NuGet Packages** | ~25 pacchetti |
| **Righe codice stimate** | ~40.000-50.000 LOC |

---

### **💰 Business Model**

**Pricing (Licenze Perpetue):**
- Trial: GRATIS (30gg)
- TODO Only: €150
- Document: €300
- Professional: €450
- Business: €500
- **Full Complete: €650** ⭐

**Revenue Potenziale (100 licenze):**
- 20 TODO Only: €3.000
- 30 Professional: €13.500
- 30 Business: €15.000
- 20 Full: €13.000
- **TOTALE: €44.500**

**Costi:**
- Sviluppo: 540 ore × €50/h = €27.000 (one-time)
- Manutenzione annua: ~€5.000
- **Break-even: ~50 licenze miste**

---

### **🎯 Target di Mercato**

**Clienti Ideali:**
- 👥 Studi commercialisti 2-10 persone
- 💼 Consulenti del lavoro
- 📊 Studi di revisione contabile
- 🏢 Piccole associazioni professionali

**Vantaggi Competitivi:**
- ✅ Prezzo one-time (no abbonamenti)
- ✅ Database in-house (privacy)
- ✅ Offline-first (no internet necessario)
- ✅ Personalizzabile
- ✅ Multi-utente incluso
- ✅ 5 moduli integrati

---

### **🚀 Roadmap Post-Release**

**v2.1 (3 mesi dopo release):**
- Export PDF report personalizzati
- Stampa documenti/circolari
- Email notifiche automatiche
- Backup cloud opzionale
- Mobile app read-only (companion)

**v2.2 (6 mesi):**
- Integrazione PEC
- Firma digitale documenti
- Modulo Scadenzario
- Report builder avanzato
- Dashboard personalizzabile

**v3.0 (12 mesi):**
- Modulo Fatturazione semplificata
- AI Assistant per analisi bilanci
- Google Calendar Sync (opzionale)
- API REST per integrazioni
- Plugin system

---

### **✅ Checklist Pre-Sviluppo**

- [ ] Approvazione specifiche v2.0
- [ ] Setup repository Git (c:\dev2exe\cg_easy)
- [ ] Creazione progetto Visual Studio 2022
- [ ] Setup solution con 5 progetti moduli
- [ ] Install NuGet packages base
- [ ] Setup database LiteDB iniziale
- [ ] Configurazione CI/CD (opzionale)
- [ ] Setup License Generator project

---

### **🎬 Kick-Off Progetto**

**Step 1: Setup Iniziale (Giorno 1)**
```
1. Crea cartella: c:\dev2exe\cg_easy
2. Inizializza Git repo
3. Crea Visual Studio solution:
   - CGEasy.Core (modulo base)
   - CGEasy.TodoModule
   - CGEasy.BilanciModule
   - CGEasy.CircolariModule
   - CGEasy.ControlloModule
   - CGEasy.LicenseGenerator (tool separato)
4. Setup dependencies (NuGet)
5. Crea database LiteDB vuoto
6. Prima build di test
```

**Step 2: Sprint 1 - Week 1**
- Implementa Login/Logout
- Crea Main Window con sidebar
- Setup Dependency Injection
- Implementa SessionManager
- Crea Dashboard placeholder
- **Milestone: App avviabile con login** ✅

**Step 3: Iterazioni Successive**
- Settimana 2-12: Segui timeline dettagliata
- Ogni settimana = deliverable funzionante
- Testing continuo
- Demo settimanale (opzionale)

---

### **📞 Supporto e Contatti**

**Sviluppatore:**
- CG Group SRL
- Email: support@cg-group.it
- Sito: www.cg-group.it

**Repository:**
- Git: (da definire - GitHub/GitLab privato)
- Issue tracking: GitHub Issues
- Releases: GitHub Releases
- Wiki: GitHub Wiki

**Documentazione:**
- User Manual: PDF + Online
- Developer Docs: XML Comments + Wiki
- API Docs: (se esposta API REST)
- Video Tutorials: YouTube channel

**Licenza Software:**
- Software: Proprietaria
- LiteDB: MIT License
- Altri componenti: Vedi NuGet packages

---

## 🏆 CONCLUSIONI FINALI

### **Perché CG Easy v2.0 è la Scelta Giusta**

**✅ Completezza:**
5 moduli integrati coprono tutte le esigenze operative di uno studio commercialista: task management, bilanci, documenti, controllo gestione, dashboard unificata.

**✅ Tecnologia Moderna:**
WPF + .NET 8.0 + LiteDB = Stack collaudato, performante, manutenibile. No dipendenze cloud, no costi ricorrenti infrastruttura.

**✅ Business Model Sostenibile:**
Licenze perpetue da €150 a €650 = Break-even a 50 licenze. Mercato potenziale: migliaia di studi in Italia. Upselling e manutenzione = revenue ricorrente.

**✅ Time-to-Market:**
12 settimane = 3 mesi per avere un prodotto completo e competitivo. Roadmap chiara per v2.1, v2.2, v3.0 = crescita continua.

**✅ Scalabilità:**
Architettura modulare pronta per futuri moduli (Fatturazione, Scadenzario, PEC, AI). Licensing system già supporta multi-moduli.

---

### **🚀 Ready to Start?**

Tutte le specifiche sono complete e dettagliate:
- ✅ 5 moduli definiti
- ✅ 26 collections database
- ✅ 16 schermate UI
- ✅ Timeline 12 settimane
- ✅ Licensing system
- ✅ Pricing strategy
- ✅ Tecnologie scelte

**Prossimo passo:** 
Creare progetto in `c:\dev2exe\cg_easy` e iniziare Settimana 1!

---

**Fine Documento**

*Versione 2.0 - 16 Ottobre 2025*  
*Ultima modifica: 16/10/2025*  
*Aggiornamenti v2.0:*
- *Aggiunti Modulo Circolari e Controllo Gestione*
- *Database esteso a 26 collections*
- *Timeline estesa a 12 settimane*
- *Licensing aggiornato con 6 tipi licenza*
- *Pricing aggiornato (€150-€650)*
- *Stime effort: 530-610 ore*

