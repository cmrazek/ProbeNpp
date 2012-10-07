@echo off
setlocal enableextensions enabledelayedexpansion

if exist "C:\Program Files\NSIS\makensis.exe" (
	set makensis="C:\Program Files\NSIS\makensis.exe"
) else if exist "C:\Program Files (x86)\NSIS\makensis.exe" (
	set makensis="C:\Program Files (x86)\NSIS\makensis.exe"
) else (
	echo Error: makensis.exe could not be found
	goto :eof
)

if not exist bin mkdir bin
if not exist output mkdir output

copy /Y ..\ProbeNpp\bin\Release\ProbeNpp.dll bin\
if errorlevel 1 (
	echo Failed to copy ProbeNpp.dll
	goto :eof
)

copy /Y ..\ProbeNpp\bin\Release\ProbeNppLexer.xml bin\
if errorlevel 1 (
	echo Failed to copy ProbeNppLexer.xml
	goto :eof
)

%makensis% ProbeNppSetup.nsi
if errorlevel 1 (
	echo MakeNSIS Failed.
	goto :eof
)
