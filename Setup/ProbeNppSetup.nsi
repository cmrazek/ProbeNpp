;---------------------------------------------------------------------------------------------------
; NppSharpSetup.nsi
;---------------------------------------------------------------------------------------------------

Name "ProbeNpp"
OutFile "Output\ProbeNpp_Setup_1.2.exe"
InstallDir "$PROGRAMFILES32\Notepad++"

;---------------------------------------------------------------------------------------------------
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;---------------------------------------------------------------------------------------------------
Section ""
	
	; Check for running Notepad++
	FindWindow $0 "Notepad++"
	IntCmp $0 0 NotepadNotRunning
		MessageBox MB_OK|MB_ICONEXCLAMATION "Please ensure Notepad++ is closed before installation."
	NotepadNotRunning:
	
	; Write program files
	SetOutPath "$INSTDIR\plugins\config\NppSharp\Scripts"
	File "bin\ProbeNpp.dll"
	File "bin\ProbeNppLexer.xml"
	
	; Write uninstall strings
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ProbeNpp" "DisplayName" "ProbeNpp"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ProbeNpp" "UninstallString" '"$INSTDIR\UninstallProbeNpp.exe"'
	WriteUninstaller "$INSTDIR\UninstallProbeNpp.exe"
	
SectionEnd

;---------------------------------------------------------------------------------------------------
Section "Uninstall"
	; Check for running Notepad++
	FindWindow $0 "Notepad++"
	IntCmp $0 0 NotepadNotRunning
		MessageBox MB_OK|MB_ICONEXCLAMATION "Please ensure Notepad++ is closed before uninstalling."
	NotepadNotRunning:
	
	DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\ProbeNpp"
	
	Delete /REBOOTOK "$INSTDIR\plugins\config\NppSharp\Scripts\ProbeNpp.dll"
	Delete /REBOOTOK "$INSTDIR\plugins\config\NppSharp\Scripts\ProbeNppLexer.xml"
	
	Delete /REBOOTOK "$INSTDIR\UninstallProbeNpp.exe"
	
	IfRebootFlag 0 NoUninstReboot
		MessageBox MB_YESNO "A reboot is required to finish the uninstall.  Do you wish to reboot now?" IDNO NoUninstReboot
		Reboot
	NoUninstReboot:
SectionEnd
