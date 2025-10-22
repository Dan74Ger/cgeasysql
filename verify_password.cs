using System;
using System.Linq;
using LiteDB;

var dbPath = @"C:\ProgramData\CGEasy\cgeasy.db";
if (!System.IO.File.Exists(dbPath)) {
    Console.WriteLine("❌ Database non trovato!");
    return;
}

try {
    var mapper = new BsonMapper();
    var connStr = new ConnectionString { Filename = dbPath, Connection = ConnectionType.Direct };
    using (var db = new LiteDatabase(connStr, mapper)) {
        var utenti = db.GetCollection("utenti");
        var admin = utenti.FindOne(Query.EQ("username", "admin"));
        
        if (admin == null) {
            Console.WriteLine("❌ Utente admin non trovato!");
            return;
        }
        
        var passwordHash = admin["password_hash"].AsString;
        Console.WriteLine("✅ Utente admin trovato!");
        Console.WriteLine($"Username: {admin["username"].AsString}");
        Console.WriteLine($"Email: {admin["email"].AsString}");
        Console.WriteLine($"Password Hash: {passwordHash.Substring(0, 30)}...");
        Console.WriteLine($"Data Modifica: {admin["data_modifica"].AsDateTime}");
        
        // Verifica la password con BCrypt
        try {
            bool match123456 = BCrypt.Net.BCrypt.Verify("123456", passwordHash);
            bool matchAdmin123 = BCrypt.Net.BCrypt.Verify("admin123", passwordHash);
            
            Console.WriteLine("\n=== VERIFICA PASSWORD ===");
            Console.WriteLine($"Password '123456' (nuova): {(match123456 ? "✅ CORRETTA" : "❌ ERRATA")}");
            Console.WriteLine($"Password 'admin123' (vecchia): {(matchAdmin123 ? "✅ CORRETTA" : "❌ ERRATA")}");
            
            if (match123456) {
                Console.WriteLine("\n🎉 LA NUOVA PASSWORD È STATA SALVATA CORRETTAMENTE!");
            } else if (matchAdmin123) {
                Console.WriteLine("\n⚠️ PROBLEMA: La password vecchia è ancora salvata!");
            } else {
                Console.WriteLine("\n❌ ERRORE: Nessuna password corrisponde!");
            }
        } catch (Exception ex) {
            Console.WriteLine($"❌ Errore verifica password: {ex.Message}");
        }
    }
} catch (Exception ex) {
    Console.WriteLine($"❌ Errore: {ex.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
}
