using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Audit Log per tracciare operazioni (EF Core)
    /// </summary>
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_utente")]
        public int IdUtente { get; set; }

        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Column("azione")]
        [Required]
        [MaxLength(50)]
        public string Azione { get; set; } = string.Empty;

        [Column("entita")]
        [Required]
        [MaxLength(100)]
        public string Entita { get; set; } = string.Empty;

        [Column("id_entita")]
        public int? IdEntita { get; set; }

        [Column("descrizione")]
        [MaxLength(500)]
        public string? Descrizione { get; set; }

        [Column("valori_precedenti")]
        public string? ValoriPrecedenti { get; set; }

        [Column("valori_nuovi")]
        public string? ValoriNuovi { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column("ip_address")]
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [NotMapped]
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
