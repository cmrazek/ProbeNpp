@echo off
set deploy_dir=C:\Program Files (x86)\Notepad++
if exist "%deploy_dir%" goto deploy
set deploy_dir=C:\Program Files\Notepad++
if exist "%deploy_dir%" goto deploy

echo Error: Notepad++ directory could not be found.
goto :eof

:deploy
copy bin\ProbeNpp.dll "%deploy_dir%\plugins\config\NppSharp\Scripts\"
if errorlevel 1 (
	echo Error: Failed to copy ProbeNpp.dll
	goto :eof
)

if exist bin\ProbeNpp.pdb (
	copy bin\ProbeNpp.pdb "%deploy_dir%\plugins\config\NppSharp\Scripts\"
	if errorlevel 1 (
		echo Error: Failed to copy ProbeNpp.pdb
		goto :eof
	)
) else if exist "%deploy_dir%\plugins\config\NppSharp\Scripts\ProbeNpp.pdb" (
	del "%deploy_dir%\plugins\config\NppSharp\Scripts\ProbeNpp.pdb"
	if errorlevel 1 (
		echo Error: Failed to delete ProbeNpp.pdb
		goto :eof
	)
)

copy bin\ProbeNppLexer.xml "%deploy_dir%\plugins\config\NppSharp\Scripts\"
if errorlevel 1 (
	echo Error: Failed to copy ProbeNppLexer.xml
	goto :eof
)

echo Deploy succeeded
