# 🔄 PIANO MIGRAZIONE: DA LITEDB A SQL SERVER EXPRESS

**Progetto**: CGEasy - Software Gestionale per Studi Commercialisti  
**Data Analisi**: 18 Novembre 2025  
**Stima Totale**: 5-10 giorni lavorativi (29-39 ore)

---

## 📊 STATO ATTUALE DEL PROGETTO

### Architettura Corrente
- **Pattern**: MVVM (Model-View-ViewModel)
- **Database**: LiteDB 5.0.21 (file-based NoSQL)
- **Framework**: .NET 8.0 WPF
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Repository Pattern**: Implementato con astrazione IRepository<T>

### Statistiche Database
- **33 Modelli/Entità** nel progetto
- **20 Repository** implementati
- **24 Collections** LiteDB (tabelle)
- **13 Services** business logic
- **1 DbContext** (LiteDbContext.cs - 680 righe)

---

## 📋 ENTITÀ/TABELLE DA MIGRARE (24 Tabelle)

### 1️⃣ ANAGRAFICHE (5 tabelle)
| # | Tabella | Descrizione | Righe Tipiche |
|---|---------|-------------|---------------|
| 1 | `Utente` | Utenti sistema | 5-20 |
| 2 | `UserPermissions` | Permessi utenti | 5-20 |
| 3 | `Cliente` | Anagrafica clienti | 100-1000 |
| 4 | `Professionista` | Anagrafica professionisti | 5-50 |
| 5 | `TipoPratica` | Tipi pratiche/servizi | 20-50 |

### 2️⃣ MODULO TODO (2 tabelle)
| # | Tabella | Descrizione | Righe Tipiche |
|---|---------|-------------|---------------|
| 6 | `TodoStudio` | Task/attività studio | 100-5000 |
| 7 | `AuditLog` | Log operazioni | 1000-50000 |

### 3️⃣ MODULO BILANCI (8 tabelle)
| # | Tabella | Descrizione | Righe Tipiche |
|---|---------|-------------|---------------|
| 8 | `BilancioContabile` | Mastrini importati Excel | 10000-100000 |
| 9 | `BilancioTemplate` | Template riclassificazione | 5-20 |
| 10 | `AssociazioneMastrino` | Mappature mastrini (testata) | 100-1000 |
| 11 | `AssociazioneMastrinoDettaglio` | Dettagli mappature | 1000-10000 |
| 12 | `StatisticaSPSalvata` | Statistiche Stato Patrimoniale | 50-500 |
| 13 | `StatisticaCESalvata` | Statistiche Conto Economico | 50-500 |
| 14 | `IndicePersonalizzato` | Indici personalizzati cliente | 20-200 |
| 15 | `IndiceConfigurazione` | Configurazione indici | 50-500 |

### 4️⃣ MODULO CIRCOLARI (2 tabelle)
| # | Tabella | Descrizione | Righe Tipiche |
|---|---------|-------------|---------------|
| 16 | `Argomento` | Categorie circolari | 10-50 |
| 17 | `Circolare` | Circolari/documenti | 100-5000 |

### 5️⃣ MODULO CONTROLLO GESTIONE (6 tabelle)
| # | Tabella | Descrizione | Righe Tipiche |
|---|---------|-------------|---------------|
| 18 | `Banca` | Anagrafica banche | 5-50 |
| 19 | `BancaIncasso` | Incassi bancari | 1000-10000 |
| 20 | `BancaPagamento` | Pagamenti bancari | 1000-10000 |
| 21 | `BancaUtilizzoAnticipo` | Utilizzo anticipi | 100-1000 |
| 22 | `BancaSaldoGiornaliero` | Saldi giornalieri | 1000-10000 |
| 23 | `FinanziamentoImport` | Finanziamenti importati | 50-500 |

### 6️⃣ LICENZE (2 tabelle)
| # | Tabella | Descrizione | Righe Tipiche |
|---|---------|-------------|---------------|
| 24 | `LicenseClient` | Clienti licenze | 10-100 |
| 25 | `LicenseKey` | Chiavi licenza | 10-100 |

---

## 🛠️ PIANO DI MIGRAZIONE COMPLETO

## ═══════════════════════════════════════════════════════════
## FASE 1: SETUP INFRASTRUTTURA SQL SERVER
## ⏱️ Tempo Stimato: 1-2 ore
## ═══════════════════════════════════════════════════════════

### STEP 1.1: Installazione SQL Server Express

**Download SQL Server 2022 Express**
```
URL: https://www.microsoft.com/it-it/sql-server/sql-server-downloads
Versione: SQL Server 2022 Express (gratuita)
Dimensione: ~700 MB
```

**Procedura Installazione:**
1. Eseguire setup SQL Server Express
2. Scegliere "Custom Installation"
3. Installare componenti:
   - ✅ Database Engine Services
   - ✅ SQL Server Replication
   - ✅ Management Tools - Basic
4. Configurazione istanza:
   - Nome istanza: `SQLEXPRESS` (default)
   - Modalità autenticazione: **Mixed Mode** (importante!)
   - Password utente `sa`: Impostare password sicura
   - Aggiungi utente Windows corrente come amministratore
5. Completare installazione

**Verifica Installazione:**
```powershell
# Verifica servizio SQL Server attivo
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Output atteso:
# MSSQL$SQLEXPRESS    Running
# SQLBrowser          Running (opzionale)
```

