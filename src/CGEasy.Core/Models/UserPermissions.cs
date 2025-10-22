using LiteDB;
using System;

namespace CGEasy.Core.Models
{
    /// <summary>
    /// Permessi utente per accesso moduli (License-based)
    /// </summary>
    public class UserPermissions
    {
        [BsonId]
        public int Id { get; set; }

        [BsonField("id_utente")]
        public int IdUtente { get; set; }

        // Accesso Moduli (License-based)
        [BsonField("modulo_todo")]
        public bool ModuloTodo { get; set; } = false;

        [BsonField("modulo_bilanci")]
        public bool ModuloBilanci { get; set; } = false;

        [BsonField("modulo_circolari")]
        public bool ModuloCircolari { get; set; } = false;

        [BsonField("modulo_controllo_gestione")]
        public bool ModuloControlloGestione { get; set; } = false;

        // Permessi CRUD per Anagrafica
        [BsonField("clienti_create")]
        public bool ClientiCreate { get; set; } = false;

        [BsonField("clienti_read")]
        public bool ClientiRead { get; set; } = true; // Default read

        [BsonField("clienti_update")]
        public bool ClientiUpdate { get; set; } = false;

        [BsonField("clienti_delete")]
        public bool ClientiDelete { get; set; } = false;

        [BsonField("professionisti_create")]
        public bool ProfessionistiCreate { get; set; } = false;

        [BsonField("professionisti_read")]
        public bool ProfessionistiRead { get; set; } = true;

        [BsonField("professionisti_update")]
        public bool ProfessionistiUpdate { get; set; } = false;

        [BsonField("professionisti_delete")]
        public bool ProfessionistiDelete { get; set; } = false;

        [BsonField("tipopratiche_create")]
        public bool TipoPraticheCreate { get; set; } = false;

        [BsonField("tipopratiche_read")]
        public bool TipoPraticheRead { get; set; } = true;

        [BsonField("tipopratiche_update")]
        public bool TipoPraticheUpdate { get; set; } = false;

        [BsonField("tipopratiche_delete")]
        public bool TipoPraticheDelete { get; set; } = false;

        // Gestione Utenti (Solo Admin)
        [BsonField("utenti_manage")]
        public bool UtentiManage { get; set; } = false;

        [BsonField("data_creazione")]
        public DateTime DataCreazione { get; set; } = DateTime.UtcNow;

        [BsonField("data_modifica")]
        public DateTime DataModifica { get; set; } = DateTime.UtcNow;

        // Helper methods
        [BsonIgnore]
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

        [BsonIgnore]
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

