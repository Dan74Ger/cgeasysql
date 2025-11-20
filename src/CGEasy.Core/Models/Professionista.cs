using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Professionista - SQL Server (EF Core)
    /// </summary>
    [Table("professionisti")]
    public class Professionista
    {
        /// <summary>
        /// ID univoco professionista (auto-increment)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nome professionista
        /// </summary>
        [Column("nome")]
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Cognome professionista
        /// </summary>
        [Column("cognome")]
        [Required]
        [MaxLength(100)]
        public string Cognome { get; set; } = string.Empty;

        /// <summary>
        /// Stato attivo/cessato
        /// </summary>
        [Column("attivo")]
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Data attivazione professionista
        /// </summary>
        [Column("data_attivazione")]
        public DateTime DataAttivazione { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ultima modifica
        /// </summary>
        [Column("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data cessazione (se cessato)
        /// </summary>
        [Column("data_cessazione")]
        public DateTime? DataCessazione { get; set; }

        /// <summary>
        /// Data creazione record
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ultimo aggiornamento record
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ===== PROPRIETÀ CALCOLATE (NON MAPPATE SU DB) =====

        /// <summary>
        /// Nome completo (Nome + Cognome)
        /// </summary>
        [NotMapped]
        public string NomeCompleto => $"{Nome} {Cognome}";

        /// <summary>
        /// Descrizione stato testuale
        /// </summary>
        [NotMapped]
        public string StatoDescrizione => Attivo ? "Attivo" : "Cessato";

        /// <summary>
        /// CSS class per UI (Bootstrap)
        /// </summary>
        [NotMapped]
        public string StatoCssClass => Attivo ? "success" : "danger";
    }
}

