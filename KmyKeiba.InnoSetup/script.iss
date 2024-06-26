; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "KMY£n"
#define MyAppVersion "5.0.0-alpha"
#define MyAppPublisher "KMY (á ·©)"
#define MyAppURL "https://github.com/kmycode/kmy-keiba"
#define MyAppExeName "KmyKeiba.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6A5C6755-3D33-41AF-9C51-2E51F5C52957}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}\KMYsofts\KMYKeiba\App
DisableDirPage=yes
DisableProgramGroupPage=yes
LicenseFile=C:\Users\tt\Documents\repo\KmyKeiba\licence.rtf
InfoBeforeFile=C:\Users\tt\Documents\repo\KmyKeiba\disclaimer.rtf
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=commandline
OutputBaseFilename=KmyKeiba.Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\tt\Documents\repo\KmyKeiba\dist\x64\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "C:\Users\tt\Documents\repo\KmyKeiba\dist\x64\Release\script\*"; DestDir: "{app}\..\script"; Flags: ignoreversion recursesubdirs   
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

