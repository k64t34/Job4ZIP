rem https://docs.microsoft.com/ru-ru/visualstudio/ide/managing-external-tools?view=vs-2019
@echo off
set App_config=App.config
set FolderScript=%~d0%~p0
echo FolderScript=%FolderScript%
set ConfigurationName=%1
echo ConfigurationName=%ConfigurationName%
echo Hostname=%COMPUTERNAME%
cd
IF /I NOT "%ConfigurationName%"=="DEBUG" GOTO :RELEASE
echo DEBUG Config
set FileConfig=%FolderScript%%COMPUTERNAME%.%App_config%
echo %FileConfig%
IF EXIST "%FileConfig%" copy /y "%FileConfig%" "%FolderScript%%App_config%"&goto :EOF
set file=%FolderScript%DEBUG.%App_config%
echo %FileConfig%
IF EXIST "%FileConfig%" copy /y "%FileConfig%" "%FolderScript%%App_config%"&goto :EOF

:RELEASE
if /I NOT "%ConfigurationName%"=="RELEASE" GOTO :NO_CONFIG
echo RELEASE Config
set FileConfig=%FolderScript%RELEASE.%App_config%
echo %FileConfig%
IF EXIST "%FileConfig%" copy /y "%FileConfig%" "%FolderScript%%App_config%"&goto :EOF

:NO_CONFIG
Echo NO App.config