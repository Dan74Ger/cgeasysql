using System;
using System.IO;
using LiteDB;

namespace ResetDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Uso: ResetDatabase <percorso_database>");
                return;
            }

            var dbPath = args[0];

            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"❌ Database non trovato: {dbPath}");
                return;
            }

            try
            {
                Console.WriteLine($"📂 Apertura database: {dbPath}");
                
                using var db = new LiteDatabase(dbPath);
                
                // Elimina tutte le collections
                Console.WriteLine("🗑️  Eliminazione clienti...");
                db.DropCollection("clienti");
                
                Console.WriteLine("🗑️  Eliminazione professionisti...");
                db.DropCollection("professionisti");
                
                Console.WriteLine("🗑️  Eliminazione tipo pratiche...");
                db.DropCollection("tipo_pratiche");
                
                Console.WriteLine("🗑️  Eliminazione audit logs...");
                db.DropCollection("audit_logs");
                
                Console.WriteLine("🗑️  Eliminazione TODO Studio...");
                db.DropCollection("todoStudio");
                
                Console.WriteLine("🗑️  Eliminazione bilanci...");
                db.DropCollection("bilancio_contabile");
                db.DropCollection("bilancio_template");
                
                Console.WriteLine("🗑️  Eliminazione associazioni mastrini...");
                db.DropCollection("associazioni_mastrini");
                db.DropCollection("associazioni_mastrini_dettagli");
                
                Console.WriteLine("🗑️  Eliminazione licenze...");
                db.DropCollection("license_clients");
                db.DropCollection("license_keys");
                
                Console.WriteLine("🗑️  Eliminazione permessi utenti...");
                db.DropCollection("user_permissions");
                
                // Ricrea collection utenti con solo admin
                Console.WriteLine("👤 Creazione utente admin...");
                var utentiCol = db.GetCollection("utenti");
                utentiCol.DeleteAll();
                
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");
                
                var admin = new BsonDocument
                {
                    ["Id"] = 1,
                    ["Username"] = "admin",
                    ["Email"] = "admin@cgeasy.local",
                    ["PasswordHash"] = passwordHash,
                    ["Nome"] = "Amministratore",
                    ["Cognome"] = "Sistema",
                    ["Ruolo"] = "Administrator",
                    ["Attivo"] = true,
                    ["DataCreazione"] = DateTime.UtcNow,
                    ["DataModifica"] = DateTime.UtcNow
                };
                
                utentiCol.Insert(admin);
                utentiCol.EnsureIndex("Username", unique: true);
                
                // Crea permessi admin
                Console.WriteLine("🔑 Creazione permessi admin...");
                var permissionsCol = db.GetCollection("user_permissions");
                
                var permissions = new BsonDocument
                {
                    ["Id"] = 1,
                    ["IdUtente"] = 1,
                    ["ModuloTodo"] = true,
                    ["ModuloBilanci"] = true,
                    ["ModuloCircolari"] = true,
                    ["ModuloControlloGestione"] = true,
                    ["ClientiCreate"] = true,
                    ["ClientiRead"] = true,
                    ["ClientiUpdate"] = true,
                    ["ClientiDelete"] = true,
                    ["ProfessionistiCreate"] = true,
                    ["ProfessionistiRead"] = true,
                    ["ProfessionistiUpdate"] = true,
                    ["ProfessionistiDelete"] = true,
                    ["UtentiManage"] = true,
                    ["DataCreazione"] = DateTime.UtcNow,
                    ["DataModifica"] = DateTime.UtcNow
                };
                
                permissionsCol.Insert(permissions);
                permissionsCol.EnsureIndex("IdUtente", unique: true);
                
                // Checkpoint finale
                db.Checkpoint();
                
                Console.WriteLine("✅ Database resettato con successo!");
                Console.WriteLine("");
                Console.WriteLine("Statistiche finali:");
                Console.WriteLine($"  Utenti: {utentiCol.Count()}");
                Console.WriteLine($"  Permessi: {permissionsCol.Count()}");
                Console.WriteLine($"  Clienti: 0");
                Console.WriteLine($"  Professionisti: 0");
                Console.WriteLine($"  Licenze: 0");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Errore: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}












