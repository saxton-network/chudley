; Chudley Windows 11 x64 per-user installer. Build only from a validated publish directory.
#ifndef PayloadDir
  #error PayloadDir must point to the validated self-contained publish directory.
#endif

#define AppVersion "0.1.0"

[Setup]
AppId={{4FEE5DFB-15CC-4E5D-8D3D-33DFD5E75AD8}
AppName=Chudley
AppVersion={#AppVersion}
AppPublisher=saxton-network
AppPublisherURL=https://github.com/saxton-network/chudley
AppSupportURL=https://github.com/saxton-network/chudley/issues
DefaultDirName={localappdata}\Programs\Chudley
DefaultGroupName=Chudley
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.22000
OutputBaseFilename=Chudley-win-x64-installer
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\Chudley.Desktop.exe
UninstallDisplayName=Chudley
CloseApplications=yes
RestartApplications=no

[Files]
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Chudley"; Filename: "{app}\Chudley.Desktop.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\Chudley.Desktop.exe"; Description: "Launch Chudley"; Flags: nowait postinstall skipifsilent
