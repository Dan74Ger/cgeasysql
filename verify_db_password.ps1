# Carica le DLL necessarie
$binPath = "src\CGEasy.App\bin\Debug\net8.0-windows"
Add-Type -Path "$binPath\LiteDB.dll"
Add-Type -Path "$binPath\BCrypt.Net-Next.dll"

$dbPath = "C:\ProgramData\CGEasy\cgeasy.db"

if (!(Test-Path $dbPath)) {
    Write-Host "âŒ Database non trovato: $dbPath"
    exit 1
}

try {
    $mapper = New-Object LiteDB.BsonMapper
    $connStr = New-Object LiteDB.ConnectionString
    $connStr.Filename = $dbPath
    $connStr.Connection = [LiteDB.ConnectionType]::Direct
    $connStr.ReadOnly = $true
    
    $db = New-Object LiteDB.LiteDatabase($connStr, $mapper)
    $utenti = $db.GetCollection("utenti")
    $query = [LiteDB.Query]::EQ("username", "admin")
    $admin = $utenti.FindOne($query)
    
    if ($admin -eq $null) {
        Write-Host "âŒ Utente admin non trovato!"
        $db.Dispose()
        exit 1
    }
    
    $passwordHash = $admin["password_hash"].AsString
    Write-Host "âœ… Utente admin trovato!"
    Write-Host "Username: $($admin['username'].AsString)"
    Write-Host "Email: $($admin['email'].AsString)"
    Write-Host "Password Hash: $($passwordHash.Substring(0,30))..."
    Write-Host "Data Modifica: $($admin['data_modifica'].AsDateTime)"
    
    # Verifica password
    $match123456 = [BCrypt.Net.BCrypt]::Verify("123456", $passwordHash)
    $matchAdmin123 = [BCrypt.Net.BCrypt]::Verify("admin123", $passwordHash)
    
    Write-Host "`n=== VERIFICA PASSWORD ==="
    Write-Host "Password '123456' (nuova): $(if ($match123456) {'âœ… CORRETTA'} else {'âŒ ERRATA'})"
    Write-Host "Password 'admin123' (vecchia): $(if ($matchAdmin123) {'âœ… CORRETTA'} else {'âŒ ERRATA'})"
    
    if ($match123456) {
        Write-Host "`nðŸŽ‰ LA NUOVA PASSWORD Ãˆ STATA SALVATA CORRETTAMENTE!"
    } elseif ($matchAdmin123) {
        Write-Host "`nâš ï¸ PROBLEMA: La password vecchia Ã¨ ancora salvata!"
    } else {
        Write-Host "`nâŒ ERRORE: Nessuna password corrisponde!"
    }
    
    $db.Dispose()
} catch {
    Write-Host "âŒ Errore: $($_.Exception.Message)"
    Write-Host "StackTrace: $($_.Exception.StackTrace)"
}
