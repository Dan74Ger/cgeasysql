using System;
using LiteDB;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Rappresenta un argomento per classificare le circolari
    /// </summary>
    public class Argomento
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("nome")]
        public string Nome { get; set; } = string.Empty;

        [BsonField("descrizione")]
        public string? Descrizione { get; set; }

        [BsonField("data_creazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        [BsonField("utente_id")]
        public int UtenteId { get; set; }
    }
}

