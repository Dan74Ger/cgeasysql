# 🎯 PIANO COMPLETO MIGRAZIONE CGEASY A SQL SERVER

## 📊 STATO ATTUALE (19/11/2025 - 09:15)

### ✅ COMPLETATO:
1. ✅ Database SQL Server "CGEasy" creato
2. ✅ Tabelle migrate: `professionisti`, `utenti`, `user_permissions`
3. ✅ AuthService convertito a EF Core async
4. ✅ LoginViewModel funzionante
5. ✅ DashboardViewModel migrato
6. ✅ SistemaViewModel temporaneo creato
7. ✅ Login funzionante con credenziali SQL Server

### ⚠️ WARNING ATTIVI (da sistemare alla fine):
- **NU1701**: SkiaSharp.Views.WPF (pacchetto .NET Framework su .NET 8)
- **MVVMTK0039**: LoginViewModel usa `async void` invece di `async Task`

---

## 🔧 FASE 1: MIGRAZIONE MODULI (PRIORITÀ)

### 1️⃣ ClientiViewModel (PRIORITÀ MASSIMA)
**Passi:**
1. Migrare `Cliente.cs` model (LiteDB → EF Core)
2. Aggiungere `DbSet<Cliente>` a CGEasyDbContext
3. Configurare in OnModelCreating (tabella, colonne, indici)
4. Creare migration: `Add-Migration AddClienteTable`
5. Apply migration: `Update-Database`
6. Convertire `ClienteRepository` da sincrono a async
7. Aggiornare `ClientiViewModel` per usare async
8. Testare CRUD completo

**Tempo stimato**: 1-2 ore

---

### 2️⃣ ProfessionistiViewModel
**Nota**: Tabella già creata, serve solo convertire ViewModel
**Passi:**
1. Creare `ProfessionistaRepository` async
2. Convertire `ProfessionistiViewModel` a async
3. Testare CRUD

**Tempo stimato**: 30-45 minuti

---

### 3️⃣ TipoPraticaViewModel
**Passi:**
1. Migrare `TipoPratica.cs` model
2. Aggiungere `DbSet<TipoPratica>`
3. Migration + Update
4. Repository async
5. ViewModel async

**Tempo stimato**: 45 minuti

---

### 4️⃣ UtentiViewModel
**Nota**: Tabella già creata, funziona già (da verificare)
**Tempo stimato**: 15 minuti (test)

---

### 5️⃣ TodoStudioViewModel (se usato)
**Passi completi come sopra**
**Tempo stimato**: 1 ora

---

### 6️⃣ Moduli Bilanci (7 ViewModels)
Lista:
- BilancioContabileViewModel
- BilancioDettaglioViewModel
- BilancioDialogViewModel
- BilancioTemplateViewModel
- BilancioTemplateDettaglioViewModel
- ImportBilancioViewModel
- StatisticheBilanciViewModel + CE + SP

**Tempo stimato**: 6-8 ore (complessi, molte relazioni)

---

### 7️⃣ Moduli Banche (7 ViewModels)
Lista:
- GestioneBancheViewModel
- BancaDettaglioViewModel
- RiepilogoBancheViewModel
- IncassoDialogViewModel
- PagamentoDialogViewModel
- PagamentoMensileDialogViewModel
- AnticipoDialogViewModel

**Tempo stimato**: 5-7 ore

---

### 8️⃣ Moduli Circolari (4 ViewModels)
Lista:
- ArgomentiViewModel
- RicercaCircolariViewModel
- ImportaCircolareViewModel
- ModificaCircolareDialogViewModel

**Tempo stimato**: 3-4 ore

---

### 9️⃣ Altri Moduli (6 ViewModels)
Lista:
- AssociazioniMastriniViewModel
- AssociazioneMastrinoDialogViewModel
- IndiciDiBilancioViewModel
- ConfigurazioneIndiciViewModel
- IndicePersonalizzatoDialogViewModel
- GraficiViewModel
- LicenseManagerViewModel

**Tempo stimato**: 4-6 ore

---

## ⏱️ TEMPO TOTALE STIMATO MIGRAZIONE:
**22-30 ore di lavoro effettivo** (distribuibile in più giorni)

---

## 🐛 FASE 2: SISTEMAZIONE WARNING (FINALE)

### Warning NU1701 - SkiaSharp.Views.WPF

