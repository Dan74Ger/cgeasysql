using LiteDB;
using System;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello Tipo Pratica (per TODO Studio)
    /// </summary>
    public class TipoPratica
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("nome_pratica")]
        public string NomePratica { get; set; } = string.Empty;

        [BsonField("descrizione")]
        public string? Descrizione { get; set; }

        [BsonField("categoria")]
        public CategoriaPratica Categoria { get; set; } = CategoriaPratica.Altra;

        [BsonField("priorita_default")]
        public int PrioritaDefault { get; set; } = 2; // 1=Bassa, 2=Media, 3=Alta

        [BsonField("durata_stimata_giorni")]
        public int? DurataStimataGiorni { get; set; }

        [BsonField("attivo")]
        public bool Attivo { get; set; } = true;

        [BsonField("ordine")]
        public int Ordine { get; set; } = 0;

        [BsonField("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonField("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Proprietà calcolate
        [BsonIgnore]
        public string CategoriaDescrizione => Categoria switch
        {
            CategoriaPratica.Fiscale => "Fiscale",
            CategoriaPratica.Contabile => "Contabile",
            CategoriaPratica.Amministrativo => "Amministrativo",
            CategoriaPratica.Cliente => "Cliente",
            CategoriaPratica.Altra => "Altra",
            _ => "Non definita"
        };

        [BsonIgnore]
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

