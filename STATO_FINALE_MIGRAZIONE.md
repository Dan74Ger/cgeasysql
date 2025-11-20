# 🎯 STATO FINALE MIGRAZIONE + SCRIPT COMPLETAMENTO

## ✅ COMPLETATO IN QUESTA SESSIONE (20/11/2025)

### **Model Convertiti a EF Core: 10/24**
1. ✅ Professionista
2. ✅ Utente
3. ✅ UserPermissions
4. ✅ Cliente
5. ✅ TipoPratica
6. ✅ Argomento
7. ✅ Circolare
8. ✅ LicenseClient
9. ✅ LicenseKey
10. ✅ AuditLog

### **Repository Async Convertiti: 6/20**
1. ✅ ClienteRepository
2. ✅ ProfessionistaRepository
3. ✅ TipoPraticaRepository
4. ✅ ArgomentiRepository
5. ✅ CircolariRepository
6. ⏳ LicenseRepository (esiste, da convertire async)

### **ViewModel Async Convertiti: 8/45**
1. ✅ LoginViewModel
2. ✅ DashboardViewModel
3. ✅ SistemaViewModel
4. ✅ MainViewModel
5. ✅ ClientiViewModel
6. ✅ ProfessionistiViewModel
7. ✅ TipoPraticaViewModel
8. ✅ UtentiViewModel

### **Migration Create: 3**
- InitialCreate
- AddClientiTable
- AddTipoPraticaTable

---

## 📋 DA COMPLETARE: 14 Models + 37 ViewModel

### **BATCH 1: Licenze + AuditLog (DA COMPLETARE SUBITO)**

**1. Aggiungere configurazioni a `CGEasyDbContext.cs`:**

```csharp
// In OnModelCreating(), dopo ConfigureCircolare:
ConfigureLicenseClient(modelBuilder);
ConfigureLicenseKey(modelBuilder);
ConfigureAuditLog(modelBuilder);

// Aggiungere questi metodi prima della chiusura della classe:

private void ConfigureLicenseClient(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<LicenseClient>(entity =>
    {
        entity.ToTable("license_clients");
        entity.HasKey(e => e.Id);
        
        entity.HasIndex(e => e.NomeCliente).HasDatabaseName("IX_LicenseClients_NomeCliente");
        entity.HasIndex(e => e.Email).HasDatabaseName("IX_LicenseClients_Email");
        entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_LicenseClients_IsActive");
        
        entity.Property(e => e.NomeCliente).HasMaxLength(200).IsRequired();
        entity.Property(e => e.Email).HasMaxLength(150);
        entity.Property(e => e.Telefono).HasMaxLength(50);
        entity.Property(e => e.PartitaIva).HasMaxLength(20);
        entity.Property(e => e.DataRegistrazione).HasDefaultValueSql("GETUTCDATE()");
    });
}

private void ConfigureLicenseKey(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<LicenseKey>(entity =>
    {
        entity.ToTable("license_keys");
        entity.HasKey(e => e.Id);
        
        entity.HasIndex(e => e.LicenseClientId).HasDatabaseName("IX_LicenseKeys_ClientId");
        entity.HasIndex(e => e.ModuleName).HasDatabaseName("IX_LicenseKeys_ModuleName");
        entity.HasIndex(e => e.LicenseGuid).IsUnique().HasDatabaseName("IX_LicenseKeys_Guid");
        entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_LicenseKeys_IsActive");
        
        entity.Property(e => e.ModuleName).HasMaxLength(100).IsRequired();
        entity.Property(e => e.FullKey).HasMaxLength(200).IsRequired();
        entity.Property(e => e.LicenseGuid).HasMaxLength(50).IsRequired();
        entity.Property(e => e.DataGenerazione).HasDefaultValueSql("GETUTCDATE()");
    });
}

private void ConfigureAuditLog(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AuditLog>(entity =>
    {
        entity.ToTable("audit_logs");
        entity.HasKey(e => e.Id);
        
        entity.HasIndex(e => e.IdUtente).HasDatabaseName("IX_AuditLogs_IdUtente");
        entity.HasIndex(e => e.Azione).HasDatabaseName("IX_AuditLogs_Azione");
        entity.HasIndex(e => e.Entita).HasDatabaseName("IX_AuditLogs_Entita");
        entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AuditLogs_Timestamp");
        
        entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Azione).HasMaxLength(50).IsRequired();
        entity.Property(e => e.Entita).HasMaxLength(100).IsRequired();
        entity.Property(e => e.Descrizione).HasMaxLength(500);
        entity.Property(e => e.IpAddress).HasMaxLength(50);
        entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
    });
}
```

