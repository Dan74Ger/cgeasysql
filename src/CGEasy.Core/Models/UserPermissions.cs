using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Modello User Permissions - Permessi granulari per utente - SQL Server (EF Core)
    /// </summary>
    [Table("user_permissions")]
    public class UserPermissions
    {
        /// <summary>
        /// ID univoco record (auto-increment)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID Utente (relazione 1:1 con Utente)
        /// </summary>
        [Column("id_utente")]
        [Required]
        public int IdUtente { get; set; }

        // ===== PERMESSI MODULI =====

        /// <summary>
        /// Accesso modulo TODO Studio
        /// </summary>
        [Column("modulo_todo")]
        public bool ModuloTodo { get; set; } = false;

        /// <summary>
        /// Accesso modulo Bilanci
        /// </summary>
        [Column("modulo_bilanci")]
        public bool ModuloBilanci { get; set; } = false;

        /// <summary>
        /// Accesso modulo Circolari
        /// </summary>
        [Column("modulo_circolari")]
        public bool ModuloCircolari { get; set; } = false;

        /// <summary>
        /// Accesso modulo Controllo Gestione
        /// </summary>
        [Column("modulo_controllo_gestione")]
        public bool ModuloControlloGestione { get; set; } = false;

        // ===== PERMESSI CLIENTI =====

        /// <summary>
        /// Può creare nuovi clienti
        /// </summary>
        [Column("clienti_create")]
        public bool ClientiCreate { get; set; } = false;

        /// <summary>
        /// Può visualizzare clienti
        /// </summary>
        [Column("clienti_read")]
        public bool ClientiRead { get; set; } = true; // Default read

        /// <summary>
        /// Può modificare clienti
        /// </summary>
        [Column("clienti_update")]
        public bool ClientiUpdate { get; set; } = false;

        /// <summary>
        /// Può eliminare clienti
        /// </summary>
        [Column("clienti_delete")]
        public bool ClientiDelete { get; set; } = false;

        // ===== PERMESSI PROFESSIONISTI =====

        /// <summary>
        /// Può creare nuovi professionisti
        /// </summary>
        [Column("professionisti_create")]
        public bool ProfessionistiCreate { get; set; } = false;

        /// <summary>
        /// Può visualizzare professionisti
        /// </summary>
        [Column("professionisti_read")]
        public bool ProfessionistiRead { get; set; } = true;

        /// <summary>
        /// Può modificare professionisti
        /// </summary>
        [Column("professionisti_update")]
        public bool ProfessionistiUpdate { get; set; } = false;

        /// <summary>
        /// Può eliminare professionisti
        /// </summary>
        [Column("professionisti_delete")]
        public bool ProfessionistiDelete { get; set; } = false;

        // ===== PERMESSI TIPO PRATICHE =====

        /// <summary>
        /// Può creare nuovi tipi pratica
        /// </summary>
        [Column("tipopratiche_create")]
        public bool TipoPraticheCreate { get; set; } = false;

        /// <summary>
        /// Può visualizzare tipi pratica
        /// </summary>
        [Column("tipopratiche_read")]
        public bool TipoPraticheRead { get; set; } = true;

        /// <summary>
        /// Può modificare tipi pratica
        /// </summary>
        [Column("tipopratiche_update")]
        public bool TipoPraticheUpdate { get; set; } = false;

        /// <summary>
        /// Può eliminare tipi pratica
        /// </summary>
        [Column("tipopratiche_delete")]
        public bool TipoPraticheDelete { get; set; } = false;

        // ===== PERMESSI GESTIONE UTENTI =====

        /// <summary>
        /// Può gestire utenti e permessi (solo Administrator)
        /// </summary>
        [Column("utenti_manage")]
        public bool UtentiManage { get; set; } = false;

        // ===== METADATA =====

        /// <summary>
        /// Data creazione record
        /// </summary>
        [Column("data_creazione")]
        public DateTime DataCreazione { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data ultima modifica
        /// </summary>
        [Column("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        // ===== HELPER METHODS (NON MAPPATI SU DB) =====

        /// <summary>
        /// Verifica se ha accesso a un modulo specifico
        /// </summary>
        public bool HasModuleAccess(string moduleName)
        {
            return moduleName.ToLower() switch
            {
                "todo" => ModuloTodo,
                "bilanci" => ModuloBilanci,
                "circolari" => ModuloCircolari,
                "controllo" => ModuloControlloGestione,
                _ => false
            };
        }

        /// <summary>
        /// Numero moduli attivi (licenziati)
        /// </summary>
        [NotMapped]
        public int ModuliAttivi
        {
            get
            {
                int count = 0;
                if (ModuloTodo) count++;
                if (ModuloBilanci) count++;
                if (ModuloCircolari) count++;
                if (ModuloControlloGestione) count++;
                return count;
            }
        }
    }
}
