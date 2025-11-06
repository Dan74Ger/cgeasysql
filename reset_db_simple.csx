using System;
using System.IO;
using LiteDB;

var dbPath = @"C:\devcg-group\dbtest_prova\cgeasy.db";

if (!File.Exists(dbPath))
{
    Console.WriteLine($"[X] Database non trovato: {dbPath}");
    return;
}

Console.WriteLine($"[*] Apertura database: {dbPath}");

using var db = new LiteDatabase(dbPath);

// Elimina tutte le collections
Console.WriteLine("[*] Eliminazione clienti...");
db.DropCollection("clienti");

Console.WriteLine("[*] Eliminazione professionisti...");
db.DropCollection("professionisti");

Console.WriteLine("[*] Eliminazione tipo pratiche...");
db.DropCollection("tipo_pratiche");

Console.WriteLine("[*] Eliminazione audit logs...");
db.DropCollection("audit_logs");

Console.WriteLine("[*] Eliminazione TODO Studio...");
db.DropCollection("todoStudio");

Console.WriteLine("[*] Eliminazione bilanci...");
db.DropCollection("bilancio_contabile");
db.DropCollection("bilancio_template");

Console.WriteLine("[*] Eliminazione associazioni mastrini...");
db.DropCollection("associazioni_mastrini");
db.DropCollection("associazioni_mastrini_dettagli");

Console.WriteLine("[*] Eliminazione licenze...");
db.DropCollection("license_clients");
db.DropCollection("license_keys");

Console.WriteLine("[*] Eliminazione permessi utenti...");
db.DropCollection("user_permissions");

// Ricrea collection utenti con solo admin
Console.WriteLine("[*] Creazione utente admin...");
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
Console.WriteLine("[*] Creazione permessi admin...");
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

Console.WriteLine("");
Console.WriteLine("=====================================");
Console.WriteLine("[OK] DATABASE RESETTATO CON SUCCESSO!");
Console.WriteLine("=====================================");
Console.WriteLine("");
Console.WriteLine("Credenziali admin:");
Console.WriteLine("  Username: admin");
Console.WriteLine("  Password: 123456");
Console.WriteLine("");
Console.WriteLine($"Statistiche finali:");
Console.WriteLine($"  Utenti: {utentiCol.Count()}");
Console.WriteLine($"  Permessi: {permissionsCol.Count()}");
Console.WriteLine($"  Clienti: 0");
Console.WriteLine($"  Professionisti: 0");
Console.WriteLine($"  Licenze: 0");












