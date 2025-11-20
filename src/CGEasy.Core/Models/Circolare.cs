using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Rappresenta una circolare importata con metadati - EF Core
    /// </summary>
    [Table("circolari")]
    public class Circolare
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("argomento_id")]
        public int ArgomentoId { get; set; }

        [Column("descrizione")]
        [Required]
        [MaxLength(500)]
        public string Descrizione { get; set; } = string.Empty;

        [Column("anno")]
        public int Anno { get; set; }

        [Column("nome_file")]
        [Required]
        [MaxLength(300)]
        public string NomeFile { get; set; } = string.Empty;

        [Column("percorso_file")]
        [Required]
        [MaxLength(500)]
        public string PercorsoFile { get; set; } = string.Empty;

        [Column("data_importazione")]
        public DateTime DataImportazione { get; set; } = DateTime.Now;

        [Column("utente_id")]
        public int UtenteId { get; set; }

        // Proprietà di navigazione (non salvata in DB)
        [NotMapped]
        public string? ArgomentoNome { get; set; }
    }
}