### STEP 1.2: Installazione SQL Server Management Studio (SSMS)

**Download SSMS**
```
URL: https://aka.ms/ssmsfullsetup
Versione: SSMS 19 o superiore
Dimensione: ~600 MB
```

**Test Connessione:**
1. Aprire SSMS
2. Server name: `localhost\SQLEXPRESS`
3. Authentication: `Windows Authentication`
4. Click "Connect"
5. Se connesso con successo ✅ SQL Server è pronto

### STEP 1.3: Creazione Database CGEasy

**Opzione A - Via SSMS (GUI):**
1. Aprire SSMS
2. Right-click su "Databases" → "New Database"
3. Nome: `CGEasy`
4. Initial Size: 100 MB
5. Autogrowth: 10 MB
6. Click OK

**Opzione B - Via Script SQL:**
```sql
-- Crea database
CREATE DATABASE CGEasy
ON PRIMARY 
(
    NAME = CGEasy_Data,
    FILENAME = 'C:\db_CGEASY\CGEasy.mdf',
    SIZE = 100MB,
    MAXSIZE = 10GB,
    FILEGROWTH = 10MB
)
LOG ON
(
    NAME = CGEasy_Log,
    FILENAME = 'C:\db_CGEASY\CGEasy_log.ldf',
    SIZE = 50MB,
    MAXSIZE = 2GB,
    FILEGROWTH = 10MB
);
GO

-- Configurazione database per performance desktop
ALTER DATABASE CGEasy SET RECOVERY SIMPLE;
GO
ALTER DATABASE CGEasy SET READ_COMMITTED_SNAPSHOT ON;
GO
```

### STEP 1.4: Creazione Utente Applicazione (Opzionale)

**Per maggiore sicurezza, crea utente dedicato:**
```sql
USE [master];
GO

-- Crea login SQL
CREATE LOGIN cgeasy_app 
WITH PASSWORD = 'CGEasy2025!Secure',
     DEFAULT_DATABASE = CGEasy,
     CHECK_POLICY = OFF;
GO

-- Assegna permessi al database
USE [CGEasy];
GO

CREATE USER cgeasy_app FOR LOGIN cgeasy_app;
GO

ALTER ROLE db_owner ADD MEMBER cgeasy_app;
GO
```

### STEP 1.5: Test Connection String

**Connection String da usare nel codice:**

**Opzione A - Windows Authentication (consigliata per LAN):**
```
Server=localhost\SQLEXPRESS;Database=CGEasy;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;
```

**Opzione B - SQL Authentication:**
```
Server=localhost\SQLEXPRESS;Database=CGEasy;User Id=cgeasy_app;Password=CGEasy2025!Secure;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;
```

**Test connessione da codice C#:**
```csharp
using System.Data.SqlClient;

var connectionString = "Server=localhost\\SQLEXPRESS;Database=CGEasy;Trusted_Connection=True;";
using var conn = new SqlConnection(connectionString);
try 
{
    conn.Open();
    Console.WriteLine("✅ Connessione SQL Server riuscita!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Errore: {ex.Message}");
}
```

---

## ═══════════════════════════════════════════════════════════
## FASE 2: MIGRAZIONE CODICE
## ⏱️ Tempo Stimato: 16-20 ore
## ═══════════════════════════════════════════════════════════

### STEP 2.1: Installazione NuGet Packages Entity Framework Core

**File da modificare**: `src/CGEasy.Core/CGEasy.Core.csproj`

**RIMUOVERE:**
```xml
<PackageReference Include="LiteDB" Version="5.0.21" />
```

**AGGIUNGERE:**
```xml
<!-- Entity Framework Core per SQL Server -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

**Comando da eseguire:**
```bash
cd src/CGEasy.Core
dotnet restore
```

### STEP 2.2: Creazione Nuovo DbContext Entity Framework

**File da creare**: `src/CGEasy.Core/Data/CGEasyDbContext.cs`

**Template Base:**
```csharp
using Microsoft.EntityFrameworkCore;
using CGEasy.Core.Models;
using System;
using System.IO;

namespace CGEasy.Core.Data
{
    public class CGEasyDbContext : DbContext
    {
        private readonly string _connectionString;
        
        public CGEasyDbContext() : this(GetDefaultConnectionString())
        {
        }
        
        public CGEasyDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        
        // ===== DBSETS (24 TABELLE) =====
        
        // Anagrafiche
        public DbSet<Utente> Utenti { get; set; }
        public DbSet<UserPermissions> UserPermissions { get; set; }
        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<Professionista> Professionisti { get; set; }
        public DbSet<TipoPratica> TipoPratiche { get; set; }
        
