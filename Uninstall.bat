@echo off
setlocal
cd /d "%~dp0"
chcp 65001 >nul

echo Uninstalling Palia Voice AI...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Uninstall.ps1"
if errorlevel 1 goto fail

echo.
echo Uninstalled.
goto end

:fail
echo.
echo Uninstall failed. Read the messages above.

:end
echo.
pause
endlocal
