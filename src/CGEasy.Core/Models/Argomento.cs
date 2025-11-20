using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Rappresenta un argomento per classificare le circolari - EF Core
    /// </summary>
    [Table("argomenti")]
    public class Argomento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("nome")]
        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Column("descrizione")]
        [MaxLength(1000)]
        public string? Descrizione { get; set; }

        [Column("data_creazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        [Column("utente_id")]
        public int UtenteId { get; set; }
    }
}