        // TODO
        public DbSet<TodoStudio> TodoStudio { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        // Bilanci
        public DbSet<BilancioContabile> BilancioContabile { get; set; }
        public DbSet<BilancioTemplate> BilancioTemplate { get; set; }
        public DbSet<AssociazioneMastrino> AssociazioniMastrini { get; set; }
        public DbSet<AssociazioneMastrinoDettaglio> AssociazioniMastriniDettagli { get; set; }
        public DbSet<StatisticaSPSalvata> StatisticheSPSalvate { get; set; }
        public DbSet<StatisticaCESalvata> StatisticheCESalvate { get; set; }
        public DbSet<IndicePersonalizzato> IndiciPersonalizzati { get; set; }
        public DbSet<IndiceConfigurazione> IndiciConfigurazioni { get; set; }
        
        // Circolari
        public DbSet<Argomento> Argomenti { get; set; }
        public DbSet<Circolare> Circolari { get; set; }
        
        // Controllo Gestione
        public DbSet<Banca> Banche { get; set; }
        public DbSet<BancaIncasso> BancaIncassi { get; set; }
        public DbSet<BancaPagamento> BancaPagamenti { get; set; }
        public DbSet<BancaUtilizzoAnticipo> BancaUtilizzoAnticipo { get; set; }
        public DbSet<BancaSaldoGiornaliero> BancaSaldoGiornaliero { get; set; }
        public DbSet<FinanziamentoImport> FinanziamentoImport { get; set; }
        
        // Licenze
        public DbSet<LicenseClient> LicenseClients { get; set; }
        public DbSet<LicenseKey> LicenseKeys { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurazione entità, indici, relazioni
            // (DA IMPLEMENTARE NEL PROSSIMO STEP)
        }
        
        private static string GetDefaultConnectionString()
        {
            // Percorso file config connection string
            var configPath = Path.Combine(@"C:\db_CGEASY", "connectionstring.txt");
            
            if (File.Exists(configPath))
            {
                return File.ReadAllText(configPath).Trim();
            }
            
            // Default: SQL Server Express locale
            return "Server=localhost\\SQLEXPRESS;Database=CGEasy;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
        }
    }
}
```

### STEP 2.3: Conversione Modelli da LiteDB a EF Core

**Attributi da convertire:**

| LiteDB | EF Core | Note |
|--------|---------|------|
| `[BsonId]` | `[Key]` | Chiave primaria |
| `[BsonField("nome")]` | `[Column("nome")]` | Nome colonna |
| `[BsonIgnore]` | `[NotMapped]` | Proprietà non mappata |
| Auto-increment in mapper | `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` | ID auto |

**Esempio conversione - File**: `src/CGEasy.Core/Models/Cliente.cs`

**PRIMA (LiteDB):**
```csharp
using LiteDB;

[BsonId]
public int Id { get; set; }

[BsonField("nome_cliente")]
public string NomeCliente { get; set; } = string.Empty;

[BsonIgnore]
public string Display => $"{NomeCliente} ({PivaCliente})";
```

**DOPO (EF Core):**
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
public int Id { get; set; }

[Column("nome_cliente")]
[MaxLength(200)]
[Required]
public string NomeCliente { get; set; } = string.Empty;

[NotMapped]
public string Display => $"{NomeCliente} ({PivaCliente})";
```

**TUTTI i 33 Models da convertire (lista completa):**
1. Utente.cs
2. UserPermissions.cs
3. Cliente.cs
4. Professionista.cs
5. TipoPratica.cs
6. TodoStudio.cs
7. AuditLog.cs
8. BilancioContabile.cs
9. BilancioTemplate.cs
10. AssociazioneMastrino.cs
11. AssociazioneMastrinoDettaglio.cs
12. StatisticaSPSalvata.cs
13. StatisticaCESalvata.cs
14. IndicePersonalizzato.cs
15. IndiceConfigurazione.cs
16. Argomento.cs
17. Circolare.cs
18. Banca.cs
19. BancaIncasso.cs
20. BancaPagamento.cs
21. BancaUtilizzoAnticipo.cs
22. BancaSaldoGiornaliero.cs
23. FinanziamentoImport.cs
24. LicenseClient.cs
25. LicenseKey.cs
26. BilancioGruppo.cs
27. BilancioStatistica.cs
28. BilancioStatisticaMultiPeriodo.cs
29. CalendarDay.cs
30. ContoContabileAssociato.cs
31. ContoContabileAssociatoConPeriodo.cs
32. IndiceCalcolato.cs
33. VoceBilancio.cs

### STEP 2.4: Configurazione OnModelCreating (Indici, Relazioni, Constraints)

**File**: `src/CGEasy.Core/Data/CGEasyDbContext.cs`

**Metodo OnModelCreating completo:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ===== UTENTI =====
    modelBuilder.Entity<Utente>(entity =>
    {
        entity.ToTable("utenti");
        entity.HasIndex(e => e.Username).IsUnique();
        entity.HasIndex(e => e.Email);
        entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Email).HasMaxLength(150);
        entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
    });
    
    // ===== CLIENTI =====
    modelBuilder.Entity<Cliente>(entity =>
    {
        entity.ToTable("clienti");
        entity.HasIndex(e => e.NomeCliente);
        entity.HasIndex(e => e.Attivo);
        entity.HasIndex(e => e.IdProfessionista);
        entity.Property(e => e.NomeCliente).HasMaxLength(200).IsRequired();
        entity.Property(e => e.PivaCliente).HasMaxLength(20);
        entity.Property(e => e.CfCliente).HasMaxLength(16);
    });
    
    // ===== PROFESSIONISTI =====
    modelBuilder.Entity<Professionista>(entity =>
    {
        entity.ToTable("professionisti");
        entity.HasIndex(e => e.Cognome);
        entity.HasIndex(e => e.Attivo);
        entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Cognome).HasMaxLength(100).IsRequired();
    });
    
    // ===== TODO STUDIO =====
    modelBuilder.Entity<TodoStudio>(entity =>
    {
        entity.ToTable("todoStudio");
        entity.HasIndex(e => e.Stato);
        entity.HasIndex(e => e.Priorita);
        entity.HasIndex(e => e.DataScadenza);
        entity.HasIndex(e => e.CreatoreId);
        entity.HasIndex(e => e.ClienteId);
    });
    
    // ===== BILANCIO CONTABILE =====
    modelBuilder.Entity<BilancioContabile>(entity =>
    {
        entity.ToTable("bilancio_contabile");
        entity.HasIndex(e => new { e.ClienteId, e.Anno, e.Mese });
        entity.HasIndex(e => e.CodiceMastrino);
        entity.Property(e => e.ImportoDare).HasPrecision(18, 2);
        entity.Property(e => e.ImportoAvere).HasPrecision(18, 2);
    });
    
    // ===== LICENSE KEYS =====
    modelBuilder.Entity<LicenseKey>(entity =>
    {
        entity.ToTable("license_keys");
        entity.HasIndex(e => e.FullKey).IsUnique();
        entity.HasIndex(e => e.LicenseClientId);
        entity.HasIndex(e => e.ModuleName);
    });
    
    // ... CONFIGURARE TUTTE LE ALTRE 19 ENTITÀ ...
    // (Completare con tutte le tabelle)
}
```

### STEP 2.5: Riscrittura Repositories (20 Repository)

**Conversione da LiteDB a EF Core - Pattern:**

**PRIMA (LiteDB) - File**: `src/CGEasy.Core/Repositories/ClienteRepository.cs`
```csharp
public IEnumerable<Cliente> GetAll()
{
    return _context.Clienti.FindAll();
}

