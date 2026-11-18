@echo off
setlocal
cd /d "%~dp0"
chcp 65001 >nul

echo Building Palia Voice AI, then installing it. This can take a few minutes.
echo.

dotnet publish ".\src\PaliaVoiceAI.App\PaliaVoiceAI.App.csproj" -c Release -r win-x64 --self-contained true -o ".\artifacts\win-x64" --nologo
if errorlevel 1 goto fail

echo.
echo Installing...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Install.ps1"
if errorlevel 1 goto fail

echo.
echo Installed. Open Palia Voice AI from the Start menu.
goto end

:fail
echo.
echo Install failed. Read the messages above.

:end
echo.
pause
endlocal
