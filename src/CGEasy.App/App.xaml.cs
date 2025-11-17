using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Services;
using CGEasy.Core.Repositories;
using CGEasy.App.Views;
using CGEasy.App.ViewModels;

namespace CGEasy.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// ServiceProvider pubblico per accesso ai servizi registrati
    /// </summary>
    public ServiceProvider? Services => _serviceProvider;

    public App()
    {
        // Configura Dependency Injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // === CORE SERVICES ===
        
        // DatabaseConfigService è statico, non necessita registrazione
        // Ma lo aggiungiamo per completezza e per future estensioni
        
        // LiteDbContext (Singleton - unica istanza per tutta l'app)
        services.AddSingleton<LiteDbContext>(provider =>
        {
            var context = new LiteDbContext();
            context.MarkAsSingleton(); // Marca come Singleton per non chiudere mai il DB
            return context;
        });

        // AuthService (Singleton)
        services.AddSingleton<AuthService>();

        // AuditLogService (Singleton)
        services.AddSingleton<AuditLogService>();

        // === REPOSITORIES ===
        services.AddSingleton<ClienteRepository>();
        services.AddSingleton<ProfessionistaRepository>();
        services.AddSingleton<TipoPraticaRepository>();
        services.AddSingleton<LicenseRepository>();  // ✅ Repository licenze

        // === VIEWS ===
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();  // ✅ Transient = nuova istanza ogni volta
        services.AddTransient<DashboardView>();
        services.AddTransient<ClientiView>();
        services.AddTransient<ProfessionistiView>();
        services.AddTransient<TipoPraticaView>();
        services.AddTransient<UtentiView>();
        services.AddTransient<SistemaView>();
        services.AddTransient<ImportBilancioView>();

        // === VIEWMODELS ===
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();  // ✅ Transient = nuova istanza ogni volta
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ClientiViewModel>();
        services.AddTransient<ProfessionistiViewModel>();
        services.AddTransient<TipoPraticaViewModel>();
        services.AddTransient<UtentiViewModel>();
        services.AddTransient<SistemaViewModel>();
        services.AddTransient<ImportBilancioViewModel>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Verifica se c'è un ripristino pendente
        CheckPendingRestore();

        // Verifica se c'è una crittografia/decrittografia pendente
        CheckPendingEncryption();

        // Inizializza database e verifica/crea admin di default
        InitializeDatabase();

        // Mostra LoginWindow invece di MainWindow
        var loginWindow = _serviceProvider?.GetRequiredService<LoginWindow>();
        if (loginWindow != null)
        {
            loginWindow.Show();
        }
    }

    private void CheckPendingRestore()
    {
        try
        {
            var dbPath = LiteDbContext.DefaultDatabasePath;
            var restoreInfoFile = Path.Combine(Path.GetDirectoryName(dbPath)!, "restore_pending.txt");

            if (File.Exists(restoreInfoFile))
            {
                var backupPath = File.ReadAllText(restoreInfoFile).Trim();

                if (File.Exists(backupPath))
                {
                    // Ripristina il database
                    File.Copy(backupPath, dbPath, true);

                    // Elimina il file di controllo
                    File.Delete(restoreInfoFile);

                    MessageBox.Show(
                        $"Database ripristinato con successo da:\n{Path.GetFileName(backupPath)}",
                        "Ripristino Completato",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    // File di backup non trovato, elimina comunque il file di controllo
                    File.Delete(restoreInfoFile);
                    MessageBox.Show(
                        "File di backup non trovato. Ripristino annullato.",
                        "Errore Ripristino",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante il ripristino automatico:\n{ex.Message}",
                "Errore Ripristino",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void CheckPendingEncryption()
    {
        try
        {
            var dbPath = LiteDbContext.DefaultDatabasePath;
            var encryptionFile = Path.Combine(Path.GetDirectoryName(dbPath)!, "encryption_pending.json");

            if (!File.Exists(encryptionFile))
                return;

            var json = File.ReadAllText(encryptionFile);
            var operation = JsonSerializer.Deserialize<EncryptionOperation>(json);

            if (operation == null || string.IsNullOrEmpty(operation.Operation))
            {
                File.Delete(encryptionFile);
                return;
            }

            if (operation.Operation == "encrypt")
            {
                // Cripta database SEMPRE con password master
                // La password utente serve solo per verificare accesso
                var masterPassword = DatabaseEncryptionService.GetMasterPassword();
                var tempDbPath = dbPath + ".encrypted.tmp";

                // Copia database con PASSWORD MASTER
                using (var sourceDb = new LiteDbContext(dbPath, null))
                using (var targetDb = new LiteDbContext(tempDbPath, masterPassword))
                {
                    // ⭐ Copia TUTTE le 23 collezioni
                    foreach (var u in sourceDb.Utenti.FindAll())
                        targetDb.Utenti.Insert(u);
                    foreach (var p in sourceDb.UserPermissions.FindAll())
                        targetDb.UserPermissions.Insert(p);
                    foreach (var c in sourceDb.Clienti.FindAll())
                        targetDb.Clienti.Insert(c);
                    foreach (var pr in sourceDb.Professionisti.FindAll())
                        targetDb.Professionisti.Insert(pr);
                    foreach (var tp in sourceDb.TipoPratiche.FindAll())
                        targetDb.TipoPratiche.Insert(tp);
                    foreach (var l in sourceDb.AuditLogs.FindAll())
                        targetDb.AuditLogs.Insert(l);
                    
                    // Todo Studio
                    foreach (var todo in sourceDb.TodoStudio.FindAll())
                        targetDb.TodoStudio.Insert(todo);
                    
                    // Licenze
                    foreach (var lc in sourceDb.LicenseClients.FindAll())
                        targetDb.LicenseClients.Insert(lc);
                    foreach (var lk in sourceDb.LicenseKeys.FindAll())
                        targetDb.LicenseKeys.Insert(lk);
                    
                    // Bilanci
                    foreach (var bc in sourceDb.BilancioContabile.FindAll())
                        targetDb.BilancioContabile.Insert(bc);
                    foreach (var bt in sourceDb.BilancioTemplate.FindAll())
                        targetDb.BilancioTemplate.Insert(bt);
                    
                    // Banche
                    foreach (var b in sourceDb.Banche.FindAll())
                        targetDb.Banche.Insert(b);
                    foreach (var bi in sourceDb.BancaIncassi.FindAll())
                        targetDb.BancaIncassi.Insert(bi);
                    foreach (var bp in sourceDb.BancaPagamenti.FindAll())
                        targetDb.BancaPagamenti.Insert(bp);
                    foreach (var bu in sourceDb.BancaUtilizzoAnticipo.FindAll())
                        targetDb.BancaUtilizzoAnticipo.Insert(bu);
                    foreach (var bs in sourceDb.BancaSaldoGiornaliero.FindAll())
                        targetDb.BancaSaldoGiornaliero.Insert(bs);
                    foreach (var fi in sourceDb.FinanziamentoImport.FindAll())
                        targetDb.FinanziamentoImport.Insert(fi);
                    
                    // Associazioni Mastrini
                    foreach (var am in sourceDb.AssociazioniMastrini.FindAll())
                        targetDb.AssociazioniMastrini.Insert(am);
                    foreach (var amd in sourceDb.AssociazioniMastriniDettagli.FindAll())
                        targetDb.AssociazioniMastriniDettagli.Insert(amd);
                    
                    // Statistiche salvate
                    foreach (var sce in sourceDb.StatisticheCESalvate.FindAll())
                        targetDb.StatisticheCESalvate.Insert(sce);
                    foreach (var ssp in sourceDb.StatisticheSPSalvate.FindAll())
                        targetDb.StatisticheSPSalvate.Insert(ssp);
                    
                    // Indici personalizzati
                    foreach (var ip in sourceDb.IndiciPersonalizzati.FindAll())
                        targetDb.IndiciPersonalizzati.Insert(ip);
                    
                    // Configurazioni indici
                    foreach (var ic in sourceDb.IndiciConfigurazioni.FindAll())
                        targetDb.IndiciConfigurazioni.Insert(ic);
                    
                    // Argomenti e Circolari
                    foreach (var arg in sourceDb.Argomenti.FindAll())
                        targetDb.Argomenti.Insert(arg);
                    foreach (var circ in sourceDb.Circolari.FindAll())
                        targetDb.Circolari.Insert(circ);
                    
                    targetDb.Checkpoint();
                }

                // Sostituisci database
                File.Delete(dbPath);
                File.Move(tempDbPath, dbPath);

                MessageBox.Show(
                    "✅ Database criptato con successo!\n\nTutti i dati sono stati preservati.\n\n" +
                    "Il database è protetto con password master interna.\n" +
                    "Usa la tua password O la master password per accedere.",
                    "Crittografia Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (operation.Operation == "decrypt")
            {
                // Decripta database (che è sempre criptato con master password)
                var masterPassword = DatabaseEncryptionService.GetMasterPassword();
                var tempDbPath = dbPath + ".decrypted.tmp";

                // Copia database senza password
                using (var sourceDb = new LiteDbContext(dbPath, masterPassword))
                using (var targetDb = new LiteDbContext(tempDbPath, null))
                {
                    // ⭐ Copia TUTTE le 23 collezioni
                    foreach (var u in sourceDb.Utenti.FindAll())
                        targetDb.Utenti.Insert(u);
                    foreach (var p in sourceDb.UserPermissions.FindAll())
                        targetDb.UserPermissions.Insert(p);
                    foreach (var c in sourceDb.Clienti.FindAll())
                        targetDb.Clienti.Insert(c);
                    foreach (var pr in sourceDb.Professionisti.FindAll())
                        targetDb.Professionisti.Insert(pr);
                    foreach (var tp in sourceDb.TipoPratiche.FindAll())
                        targetDb.TipoPratiche.Insert(tp);
                    foreach (var l in sourceDb.AuditLogs.FindAll())
                        targetDb.AuditLogs.Insert(l);
                    
                    // Todo Studio
                    foreach (var todo in sourceDb.TodoStudio.FindAll())
                        targetDb.TodoStudio.Insert(todo);
                    
                    // Licenze
                    foreach (var lc in sourceDb.LicenseClients.FindAll())
                        targetDb.LicenseClients.Insert(lc);
                    foreach (var lk in sourceDb.LicenseKeys.FindAll())
                        targetDb.LicenseKeys.Insert(lk);
                    
                    // Bilanci
                    foreach (var bc in sourceDb.BilancioContabile.FindAll())
                        targetDb.BilancioContabile.Insert(bc);
                    foreach (var bt in sourceDb.BilancioTemplate.FindAll())
                        targetDb.BilancioTemplate.Insert(bt);
                    
                    // Banche
                    foreach (var b in sourceDb.Banche.FindAll())
                        targetDb.Banche.Insert(b);
                    foreach (var bi in sourceDb.BancaIncassi.FindAll())
                        targetDb.BancaIncassi.Insert(bi);
                    foreach (var bp in sourceDb.BancaPagamenti.FindAll())
                        targetDb.BancaPagamenti.Insert(bp);
                    foreach (var bu in sourceDb.BancaUtilizzoAnticipo.FindAll())
                        targetDb.BancaUtilizzoAnticipo.Insert(bu);
                    foreach (var bs in sourceDb.BancaSaldoGiornaliero.FindAll())
                        targetDb.BancaSaldoGiornaliero.Insert(bs);
                    foreach (var fi in sourceDb.FinanziamentoImport.FindAll())
                        targetDb.FinanziamentoImport.Insert(fi);
                    
                    // Associazioni Mastrini
                    foreach (var am in sourceDb.AssociazioniMastrini.FindAll())
                        targetDb.AssociazioniMastrini.Insert(am);
                    foreach (var amd in sourceDb.AssociazioniMastriniDettagli.FindAll())
                        targetDb.AssociazioniMastriniDettagli.Insert(amd);
                    
                    // Statistiche salvate
                    foreach (var sce in sourceDb.StatisticheCESalvate.FindAll())
                        targetDb.StatisticheCESalvate.Insert(sce);
                    foreach (var ssp in sourceDb.StatisticheSPSalvate.FindAll())
                        targetDb.StatisticheSPSalvate.Insert(ssp);
                    
                    // Indici personalizzati
                    foreach (var ip in sourceDb.IndiciPersonalizzati.FindAll())
                        targetDb.IndiciPersonalizzati.Insert(ip);
                    
                    // Configurazioni indici
                    foreach (var ic in sourceDb.IndiciConfigurazioni.FindAll())
                        targetDb.IndiciConfigurazioni.Insert(ic);
                    
                    // Argomenti e Circolari
                    foreach (var arg in sourceDb.Argomenti.FindAll())
                        targetDb.Argomenti.Insert(arg);
                    foreach (var circ in sourceDb.Circolari.FindAll())
                        targetDb.Circolari.Insert(circ);
                    
                    targetDb.Checkpoint();
                }

                // Sostituisci database
                File.Delete(dbPath);
                File.Move(tempDbPath, dbPath);
                
                // ⭐ IMPORTANTE: Elimina il file db.key per indicare che il database NON è più criptato
                var keyPath = Path.Combine(Path.GetDirectoryName(dbPath)!, "db.key");
                if (File.Exists(keyPath))
                {
                    File.Delete(keyPath);
                }

                MessageBox.Show(
                    "✅ Crittografia rimossa con successo!\n\nTutti i dati sono stati preservati.\n\n" +
                    "⚠️ Il database NON è più protetto da password.",
                    "Decriptazione Completata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            // Elimina file operazione
            File.Delete(encryptionFile);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante operazione crittografia:\n{ex.Message}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private class EncryptionOperation
    {
        public string Operation { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BackupPath { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    private void InitializeDatabase()
    {
        try
        {
            var dbContext = _serviceProvider?.GetRequiredService<LiteDbContext>();
            if (dbContext != null)
            {
                // ✅ Inietta il repository globale per validazione licenze
                var licenseRepo = _serviceProvider?.GetRequiredService<LicenseRepository>();
                if (licenseRepo != null)
                {
                    LicenseService.SetGlobalRepository(licenseRepo);
                    System.Diagnostics.Debug.WriteLine("✅ Repository licenze globale configurato");
                }
                
                try
                {
                    // Verifica se ci sono utenti
                    var totalUtenti = dbContext.Utenti.Count();
                    
                    if (totalUtenti == 0)
                    {
                        // Crea admin di default
                        dbContext.SeedDefaultAdmin();
                        System.Diagnostics.Debug.WriteLine("✅ Database inizializzato con utente admin");
                        
                        // 🔐 Criptazione manuale tramite menu Sistema
                        // Database creato NON criptato per evitare problemi al primo avvio
                        System.Diagnostics.Debug.WriteLine("ℹ️ Database creato senza criptazione");
                        
                        MessageBox.Show("Database inizializzato!\n\nCredenziali di accesso:\nUsername: admin1\nPassword: 123123", 
                                      "Primo Avvio", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Database connesso: {totalUtenti} utenti");
                    }
                }
                catch (Exception innerEx)
                {
                    // Se c'è un errore nel conteggio, prova comunque a creare l'admin
                    System.Diagnostics.Debug.WriteLine($"⚠️ Errore durante verifica utenti: {innerEx.Message}");
                    System.Diagnostics.Debug.WriteLine("Tentativo di creazione admin...");
                    
                    try
                    {
                        dbContext.SeedDefaultAdmin();
                        MessageBox.Show("Database riparato e inizializzato!\n\nCredenziali:\nUsername: admin1\nPassword: 123123", 
                                      "Database Riparato", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Information);
                    }
                    catch (Exception seedEx)
                    {
                        throw new Exception($"Impossibile inizializzare database: {seedEx.Message}", seedEx);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore inizializzazione database:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                          "Errore Database", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Cleanup DI container
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Helper per ottenere services da fuori (se necessario)
    /// </summary>
    public static T? GetService<T>() where T : class
    {
        return ((App)Current)._serviceProvider?.GetService<T>();
    }
}



