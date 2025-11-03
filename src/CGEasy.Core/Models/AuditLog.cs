using LiteDB;
using System;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Audit Log per tracciare operazioni
    /// </summary>
    public class AuditLog
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("id_utente")]
        public int IdUtente { get; set; }

        [BsonField("username")]
        public string Username { get; set; } = string.Empty;

        [BsonField("azione")]
        public string Azione { get; set; } = string.Empty;

        [BsonField("entita")]
        public string Entita { get; set; } = string.Empty;

        [BsonField("id_entita")]
        public int? IdEntita { get; set; }

        [BsonField("descrizione")]
        public string? Descrizione { get; set; }

        [BsonField("valori_precedenti")]
        public string? ValoriPrecedenti { get; set; }

        [BsonField("valori_nuovi")]
        public string? ValoriNuovi { get; set; }

        [BsonField("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [BsonField("ip_address")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Restituisce descrizione formattata
        /// </summary>
        [BsonIgnore]
        public string DescrizioneCompleta => 
            $"[{Timestamp:dd/MM/yyyy HH:mm:ss}] {Username} - {Azione} {Entita}" +
            (IdEntita.HasValue ? $" (ID: {IdEntita})" : "") +
            (!string.IsNullOrEmpty(Descrizione) ? $" - {Descrizione}" : "");
    }

    /// <summary>
    /// Enum Azioni Audit
    /// </summary>
    public static class AuditAction
    {
        public const string Login = "LOGIN";
        public const string Logout = "LOGOUT";
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
        public const string View = "VIEW";
        public const string Export = "EXPORT";
        public const string Import = "IMPORT";
    }

    /// <summary>
    /// Enum Entità Audit
    /// </summary>
    public static class AuditEntity
    {
        public const string Utente = "Utente";
        public const string Cliente = "Cliente";
        public const string Professionista = "Professionista";
        public const string TipoPratica = "TipoPratica";
        public const string Todo = "Todo";
        public const string Bilancio = "Bilancio";
        public const string Circolare = "Circolare";
        public const string Documento = "Documento";
        public const string Budget = "Budget";
    }
}

















