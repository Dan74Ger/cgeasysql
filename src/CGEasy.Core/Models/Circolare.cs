using System;
using LiteDB;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Rappresenta una circolare importata con metadati
    /// </summary>
    public class Circolare
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("argomento_id")]
        public int ArgomentoId { get; set; }

        [BsonField("descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [BsonField("anno")]
        public int Anno { get; set; }

        [BsonField("nome_file")]
        public string NomeFile { get; set; } = string.Empty;

        [BsonField("percorso_file")]
        public string PercorsoFile { get; set; } = string.Empty;

        [BsonField("data_importazione")]
        public DateTime DataImportazione { get; set; } = DateTime.Now;

        [BsonField("utente_id")]
        public int UtenteId { get; set; }

        // Proprietà di navigazione (non salvata in DB)
        [BsonIgnore]
        public string? ArgomentoNome { get; set; }
    }
}