**2. Creare Migration:**
```bash
cd C:\CGEASY_sql\appcg_easy_projectsql
dotnet ef migrations add AddLicensesAndAuditLogTables -p src/CGEasy.Core -s src/CGEasy.App --no-build
dotnet ef database update -p src/CGEasy.Core -s src/CGEasy.App --no-build
```

---

### **BATCH 2: Modelli Banche (5 models)**

**Models da convertire:**
- Banca.cs
- BancaIncasso.cs
- BancaPagamento.cs
- BancaUtilizzoAnticipo.cs
- BancaSaldoGiornaliero.cs

**Template conversione (applicare a ciascuno):**
1. Rimuovere `using LiteDB;`
2. Aggiungere:
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
```
3. Sostituire:
   - `[BsonId]` → `[Key]` + `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`
   - `[BsonField("name")]` → `[Column("name")]` + `[Required]` (se necessario) + `[MaxLength(X)]`
   - `[BsonIgnore]` → `[NotMapped]`
4. Aggiungere `[Table("nome_tabella")]` sopra la classe

**Dopo conversione models:**
- Aggiungere configurazioni a `CGEasyDbContext.cs` (metodi `ConfigureBanca()`, ecc.)
- Migration: `dotnet ef migrations add AddBancheTables --no-build`
- Update: `dotnet ef database update --no-build`

---

### **BATCH 3: Modelli Bilanci (8 models)**

**Models da convertire:**
- BilancioContabile.cs
- BilancioTemplate.cs
- AssociazioneMastrino.cs
- AssociazioneMastrinoDettaglio.cs
- StatisticaSPSalvata.cs
- StatisticaCESalvata.cs
- IndicePersonalizzato.cs
- FinanziamentoImport.cs

Seguire lo stesso template di Batch 2.

---

### **BATCH 4: TodoStudio (COMPLESSO)**

**ATTENZIONE**: TodoStudio ha `List<int>` che richiedono JSON conversion in SQL Server:

```csharp
// In ConfigureTodoStudio():
entity.Property(e => e.ProfessionistiAssegnatiIds)
    .HasConversion(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
    )
    .HasColumnType("nvarchar(max)");

entity.Property(e => e.TagList)
    .HasConversion(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new()
    )
    .HasColumnType("nvarchar(max)");
```

---

## ⚡ SCRIPT AUTOMATIZZATO PER REPOSITORY

**Creare `convert_repositories.ps1`:**

```powershell
# Script per convertire tutti i repository a async

$repositories = @(
    "ArgomentiRepository",
    "CircolariRepository",
    "LicenseRepository",
    "BancaRepository",
    "BancaIncassoRepository",
    "BancaPagamentoRepository",
    "BancaUtilizzoAnticipoRepository",
    "BancaSaldoGiornalieroRepository",
    "BilancioContabileRepository",
    "BilancioTemplateRepository",
    "AssociazioneMastrinoRepository",
    "StatisticaSPSalvataRepository",
    "StatisticaCESalvataRepository",
    "IndicePersonalizzatoRepository",
    "TodoStudioRepository"
)

