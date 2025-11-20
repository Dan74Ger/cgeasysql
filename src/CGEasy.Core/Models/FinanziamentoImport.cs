using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CGEasy.Core.Models;

/// <summary>
/// Rappresenta un finanziamento importato (EF Core)
/// </summary>
[Table("finanziamento_import")]
public class FinanziamentoImport
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("nome_finanziamento")]
    [Required]
    [MaxLength(200)]
    public string NomeFinanziamento { get; set; } = string.Empty;

    [Column("data_inizio")]
    public DateTime DataInizio { get; set; }

    [Column("data_fine")]
    public DateTime DataFine { get; set; }

    [Column("importo")]
    [Precision(18, 2)]
    public decimal Importo { get; set; }

    [Column("banca_id")]
    public int BancaId { get; set; }

    [Column("nome_fornitore")]
    [MaxLength(200)]
    public string? NomeFornitore { get; set; }

    [Column("numero_fattura")]
    [MaxLength(50)]
    public string? NumeroFattura { get; set; }

    [Column("data_fattura")]
    public DateTime? DataFattura { get; set; }

    [Column("incasso_id")]
    public int IncassoId { get; set; }

    [Column("pagamento_id")]
    public int PagamentoId { get; set; }

    [Column("data_creazione")]
    public DateTime DataCreazione { get; set; } = DateTime.Now;

    [Column("utente_id")]
    public int UtenteId { get; set; }
}
