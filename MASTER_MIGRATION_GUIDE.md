# 🎯 MASTER GUIDE: Completamento Migrazione SQL Server

## 📊 STATO FINALE SESSIONE (20/11/2025)

### ✅ COMPLETATO: 8 ViewModel + Repository + Model

1. **LoginViewModel** ✅
2. **DashboardViewModel** ✅
3. **SistemaViewModel** ✅
4. **MainViewModel** ✅
5. **ClientiViewModel** ✅ (Model + Repo + VM + Migration)
6. **ProfessionistiViewModel** ✅ (Model + Repo + VM)
7. **TipoPraticaViewModel** ✅ (Model + Repo + VM + Migration)
8. **UtentiViewModel** ✅ (VM async)

### 🔄 PARZIALMENTE COMPLETATO:

9. **Argomenti** (Model ✅, Repo ✅, VM ❌, Config ❌, Migration ❌)
10. **Circolari** (Model ✅, Repo ❌, VM ❌, Config ❌, Migration ❌)

### ❌ DA FARE: 35 ViewModel Rimanenti

---

## 📋 TEMPLATE COMPLETI PER MIGRAZIONE

### **TEMPLATE 1: Conversione Model (LiteDB → EF Core)**

```csharp
// PRIMA (LiteDB)
using LiteDB;

public class MyModel
{
    [BsonId]
    public int Id { get; set; }
    
    [BsonField("nome")]
    public string Nome { get; set; } = string.Empty;
    
    [BsonField("attivo")]
    public bool Attivo { get; set; }
    
    [BsonIgnore]
    public string DisplayName => Nome;
}

// DOPO (EF Core)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("my_models")]
public class MyModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("nome")]
    [Required]
    [MaxLength(200)]
    public string Nome { get; set; } = string.Empty;
    
    [Column("attivo")]
    public bool Attivo { get; set; }
    
    [NotMapped]
    public string DisplayName => Nome;
}
```

### **TEMPLATE 2: Repository Async**

```csharp
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Repositories
{
    public class MyModelRepository
    {
        private readonly CGEasyDbContext _context;

        public MyModelRepository(CGEasyDbContext context)
        {
            _context = context;
        }

        public async Task<List<MyModel>> GetAllAsync()
        {
            return await _context.MyModels
                .AsNoTracking()
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public async Task<MyModel?> GetByIdAsync(int id)
        {
            return await _context.MyModels.FindAsync(id);
        }

        public async Task<List<MyModel>> FindAsync(Expression<Func<MyModel, bool>> predicate)
        {
            return await _context.MyModels.Where(predicate).ToListAsync();
        }

        public async Task<int> InsertAsync(MyModel entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            _context.MyModels.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(MyModel entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.MyModels.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            _context.MyModels.Remove(entity);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
```

### **TEMPLATE 3: ViewModel Async**

```csharp
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CGEasy.App.ViewModels;

public partial class MyViewModel : ObservableObject
{
    private readonly MyModelRepository _repository;

    [ObservableProperty]
    private ObservableCollection<MyModel> _items;

    [ObservableProperty]
    private MyModel? _selectedItem;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public MyViewModel(MyModelRepository repository)
    {
        _repository = repository;
        _items = new ObservableCollection<MyModel>();
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var items = await _repository.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                items = items.Where(x => x.Nome.Contains(SearchText)).ToList();
            }
            
            Items = new ObservableCollection<MyModel>(items);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore: {ex.Message}", "Errore", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadDataAsync();

    partial void OnSearchTextChanged(string value) => _ = LoadDataAsync();

    [RelayCommand]
    private async Task NewItemAsync()
    {
        // Dialog logic
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task EditItemAsync()
    {
        if (SelectedItem == null) return;
        // Dialog logic
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteItemAsync()
    {
        if (SelectedItem == null) return;
        
        var result = MessageBox.Show("Confermi eliminazione?", 
            "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
        if (result == MessageBoxResult.Yes)
        {
            await _repository.DeleteAsync(SelectedItem.Id);
            await LoadDataAsync();
        }
    }
}
```

### **TEMPLATE 4: Configurazione OnModelCreating**

```csharp
// In CGEasyDbContext.cs - metodo OnModelCreating
ConfigureMyModel(modelBuilder);

// Aggiungere metodo privato:
private void ConfigureMyModel(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<MyModel>(entity =>
    {
        entity.ToTable("my_models");
        entity.HasKey(e => e.Id);
        
        // Indici
        entity.HasIndex(e => e.Nome).HasDatabaseName("IX_MyModels_Nome");
        entity.HasIndex(e => e.Attivo).HasDatabaseName("IX_MyModels_Attivo");
        
        // Proprietà
        entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
    });
}
```

---

## 🚀 PROCEDURA COMPLETA PER OGNI MODULO

