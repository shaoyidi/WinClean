[Setup]
AppName=WinClean
AppVersion=1.0.0
AppPublisher=WinClean
AppPublisherURL=https://github.com/winclean
DefaultDirName={autopf}\WinClean
DefaultGroupName=WinClean
OutputDir=..\output
OutputBaseFilename=WinClean_Setup_1.0.0
Compression=lzma2/ultra64
SolidCompression=yes
SetupIconFile=..\src\WinClean\Resources\app.ico
UninstallDisplayIcon={app}\WinClean.exe
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
LicenseFile=license.txt
DisableProgramGroupPage=yes
MinVersion=10.0

[Languages]
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\WinClean.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\WinClean"; Filename: "{app}\WinClean.exe"
Name: "{group}\卸载 WinClean"; Filename: "{uninstallexe}"
Name: "{autodesktop}\WinClean"; Filename: "{app}\WinClean.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\WinClean.exe"; Description: "启动 WinClean"; Flags: nowait postinstall skipifsilent
