using LiteDB;
using System;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Utente per autenticazione e gestione profilo
    /// </summary>
    public class Utente
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("username")]
        public string Username { get; set; } = string.Empty;

        [BsonField("email")]
        public string Email { get; set; } = string.Empty;

        [BsonField("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonField("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonField("cognome")]
        public string Cognome { get; set; } = string.Empty;

        [BsonField("ruolo")]
        public RuoloUtente Ruolo { get; set; } = RuoloUtente.User;

        [BsonField("attivo")]
        public bool Attivo { get; set; } = true;

        [BsonField("data_creazione")]
        public DateTime DataCreazione { get; set; } = DateTime.UtcNow;

        [BsonField("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        [BsonField("ultimo_accesso")]
        public DateTime? UltimoAccesso { get; set; }

        [BsonField("data_cessazione")]
        public DateTime? DataCessazione { get; set; }

        // Proprietà calcolata
        [BsonIgnore]
        public string NomeCompleto => $"{Nome} {Cognome}";

        [BsonIgnore]
        public string RuoloDescrizione => Ruolo switch
        {
            RuoloUtente.Administrator => "Amministratore",
            RuoloUtente.UserSenior => "Utente Senior",
            RuoloUtente.User => "Utente",
            _ => "Sconosciuto"
        };

        [BsonIgnore]
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