### **Per ogni ViewModel da migrare:**

**1. Converti Model**
- Apri `src/CGEasy.Core/Models/NomeModel.cs`
- Sostituisci `using LiteDB` → `using System.ComponentModel.DataAnnotations`
- Converti attributi con Template 1
- Salva

**2. Aggiungi Configurazione DbContext**
- Apri `src/CGEasy.Core/Data/CGEasyDbContext.cs`
- In `OnModelCreating()` aggiungi: `ConfigureNomeModel(modelBuilder);`
- Aggiungi metodo privato con Template 4
- Salva

**3. Crea/Aggiorna Repository**
- Se esiste: apri `src/CGEasy.Core/Repositories/NomeModelRepository.cs`
- Sostituisci metodi sincroni con async usando Template 2
- Se non esiste: crea nuovo file con Template 2
- Salva

**4. Converti ViewModel**
- Apri `src/CGEasy.App/ViewModels/NomeViewModel.cs`
- Converti tutti i metodi a async usando Template 3
- Aggiungi `IsLoading` property
- Sostituisci `LoadData()` → `LoadDataAsync()`
- Converti tutti i `[RelayCommand]` a async Task
- Salva

**5. Crea Migration** (ogni 5-10 modelli)
```bash
cd C:\CGEASY_sql\appcg_easy_projectsql
dotnet ef migrations add AddNomeModelTable -p src/CGEasy.Core -s src/CGEasy.App --no-build
dotnet ef database update -p src/CGEasy.Core -s src/CGEasy.App --no-build
```

---

## 📋 ORDINE ESECUZIONE COMPLETO (35 rimanenti)

### **BATCH 1: Circolari (1 modulo)**
- [ ] CircolariRepository async
- [ ] ArgomentiViewModel async
- [ ] RicercaCircolariViewModel async
- [ ] ImportaCircolareViewModel async
- [ ] ModificaCircolareDialogViewModel async
- Migration: `AddCircolariModule`

### **BATCH 2: Licenze (1 modulo)**
Models da convertire:
- [ ] LicenseClient.cs
- [ ] LicenseKey.cs

Repository:
- [ ] LicenseRepository async (già esiste, convertire)

ViewModel:
- [ ] LicenseManagerViewModel async

Migration: `AddLicensesTables`

### **BATCH 3: AuditLog (ausiliario)**
- [ ] AuditLog.cs model
- [ ] Nessun repository (usato da service)
- Migration: `AddAuditLogTable`

### **BATCH 4: Banche (7 ViewModel)**
Models da convertire:
- [ ] Banca.cs
- [ ] BancaIncasso.cs
- [ ] BancaPagamento.cs
- [ ] BancaUtilizzoAnticipo.cs
- [ ] BancaSaldoGiornaliero.cs
- [ ] FinanziamentoImport.cs

Repository async:
- [ ] BancaRepository
- [ ] BancaIncassoRepository
- [ ] BancaPagamentoRepository
- [ ] BancaUtilizzoAnticipoRepository
- [ ] BancaSaldoGiornalieroRepository
- [ ] FinanziamentoImportRepository

ViewModel async:
- [ ] GestioneBancheViewModel
- [ ] BancaDettaglioViewModel
- [ ] RiepilogoBancheViewModel
- [ ] IncassoDialogViewModel
- [ ] PagamentoDialogViewModel
- [ ] PagamentoMensileDialogViewModel
- [ ] AnticipoDialogViewModel

Migration: `AddBancheModule`

### **BATCH 5: Bilanci (10+ ViewModel - COMPLESSO)**
Models da convertire:
- [ ] BilancioContabile.cs
- [ ] BilancioTemplate.cs
- [ ] AssociazioneMastrino.cs
- [ ] AssociazioneMastrinoDettaglio.cs
- [ ] StatisticaSPSalvata.cs
- [ ] StatisticaCESalvata.cs
- [ ] IndicePersonalizzato.cs
- [ ] BilancioGruppo.cs (se esiste)
- [ ] VoceBilancio.cs (se esiste)

Repository async:
- [ ] BilancioContabileRepository
- [ ] BilancioTemplateRepository
- [ ] AssociazioneMastrinoRepository
- [ ] StatisticaSPSalvataRepository
- [ ] StatisticaCESalvataRepository
- [ ] IndicePersonalizzatoRepository

ViewModel async:
- [ ] BilancioContabileViewModel
- [ ] BilancioDettaglioViewModel
- [ ] BilancioDialogViewModel
- [ ] BilancioTemplateViewModel
- [ ] BilancioTemplateDettaglioViewModel
- [ ] ImportBilancioViewModel
- [ ] StatisticheBilanciViewModel
- [ ] StatisticheBilanciCEViewModel
- [ ] StatisticheBilanciSPViewModel
- [ ] IndiciDiBilancioViewModel
- [ ] ConfigurazioneIndiciViewModel
- [ ] IndicePersonalizzatoDialogViewModel
- [ ] AssociazioniMastriniViewModel
- [ ] AssociazioneMastrinoDialogViewModel
- [ ] BilanciViewModel (se diverso)

