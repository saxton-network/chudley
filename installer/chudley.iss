; Installs the verified Codex v2 pet. Uninstaller metadata stays outside Codex's pet directory.
#ifndef PayloadDir
  #error PayloadDir must point to the validated Codex pet payload.
#endif

#define AppVersion "0.2.0"

[Setup]
AppId={{B1BB7D11-4BD3-4C71-9D89-95A38DD74C91}
AppName=Chudley for Codex
AppVersion={#AppVersion}
AppPublisher=saxton-network
AppPublisherURL=https://github.com/saxton-network/chudley
AppSupportURL=https://github.com/saxton-network/chudley/issues
DefaultDirName={localappdata}\Programs\Chudley Codex Pet
UsePreviousAppDir=no
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
MinVersion=10.0.22000
OutputBaseFilename=Chudley-Codex-pet-installer
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName=Chudley for Codex
CloseApplications=no
RestartApplications=no

[Files]
Source: "{#PayloadDir}\chudley-v2\pet.json"; DestDir: "{%CODEX_HOME|{%USERPROFILE}\.codex}\pets\chudley-v2"; Flags: ignoreversion
Source: "{#PayloadDir}\chudley-v2\spritesheet.webp"; DestDir: "{%CODEX_HOME|{%USERPROFILE}\.codex}\pets\chudley-v2"; Flags: ignoreversion
Source: "{#PayloadDir}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PayloadDir}\THIRD_PARTY_NOTICES.md"; DestDir: "{app}"; Flags: ignoreversion

[UninstallDelete]
Type: dirifempty; Name: "{%CODEX_HOME|{%USERPROFILE}\.codex}\pets\chudley-v2"
