# CGEasy SQL Server

**Gestionale Controllo di Gestione per Studi Professionali**

Applicazione desktop WPF (.NET 8.0) per il controllo di gestione professionale, completamente migrata da LiteDB a SQL Server con Entity Framework Core.

## 🎯 Caratteristiche

- **Gestione Clienti e Professionisti**
- **Bilanci Contabili e Template**
- **Controllo di Gestione Avanzato**
- **Statistiche e Grafici**
- **Gestione TODO Studio**
- **Circolari Professionali**
- **Sistema Licenze**
- **Multi-utente con permessi**

## 🗄️ Tecnologie

- **.NET 8.0** - Framework applicativo
- **WPF** - User Interface
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **CommunityToolkit.Mvvm** - Pattern MVVM
- **ClosedXML** - Export Excel

## ✅ Stato Progetto

**Conversione completata al 100%** - 20 Novembre 2025

- ✅ Tutti i Models convertiti (33/33)
- ✅ Tutti i Repository convertiti (15/15)
- ✅ Tutti i Services convertiti (6/6)
- ✅ DbContext configurato (26 tabelle)
- ✅ Migrations create e applicate
- ✅ Build SUCCESS (0 errori, 0 warnings)

## 📦 Struttura

```
CGEasy/
├── src/
│   ├── CGEasy.Core/          # Business Logic & Data Access
│   │   ├── Models/           # Entity Models (EF Core)
│   │   ├── Repositories/     # Data Repositories
│   │   ├── Services/         # Business Services
│   │   ├── Data/            # DbContext & Migrations
│   │   └── Helpers/         # Utility Classes
│   │
│   ├── CGEasy.App/          # WPF Application
│   │   ├── ViewModels/      # MVVM ViewModels
│   │   ├── Views/           # WPF Views
│   │   └── Resources/       # Assets & Styles
│   │
│   └── Modules/             # Feature Modules
│       ├── BilanciModule/
│       ├── CircolariModule/
│       ├── ControlloModule/
│       └── TodoModule/
│
├── tools/                   # Utility Scripts
└── docs/                    # Documentation

```

## 🚀 Setup

### Prerequisiti
- .NET 8.0 SDK
- SQL Server (LocalDB o Express)
- Windows 10/11

### Installazione

1. Clone il repository:
```bash
git clone https://github.com/Dan74Ger/cgeasysql.git
cd cgeasysql
```

2. Configura connection string in `C:\db_CGEASY\connectionstring.txt`:
```
Server=localhost\SQLEXPRESS;Database=CGEasy;Trusted_Connection=True;TrustServerCertificate=True;
```

3. Applica migrations:
```bash
cd src/CGEasy.Core
dotnet ef database update --startup-project ../CGEasy.App
```

4. Compila e avvia:
```bash
cd ../CGEasy.App
dotnet run
```

## 📊 Database

**26 Tabelle principali**:
- Clienti, Professionisti, Utenti
- Bilanci Contabili e Template
- Statistiche CE/SP
- Todo Studio
- Circolari e Argomenti
- Gestione Banche
- Licenze
- Audit Log

## 👥 Credenziali Default

- **Admin**: `admin` / `admin123`
- **Demo**: `demo` / `demo123`

## 📝 Licenza

Proprietario: Studio Professionale  
Uso interno - Tutti i diritti riservati

## 🔧 Supporto

Per supporto: dan74ger@gmail.com

---

**Versione**: 2.0.0  
**Data**: 20 Novembre 2025  
**Build**: SUCCESS ✅
