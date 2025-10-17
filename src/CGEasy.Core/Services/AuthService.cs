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
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            // Cerca utente per username (case-insensitive)
            var utente = _context.Utenti
                .FindOne(x => x.Username.ToLower() == username.ToLower());

            if (utente == null)
                return null;

            // Verifica se utente è attivo
            if (!utente.Attivo)
                return null;

            // Verifica password con BCrypt
            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(password, utente.PasswordHash);

            if (!passwordCorrect)
                return null;

            // Aggiorna ultimo accesso
            utente.UltimoAccesso = DateTime.UtcNow;
            _context.Utenti.Update(utente);

            return utente;
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

            return userId;
        }

        /// <summary>
        /// Cambia password utente
        /// </summary>
        /// <param name="userId">ID utente</param>
        /// <param name="oldPassword">Vecchia password</param>
        /// <param name="newPassword">Nuova password</param>
        /// <returns>True se password cambiata con successo</returns>
        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ArgumentException("La nuova password deve essere di almeno 6 caratteri");

            var utente = _context.Utenti.FindById(userId);
            if (utente == null)
                return false;

            // Verifica vecchia password
            bool oldPasswordCorrect = BCrypt.Net.BCrypt.Verify(oldPassword, utente.PasswordHash);
            if (!oldPasswordCorrect)
                return false;

            // Aggiorna con nuova password hashata
            utente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            utente.DataModifica = DateTime.UtcNow;

            return _context.Utenti.Update(utente);
        }

        /// <summary>
        /// Reset password (solo per admin)
        /// </summary>
        /// <param name="userId">ID utente</param>
        /// <param name="newPassword">Nuova password</param>
        /// <returns>True se reset effettuato</returns>
        public bool ResetPassword(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ArgumentException("La password deve essere di almeno 6 caratteri");

            var utente = _context.Utenti.FindById(userId);
            if (utente == null)
                return false;

            utente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            utente.DataModifica = DateTime.UtcNow;

            return _context.Utenti.Update(utente);
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

            return _context.Utenti.Update(utente);
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
    }
}