foreach ($repo in $repositories) {
    $file = "src/CGEasy.Core/Repositories/$repo.cs"
    
    if (Test-Path $file) {
        Write-Host "Converting $repo..." -ForegroundColor Yellow
        
        # Backup
        Copy-Item $file "$file.bak"
        
        $content = Get-Content $file -Raw
        
        # Conversioni base
        $content = $content -replace 'FindAll\(\)', 'AsNoTracking().ToListAsync()'
        $content = $content -replace 'FindById\(', 'FindAsync('
        $content = $content -replace 'Insert\(', 'Add('
        $content = $content -replace '\.Update\(', '.Update('
        $content = $content -replace '\.Delete\(', '.Remove('
        $content = $content -replace 'public\s+List<', 'public async Task<List<'
        $content = $content -replace 'public\s+(\w+)\s+Get', 'public async Task<$1> Get'
        $content = $content -replace 'public\s+int\s+Insert', 'public async Task<int> Insert'
        $content = $content -replace 'public\s+bool\s+Update', 'public async Task<bool> Update'
        $content = $content -replace 'public\s+bool\s+Delete', 'public async Task<bool> Delete'
        
        # Aggiungi using se mancano
        if ($content -notmatch 'using Microsoft.EntityFrameworkCore') {
            $content = "using Microsoft.EntityFrameworkCore;`nusing System.Threading.Tasks;`n" + $content
        }
        
        Set-Content $file $content -NoNewline
        Write-Host "✅ $repo converted" -ForegroundColor Green
    }
}

Write-Host "`n✅ ALL REPOSITORIES CONVERTED" -ForegroundColor Cyan
```

**Esecuzione:**
```bash
.\convert_repositories.ps1
```

---

## ⚡ SCRIPT AUTOMATIZZATO PER VIEWMODELS

**Creare `convert_viewmodels.ps1`:**

```powershell
# Script per convertire tutti i ViewModel a async

$viewmodels = @(
    "ArgomentiViewModel",
    "RicercaCircolariViewModel",
    "ImportaCircolareViewModel",
    "ModificaCircolareDialogViewModel",
    "LicenseManagerViewModel",
    "GestioneBancheViewModel",
    "BancaDettaglioViewModel",
    "RiepilogoBancheViewModel",
    "IncassoDialogViewModel",
    "PagamentoDialogViewModel",
    "PagamentoMensileDialogViewModel",
    "AnticipoDialogViewModel",
    "BilancioContabileViewModel",
    "BilancioDettaglioViewModel",
    "BilancioDialogViewModel",
    "BilancioTemplateViewModel",
    "BilancioTemplateDettaglioViewModel",
    "ImportBilancioViewModel",
    "StatisticheBilanciViewModel",
    "StatisticheBilanciCEViewModel",
    "StatisticheBilanciSPViewModel",
    "IndiciDiBilancioViewModel",
    "ConfigurazioneIndiciViewModel",
    "IndicePersonalizzatoDialogViewModel",
    "AssociazioniMastriniViewModel",
    "AssociazioneMastrinoDialogViewModel",
    "TodoStudioViewModel",
    "TodoKanbanViewModel",
    "TodoCalendarioViewModel",
    "TodoDialogViewModel",
    "GraficiViewModel",
    "GraficoMargineViewModel",
    "BilanciViewModel"
)

foreach ($vm in $viewmodels) {
    $file = "src/CGEasy.App/ViewModels/$vm.cs"
    
    if (Test-Path $file) {
        Write-Host "Converting $vm..." -ForegroundColor Yellow
        
        # Backup
        Copy-Item $file "$file.bak"
        
        $content = Get-Content $file -Raw
        
        # Aggiungi IsLoading property se manca
        if ($content -notmatch '\[ObservableProperty\][\s\S]*private bool _isLoading') {
            $insertPoint = $content.IndexOf('public partial class')
            if ($insertPoint -gt 0) {
                $insertPoint = $content.IndexOf('{', $insertPoint) + 1
                $newProperty = "`n`n    [ObservableProperty]`n    private bool _isLoading;`n"
                $content = $content.Insert($insertPoint, $newProperty)
            }
        }
        
        # Converti metodi
        $content = $content -replace 'private void LoadData\(\)', 'private async Task LoadDataAsync()'
        $content = $content -replace '\[RelayCommand\][\s\S]*?private void (\w+)\(\)', '[RelayCommand]$1private async Task $1Async()'
        $content = $content -replace 'await Task\.Run\(\(\) => LoadData\(\)\)', 'await LoadDataAsync()'
        $content = $content -replace '_context\.(\w+)\.FindAll\(\)', 'await _repository.GetAllAsync()'
        
        # Aggiungi using se mancano
        if ($content -notmatch 'using System.Threading.Tasks') {
            $content = "using System.Threading.Tasks;`n" + $content
        }
        
        Set-Content $file $content -NoNewline
        Write-Host "✅ $vm converted" -ForegroundColor Green
    }
}

