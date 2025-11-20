-- =====================================================
-- SCRIPT INSERIMENTO UTENTI DEFAULT
-- CGEasy - SQL Server
-- =====================================================

USE CGEasy;
GO

PRINT '=== INSERIMENTO UTENTI DI DEFAULT ===';
GO

-- =====================================================
-- INSERIMENTO UTENTI
-- =====================================================

-- Utente 1: admin (password: 123456)
-- Hash BCrypt: $2a$11$rQZxNqE9LhXvXQH8PqWsqOvP.jYvYGWMGw6b5z5YvHKgzXqLn8sEi
INSERT INTO utenti (username, email, password_hash, nome, cognome, ruolo, attivo)
VALUES (
    'admin',
    'admin@cgeasy.local',
    '$2a$11$rQZxNqE9LhXvXQH8PqWsqOvP.jYvYGWMGw6b5z5YvHKgzXqLn8sEi',
    'Amministratore',
    'Sistema',
    1, -- Administrator
    1  -- Attivo
);

DECLARE @AdminId INT = SCOPE_IDENTITY();
PRINT 'Utente admin creato con ID: ' + CAST(@AdminId AS VARCHAR);

-- Utente 2: admin1 (password: 123123)
-- Hash BCrypt: $2a$11$nM3h7KqY1YvL4GQR5G5q6eGqI4rZK8dXJ7W2rQ3V8xN9Z5mY4f6e.
INSERT INTO utenti (username, email, password_hash, nome, cognome, ruolo, attivo)
VALUES (
    'admin1',
    'admin1@cgeasy.local',
    '$2a$11$nM3h7KqY1YvL4GQR5G5q6eGqI4rZK8dXJ7W2rQ3V8xN9Z5mY4f6e.',
    'Amministratore',
    'Cliente',
    1, -- Administrator
    1  -- Attivo
);

DECLARE @Admin1Id INT = SCOPE_IDENTITY();
PRINT 'Utente admin1 creato con ID: ' + CAST(@Admin1Id AS VARCHAR);
GO

PRINT '';
PRINT '=== INSERIMENTO PERMESSI ===';
GO

-- =====================================================
-- INSERIMENTO PERMESSI UTENTE ADMIN
-- =====================================================

INSERT INTO user_permissions (
    id_utente,
    modulo_todo,
    modulo_bilanci,
    modulo_circolari,
    modulo_controllo_gestione,
    clienti_create,
    clienti_read,
    clienti_update,
    clienti_delete,
    professionisti_create,
    professionisti_read,
    professionisti_update,
    professionisti_delete,
    tipopratiche_create,
    tipopratiche_read,
    tipopratiche_update,
    tipopratiche_delete,
    utenti_manage
)
SELECT 
    Id,
    1, -- modulo_todo
    1, -- modulo_bilanci
    1, -- modulo_circolari
    1, -- modulo_controllo_gestione
    1, -- clienti_create
    1, -- clienti_read
    1, -- clienti_update
    1, -- clienti_delete
    1, -- professionisti_create
    1, -- professionisti_read
    1, -- professionisti_update
    1, -- professionisti_delete
    1, -- tipopratiche_create
    1, -- tipopratiche_read
    1, -- tipopratiche_update
    1, -- tipopratiche_delete
    1  -- utenti_manage
FROM utenti WHERE username = 'admin';

PRINT 'Permessi completi assegnati a admin';

-- =====================================================
-- INSERIMENTO PERMESSI UTENTE ADMIN1
-- =====================================================

INSERT INTO user_permissions (
    id_utente,
    modulo_todo,
    modulo_bilanci,
    modulo_circolari,
    modulo_controllo_gestione,
    clienti_create,
    clienti_read,
    clienti_update,
    clienti_delete,
    professionisti_create,
    professionisti_read,
    professionisti_update,
    professionisti_delete,
    tipopratiche_create,
    tipopratiche_read,
    tipopratiche_update,
    tipopratiche_delete,
    utenti_manage
)
SELECT 
    Id,
    1, -- modulo_todo
    1, -- modulo_bilanci
    1, -- modulo_circolari
    1, -- modulo_controllo_gestione
    1, -- clienti_create
    1, -- clienti_read
    1, -- clienti_update
    1, -- clienti_delete
    1, -- professionisti_create
    1, -- professionisti_read
    1, -- professionisti_update
    1, -- professionisti_delete
    1, -- tipopratiche_create
    1, -- tipopratiche_read
    1, -- tipopratiche_update
    1, -- tipopratiche_delete
    1  -- utenti_manage
FROM utenti WHERE username = 'admin1';

PRINT 'Permessi completi assegnati a admin1';
GO

PRINT '';
PRINT '=== VERIFICA UTENTI CREATI ===';
GO

-- =====================================================
-- VERIFICA
-- =====================================================

SELECT 
    u.Id,
    u.username,
    u.email,
    u.nome,
    u.cognome,
    u.ruolo,
    u.attivo,
    p.modulo_todo,
    p.modulo_bilanci,
    p.modulo_circolari,
    p.modulo_controllo_gestione,
    p.utenti_manage
FROM utenti u
LEFT JOIN user_permissions p ON u.Id = p.id_utente
WHERE u.username IN ('admin', 'admin1')
ORDER BY u.Id;

PRINT '';
PRINT '=== SETUP COMPLETATO ===';
PRINT 'Login disponibili:';
PRINT '  - admin / 123456';
PRINT '  - admin1 / 123123';
GO

