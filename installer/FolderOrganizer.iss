#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif

#define MyAppName "Folder Organizer"
#define MyAppPublisher "Folder Organizer"
#define MyAppExeName "FolderOrganizer.msi"

[Setup]
AppId={{59E9612D-315C-49F1-A570-E866391A91EA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

DefaultDirName={autopf}\Folder Organizer
DefaultGroupName=Folder Organizer

DisableProgramGroupPage=yes

OutputDir=output
OutputBaseFilename=FolderOrganizer-Setup

Compression=lzma2
SolidCompression=yes

WizardStyle=modern

PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

UninstallDisplayName={#MyAppName}

SetupLogging=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; \
    Description: "Create a desktop shortcut"; \
    GroupDescription: "Additional shortcuts:"; \
    Flags: unchecked

[Files]
Source: "..\publish\FolderOrganizer\*"; \
    DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Folder Organizer"; \
    Filename: "{app}\{#MyAppExeName}"

Name: "{autodesktop}\Folder Organizer"; \
    Filename: "{app}\{#MyAppExeName}"; \
    Tasks: desktopicon

[Run]
Filename: "{app}\FolderOrganizer.exe"; \
    Description: "Launch Folder Organizer"; \
    Flags: nowait postinstall
