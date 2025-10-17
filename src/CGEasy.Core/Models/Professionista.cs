using LiteDB;
using System;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Professionista
    /// </summary>
    public class Professionista
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonField("cognome")]
        public string Cognome { get; set; } = string.Empty;

        // Campi di gestione stato
        [BsonField("attivo")]
        public bool Attivo { get; set; } = true;

        [BsonField("data_attivazione")]
        public DateTime DataAttivazione { get; set; } = DateTime.UtcNow;

        [BsonField("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        [BsonField("data_cessazione")]
        public DateTime? DataCessazione { get; set; }

        [BsonField("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonField("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Proprietà calcolate
        [BsonIgnore]
        public string NomeCompleto => $"{Nome} {Cognome}";

        [BsonIgnore]
        public string StatoDescrizione => Attivo ? "Attivo" : "Cessato";

        [BsonIgnore]
        public string StatoCssClass => Attivo ? "success" : "danger";
    }
}

