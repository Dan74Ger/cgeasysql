using CGEasy.Core.Data;
using CGEasy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Service per autenticazione e gestione sessione utente - SQL Server (EF Core)
    /// </summary>
    public class AuthService
    {
        private readonly CGEasyDbContext _context;

        public AuthService(CGEasyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Autentica utente con username e password (async)
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password in chiaro</param>
        /// <returns>Utente autenticato o null se credenziali errate</returns>
        public async Task<Utente?> LoginAsync(string username, string password)
        {
            var logFile = Path.Combine(Path.GetTempPath(), "CGEasy_Login_Debug.txt");
            
            try
            {
                File.AppendAllText(logFile, $"\n\n=== TENTATIVO LOGIN {DateTime.Now} ===\n");
                File.AppendAllText(logFile, $"Username: '{username}'\n");
                File.AppendAllText(logFile, $"Password length: {password?.Length ?? 0}\n");
                
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    File.AppendAllText(logFile, "❌ Username o password vuoti\n");
                    return null;
                }

                // Cerca utente per username (case-insensitive) - ASYNC
                var utente = await _context.Utenti
                    .FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());

                if (utente == null)
                {
                    File.AppendAllText(logFile, $"❌ Utente '{username}' NON trovato nel DB\n");
                    return null;
                }

                File.AppendAllText(logFile, $"✅ Utente trovato: ID={utente.Id}, Username={utente.Username}\n");
                File.AppendAllText(logFile, $"Hash nel DB: {(string.IsNullOrEmpty(utente.PasswordHash) ? "VUOTO!" : utente.PasswordHash.Substring(0, Math.Min(30, utente.PasswordHash.Length)))}...\n");

                // Verifica se utente è attivo
                if (!utente.Attivo)
                {
                    File.AppendAllText(logFile, "❌ Utente NON attivo\n");
                    return null;
                }

                File.AppendAllText(logFile, "✅ Utente attivo\n");

                // Verifica password con BCrypt
                File.AppendAllText(logFile, $"🔐 Verifico password '{password}' contro hash...\n");
                bool passwordCorrect = BCrypt.Net.BCrypt.Verify(password, utente.PasswordHash);
                File.AppendAllText(logFile, $"Risultato BCrypt.Verify: {passwordCorrect}\n");

                if (!passwordCorrect)
                {
                    File.AppendAllText(logFile, "❌ Password ERRATA\n");
                    return null;
                }

                File.AppendAllText(logFile, "✅ Password CORRETTA - Login riuscito!\n");

                // Aggiorna ultimo accesso - ASYNC
                utente.UltimoAccesso = DateTime.UtcNow;
                _context.Utenti.Update(utente);
                await _context.SaveChangesAsync(); // EF Core async

                return utente;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"❌ ECCEZIONE: {ex.Message}\n{ex.StackTrace}\n");
                return null;
            }
        }

        /// <summary>
        /// Registra nuovo utente (async)
        /// </summary>
        public async Task<int> RegisterAsync(string username, string password, string email, string nome, string cognome, 
                           RuoloUtente ruolo = RuoloUtente.User)
        {
            // Validazioni
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username e password sono obbligatori");

            if (password.Length < 6)
                throw new ArgumentException("La password deve essere di almeno 6 caratteri");

            // Verifica username univoco - ASYNC
            var existingUser = await _context.Utenti
                .FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
            if (existingUser != null)
                return 0; // Username già esistente

            // Crea nuovo utente con password hashata
            var utente = new Utente
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Nome = nome,
                Cognome = cognome,
                Ruolo = ruolo,
                Attivo = true,
                DataCreazione = DateTime.UtcNow,
                DataModifica = DateTime.UtcNow
            };

            _context.Utenti.Add(utente);
            await _context.SaveChangesAsync();

            var userId = utente.Id; // EF popola automaticamente l'ID

            // Crea permessi di default per utente
            var permissions = new UserPermissions
            {
                IdUtente = userId,
                ModuloTodo = ruolo == RuoloUtente.Administrator,
                ModuloBilanci = false,
                ModuloCircolari = false,
                ModuloControlloGestione = false,
                ClientiCreate = false,
                ClientiRead = true,
                ClientiUpdate = false,
                ClientiDelete = false,
                ProfessionistiCreate = false,
                ProfessionistiRead = true,
                ProfessionistiUpdate = false,
                ProfessionistiDelete = false,
                UtentiManage = ruolo == RuoloUtente.Administrator,
                DataCreazione = DateTime.UtcNow,
                DataModifica = DateTime.UtcNow
            };

            _context.UserPermissions.Add(permissions);
            await _context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"✅ Utente {username} creato e salvato (ID: {userId})");

            return userId;
        }

        /// <summary>
        /// Ottiene permessi utente (async)
        /// </summary>
        public async Task<UserPermissions?> GetUserPermissionsAsync(int userId)
        {
            return await _context.UserPermissions
                .FirstOrDefaultAsync(x => x.IdUtente == userId);
        }

        /// <summary>
        /// Verifica se utente ha accesso a un modulo (async)
        /// </summary>
        public async Task<bool> HasModuleAccessAsync(int userId, string moduleName)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            if (permissions == null)
                return false;

            return permissions.HasModuleAccess(moduleName);
        }

        /// <summary>
        /// Attiva/Disattiva utente (async)
        /// </summary>
        public async Task<bool> SetUserActiveAsync(int userId, bool attivo)
        {
            var utente = await _context.Utenti.FindAsync(userId);
            if (utente == null)
                return false;

            utente.Attivo = attivo;
            utente.DataModifica = DateTime.UtcNow;

            if (!attivo)
                utente.DataCessazione = DateTime.UtcNow;

            _context.Utenti.Update(utente);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Ottiene tutti gli utenti (async)
        /// </summary>
        public async Task<List<Utente>> GetAllUsersAsync(bool includeInactive = false)
        {
            if (includeInactive)
                return await _context.Utenti.ToListAsync();

            return await _context.Utenti.Where(x => x.Attivo).ToListAsync();
        }

        /// <summary>
        /// Conta utenti per ruolo (async)
        /// </summary>
        public async Task<(int Admins, int UserSenior, int Users)> CountUsersByRoleAsync()
        {
            var all = await _context.Utenti.Where(x => x.Attivo).ToListAsync();
            return (
                Admins: all.Count(x => x.Ruolo == RuoloUtente.Administrator),
                UserSenior: all.Count(x => x.Ruolo == RuoloUtente.UserSenior),
                Users: all.Count(x => x.Ruolo == RuoloUtente.User)
            );
        }

        /// <summary>
        /// Trova utente per username (async)
        /// </summary>
        public async Task<Utente?> FindUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.Utenti
                .FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Modifica dati utente esistente (async)
        /// </summary>
        public async Task<bool> UpdateUserAsync(int userId, string email, string nome, string cognome, RuoloUtente ruolo, bool attivo)
        {
            var utente = await _context.Utenti.FindAsync(userId);
            if (utente == null)
                return false;

            utente.Email = email;
            utente.Nome = nome;
            utente.Cognome = cognome;
            utente.Ruolo = ruolo;
            utente.Attivo = attivo;
            utente.DataModifica = DateTime.UtcNow;

            if (!attivo && utente.DataCessazione == null)
                utente.DataCessazione = DateTime.UtcNow;

            _context.Utenti.Update(utente);
            var result = await _context.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"✅ Utente ID {userId} aggiornato e salvato");
            
            return result > 0;
        }

        /// <summary>
        /// Cambia password (async)
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var logFile = Path.Combine(Path.GetTempPath(), "CGEasy_ChangePassword_Debug.txt");
            
            try
            {
                File.AppendAllText(logFile, $"\n\n=== CAMBIO PASSWORD {DateTime.Now} ===\n");
                
                if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                {
                    File.AppendAllText(logFile, "❌ Password troppo corta\n");
                    throw new ArgumentException("La password deve essere di almeno 6 caratteri");
                }

                var utente = await _context.Utenti.FindAsync(userId);
                if (utente == null)
                {
                    File.AppendAllText(logFile, $"❌ Utente ID {userId} non trovato!\n");
                    return false;
                }

                var oldHash = utente.PasswordHash;
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                
                File.AppendAllText(logFile, 
                    $"UserId: {userId}\n" +
                    $"Username: {utente.Username}\n" +
                    $"Vecchio Hash: {oldHash.Substring(0, 30)}...\n" +
                    $"Nuovo Hash: {newPasswordHash.Substring(0, 30)}...\n");
                
                // Aggiorna password e data modifica
                utente.PasswordHash = newPasswordHash;
                utente.DataModifica = DateTime.UtcNow;

                // Salva nel database - ASYNC
                _context.Utenti.Update(utente);
                var result = await _context.SaveChangesAsync();
                
                File.AppendAllText(logFile, $"SaveChanges returned: {result}\n");
                
                if (result > 0)
                {
                    // Verifica che sia stata salvata
                    var verificaUtente = await _context.Utenti.FindAsync(userId);
                    if (verificaUtente != null)
                    {
                        var hashSalvato = verificaUtente.PasswordHash;
                        bool hashCorretto = (hashSalvato == newPasswordHash);
                        bool passwordValida = BCrypt.Net.BCrypt.Verify(newPassword, hashSalvato);
                        
                        File.AppendAllText(logFile,
                            $"\n=== VERIFICA POST-SALVATAGGIO ===\n" +
                            $"Hash nel DB: {hashSalvato.Substring(0, 30)}...\n" +
                            $"Hash atteso: {newPasswordHash.Substring(0, 30)}...\n" +
                            $"Hash Match: {hashCorretto}\n" +
                            $"Password valida: {passwordValida}\n" +
                            $"✅ SUCCESSO!\n");
                        
                        return hashCorretto;
                    }
                }
                
                File.AppendAllText(logFile, "❌ SaveChanges fallito!\n");
                return false;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"❌ ECCEZIONE: {ex.Message}\n{ex.StackTrace}\n");
                return false;
            }
        }

        /// <summary>
        /// Salva permessi utente (async)
        /// </summary>
        public async Task<bool> SaveUserPermissionsAsync(UserPermissions permissions)
        {
            if (permissions == null || permissions.IdUtente == 0)
                return false;

            permissions.DataModifica = DateTime.UtcNow;

            // Se esiste già un record, aggiorna; altrimenti, inserisci - ASYNC
            var existing = await _context.UserPermissions
                .FirstOrDefaultAsync(x => x.IdUtente == permissions.IdUtente);
            
            if (existing != null)
            {
                permissions.Id = existing.Id;
                permissions.DataCreazione = existing.DataCreazione;
                _context.UserPermissions.Update(permissions);
            }
            else
            {
                permissions.DataCreazione = DateTime.UtcNow;
                _context.UserPermissions.Add(permissions);
            }
            
            return await _context.SaveChangesAsync() > 0;
        }

        // ===== METODI SINCRONIZZATI TEMPORANEI (per retrocompatibilità) =====
        // Questi metodi verranno rimossi quando tutti i chiamanti saranno async

        public Utente? Login(string username, string password)
            => LoginAsync(username, password).GetAwaiter().GetResult();

        public UserPermissions? GetUserPermissions(int userId)
            => GetUserPermissionsAsync(userId).GetAwaiter().GetResult();

        public int Register(string username, string password, string email, string nome, string cognome, RuoloUtente ruolo = RuoloUtente.User)
            => RegisterAsync(username, password, email, nome, cognome, ruolo).GetAwaiter().GetResult();

        public List<Utente> GetAllUsers(bool includeInactive = false)
            => GetAllUsersAsync(includeInactive).GetAwaiter().GetResult();

        public (int Admins, int UserSenior, int Users) CountUsersByRole()
            => CountUsersByRoleAsync().GetAwaiter().GetResult();

        public Utente? FindUserByUsername(string username)
            => FindUserByUsernameAsync(username).GetAwaiter().GetResult();

        public bool SetUserActive(int userId, bool attivo)
            => SetUserActiveAsync(userId, attivo).GetAwaiter().GetResult();

        public bool UpdateUser(int userId, string email, string nome, string cognome, RuoloUtente ruolo, bool attivo)
            => UpdateUserAsync(userId, email, nome, cognome, ruolo, attivo).GetAwaiter().GetResult();

        public bool ChangePassword(int userId, string newPassword)
            => ChangePasswordAsync(userId, newPassword).GetAwaiter().GetResult();

        public bool SaveUserPermissions(UserPermissions permissions)
            => SaveUserPermissionsAsync(permissions).GetAwaiter().GetResult();
    }
}
