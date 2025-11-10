; ========================================
; Script Inno Setup per CGEasy
; ========================================
; Questo script crea un installer professionale per CGEasy
; Include: applicazione, database, licenze, configurazione

#define MyAppName "CGEasy"
#define MyAppVersion "2.0"
#define MyAppPublisher "Dott. Geron Daniele"
#define MyAppURL "https://github.com/Dan74Ger/CGEasy"
#define MyAppExeName "CGEasy.App.exe"

[Setup]
; Informazioni applicazione
AppId={{8F7A3B2C-1D4E-5F6A-9B8C-7D6E5F4A3B2C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Percorsi installazione
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Output
OutputDir=installer_output
OutputBaseFilename=CGEasy_Setup_v{#MyAppVersion}
SetupIconFile=src\CGEasy.App\Images\logo.ico
Compression=lzma2/max
SolidCompression=yes

; Privilegi
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Interfaccia
WizardStyle=modern
DisableWelcomePage=no
LicenseFile=LICENSE.txt
InfoBeforeFile=INSTALLAZIONE_DA_ZERO.md

; Opzioni
AllowNoIcons=yes
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"

[Tasks]
Name: "desktopicon"; Description: "Crea un'icona sul desktop"; GroupDescription: "Icone aggiuntive:"
Name: "startupicon"; Description: "Avvia CGEasy all'avvio di Windows"; GroupDescription: "Opzioni avvio:"

[Files]
; File applicazione (binari compilati)
Source: "src\CGEasy.App\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; File documentazione
Source: "README.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "INSTALLAZIONE_DA_ZERO.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "GUIDA_DATABASE.md"; DestDir: "{app}\Docs"; Flags: ignoreversion

; Script di supporto
Source: "prepara_installazione.ps1"; DestDir: "{app}\Scripts"; Flags: ignoreversion
Source: "gestione_task.bat"; DestDir: "{app}\Scripts"; Flags: ignoreversion

[Dirs]
; Crea cartella database
Name: "C:\db_CGEASY"; Permissions: users-full
Name: "C:\db_CGEASY\Backups"; Permissions: users-full
Name: "C:\db_CGEASY\Logs"; Permissions: users-full
Name: "C:\db_CGEASY\Allegati"; Permissions: users-full

[Icons]
; Icona menu Start
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Documentazione"; Filename: "{app}\Docs"
Name: "{group}\Database"; Filename: "C:\db_CGEASY"
Name: "{group}\Disinstalla {#MyAppName}"; Filename: "{uninstallexe}"

; Icona desktop (opzionale)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; Icona avvio automatico (opzionale)
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
; Crea database vuoto con solo admin e admin1 al primo avvio
Filename: "{app}\{#MyAppExeName}"; Description: "Avvia {#MyAppName} ora"; Flags: nowait postinstall skipifsilent
Filename: "explorer.exe"; Parameters: "C:\db_CGEASY"; Description: "Apri cartella database"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
; NON eliminare il database durante la disinstallazione (sicurezza dati)
Type: filesandordirs; Name: "{app}\Logs"
Type: filesandordirs; Name: "{app}\Temp"

[Code]
var
  DatabasePage: TInputOptionWizardPage;
  SharedInstallPage: TInputQueryWizardPage;

procedure InitializeWizard;
begin
  { Pagina 1: Opzioni Database }
  DatabasePage := CreateInputOptionPage(wpSelectTasks,
    'Configurazione Database', 'Seleziona le opzioni per il database',
    'Il database verrà creato in C:\db_CGEASY\ con le seguenti opzioni:',
    True, False);
  DatabasePage.Add('Database vuoto (solo utenti admin e admin1)');
  DatabasePage.Add('Criptazione automatica con password master');
  DatabasePage.Values[0] := True;
  DatabasePage.Values[1] := True;

  { Pagina 2: Installazione Multi-PC (opzionale) }
  SharedInstallPage := CreateInputQueryPage(DatabasePage.ID,
    'Configurazione Rete (Opzionale)', 'Vuoi condividere il database in rete locale?',
    'Se installi su più PC, puoi condividere il database per accesso multi-utente.');
  SharedInstallPage.Add('Nome condivisione (es: CGEasy):', False);
  SharedInstallPage.Values[0] := 'CGEasy';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
  ShareName: String;
  SharePath: String;
begin
  Result := True;

  { Dopo la selezione dei task, configura la condivisione }
  if CurPageID = SharedInstallPage.ID then
  begin
    ShareName := SharedInstallPage.Values[0];
    SharePath := 'C:\db_CGEASY';
    
    if ShareName <> '' then
    begin
      { Crea condivisione di rete usando net share }
      if MsgBox('Vuoi creare la condivisione di rete "' + ShareName + '"?' + #13#10 +
                'Questo permetterà ad altri PC di accedere al database.' + #13#10#13#10 +
                'Percorso: \\' + ExpandConstant('{computername}') + '\' + ShareName,
                mbConfirmation, MB_YESNO) = IDYES then
      begin
        { Rimuovi condivisione esistente }
        Exec('net.exe', 'share ' + ShareName + ' /delete', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        
        { Crea nuova condivisione }
        if Exec('net.exe', 'share ' + ShareName + '=' + SharePath + ' /grant:everyone,FULL', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
        begin
          MsgBox('Condivisione creata con successo!' + #13#10#13#10 +
                 'I client possono accedere al database tramite:' + #13#10 +
                 '\\' + ExpandConstant('{computername}') + '\' + ShareName + '\cgeasy.db',
                 mbInformation, MB_OK);
        end
        else
        begin
          MsgBox('ATTENZIONE: Impossibile creare la condivisione.' + #13#10 +
                 'Potrebbe essere necessario configurarla manualmente.', 
                 mbError, MB_OK);
        end;
      end;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  LicensesJson: String;
begin
  if CurStep = ssPostInstall then
  begin
    { Crea file licenses.json se non esiste }
    if not FileExists('C:\db_CGEASY\licenses.json') then
    begin
      LicensesJson := '{"LicenseInfo":{"ProductName":"CGEasy","Version":"1.0.0","LicenseType":"Trial","IsActivated":false},"Licenses":[]}';
      SaveStringToFile('C:\db_CGEASY\licenses.json', LicensesJson, False);
    end;
    
    { NOTA: db.key e cgeasy.db verranno creati automaticamente al primo avvio dell'applicazione }
    { L'app rileva se il database non esiste e lo crea pulito con admin/admin1 }
  end;
end;

procedure DeinitializeSetup();
var
  ButtonPressed: Integer;
begin
  { Messaggio finale con credenziali }
  if not WizardSilent then
  begin
    ButtonPressed := MsgBox(
      'Installazione completata con successo!' + #13#10#13#10 +
      '==================================' + #13#10 +
      'CREDENZIALI DI ACCESSO' + #13#10 +
      '==================================' + #13#10 +
      'Username: admin1' + #13#10 +
      'Password: 123123' + #13#10#13#10 +
      'Database: C:\db_CGEASY\cgeasy.db' + #13#10 +
      'Password DB: Woodstockac@74' + #13#10#13#10 +
      'IMPORTANTE: Annota queste credenziali!',
      mbInformation, MB_OK);
  end;
end;

[Registry]
; Registra percorso database (opzionale, per configurazioni avanzate)
Root: HKLM; Subkey: "Software\{#MyAppName}"; ValueType: string; ValueName: "DatabasePath"; ValueData: "C:\db_CGEASY\cgeasy.db"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey

[Messages]
; Messaggi personalizzati in italiano
italian.WelcomeLabel2=Questo installerà [name/ver] sul tuo computer.%n%nCGEasy è un software gestionale completo per studi professionali.%n%nSi consiglia di chiudere tutte le altre applicazioni prima di continuare.
italian.FinishedLabel=CGEasy è stato installato con successo!%n%nPer avviare l'applicazione, usa l'icona sul desktop o dal menu Start.%n%nCredenziali di accesso:%nUsername: admin1%nPassword: 123123

