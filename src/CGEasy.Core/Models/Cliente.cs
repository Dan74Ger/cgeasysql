using LiteDB;
using System;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Cliente (Anagrafica)
    /// </summary>
    public class Cliente
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("nome_cliente")]
        public string NomeCliente { get; set; } = string.Empty;

        [BsonField("id_professionista")]
        public int? IdProfessionista { get; set; }

        [BsonField("mail_cliente")]
        public string? MailCliente { get; set; }

        // DATI FISCALI
        [BsonField("cf_cliente")]
        public string? CfCliente { get; set; }

        [BsonField("piva_cliente")]
        public string? PivaCliente { get; set; }

        [BsonField("codice_ateco")]
        public string? CodiceAteco { get; set; }

        // INDIRIZZO
        [BsonField("indirizzo")]
        public string? Indirizzo { get; set; }

        [BsonField("citta")]
        public string? Citta { get; set; }

        [BsonField("provincia")]
        public string? Provincia { get; set; }

        [BsonField("cap")]
        public string? Cap { get; set; }

        // LEGALE RAPPRESENTANTE
        [BsonField("legale_rappresentante")]
        public string? LegaleRappresentante { get; set; }

        [BsonField("cf_legale_rappresentante")]
        public string? CfLegaleRappresentante { get; set; }

        // STATO
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
        public string StatoDescrizione => Attivo ? "Attivo" : "Cessato";

        [BsonIgnore]
        public string IndirizzoCompleto
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Indirizzo))
                    return string.Empty;

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(Indirizzo)) parts.Add(Indirizzo);
                if (!string.IsNullOrWhiteSpace(Cap)) parts.Add(Cap);
                if (!string.IsNullOrWhiteSpace(Citta)) parts.Add(Citta);
                if (!string.IsNullOrWhiteSpace(Provincia)) parts.Add(Provincia);
                return string.Join(", ", parts);
            }
        }

        [BsonIgnore]
        public string DisplayName
        {
            get
            {
                var nome = NomeCliente;
                if (!string.IsNullOrWhiteSpace(CfCliente))
                    nome += $" (CF: {CfCliente})";
                else if (!string.IsNullOrWhiteSpace(PivaCliente))
                    nome += $" (P.IVA: {PivaCliente})";
                return nome;
            }
        }
    }
}