Migration: `AddBilanciModule`

### **BATCH 6: TodoStudio (COMPLESSO con JSON)**
Model:
- [ ] TodoStudio.cs - **SPECIALE** usa List<> → JSON conversion

Configurazione speciale:
```csharp
entity.Property(e => e.ProfessionistiAssegnatiIds)
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
        v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions)null) ?? new()
    )
    .HasColumnType("nvarchar(max)");
```

Repository:
- [ ] TodoStudioRepository async

ViewModel:
- [ ] TodoStudioViewModel async
- [ ] TodoKanbanViewModel async
- [ ] TodoCalendarioViewModel async
- [ ] TodoDialogViewModel async

Migration: `AddTodoStudioTable`

### **BATCH 7: Vari**
- [ ] GraficiViewModel async
- [ ] GraficoMargineViewModel async (se esiste)

---

## ⚡ COMANDI VELOCI

### **Build & Test**
```bash
cd C:\CGEASY_sql\appcg_easy_projectsql
dotnet build src/CGEasy.App/CGEasy.App.csproj 2>&1 | Select-String -Pattern "error"
```

### **Migration**
```bash
# Crea migration
dotnet ef migrations add AddNomeTable -p src/CGEasy.Core -s src/CGEasy.App --no-build

# Applica migration
dotnet ef database update -p src/CGEasy.Core -s src/CGEasy.App --no-build

# Lista migrations
dotnet ef migrations list -p src/CGEasy.Core -s src/CGEasy.App
```

### **Database Check**
```bash
# Verifica tabelle create
sqlcmd -S "localhost\SQLEXPRESS" -d CGEasy -Q "SELECT name FROM sys.tables ORDER BY name"

# Conta record tabella
sqlcmd -S "localhost\SQLEXPRESS" -d CGEasy -Q "SELECT COUNT(*) FROM nome_tabella"
```

---

## 🎯 CHECKLIST FINALE

### Prima di ogni migration:
- [ ] Tutti i model convertiti a EF Core
- [ ] Tutte le configurazioni aggiunte a OnModelCreating
- [ ] Build senza errori
- [ ] Backup database (opzionale)

### Dopo ogni migration:
- [ ] Verificare tabella creata in SQL Server
- [ ] Testare repository async
- [ ] Testare ViewModel async
- [ ] Aggiornare MODULI_DA_SISTEMARE.md

### Al completamento:
- [ ] Tutti i 45 ViewModel migrati
- [ ] Tutte le 24 tabelle create
- [ ] Build senza errori
- [ ] Test login funzionante
- [ ] Test CRUD moduli principali
- [ ] Rimuovere LiteDB dal progetto
- [ ] Aggiornare documentazione

---

## 📊 TRACKING PROGRESSO

Aggiorna questo dopo ogni batch:

```markdown
## PROGRESSO MIGRAZIONE

| Batch | ViewModel | Completato | Data |
|-------|-----------|------------|------|
| Base  | 8/45      | ✅         | 20/11 |
| Circolari | 5/45  | ⏳         | - |
| Licenze | 1/45    | ⏳         | - |
| AuditLog | 0/45   | ⏳         | - |
| Banche | 7/45     | ⏳         | - |
| Bilanci | 14/45   | ⏳         | - |
| TodoStudio | 4/45 | ⏳         | - |
| Vari | 2/45       | ⏳         | - |

TOTALE: 8/45 (17.8%)
```

---

## 💡 TIPS & TROUBLESHOOTING

### Errore "FindAll() not found"
→ Stai ancora usando metodi LiteDB, converti a EF Core async

### Errore "Connection string not found"
→ Verifica file: `C:\db_CGEASY\connectionstring.txt`

### Migration fallisce
→ Verifica OnModelCreating() configurazioni complete

### ViewModel non carica dati
→ Verifica repository iniettato correttamente in App.xaml.cs

### Build lento
→ Usa `--no-incremental` per rebuild completo

---

## 📞 SUPPORTO

File di riferimento creati:
- `STATO_MIGRAZIONE_20NOV.md` - Stato sessione dettagliato
- `PIANO_COMPLETAMENTO_FINALE.md` - Piano strategico
- `MODULI_DA_SISTEMARE.md` - Lista moduli
- **`MASTER_MIGRATION_GUIDE.md`** - Questo documento

**Prossima sessione**: Continuare da BATCH 1 (Circolari)

**Tempo stimato rimanente**: 25-30 ore

---

BUON LAVORO! 🚀