public Cliente? GetById(int id)
{
    return _context.Clienti.FindById(id);
}

public int Insert(Cliente entity)
{
    entity.CreatedAt = DateTime.UtcNow;
    return _context.Clienti.Insert(entity);
}

public bool Update(Cliente entity)
{
    entity.UpdatedAt = DateTime.UtcNow;
    return _context.Clienti.Update(entity);
}
```

**DOPO (EF Core):**
```csharp
public async Task<List<Cliente>> GetAllAsync()
{
    return await _context.Clienti.ToListAsync();
}

public async Task<Cliente?> GetByIdAsync(int id)
{
    return await _context.Clienti.FindAsync(id);
}

public async Task<int> InsertAsync(Cliente entity)
{
    entity.CreatedAt = DateTime.UtcNow;
    _context.Clienti.Add(entity);
    await _context.SaveChangesAsync();
    return entity.Id;
}

public async Task<bool> UpdateAsync(Cliente entity)
{
    entity.UpdatedAt = DateTime.UtcNow;
    _context.Clienti.Update(entity);
    return await _context.SaveChangesAsync() > 0;
}

public async Task<bool> DeleteAsync(int id)
{
    var entity = await GetByIdAsync(id);
    if (entity == null) return false;
    _context.Clienti.Remove(entity);
    return await _context.SaveChangesAsync() > 0;
}
```

**IMPORTANTE - Interface IRepository aggiornata:**
```csharp
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    Task<int> InsertAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
```

**Lista Repository da convertire (20 file):**
1. ClienteRepository.cs
2. ProfessionistaRepository.cs
3. TipoPraticaRepository.cs
4. TodoStudioRepository.cs
5. BilancioContabileRepository.cs
6. BilancioTemplateRepository.cs
7. AssociazioneMastrinoRepository.cs
8. ArgomentiRepository.cs
9. CircolariRepository.cs
10. BancaRepository.cs
11. BancaIncassoRepository.cs
12. BancaPagamentoRepository.cs
13. BancaUtilizzoAnticipoRepository.cs
14. BancaSaldoGiornalieroRepository.cs
15. FinanziamentoImportRepository.cs
16. LicenseRepository.cs
17. StatisticaSPSalvataRepository.cs
18. StatisticaCESalvataRepository.cs
19. IndicePersonalizzatoRepository.cs
20. (Eventuali altri custom repository)

### STEP 2.6: Aggiornamento Services (13 Services)

**Conversione chiamate sincrone → asincrone**

**PRIMA:**
```csharp
public class ClienteService
{
    public List<Cliente> GetClientiAttivi()
    {
        return _repo.Find(c => c.Attivo).ToList();
    }
    
    public void CreaCliente(Cliente cliente)
    {
        _repo.Insert(cliente);
        _auditLog.Log("Cliente creato");
    }
}
```

**DOPO:**
```csharp
public class ClienteService
{
    public async Task<List<Cliente>> GetClientiAttiviAsync()
    {
        return await _repo.FindAsync(c => c.Attivo);
    }
    
    public async Task CreaClienteAsync(Cliente cliente)
    {
        await _repo.InsertAsync(cliente);
        await _auditLog.LogAsync("Cliente creato");
    }
}
```

**Lista Services da aggiornare:**
1. AuthService.cs
2. AuditLogService.cs
3. AssociazioneMastrinoService.cs
4. BancaService.cs
5. BilancioStatisticaService.cs
6. CircolariService.cs
7. DatabaseConfigService.cs
8. DatabaseEncryptionService.cs (DA RIMUOVERE - SQL Server non usa più criptazione file)
9. ExcelBilancioService.cs
10. IndiciDiBilancioService.cs
11. LicenseService.cs
12. SessionManager.cs
13. TodoEventService.cs

### STEP 2.7: Aggiornamento ViewModels (45 ViewModels)

**Pattern conversione:**

**PRIMA:**
```csharp
private void LoadClienti()
{
    Clienti = new ObservableCollection<Cliente>(_repo.GetAll());
}

