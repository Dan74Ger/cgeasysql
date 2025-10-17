using CGEasy.Core.Models;
using System;

namespace CGEasy.Core.Services
{
    /// <summary>
    /// Gestione sessione utente corrente (Singleton statico)
    /// </summary>
    public static class SessionManager
    {
        private static Utente? _currentUser;
        private static UserPermissions? _currentPermissions;
        private static DateTime? _loginTime;

        /// <summary>
        /// Utente corrente autenticato
        /// </summary>
        public static Utente? CurrentUser
        {
            get => _currentUser;
            private set => _currentUser = value;
        }

        /// <summary>
        /// Permessi utente corrente
        /// </summary>
        public static UserPermissions? CurrentPermissions
        {
            get => _currentPermissions;
            private set => _currentPermissions = value;
        }

        /// <summary>
        /// Data/ora login
        /// </summary>
        public static DateTime? LoginTime => _loginTime;

        /// <summary>
        /// Verifica se utente è autenticato
        /// </summary>
        public static bool IsAuthenticated => _currentUser != null;

        /// <summary>
        /// Verifica se utente è Administrator
        /// </summary>
        public static bool IsAdministrator => 
            IsAuthenticated && _currentUser!.Ruolo == RuoloUtente.Administrator;

        /// <summary>
        /// Verifica se utente è UserSenior
        /// </summary>
        public static bool IsUserSenior => 
            IsAuthenticated && _currentUser!.Ruolo == RuoloUtente.UserSenior;

        /// <summary>
        /// Nome completo utente corrente
        /// </summary>
        public static string CurrentUserName => 
            IsAuthenticated ? _currentUser!.NomeCompleto : "Guest";

        /// <summary>
        /// Username utente corrente
        /// </summary>
        public static string CurrentUsername => 
            IsAuthenticated ? _currentUser!.Username : string.Empty;

        /// <summary>
        /// Durata sessione corrente
        /// </summary>
        public static TimeSpan SessionDuration => 
            _loginTime.HasValue ? DateTime.UtcNow - _loginTime.Value : TimeSpan.Zero;

        /// <summary>
        /// Inizia nuova sessione dopo login
        /// </summary>
        /// <param name="user">Utente autenticato</param>
        /// <param name="permissions">Permessi utente</param>
        public static void StartSession(Utente user, UserPermissions? permissions)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _currentUser = user;
            _currentPermissions = permissions;
            _loginTime = DateTime.UtcNow;

            // Trigger evento login
            OnUserLoggedIn?.Invoke(null, new UserSessionEventArgs(user));
        }

        /// <summary>
        /// Termina sessione corrente (logout)
        /// </summary>
        public static void EndSession()
        {
            var user = _currentUser;

            _currentUser = null;
            _currentPermissions = null;
            _loginTime = null;

            // Trigger evento logout
            if (user != null)
                OnUserLoggedOut?.Invoke(null, new UserSessionEventArgs(user));
        }

        /// <summary>
        /// Verifica se utente ha accesso a un modulo
        /// </summary>
        /// <param name="moduleName">Nome modulo (todo, bilanci, circolari, controllo)</param>
        /// <returns>True se ha accesso</returns>
        public static bool HasModuleAccess(string moduleName)
        {
            if (!IsAuthenticated || _currentPermissions == null)
                return false;

            // Admin ha accesso a tutto
            if (IsAdministrator)
                return true;

            return _currentPermissions.HasModuleAccess(moduleName);
        }

        /// <summary>
        /// Verifica permesso CRUD su entità
        /// </summary>
        public static bool CanCreate(string entityName)
        {
            if (!IsAuthenticated || _currentPermissions == null)
                return false;

            if (IsAdministrator)
                return true;

            return entityName.ToLower() switch
            {
                "clienti" => _currentPermissions.ClientiCreate,
                "professionisti" => _currentPermissions.ProfessionistiCreate,
                _ => false
            };
        }

        public static bool CanUpdate(string entityName)
        {
            if (!IsAuthenticated || _currentPermissions == null)
                return false;

            if (IsAdministrator)
                return true;

            return entityName.ToLower() switch
            {
                "clienti" => _currentPermissions.ClientiUpdate,
                "professionisti" => _currentPermissions.ProfessionistiUpdate,
                _ => false
            };
        }

        public static bool CanDelete(string entityName)
        {
            if (!IsAuthenticated || _currentPermissions == null)
                return false;

            if (IsAdministrator)
                return true;

            return entityName.ToLower() switch
            {
                "clienti" => _currentPermissions.ClientiDelete,
                "professionisti" => _currentPermissions.ProfessionistiDelete,
                _ => false
            };
        }

        /// <summary>
        /// Verifica se può gestire utenti (solo Admin)
        /// </summary>
        public static bool CanManageUsers => 
            IsAuthenticated && (_currentPermissions?.UtentiManage ?? false);

        /// <summary>
        /// Conta moduli attivi per utente corrente
        /// </summary>
        public static int ActiveModulesCount => 
            _currentPermissions?.ModuliAttivi ?? 0;

        /// <summary>
        /// Ottiene lista moduli accessibili
        /// </summary>
        public static string[] GetAccessibleModules()
        {
            if (!IsAuthenticated || _currentPermissions == null)
                return Array.Empty<string>();

            var modules = new System.Collections.Generic.List<string>();

            if (_currentPermissions.ModuloTodo) modules.Add("TODO Studio");
            if (_currentPermissions.ModuloBilanci) modules.Add("Bilanci");
            if (_currentPermissions.ModuloCircolari) modules.Add("Circolari");
            if (_currentPermissions.ModuloControlloGestione) modules.Add("Controllo Gestione");

            return modules.ToArray();
        }

        // ===== EVENTI =====

        /// <summary>
        /// Evento scatenato al login
        /// </summary>
        public static event EventHandler<UserSessionEventArgs>? OnUserLoggedIn;

        /// <summary>
        /// Evento scatenato al logout
        /// </summary>
        public static event EventHandler<UserSessionEventArgs>? OnUserLoggedOut;

        /// <summary>
        /// Aggiorna permessi utente corrente (dopo modifica permessi)
        /// </summary>
        public static void RefreshPermissions(UserPermissions permissions)
        {
            if (!IsAuthenticated)
                return;

            _currentPermissions = permissions;
        }
    }

    /// <summary>
    /// Event args per eventi sessione utente
    /// </summary>
    public class UserSessionEventArgs : EventArgs
    {
        public Utente User { get; }
        public DateTime Timestamp { get; }

        public UserSessionEventArgs(Utente user)
        {
            User = user;
            Timestamp = DateTime.UtcNow;
        }
    }
}

