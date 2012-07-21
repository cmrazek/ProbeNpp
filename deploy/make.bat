@echo off
set _archivename_=ProbeNpp_DotNet2.7z

if exist "C:\Program Files\7-zip\7z.exe" (
	set _7z_="C:\Program Files\7-zip\7z.exe"
) else if exist "C:\Program Files (x86)\7-zip\7z.exe" (
	set _7z_="C:\Program Files (x86)\7-zip\7z.exe"
) else (
	echo Unable to locate 7z.exe
	goto done
)

pushd %~dp0

if not exist bin (
	mkdir bin
	if errorlevel 1 (
		echo Can't create bin directory.
		goto done
	)
)

cd bin

if not exist "ProbeNpp.dll" (
	echo ProbeNpp.dll does not exist.
	goto done
)

if not exist "ProbeNppLexer.xml" (
	echo ProbeNppLexer.xml does not exist.
	goto done
)

if exist "..\%_archivename_%" (
	del "..\%_archivename_%"
	if errorlevel 1 (
		echo Can't delete old %_archivename_%
		goto done
	)
)

%_7z_% a -y "..\%_archivename_%" "ProbeNpp.dll" "ProbeNppLexer.xml"


:done
popd