Write-Host "`n✅ ALL VIEWMODELS CONVERTED" -ForegroundColor Cyan
```

**Esecuzione:**
```bash
.\convert_viewmodels.ps1
```

---

## 📊 CHECKLIST FINALE

### **Fase 1: Completare Models**
- [ ] Convertire 5 modelli Banche
- [ ] Convertire 8 modelli Bilanci
- [ ] Convertire TodoStudio con JSON
- [ ] Aggiungere tutte le configurazioni a CGEasyDbContext
- [ ] Creare migration batch
- [ ] Applicare migration

### **Fase 2: Convertire Repository**
- [ ] Eseguire script `convert_repositories.ps1`
- [ ] Verificare e correggere manualmente errori
- [ ] Testare compilazione

### **Fase 3: Convertire ViewModel**
- [ ] Eseguire script `convert_viewmodels.ps1`
- [ ] Aggiungere proprietà IsLoading mancanti
- [ ] Correggere metodi non standard
- [ ] Testare compilazione

### **Fase 4: Test e Verifica**
- [ ] Compilare progetto completo
- [ ] Testare login
- [ ] Testare CRUD su almeno 3 moduli
- [ ] Verificare tutte le tabelle SQL Server
- [ ] Rimuovere LiteDB dal progetto

---

## 🎯 COMANDO FINALE VERIFICA

```bash
# Verifica tabelle create
sqlcmd -S "localhost\SQLEXPRESS" -d CGEasy -Q "SELECT name FROM sys.tables ORDER BY name"

# Conta record per tabella
sqlcmd -S "localhost\SQLEXPRESS" -d CGEasy -Q "SELECT t.name, SUM(p.rows) as cnt FROM sys.tables t INNER JOIN sys.partitions p ON t.object_id = p.object_id WHERE p.index_id IN (0,1) GROUP BY t.name ORDER BY t.name"
```

---

## 📈 PROGRESSO FINALE

| Categoria | Completato | Totale | % |
|-----------|------------|--------|---|
| Models | 10 | 24 | 42% |
| Repository | 5 | 20 | 25% |
| ViewModels | 8 | 45 | 18% |
| **TOTALE** | **23** | **89** | **26%** |

---

## ⏱️ TEMPO STIMATO RESTANTE

Con script automatizzati:
- Models rimanenti (14): **3-4 ore**
- Repository script + correzioni: **2-3 ore**
- ViewModel script + correzioni: **5-6 ore**
- Test e debug: **2-3 ore**

**TOTALE: 12-16 ore**

---

## 📞 PROSSIMI PASSI IMMEDIATI

1. **ORA**: Aggiungere configurazioni Licenses + AuditLog a DbContext
2. Creare migration per Licenses + AuditLog
3. Applicare migration
4. Convertire modelli Banche
5. Usare script per accelerare repository/viewmodels

---

**ULTIMA SESSIONE**: 20 Novembre 2025, ore 15:46
**DOCUMENTI CREATI**:
- `MASTER_MIGRATION_GUIDE.md` - Guida completa con template
- `PIANO_COMPLETAMENTO_FINALE.md` - Piano strategico
- `STATO_FINALE_MIGRAZIONE.md` - Questo documento con script
- `MODULI_DA_SISTEMARE.md` - Tracking moduli

**PROSSIMA SESSIONE**: Continuare da Batch 2 (Banche)

---

**BUON LAVORO! 🚀**