**Opzione 1: Aggiorna il pacchetto**
```powershell
dotnet add src/CGEasy.App package SkiaSharp.Views.WPF --version 3.116.1
```

**Opzione 2: Sopprimi warning** (se compatibile)
In `CGEasy.App.csproj`:
```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);NU1701</NoWarn>
</PropertyGroup>
```

---

### Warning MVVMTK0039 - async void

**File**: `src/CGEasy.App/ViewModels/LoginViewModel.cs`

**Problema**: 
```csharp
[RelayCommand]
private async void Login(object? parameter) // ❌ async void
```

**Soluzione**:
```csharp
[RelayCommand]
private async Task Login(object? parameter) // ✅ async Task
{
    try 
    {
        // ... codice esistente ...
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Errore: {ex.Message}";
    }
}
```

**Tempo stimato**: 5 minuti

---

## 📝 TEMPLATE MIGRAZIONE STANDARD

Per ogni nuovo modulo da migrare, segui questi passi:

### A) Model
```csharp
// PRIMA (LiteDB)
[BsonId]
[BsonField("nome")]

// DOPO (EF Core)
[Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
[Column("nome")]
[Required]
[MaxLength(100)]
```

### B) DbContext
```csharp
public DbSet<NomeModello> NomeTabella { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<NomeModello>(entity =>
    {
        entity.ToTable("nome_tabella");
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Campo);
        entity.Property(e => e.Campo).HasMaxLength(100).IsRequired();
    });
}
```

### C) Migration
```powershell
dotnet ef migrations add AddNomeTabellaTable -p src/CGEasy.Core -s src/CGEasy.App
dotnet ef database update -p src/CGEasy.Core -s src/CGEasy.App
```

### D) Repository
```csharp
// PRIMA (LiteDB sincrono)
public IEnumerable<T> GetAll() => _db.Collection.FindAll();

// DOPO (EF Core async)
public async Task<List<T>> GetAllAsync() 
    => await _context.Set<T>().ToListAsync();
```

### E) ViewModel
```csharp
// PRIMA (sincrono)
private void LoadData()
{
    Items = _repository.GetAll();
}

// DOPO (async)
private async Task LoadDataAsync()
{
    Items = await _repository.GetAllAsync();
}
```

---

## 🎯 MILESTONE

### Milestone 1: Login Funzionante ✅ COMPLETATA
- Database SQL Server
- Tabelle base (utenti, professionisti, permissions)
- Login/Logout funzionante

### Milestone 2: Moduli Base (prossima)
- Clienti ❌
- Professionisti ❌
- TipoPratica ❌
- Utenti ✅

### Milestone 3: Moduli Avanzati
- Bilanci
- Banche
- TODO Studio
- Circolari

### Milestone 4: Pulizia Finale
- Rimozione LiteDB references
- Fix tutti i warning
- Test completi
- Performance tuning

---

## 🚀 COMANDI RAPIDI

### Verifica database
```sql
sqlcmd -S "localhost\SQLEXPRESS" -d CGEasy -Q "SELECT name FROM sys.tables"
```

### Crea migration
```powershell
cd C:\CGEASY_sql\appcg_easy_projectsql
dotnet ef migrations add NomeMigration -p src/CGEasy.Core -s src/CGEasy.App
```

### Applica migration
```powershell
dotnet ef database update -p src/CGEasy.Core -s src/CGEasy.App
```

### Build e Run
```powershell
dotnet build src/CGEasy.App/CGEasy.App.csproj
dotnet run --project src/CGEasy.App/CGEasy.App.csproj
```

---

## 📖 RISORSE

- **pianosql.md**: Analisi completa migrazione
- **MODULI_DA_SISTEMARE.md**: Lista moduli da migrare
- **C:\db_CGEASY\connectionstring.txt**: Connection string SQL Server
- **tools/seed_default_users.sql**: Script utenti default

---

## ✅ CHECKLIST FINALE (prima di andare in produzione)

- [ ] Tutti i moduli migrati e testati
- [ ] Warning risolti
- [ ] Performance testate (query lente?)
- [ ] Backup SQL Server configurato
- [ ] Connection string sicura (non hardcoded)
- [ ] Log di errore implementati
- [ ] Documentazione aggiornata
- [ ] Test su dati reali
- [ ] Piano di rollback preparato

---

**Creato**: 19 Novembre 2025  
**Ultimo aggiornamento**: 19 Novembre 2025 09:15  
**Prossimo step**: Migrare ClientiViewModel