private void SalvaCliente()
{
    _repo.Insert(NuovoCliente);
    MessageBox.Show("Cliente salvato!");
    LoadClienti();
}
```

**DOPO:**
```csharp
private async Task LoadClientiAsync()
{
    var clienti = await _repo.GetAllAsync();
    Clienti = new ObservableCollection<Cliente>(clienti);
}

private async Task SalvaClienteAsync()
{
    await _repo.InsertAsync(NuovoCliente);
    MessageBox.Show("Cliente salvato!");
    await LoadClientiAsync();
}
```

**⚠️ IMPORTANTE - Gestire UI blocking:**
```csharp
// Nei ViewModel: aggiungere IsBusy per progress indicator
private bool _isBusy;
public bool IsBusy 
{ 
    get => _isBusy; 
    set => SetProperty(ref _isBusy, value); 
}

private async Task LoadClientiAsync()
{
    IsBusy = true;
    try
    {
        var clienti = await _repo.GetAllAsync();
        Clienti = new ObservableCollection<Cliente>(clienti);
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Nelle View XAML - Aggiungere BusyIndicator:**
```xml
<Grid>
    <DataGrid ItemsSource="{Binding Clienti}" />
    
    <!-- Overlay loading -->
    <Grid Visibility="{Binding IsBusy, Converter={StaticResource BoolToVisibilityConverter}}">
        <Rectangle Fill="Black" Opacity="0.3" />
        <ProgressBar IsIndeterminate="True" Width="200" Height="20" />
    </Grid>
</Grid>
```

### STEP 2.8: Aggiornamento App.xaml.cs (Dependency Injection)

**File**: `src/CGEasy.App/App.xaml.cs`

**SOSTITUIRE:**
```csharp
// PRIMA
services.AddSingleton<LiteDbContext>(provider =>
{
    var context = new LiteDbContext();
    context.MarkAsSingleton();
    return context;
});
```

**CON:**
```csharp
// DOPO
services.AddDbContext<CGEasyDbContext>(options =>
{
    var connectionString = GetConnectionString();
    options.UseSqlServer(connectionString);
}, ServiceLifetime.Scoped); // Scoped per DbContext EF Core
```

**Rimuovere metodi criptazione database** (non più necessari con SQL Server):
- `InitializeDatabase()` - Semplificare
- `CheckPendingEncryption()` - Rimuovere
- Tutta la logica `DatabaseEncryptionService`

### STEP 2.9: Creazione Migrations Entity Framework

**Comandi da eseguire:**
```bash
cd src/CGEasy.Core

# Crea migration iniziale
dotnet ef migrations add InitialCreate --startup-project ../CGEasy.App/CGEasy.App.csproj

# Applica migration al database
dotnet ef database update --startup-project ../CGEasy.App/CGEasy.App.csproj
```

**Questo creerà automaticamente:**
- Tutte le 24 tabelle
- Indici definiti in OnModelCreating
- Chiavi primarie/esterne
- Constraints

---

## ═══════════════════════════════════════════════════════════
## FASE 3: SCRIPT MIGRAZIONE DATI DA LITEDB A SQL SERVER
## ⏱️ Tempo Stimato: 4-6 ore
## ═══════════════════════════════════════════════════════════

### STEP 3.1: Creare Tool Migrazione Dati

**File da creare**: `tools/MigrazioneLiteDbToSql/Program.cs`

**Progetto console:**
```bash
mkdir -p tools/MigrazioneLiteDbToSql
cd tools/MigrazioneLiteDbToSql
dotnet new console
dotnet add reference ../../src/CGEasy.Core/CGEasy.Core.csproj
```

**Codice tool migrazione:**
```csharp
using CGEasy.Core.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

Console.WriteLine("🔄 MIGRAZIONE DATI DA LITEDB A SQL SERVER");
Console.WriteLine("==========================================");

// 1. Apri database LiteDB (vecchio)
var liteDb = new LiteDbContext(@"C:\db_CGEASY\cgeasy.db", "Woodstockac@74");
Console.WriteLine($"✅ LiteDB aperto: {liteDb.GetStats().DatabasePath}");

// 2. Apri database SQL Server (nuovo)
var sqlDb = new CGEasyDbContext();
Console.WriteLine($"✅ SQL Server connesso");

// 3. Migra UTENTI
Console.Write("📋 Migrazione Utenti... ");
foreach (var utente in liteDb.Utenti.FindAll())
{
    sqlDb.Utenti.Add(utente);
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($"✅ {liteDb.Utenti.Count()} utenti migrati");

// 4. Migra USER PERMISSIONS
Console.Write("📋 Migrazione UserPermissions... ");
foreach (var perm in liteDb.UserPermissions.FindAll())
{
    sqlDb.UserPermissions.Add(perm);
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($"✅ {liteDb.UserPermissions.Count()} permessi migrati");

// 5. Migra PROFESSIONISTI
Console.Write("📋 Migrazione Professionisti... ");
foreach (var prof in liteDb.Professionisti.FindAll())
{
    sqlDb.Professionisti.Add(prof);
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($"✅ {liteDb.Professionisti.Count()} professionisti migrati");

// 6. Migra CLIENTI
Console.Write("📋 Migrazione Clienti... ");
foreach (var cliente in liteDb.Clienti.FindAll())
{
    sqlDb.Clienti.Add(cliente);
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($"✅ {liteDb.Clienti.Count()} clienti migrati");

// 7. Migra TIPO PRATICHE
Console.Write("📋 Migrazione TipoPratiche... ");
foreach (var tipo in liteDb.TipoPratiche.FindAll())
{
    sqlDb.TipoPratiche.Add(tipo);
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($"✅ {liteDb.TipoPratiche.Count()} tipi pratica migrati");

// 8. Migra TODO STUDIO
Console.Write("📋 Migrazione TodoStudio... ");
foreach (var todo in liteDb.TodoStudio.FindAll())
{
    sqlDb.TodoStudio.Add(todo);
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($"✅ {liteDb.TodoStudio.Count()} todo migrati");

// 9. Migra AUDIT LOG
Console.Write("📋 Migrazione AuditLog... ");
int auditCount = 0;
foreach (var audit in liteDb.AuditLogs.FindAll())
{
    sqlDb.AuditLogs.Add(audit);
    auditCount++;
    
    // Salva a batch di 1000 per performance
    if (auditCount % 1000 == 0)
    {
        await sqlDb.SaveChangesAsync();
        Console.Write($"{auditCount}...");
    }
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($" ✅ {auditCount} audit log migrati");

// 10. Migra BILANCIO CONTABILE (TABELLA PIÙ GRANDE)
Console.Write("📋 Migrazione BilancioContabile... ");
int bilancioCount = 0;
foreach (var bilancio in liteDb.BilancioContabile.FindAll())
{
    sqlDb.BilancioContabile.Add(bilancio);
    bilancioCount++;
    
    if (bilancioCount % 1000 == 0)
    {
        await sqlDb.SaveChangesAsync();
        Console.Write($"{bilancioCount}...");
    }
}
await sqlDb.SaveChangesAsync();
Console.WriteLine($" ✅ {bilancioCount} bilanci migrati");

// ... CONTINUARE PER TUTTE LE ALTRE 14 TABELLE ...

Console.WriteLine("\n🎉 MIGRAZIONE COMPLETATA CON SUCCESSO!");
Console.WriteLine($"📊 Totale record migrati: {/* somma */}");
```

### STEP 3.2: Eseguire Migrazione Dati

**Prerequisiti:**
1. ✅ Database SQL Server creato e vuoto
2. ✅ Migrations EF Core applicate (tabelle create)
3. ✅ Backup LiteDB esistente fatto

**Comandi:**
```bash
cd tools/MigrazioneLiteDbToSql
dotnet run
```

**Output atteso:**
```
🔄 MIGRAZIONE DATI DA LITEDB A SQL SERVER
==========================================
✅ LiteDB aperto: C:\db_CGEASY\cgeasy.db
✅ SQL Server connesso
📋 Migrazione Utenti... ✅ 12 utenti migrati
📋 Migrazione Clienti... ✅ 347 clienti migrati
...
🎉 MIGRAZIONE COMPLETATA CON SUCCESSO!
```

### STEP 3.3: Verifica Migrazione Dati

**Script SQL verifica:**
```sql
USE CGEasy;
GO

-- Conta record per tabella
SELECT 'Utenti' AS Tabella, COUNT(*) AS Records FROM utenti
UNION ALL
SELECT 'Clienti', COUNT(*) FROM clienti
UNION ALL
SELECT 'TodoStudio', COUNT(*) FROM todoStudio
UNION ALL
SELECT 'BilancioContabile', COUNT(*) FROM bilancio_contabile
-- ... tutte le altre tabelle
ORDER BY Tabella;
```

---

## ═══════════════════════════════════════════════════════════
## FASE 4: TESTING COMPLETO
## ⏱️ Tempo Stimato: 6-8 ore
## ═══════════════════════════════════════════════════════════

### TEST 4.1: Test CRUD Base per ogni modulo

**Checklist test manuali:**
- [ ] Login utenti
- [ ] Creazione/modifica/eliminazione Clienti
- [ ] Creazione/modifica/eliminazione Professionisti
- [ ] Creazione TODO e assegnazione
- [ ] Import bilanci da Excel
- [ ] Visualizzazione statistiche bilanci
- [ ] Import circolari
- [ ] Modulo Controllo Gestione (banche)
- [ ] Gestione licenze

### TEST 4.2: Test Performance

```sql
-- Test query pesanti
SET STATISTICS TIME ON;

-- Query 1: Bilanci cliente
SELECT * FROM bilancio_contabile 
WHERE ClienteId = 1 AND Anno = 2024;

-- Query 2: TODO scaduti
SELECT * FROM todoStudio 
WHERE Stato != 'Completata' AND DataScadenza < GETDATE();

-- Query 3: Statistiche cliente
SELECT ClienteId, COUNT(*) AS Totale, AVG(ImportoDare) AS MediaDare
FROM bilancio_contabile
WHERE Anno = 2024
GROUP BY ClienteId;
```

### TEST 4.3: Test Multi-Utente

**Scenario test:**
1. Aprire app su PC 1 e PC 2 contemporaneamente
2. PC 1: Crea nuovo cliente
3. PC 2: Verifica cliente compare in lista (refresh)
4. PC 1 e PC 2: Modificare stesso cliente → Gestione conflitti?
5. PC 1: Import bilancio grande (10k righe)
6. PC 2: Deve rimanere responsive

### TEST 4.4: Test Backup/Restore SQL Server

```sql
-- BACKUP database
BACKUP DATABASE CGEasy 
TO DISK = 'C:\Backups\CGEasy_Test.bak'
WITH COMPRESSION, INIT;

-- RESTORE database (su database diverso per test)
RESTORE DATABASE CGEasy_Test
FROM DISK = 'C:\Backups\CGEasy_Test.bak'
WITH MOVE 'CGEasy_Data' TO 'C:\db_CGEASY\CGEasy_Test.mdf',
     MOVE 'CGEasy_Log' TO 'C:\db_CGEASY\CGEasy_Test_log.ldf';
```

---

## ═══════════════════════════════════════════════════════════
## FASE 5: DEPLOYMENT E INSTALLER
## ⏱️ Tempo Stimato: 2-3 ore
## ═══════════════════════════════════════════════════════════

### STEP 5.1: Aggiornamento Installer Inno Setup

**File**: `CGEasy_Installer.iss`

**Aggiungere sezione SQL Server:**
```innosetup
[Code]
function IsSqlServerInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  // Verifica se SQL Server Express è installato
  Result := RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server\SQLEXPRESS');
  
  if not Result then
  begin
    MsgBox('SQL Server Express non trovato!' + #13#10 + 
           'Installare SQL Server Express prima di continuare.', 
           mbError, MB_OK);
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := IsSqlServerInstalled();
end;
```

**Aggiungere file connection string:**
```innosetup
[Files]
Source: "connectionstring.txt"; DestDir: "{app}\Config"; Flags: ignoreversion

[Run]
; Crea database al primo avvio
Filename: "{app}\CGEasy.App.exe"; Parameters: "--init-database"; StatusMsg: "Inizializzazione database..."; Flags: runhidden
```

### STEP 5.2: Creare Script Inizializzazione Database

**File**: `scripts/init_database.sql`

```sql
USE master;
GO

-- Verifica se database esiste
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CGEasy')
BEGIN
    CREATE DATABASE CGEasy;
    PRINT 'Database CGEasy creato';
END
ELSE
BEGIN
    PRINT 'Database CGEasy già esistente';
END
GO

USE CGEasy;
GO

-- Verifica tabelle (se migrations non ancora applicate)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'utenti')
BEGIN
    PRINT 'ATTENZIONE: Tabelle non create. Eseguire migrations EF Core.';
END
GO
```

### STEP 5.3: Documentazione Installazione

**Creare file**: `INSTALLAZIONE_SQL_SERVER.md`

Contenuto:
```markdown
# INSTALLAZIONE CGEASY CON SQL SERVER

## PREREQUISITI
1. SQL Server Express 2022 installato
2. Windows 10/11
3. .NET 8 Desktop Runtime

## INSTALLAZIONE

### 1. Installare SQL Server Express
- Download: https://www.microsoft.com/sql-server/sql-server-downloads
- Scegliere "Express Edition"
- Abilitare "Mixed Mode Authentication"

### 2. Creare Database
- Aprire SQL Server Management Studio (SSMS)
- Connettere a `localhost\SQLEXPRESS`
- Eseguire script: `scripts/init_database.sql`

### 3. Configurare Connection String
- File: `C:\db_CGEASY\connectionstring.txt`
- Contenuto: `Server=localhost\SQLEXPRESS;Database=CGEasy;Trusted_Connection=True;`

### 4. Installare CGEasy
- Eseguire `CGEasy_Setup_v2.0.exe`
- Seguire wizard installazione

### 5. Primo Avvio
- L'applicazione creerà automaticamente le tabelle (migrations)
- Login con: `admin` / `123456`
```

---

## 📊 STIMA TEMPI DETTAGLIATA

| Fase | Task | Ore Min | Ore Max |
|------|------|---------|---------|
| **FASE 1** | **Setup SQL Server** | **1** | **2** |
| 1.1 | Installazione SQL Server Express | 0.5 | 1 |
| 1.2 | Installazione SSMS | 0.5 | 0.5 |
| 1.3 | Creazione database | 0.5 | 0.5 |
| **FASE 2** | **Migrazione Codice** | **16** | **20** |
| 2.1 | Installazione NuGet EF Core | 0.5 | 0.5 |
| 2.2 | Creazione CGEasyDbContext | 2 | 3 |
| 2.3 | Conversione 33 Models | 2 | 3 |
| 2.4 | OnModelCreating (indici, relazioni) | 2 | 3 |
| 2.5 | Riscrittura 20 Repository | 4 | 5 |
| 2.6 | Aggiornamento 13 Services | 2 | 2 |
| 2.7 | Aggiornamento 45 ViewModels | 2 | 3 |
| 2.8 | App.xaml.cs e DI | 0.5 | 0.5 |
| 2.9 | Creazione Migrations | 1 | 1 |
| **FASE 3** | **Migrazione Dati** | **4** | **6** |
| 3.1 | Tool migrazione dati | 2 | 3 |
| 3.2 | Esecuzione migrazione | 1 | 2 |
| 3.3 | Verifica dati | 1 | 1 |
| **FASE 4** | **Testing** | **6** | **8** |
| 4.1 | Test CRUD | 2 | 3 |
| 4.2 | Test Performance | 1 | 2 |
| 4.3 | Test Multi-utente | 2 | 2 |
| 4.4 | Test Backup/Restore | 1 | 1 |
| **FASE 5** | **Deployment** | **2** | **3** |
| 5.1 | Aggiornamento installer | 1 | 1.5 |
| 5.2 | Script init database | 0.5 | 0.5 |
| 5.3 | Documentazione | 0.5 | 1 |
| **TOTALE** | | **29** | **39** |

### 📅 CONVERSIONE IN GIORNI LAVORATIVI (8 ore/giorno)

| Scenario | Ore | Giorni |
|----------|-----|--------|
| Ottimistico | 29 | 3.5-4 |
| Realistico | 34 | 4-5 |
| Pessimistico | 39 | 5-6 |
| Con imprevisti (+20%) | 47 | 6-7 |
| Con testing esteso | 50+ | 7-10 |

---

## 🚨 CRITICITÀ E RISCHI

### CRITICITÀ ALTE ⚠️

1. **Async/Await su tutta l'app**
   - Impatto: 100% del codice UI
   - Rischio: Deadlock se mal gestito
   - Soluzione: ConfigureAwait(false) nei repository

2. **Gestione transazioni distribuite**
   - LiteDB: transazione implicita per operazione
   - SQL Server: transazioni esplicite necessarie
   - Soluzione: Usare `using var transaction = _context.Database.BeginTransaction()`

3. **Queries LiteDB-specific**
   - Alcune query LINQ funzionano solo su LiteDB
   - Esempio: `.Contains()` su liste in memoria vs SQL
   - Soluzione: Riscrivere query problematiche

4. **Performance bulk insert**
   - LiteDB: InsertBulk molto veloce
   - EF Core: Più lento (ogni insert = query SQL)
   - Soluzione: Usare `EFCore.BulkExtensions`

### CRITICITÀ MEDIE ⚠️

5. **Connection pooling**
   - Gestione automatica ma va configurata
   - Max pool size per multi-utente

6. **Migration schema esistente**
   - Se database già popolato, migrations complesse

7. **Backup durante uso**
   - SQL Server: backup online possibile
   - Va testato impatto performance

### CRITICITÀ BASSE ℹ️

8. **Dimensione database**
   - SQL Express: limite 10 GB
   - Monitorare crescita

9. **Licensing**
   - Express: gratis
   - Upgrade a Standard se serve

---

## ✅ VANTAGGI SQL SERVER VS LITEDB

| Caratteristica | LiteDB | SQL Server |
|----------------|---------|------------|
| **Multi-utenza** | Shared mode (limitato) | ✅ Nativo, robusto |
| **Transazioni** | File-lock | ✅ ACID completo |
| **Performance query complesse** | Limitato | ✅ Query optimizer |
| **Backup online** | No (file locked) | ✅ Sì |
| **Replication** | No | ✅ Sì (Standard+) |
| **Dimensione max** | Illimitata | 10 GB (Express) |
| **Monitoring** | Limitato | ✅ SSMS, DMV, profiler |
| **Sicurezza** | Password file | ✅ Utenti, ruoli, TDE |

---

## 🎯 RACCOMANDAZIONI FINALI

### PER PARTIRE SUBITO:

1. ✅ **Installa SQL Server Express oggi**
   - Non serve aspettare la migrazione codice
   - Familiarizzati con SSMS

2. ✅ **Fai backup completo LiteDB**
   ```powershell
   Copy-Item "C:\db_CGEASY\cgeasy.db" "C:\Backups\cgeasy_premigrazione_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
   ```

3. ✅ **Crea database vuoto SQL e testa connessione**
   - Verifica da altri PC sulla rete
   - Testa firewall/porte (1433)

4. ✅ **Parti da un modulo semplice** (consiglio: Professionisti)
   - Model → Repository → Service → ViewModel → View
   - Test completo prima di passare al prossimo

### ORDINE CONSIGLIATO MIGRAZIONE MODULI:

1. **Professionisti** (semplice, poche dipendenze)
2. **Clienti** (dipende da Professionisti)
3. **Tipo Pratica** (semplice)
4. **Utenti + Permessi** (critico ma piccolo)
5. **TODO Studio** (medio, usa Clienti/Professionisti)
6. **Licenze** (piccolo, indipendente)
7. **Argomenti + Circolari** (medio)
8. **Banche** (complesso)
9. **Bilanci** (PIÙ COMPLESSO - ultimo!)

---

## 📞 SUPPORTO E RISORSE

### Documentazione
- EF Core: https://learn.microsoft.com/ef/core/
- SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
- Migrations: https://learn.microsoft.com/ef/core/managing-schemas/migrations/

### Tool utili
- **SSMS**: SQL Server Management Studio
- **Azure Data Studio**: Alternativa moderna a SSMS
- **LINQPad**: Test query LINQ/EF Core
- **EF Core Power Tools**: Visual Studio extension

---

## 📋 CHECKLIST PRE-MIGRAZIONE

- [ ] Backup completo database LiteDB
- [ ] SQL Server Express installato e funzionante
- [ ] SSMS installato
- [ ] Database CGEasy creato
- [ ] Connection string testata da tutti i PC
- [ ] Firewall configurato (porta 1433)
- [ ] NuGet packages EF Core installati
- [ ] Primo modello convertito e testato

---

## 🚀 PROSSIMI PASSI

**DIMMI DA DOVE VUOI INIZIARE:**

1. Installazione SQL Server?
2. Conversione primo modello (Professionisti)?
3. Setup DbContext e Migrations?
4. Altro?

**Sono pronto ad aiutarti step-by-step in ogni fase!** 🎯

