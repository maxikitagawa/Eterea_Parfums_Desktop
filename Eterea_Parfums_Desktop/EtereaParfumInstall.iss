#define MyAppName "Eterea Desktop"
#define MyAppVer  "1.0.0"
#define MyCompany "EtereaParfums"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVer}
; Icono del instalador
SetupIconFile="{#SourcePath}\assets\LogoEterea.ico"
; (opcional) Icono que muestra el desinstalador en Agregar/Quitar programas
UninstallDisplayIcon="{app}\Eterea_Parfums_Desktop.exe"

; Instalación por equipo (requiere admin)
PrivilegesRequired=admin
DefaultDirName={pf}\{#MyCompany}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=.
ArchitecturesInstallIn64BitMode=x64
Compression=lzma2
SolidCompression=yes

[Files]
; === TU APP ===
; Si compilas x64 a bin\x64\Release, cambiá esta ruta por "{#SourcePath}\bin\x64\Release\*"
Source: "{#SourcePath}\bin\Release\*"; DestDir: "{app}"; Flags: recursesubdirs

; === ICONO PARA ACCESOS (opcional, si querés forzar un icono específico) ===
Source: "{#SourcePath}\assets\LogoEterea.ico"; DestDir: "{app}"; Flags: ignoreversion

; === COPIA DE RECURSOS INICIALES A LA RUTA BASE ELEGIDA ===
; Si querés NO sobrescribir archivos existentes, agregá la flag "onlyifdoesntexist"
Source: "{#SourcePath}\assets\InitialResources\*"; DestDir: "{code:GetRutaBase}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Tasks]
Name: "desktopicon"; Description: "Crear acceso directo en el escritorio"; GroupDescription: "Accesos directos:"

[Icons]
; Menú Inicio (si tu EXE ya tiene icono, podés omitir IconFilename)
Name: "{group}\Eterea Desktop"; Filename: "{app}\Eterea_Parfums_Desktop.exe"; IconFilename: "{app}\LogoEterea.ico"
; Escritorio
Name: "{commondesktop}\Eterea Desktop"; Filename: "{app}\Eterea_Parfums_Desktop.exe"; IconFilename: "{app}\LogoEterea.ico"; Tasks: desktopicon

[Dirs]
; Carpeta compartida para guardar el config.json
Name: "{commonappdata}\{#MyCompany}\EtereaDesktop"

[Code]
var
  // P1: Sucursal
  SucPage: TInputQueryWizardPage;

  // P2: Ruta Base (selector de carpeta)
  RutaPage: TInputDirWizardPage;

  // P3: Modo BD
  ModePage: TWizardPage;
  RbLocal, RbServer: TRadioButton;

  // P4: Local
  LocalPage: TWizardPage;
  CbUseLocalDB: TNewCheckBox;
  EdAttachPath: TNewEdit;
  LblAttach, LblLocalHint: TNewStaticText;

  // P5: Servidor
  ServerPage: TWizardPage;
  EdServer, EdDB, EdUser, EdPass: TNewEdit;
  LblServer, LblDB, LblUser, LblPass, LblSrvHint: TNewStaticText;
  CbIntegrated: TNewCheckBox;

function IsPositiveInt(const S: string): Boolean;
var I: Integer;
begin
  Result := TryStrToInt(S, I) and (I > 0);
end;

// === Devuelve la Ruta Base elegida por el usuario (usada en [Files]) ===
function GetRutaBase(Param: string): string;
begin
  Result := RutaPage.Values[0];
  if Result = '' then
    Result := ExpandConstant('{commonappdata}\{#MyCompany}\EtereaDesktop');
end;

procedure InitializeWizard;
var
  defaultDataDir: string;
begin
  // --- P1: Sucursal
  SucPage := CreateInputQueryPage(wpSelectDir,
    'Número de sucursal',
    'Elegí el número de sucursal para esta PC',
    'Este valor se guardará en config.json (compartido por todos los usuarios de la PC).');
  SucPage.Add('Sucursal (entero > 0):', False);
  SucPage.Values[0] := '1';

  // --- P2: Ruta Base (por defecto ProgramData\EtereaParfums\EtereaDesktop)
  defaultDataDir := ExpandConstant('{commonappdata}\{#MyCompany}\EtereaDesktop');
  RutaPage := CreateInputDirWizardPage(SucPage.ID,
    'Carpeta de datos (Ruta Base)',
    'Elegí dónde guardar los archivos locales de la aplicación',
    'Recomendado: ProgramData (compartido por todos los usuarios de esta PC).', False, '');
  RutaPage.Add(defaultDataDir);
  RutaPage.Values[0] := defaultDataDir;

  // --- P3: Modo BD
  ModePage := CreateCustomPage(RutaPage.ID, 'Origen de base de datos',
    'Elegí cómo conectará la aplicación a la base de datos');
  RbLocal := TNewRadioButton.Create(ModePage);
  RbLocal.Parent := ModePage.Surface;
  RbLocal.Top := 16; RbLocal.Left := 0; RbLocal.Width := ScaleX(420);
  RbLocal.Caption := 'Local (SQL Express o LocalDB en esta PC)';
  RbLocal.Checked := True;

  RbServer := TNewRadioButton.Create(ModePage);
  RbServer.Parent := ModePage.Surface;
  RbServer.Top := RbLocal.Top + 24; RbServer.Left := 0; RbServer.Width := ScaleX(420);
  RbServer.Caption := 'Servidor (SQL Server remoto o de la red)';

  // --- P4: Local
  LocalPage := CreateCustomPage(ModePage.ID, 'Configuración local',
    'Elegí entre SQL Express o LocalDB');
  CbUseLocalDB := TNewCheckBox.Create(LocalPage);
  CbUseLocalDB.Parent := LocalPage.Surface;
  CbUseLocalDB.Top := 12; CbUseLocalDB.Left := 0; CbUseLocalDB.Width := ScaleX(520);
  CbUseLocalDB.Caption := 'Usar LocalDB con archivo .mdf adjunto';

  LblAttach := TNewStaticText.Create(LocalPage);
  LblAttach.Parent := LocalPage.Surface;
  LblAttach.Top := CbUseLocalDB.Top + 24; LblAttach.Left := 0;
  LblAttach.Caption := 'Ruta del .mdf (opcional):';

  EdAttachPath := TNewEdit.Create(LocalPage);
  EdAttachPath.Parent := LocalPage.Surface;
  EdAttachPath.Top := LblAttach.Top + 16; EdAttachPath.Left := 0; EdAttachPath.Width := ScaleX(520);

  LblLocalHint := TNewStaticText.Create(LocalPage);
  LblLocalHint.Parent := LocalPage.Surface;
  LblLocalHint.Top := EdAttachPath.Top + 28; LblLocalHint.Left := 0; LblLocalHint.Width := ScaleX(540);
  LblLocalHint.Caption :=
    '• SQL Express local: Server=.\SQLEXPRESS; Database=eterea; Integrated Security=True' + #13#10 +
    '• LocalDB: Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename=<ruta>; Integrated Security=True';

  // --- P5: Servidor
  ServerPage := CreateCustomPage(LocalPage.ID, 'Configuración de servidor',
    'Datos de conexión a SQL Server');
  LblServer := TNewStaticText.Create(ServerPage);
  LblServer.Parent := ServerPage.Surface;
  LblServer.Top := 8; LblServer.Left := 0;
  LblServer.Caption := 'Servidor/Instancia (ej: SRVSQL01 o .\SQLEXPRESS):';

  EdServer := TNewEdit.Create(ServerPage);
  EdServer.Parent := ServerPage.Surface;
  EdServer.Top := LblServer.Top + 16; EdServer.Left := 0; EdServer.Width := ScaleX(420);
  EdServer.Text := '.\SQLEXPRESS';

  LblDB := TNewStaticText.Create(ServerPage);
  LblDB.Parent := ServerPage.Surface;
  LblDB.Top := EdServer.Top + 28; LblDB.Left := 0;
  LblDB.Caption := 'Base de datos:';

  EdDB := TNewEdit.Create(ServerPage);
  EdDB.Parent := ServerPage.Surface;
  EdDB.Top := LblDB.Top + 16; EdDB.Left := 0; EdDB.Width := ScaleX(420);
  EdDB.Text := 'eterea';

  CbIntegrated := TNewCheckBox.Create(ServerPage);
  CbIntegrated.Parent := ServerPage.Surface;
  CbIntegrated.Top := EdDB.Top + 28; CbIntegrated.Left := 0; CbIntegrated.Width := ScaleX(520);
  CbIntegrated.Caption := 'Usar autenticación de Windows (Integrated Security)';
  CbIntegrated.Checked := True;

  LblUser := TNewStaticText.Create(ServerPage);
  LblUser.Parent := ServerPage.Surface;
  LblUser.Top := CbIntegrated.Top + 28; LblUser.Left := 0;
  LblUser.Caption := 'Usuario (SQL Auth):';

  EdUser := TNewEdit.Create(ServerPage);
  EdUser.Parent := ServerPage.Surface;
  EdUser.Top := LblUser.Top + 16; EdUser.Left := 0; EdUser.Width := ScaleX(420);

  LblPass := TNewStaticText.Create(ServerPage);
  LblPass.Parent := ServerPage.Surface;
  LblPass.Top := EdUser.Top + 28; LblPass.Left := 0;
  LblPass.Caption := 'Password (SQL Auth):';

  EdPass := TNewEdit.Create(ServerPage);
  EdPass.Parent := ServerPage.Surface;
  EdPass.Top := LblPass.Top + 16; EdPass.Left := 0; EdPass.Width := ScaleX(420);
  EdPass.Password := True;

  LblSrvHint := TNewStaticText.Create(ServerPage);
  LblSrvHint.Parent := ServerPage.Surface;
  LblSrvHint.Top := EdPass.Top + 28; LblSrvHint.Left := 0; LblSrvHint.Width := ScaleX(540);
  LblSrvHint.Caption :=
    'Si usás Windows Auth, dejá usuario y clave vacíos. La app usa Encrypt/TrustServerCertificate para evitar errores TLS.';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  if CurPageID = SucPage.ID then
  begin
    if not IsPositiveInt(SucPage.Values[0]) then
    begin
      MsgBox('Ingresá un número de sucursal válido (entero > 0).', mbError, MB_OK);
      Result := False;
    end;
  end
  else if CurPageID = ServerPage.ID then
  begin
    if RbServer.Checked then
    begin
      if EdServer.Text = '' then
      begin
        MsgBox('Ingresá el nombre del servidor/instancia.', mbError, MB_OK);
        Result := False;
      end;
      if EdDB.Text = '' then
      begin
        MsgBox('Ingresá el nombre de la base de datos.', mbError, MB_OK);
        Result := False;
      end;
      if not CbIntegrated.Checked then
      begin
        if (EdUser.Text = '') or (EdPass.Text = '') then
        begin
          MsgBox('Para SQL Authentication, completá usuario y contraseña.', mbError, MB_OK);
          Result := False;
        end;
      end;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ProgramDataCfgDir, JsonPath, Json, Mode, DataSource, Database, UserId, Password, RutaBase: string;
  Integrated, UseLocalDb: Boolean;
  AttachDb: string;
begin
  if CurStep = ssPostInstall then
  begin
    // Donde guardamos el config.json (siempre ProgramData)
    ProgramDataCfgDir := ExpandConstant('{commonappdata}\{#MyCompany}\EtereaDesktop');
    if not DirExists(ProgramDataCfgDir) then
      ForceDirectories(ProgramDataCfgDir);

    // Ruta Base elegida por el usuario (página P2)
    RutaBase := RutaPage.Values[0];
    if RutaBase = '' then
      RutaBase := ExpandConstant('{commonappdata}\{#MyCompany}\EtereaDesktop');

    // Crear carpeta de Ruta Base si no existe
    if not DirExists(RutaBase) then
      ForceDirectories(RutaBase);

    // Datos de BD
    if RbLocal.Checked then
    begin
      Mode := 'Local';
      DataSource := '.\\SQLEXPRESS';
      Database := 'eterea';
      Integrated := True;
      UserId := '';
      Password := '';
      UseLocalDb := CbUseLocalDB.Checked;
      AttachDb := EdAttachPath.Text;
    end
    else
    begin
      Mode := 'Server';
      DataSource := EdServer.Text;
      Database := EdDB.Text;
      Integrated := CbIntegrated.Checked;
      UserId := EdUser.Text;
      Password := EdPass.Text;
      UseLocalDb := False;
      AttachDb := '';
    end;

    // Escribir config.json
    Json :=
      '{' + #13#10 +
      '  "Sucursal": ' + SucPage.Values[0] + ',' + #13#10 +
      '  "RutaBase": "' + StringChange(RutaBase, '\', '\\') + '",' + #13#10 +
      '  "RutaWeb": "https://etereaparfums.com.ar/imagenes",' + #13#10 +
      '  "Db": {' + #13#10 +
      '    "Mode": "' + Mode + '",' + #13#10 +
      '    "DataSource": "' + StringChange(DataSource, '\', '\\') + '",' + #13#10 +
      '    "Database": "' + Database + '",' + #13#10 +
      '    "IntegratedSecurity": ' + (IfThen(Integrated, 'true', 'false')) + ',' + #13#10 +
      '    "UserId": "' + UserId + '",' + #13#10 +
      '    "Password": "' + Password + '",' + #13#10 +
      '    "UseLocalDb": ' + (IfThen(UseLocalDb, 'true', 'false')) + ',' + #13#10 +
      '    "AttachDbFile": "' + StringChange(AttachDb, '\', '\\') + '"' + #13#10 +
      '  }' + #13#10 +
      '}';
    JsonPath := AddBackslash(ProgramDataCfgDir) + 'config.json';
    SaveStringToFile(JsonPath, Json, False);
  end;
end;
