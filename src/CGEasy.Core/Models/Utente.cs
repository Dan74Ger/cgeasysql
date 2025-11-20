using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Utente per autenticazione e gestione profilo - SQL Server (EF Core)
    /// </summary>
    [Table("utenti")]
    public class Utente
    {
        /// <summary>
        /// ID univoco utente (auto-increment)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Username univoco per login
        /// </summary>
        [Column("username")]
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email utente
        /// </summary>
        [Column("email")]
        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Password hash (BCrypt)
        /// </summary>
        [Column("password_hash")]
        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Nome utente
        /// </summary>
        [Column("nome")]
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Cognome utente
        /// </summary>
        [Column("cognome")]
        [Required]
        [MaxLength(100)]
        public string Cognome { get; set; } = string.Empty;

        /// <summary>
        /// Ruolo utente (Administrator, UserSenior, User)
        /// </summary>
        [Column("ruolo")]
        [Required]
        public RuoloUtente Ruolo { get; set; } = RuoloUtente.User;

        /// <summary>
        /// Stato attivo/disabilitato
        /// </summary>
        [Column("attivo")]
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Data creazione utente
        /// </summary>
        [Column("data_creazione")]
        public DateTime DataCreazione { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ultima modifica
        /// </summary>
        [Column("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ultimo accesso
        /// </summary>
        [Column("ultimo_accesso")]
        public DateTime? UltimoAccesso { get; set; }

        /// <summary>
        /// Data cessazione (se disabilitato)
        /// </summary>
        [Column("data_cessazione")]
        public DateTime? DataCessazione { get; set; }

        // ===== PROPRIETÀ CALCOLATE (NON MAPPATE SU DB) =====

        /// <summary>
        /// Nome completo (Nome + Cognome)
        /// </summary>
        [NotMapped]
        public string NomeCompleto => $"{Nome} {Cognome}";

        /// <summary>
        /// Descrizione ruolo testuale
        /// </summary>
        [NotMapped]
        public string RuoloDescrizione => Ruolo switch
        {
            RuoloUtente.Administrator => "Amministratore",
            RuoloUtente.UserSenior => "Utente Senior",
            RuoloUtente.User => "Utente",
            _ => "Sconosciuto"
        };

        /// <summary>
        /// Colore ruolo per UI
        /// </summary>
        [NotMapped]
        public string RuoloUtenteColor => Ruolo switch
        {
            RuoloUtente.Administrator => "#e74c3c",  // Rosso
            RuoloUtente.UserSenior => "#f39c12",     // Arancione
            RuoloUtente.User => "#27ae60",           // Verde
            _ => "#95a5a6"                           // Grigio
        };
    }

    /// <summary>
    /// Enum Ruoli Utente
    /// </summary>
    public enum RuoloUtente
    {
        Administrator = 1,
        UserSenior = 2,
        User = 3
    }
}

