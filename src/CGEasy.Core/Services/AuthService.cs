using CGEasy.Core.Data;
using CGEasy.Core.Models;
using System;
using System.Linq;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Service per autenticazione e gestione sessione utente
    /// </summary>
    public class AuthService
    {
        private readonly LiteDbContext _context;

        public AuthService(LiteDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Autentica utente con username e password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password in chiaro</param>
        /// <returns>Utente autenticato o null se credenziali errate</returns>
        public Utente? Login(string username, string password)
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

                // Cerca utente per username (case-insensitive)
                var utente = _context.Utenti
                    .FindOne(x => x.Username.ToLower() == username.ToLower());

                if (utente == null)
                {
                    File.AppendAllText(logFile, $"❌ Utente '{username}' NON trovato nel DB\n");
                    return null;
                }

                File.AppendAllText(logFile, $"✅ Utente trovato: ID={utente.Id}, Username={utente.Username}\n");
                File.AppendAllText(logFile, $"Hash nel DB: {utente.PasswordHash.Substring(0, 30)}...\n");

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

                // Aggiorna ultimo accesso
                utente.UltimoAccesso = DateTime.UtcNow;
                _context.Utenti.Update(utente);
                _context.Checkpoint(); // Forza salvataggio

                return utente;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"❌ ECCEZIONE: {ex.Message}\n{ex.StackTrace}\n");
                return null;
            }
        }

        /// <summary>
        /// Registra nuovo utente
        /// </summary>
        /// <param name="username">Username (univoco)</param>
        /// <param name="password">Password in chiaro (verrà hashata)</param>
        /// <param name="email">Email</param>
        /// <param name="nome">Nome</param>
        /// <param name="cognome">Cognome</param>
        /// <param name="ruolo">Ruolo utente</param>
        /// <returns>ID utente creato o 0 se username già esistente</returns>
        public int Register(string username, string password, string email, string nome, string cognome, 
                           RuoloUtente ruolo = RuoloUtente.User)
        {
            // Validazioni
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username e password sono obbligatori");

            if (password.Length < 6)
                throw new ArgumentException("La password deve essere di almeno 6 caratteri");

            // Verifica username univoco
            var existingUser = _context.Utenti.FindOne(x => x.Username.ToLower() == username.ToLower());
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

            var userId = _context.Utenti.Insert(utente);

            // Crea permessi di default per utente
            var permissions = new UserPermissions
            {
                IdUtente = userId,
                ModuloTodo = ruolo == RuoloUtente.Administrator, // Admin ha tutto attivo di default
                ModuloBilanci = false,
                ModuloCircolari = false,
                ModuloControlloGestione = false,
                ClientiCreate = false,
                ClientiRead = true, // Tutti possono leggere clienti
                ClientiUpdate = false,
                ClientiDelete = false,
                ProfessionistiCreate = false,
                ProfessionistiRead = true,
                ProfessionistiUpdate = false,
                ProfessionistiDelete = false,
                UtentiManage = ruolo == RuoloUtente.Administrator, // Solo admin gestisce utenti
                DataCreazione = DateTime.UtcNow,
                DataModifica = DateTime.UtcNow
            };

            _context.UserPermissions.Insert(permissions);

            // FORZA checkpoint per salvare immediatamente
            _context.Checkpoint();
            System.Diagnostics.Debug.WriteLine($"✅ Utente {username} creato e salvato (ID: {userId})");

            return userId;
        }


        /// <summary>
        /// Ottiene permessi utente
        /// </summary>
        /// <param name="userId">ID utente</param>
        /// <returns>Permessi utente o null</returns>
        public UserPermissions? GetUserPermissions(int userId)
        {
            return _context.UserPermissions.FindOne(x => x.IdUtente == userId);
        }

        /// <summary>
        /// Verifica se utente ha accesso a un modulo
        /// </summary>
        /// <param name="userId">ID utente</param>
        /// <param name="moduleName">Nome modulo (todo, bilanci, circolari, controllo)</param>
        /// <returns>True se ha accesso</returns>
        public bool HasModuleAccess(int userId, string moduleName)
        {
            var permissions = GetUserPermissions(userId);
            if (permissions == null)
                return false;

            return permissions.HasModuleAccess(moduleName);
        }

        /// <summary>
        /// Attiva/Disattiva utente
        /// </summary>
        /// <param name="userId">ID utente</param>
        /// <param name="attivo">True per attivare, False per disattivare</param>
        /// <returns>True se operazione riuscita</returns>
        public bool SetUserActive(int userId, bool attivo)
        {
            var utente = _context.Utenti.FindById(userId);
            if (utente == null)
                return false;

            utente.Attivo = attivo;
            utente.DataModifica = DateTime.UtcNow;

            if (!attivo)
                utente.DataCessazione = DateTime.UtcNow;

            var success = _context.Utenti.Update(utente);
            
            if (success)
            {
                _context.Checkpoint(); // Forza salvataggio
            }
            
            return success;
        }

        /// <summary>
        /// Ottiene tutti gli utenti
        /// </summary>
        /// <param name="includeInactive">Include utenti disattivati</param>
        /// <returns>Lista utenti</returns>
        public IEnumerable<Utente> GetAllUsers(bool includeInactive = false)
        {
            if (includeInactive)
                return _context.Utenti.FindAll();

            return _context.Utenti.Find(x => x.Attivo);
        }

        /// <summary>
        /// Conta utenti per ruolo
        /// </summary>
        public (int Admins, int UserSenior, int Users) CountUsersByRole()
        {
            var all = _context.Utenti.FindAll().ToList();
            return (
                Admins: all.Count(x => x.Ruolo == RuoloUtente.Administrator && x.Attivo),
                UserSenior: all.Count(x => x.Ruolo == RuoloUtente.UserSenior && x.Attivo),
                Users: all.Count(x => x.Ruolo == RuoloUtente.User && x.Attivo)
            );
        }

        /// <summary>
        /// Trova utente per username
        /// </summary>
        public Utente? FindUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return _context.Utenti.FindOne(x => x.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Modifica dati utente esistente
        /// </summary>
        public bool UpdateUser(int userId, string email, string nome, string cognome, RuoloUtente ruolo, bool attivo)
        {
            var utente = _context.Utenti.FindById(userId);
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

            var success = _context.Utenti.Update(utente);
            
            if (success)
            {
                // FORZA checkpoint per salvare immediatamente
                _context.Checkpoint();
                System.Diagnostics.Debug.WriteLine($"✅ Utente ID {userId} aggiornato e salvato");
            }
            
            return success;
        }

        /// <summary>
        /// Cambia password (solo nuova password - per admin che resetta)
        /// </summary>
        public bool ChangePassword(int userId, string newPassword)
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

                var utente = _context.Utenti.FindById(userId);
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

                // Salva nel database
                var success = _context.Utenti.Update(utente);
                File.AppendAllText(logFile, $"Update returned: {success}\n");
                
                if (success)
                {
                    // FORZA checkpoint per salvare immediatamente su disco
                    _context.Checkpoint();
                    File.AppendAllText(logFile, "Checkpoint eseguito\n");
                    
                    // Verifica che sia stata salvata
                    var verificaUtente = _context.Utenti.FindById(userId);
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
                
                File.AppendAllText(logFile, "❌ Update fallito!\n");
                return false;
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, $"❌ ECCEZIONE: {ex.Message}\n{ex.StackTrace}\n");
                return false;
            }
        }

        /// <summary>
        /// Salva permessi utente
        /// </summary>
        public bool SaveUserPermissions(UserPermissions permissions)
        {
            if (permissions == null || permissions.IdUtente == 0)
                return false;

            permissions.DataModifica = DateTime.UtcNow;

            // Se esiste già un record, aggiorna; altrimenti, inserisci
            var existing = _context.UserPermissions.FindOne(x => x.IdUtente == permissions.IdUtente);
            
            bool success;
            if (existing != null)
            {
                permissions.Id = existing.Id;
                permissions.DataCreazione = existing.DataCreazione; // Mantieni data creazione originale
                success = _context.UserPermissions.Update(permissions);
            }
            else
            {
                permissions.DataCreazione = DateTime.UtcNow;
                _context.UserPermissions.Insert(permissions);
                success = true;
            }
            
            if (success)
            {
                _context.Checkpoint(); // Forza salvataggio
            }
            
            return success;
        }
    }
}

