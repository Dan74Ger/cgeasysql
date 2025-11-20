using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Servizio per gestione Audit Log
    /// </summary>
    public class AuditLogService
    {
        private readonly CGEasyDbContext _context;

        public AuditLogService(CGEasyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra un'operazione nel log (ASYNC)
        /// </summary>
        public async Task LogAsync(int idUtente, string username, string azione, string entita, 
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

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log errors silently - non vogliamo bloccare l'operazione principale
                System.Diagnostics.Debug.WriteLine($"Errore Audit Log: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra operazione dall'utente corrente di sessione (ASYNC)
        /// </summary>
        public async Task LogFromSessionAsync(string azione, string entita, int? idEntita = null, string? descrizione = null)
        {
            if (!SessionManager.IsAuthenticated || SessionManager.CurrentUser == null)
                return;

            await LogAsync(SessionManager.CurrentUser.Id, SessionManager.CurrentUser.Username, 
                azione, entita, idEntita, descrizione);
        }

        /// <summary>
        /// Ottiene tutti i log (ASYNC)
        /// </summary>
        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene log per utente (ASYNC)
        /// </summary>
        public async Task<List<AuditLog>> GetByUserAsync(int idUtente)
        {
            return await _context.AuditLogs
                .Where(x => x.IdUtente == idUtente)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene log per entità (ASYNC)
        /// </summary>
        public async Task<List<AuditLog>> GetByEntityAsync(string entita, int? idEntita = null)
        {
            if (idEntita.HasValue)
            {
                return await _context.AuditLogs
                    .Where(x => x.Entita == entita && x.IdEntita == idEntita)
                    .OrderByDescending(x => x.Timestamp)
                    .ToListAsync();
            }
            else
            {
                return await _context.AuditLogs
                    .Where(x => x.Entita == entita)
                    .OrderByDescending(x => x.Timestamp)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Ottiene log per periodo (ASYNC)
        /// </summary>
        public async Task<List<AuditLog>> GetByPeriodAsync(DateTime dataInizio, DateTime dataFine)
        {
            return await _context.AuditLogs
                .Where(x => x.Timestamp >= dataInizio && x.Timestamp <= dataFine)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Ottiene log per azione (ASYNC)
        /// </summary>
        public async Task<List<AuditLog>> GetByActionAsync(string azione)
        {
            return await _context.AuditLogs
                .Where(x => x.Azione == azione)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Cerca nei log (ASYNC)
        /// </summary>
        public async Task<List<AuditLog>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lower = searchTerm.ToLower();
            return await _context.AuditLogs
                .Where(x => 
                    EF.Functions.Like(x.Username.ToLower(), $"%{lower}%") ||
                    EF.Functions.Like(x.Azione.ToLower(), $"%{lower}%") ||
                    EF.Functions.Like(x.Entita.ToLower(), $"%{lower}%") ||
                    (x.Descrizione != null && EF.Functions.Like(x.Descrizione.ToLower(), $"%{lower}%")))
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Conta log totali (ASYNC)
        /// </summary>
        public async Task<int> CountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }

        /// <summary>
        /// Elimina log più vecchi di X giorni (pulizia) (ASYNC)
        /// </summary>
        public async Task<int> DeleteOlderThanAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            var oldLogs = await _context.AuditLogs
                .Where(x => x.Timestamp < cutoffDate)
                .ToListAsync();
            
            if (oldLogs.Count == 0)
                return 0;

            _context.AuditLogs.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();
            
            return oldLogs.Count;
        }

        /// <summary>
        /// Ottiene statistiche log (ASYNC)
        /// </summary>
        public async Task<AuditLogStats> GetStatisticsAsync()
        {
            var allLogs = await _context.AuditLogs.ToListAsync();
            
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

        // ===== WRAPPER SINCRONI PER COMPATIBILITÀ =====

        public void Log(int idUtente, string username, string azione, string entita, 
                       int? idEntita = null, string? descrizione = null, 
                       string? valoriPrecedenti = null, string? valoriNuovi = null)
        {
            LogAsync(idUtente, username, azione, entita, idEntita, descrizione, valoriPrecedenti, valoriNuovi).Wait();
        }

        public void LogFromSession(string azione, string entita, int? idEntita = null, string? descrizione = null)
        {
            LogFromSessionAsync(azione, entita, idEntita, descrizione).Wait();
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
