@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Install.ps1"
if errorlevel 1 (
  echo.
  echo Installation failed. Read the message above.
) else (
  echo.
  echo Installation complete. Open Revit 2024, then Add-Ins ^> External Tools ^> IFC Info.
)
pause
