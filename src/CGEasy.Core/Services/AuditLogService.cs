using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Servizio per gestione Audit Log
    /// </summary>
    public class AuditLogService
    {
        private readonly LiteDbContext _context;

        public AuditLogService(LiteDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra un'operazione nel log
        /// </summary>
        public void Log(int idUtente, string username, string azione, string entita, 
                       int? idEntita = null, string? descrizione = null, 
                       string? valoriPrecedenti = null, string? valoriNuovi = null)
        {
            try
            {
                var log = new AuditLog
                {
                    IdUtente = idUtente,
                    Username = username,
                    Azione = azione,
                    Entita = entita,
                    IdEntita = idEntita,
                    Descrizione = descrizione,
                    ValoriPrecedenti = valoriPrecedenti,
                    ValoriNuovi = valoriNuovi,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Insert(log);
            }
            catch (Exception ex)
            {
                // Log errors silently - non vogliamo bloccare l'operazione principale
                System.Diagnostics.Debug.WriteLine($"Errore Audit Log: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra operazione dall'utente corrente di sessione
        /// </summary>
        public void LogFromSession(string azione, string entita, int? idEntita = null, string? descrizione = null)
        {
            if (!SessionManager.IsAuthenticated || SessionManager.CurrentUser == null)
                return;

            Log(SessionManager.CurrentUser.Id, SessionManager.CurrentUser.Username, 
                azione, entita, idEntita, descrizione);
        }

        /// <summary>
        /// Ottiene tutti i log
        /// </summary>
        public IEnumerable<AuditLog> GetAll()
        {
            return _context.AuditLogs.FindAll().OrderByDescending(x => x.Timestamp);
        }

        /// <summary>
        /// Ottiene log per utente
        /// </summary>
        public IEnumerable<AuditLog> GetByUser(int idUtente)
        {
            return _context.AuditLogs.Find(x => x.IdUtente == idUtente)
                .OrderByDescending(x => x.Timestamp);
        }

        /// <summary>
        /// Ottiene log per entità
        /// </summary>
        public IEnumerable<AuditLog> GetByEntity(string entita, int? idEntita = null)
        {
            if (idEntita.HasValue)
            {
                return _context.AuditLogs.Find(x => x.Entita == entita && x.IdEntita == idEntita)
                    .OrderByDescending(x => x.Timestamp);
            }
            else
            {
                return _context.AuditLogs.Find(x => x.Entita == entita)
                    .OrderByDescending(x => x.Timestamp);
            }
        }

        /// <summary>
        /// Ottiene log per periodo
        /// </summary>
        public IEnumerable<AuditLog> GetByPeriod(DateTime dataInizio, DateTime dataFine)
        {
            return _context.AuditLogs.Find(x => x.Timestamp >= dataInizio && x.Timestamp <= dataFine)
                .OrderByDescending(x => x.Timestamp);
        }

        /// <summary>
        /// Ottiene log per azione
        /// </summary>
        public IEnumerable<AuditLog> GetByAction(string azione)
        {
            return _context.AuditLogs.Find(x => x.Azione == azione)
                .OrderByDescending(x => x.Timestamp);
        }

        /// <summary>
        /// Cerca nei log
        /// </summary>
        public IEnumerable<AuditLog> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAll();

            var lower = searchTerm.ToLower();
            return _context.AuditLogs.Find(x => 
                x.Username.ToLower().Contains(lower) ||
                x.Azione.ToLower().Contains(lower) ||
                x.Entita.ToLower().Contains(lower) ||
                (x.Descrizione != null && x.Descrizione.ToLower().Contains(lower)))
                .OrderByDescending(x => x.Timestamp);
        }

        /// <summary>
        /// Conta log totali
        /// </summary>
        public int Count()
        {
            return _context.AuditLogs.Count();
        }

        /// <summary>
        /// Elimina log più vecchi di X giorni (pulizia)
        /// </summary>
        public int DeleteOlderThan(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var oldLogs = _context.AuditLogs.Find(x => x.Timestamp < cutoffDate);
            
            int deleted = 0;
            foreach (var log in oldLogs)
            {
                if (_context.AuditLogs.Delete(log.Id))
                    deleted++;
            }
            
            return deleted;
        }

        /// <summary>
        /// Ottiene statistiche log
        /// </summary>
        public AuditLogStats GetStatistics()
        {
            var allLogs = _context.AuditLogs.FindAll().ToList();
            
            return new AuditLogStats
            {
                TotalLogs = allLogs.Count,
                LogsToday = allLogs.Count(x => x.Timestamp.Date == DateTime.UtcNow.Date),
                LogsThisWeek = allLogs.Count(x => x.Timestamp >= DateTime.UtcNow.AddDays(-7)),
                LogsThisMonth = allLogs.Count(x => x.Timestamp >= DateTime.UtcNow.AddDays(-30)),
                UniqueUsers = allLogs.Select(x => x.IdUtente).Distinct().Count(),
                MostActiveUser = allLogs.GroupBy(x => x.Username)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A",
                MostCommonAction = allLogs.GroupBy(x => x.Azione)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };
        }
    }

    /// <summary>
    /// Statistiche Audit Log
    /// </summary>
    public class AuditLogStats
    {
        public int TotalLogs { get; set; }
        public int LogsToday { get; set; }
        public int LogsThisWeek { get; set; }
        public int LogsThisMonth { get; set; }
        public int UniqueUsers { get; set; }
        public string MostActiveUser { get; set; } = string.Empty;
        public string MostCommonAction { get; set; } = string.Empty;
    }
}

















