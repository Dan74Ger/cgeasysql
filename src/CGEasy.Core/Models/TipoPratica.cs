using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Tipo Pratica (per TODO Studio) - EF Core
    /// </summary>
    [Table("tipo_pratiche")]
    public class TipoPratica
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("nome_pratica")]
        [Required]
        [MaxLength(200)]
        public string NomePratica { get; set; } = string.Empty;

        [Column("descrizione")]
        [MaxLength(500)]
        public string? Descrizione { get; set; }

        [Column("categoria")]
        public CategoriaPratica Categoria { get; set; } = CategoriaPratica.Altra;

        [Column("priorita_default")]
        public int PrioritaDefault { get; set; } = 2; // 1=Bassa, 2=Media, 3=Alta

        [Column("durata_stimata_giorni")]
        public int? DurataStimataGiorni { get; set; }

        [Column("attivo")]
        public bool Attivo { get; set; } = true;

        [Column("ordine")]
        public int Ordine { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Proprietà calcolate
        [NotMapped]
        public string CategoriaDescrizione => Categoria switch
        {
            CategoriaPratica.Fiscale => "Fiscale",
            CategoriaPratica.Contabile => "Contabile",
            CategoriaPratica.Amministrativo => "Amministrativo",
            CategoriaPratica.Cliente => "Cliente",
            CategoriaPratica.Altra => "Altra",
            _ => "Non definita"
        };

        [NotMapped]
        public string IconaCategoria => Categoria switch
        {
            CategoriaPratica.Fiscale => "📄",
            CategoriaPratica.Contabile => "🧮",
            CategoriaPratica.Amministrativo => "💼",
            CategoriaPratica.Cliente => "👤",
            CategoriaPratica.Altra => "📁",
            _ => "📋"
        };
    }

    /// <summary>
    /// Enum Categoria Pratica
    /// </summary>
    public enum CategoriaPratica
    {
        Fiscale = 1,
        Contabile = 2,
        Amministrativo = 3,
        Cliente = 4,
        Altra = 5
    }
}
