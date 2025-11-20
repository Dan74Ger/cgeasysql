using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Cliente (Anagrafica)
    /// </summary>
    [Table("clienti")]
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("nome_cliente")]
        [Required]
        [MaxLength(200)]
        public string NomeCliente { get; set; } = string.Empty;

        [Column("id_professionista")]
        public int? IdProfessionista { get; set; }

        [Column("mail_cliente")]
        [MaxLength(200)]
        public string? MailCliente { get; set; }

        // DATI FISCALI
        [Column("cf_cliente")]
        [MaxLength(16)]
        public string? CfCliente { get; set; }

        [Column("piva_cliente")]
        [MaxLength(20)]
        public string? PivaCliente { get; set; }

        [Column("codice_ateco")]
        [MaxLength(20)]
        public string? CodiceAteco { get; set; }

        // INDIRIZZO
        [Column("indirizzo")]
        [MaxLength(300)]
        public string? Indirizzo { get; set; }

        [Column("citta")]
        [MaxLength(100)]
        public string? Citta { get; set; }

        [Column("provincia")]
        [MaxLength(5)]
        public string? Provincia { get; set; }

        [Column("cap")]
        [MaxLength(10)]
        public string? Cap { get; set; }

        // LEGALE RAPPRESENTANTE
        [Column("legale_rappresentante")]
        [MaxLength(200)]
        public string? LegaleRappresentante { get; set; }

        [Column("cf_legale_rappresentante")]
        [MaxLength(16)]
        public string? CfLegaleRappresentante { get; set; }

        // STATO
        [Column("attivo")]
        public bool Attivo { get; set; } = true;

        [Column("data_attivazione")]
        public DateTime DataAttivazione { get; set; } = DateTime.UtcNow;

        [Column("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        [Column("data_cessazione")]
        public DateTime? DataCessazione { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Proprietà calcolate
        [NotMapped]
        public string StatoDescrizione => Attivo ? "Attivo" : "Cessato";

        [NotMapped]
        public string IndirizzoCompleto
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Indirizzo))
                    return string.Empty;

                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(Indirizzo)) parts.Add(Indirizzo);
                if (!string.IsNullOrWhiteSpace(Cap)) parts.Add(Cap);
                if (!string.IsNullOrWhiteSpace(Citta)) parts.Add(Citta);
                if (!string.IsNullOrWhiteSpace(Provincia)) parts.Add(Provincia);
                return string.Join(", ", parts);
            }
        }

        [NotMapped]
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

