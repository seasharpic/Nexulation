@echo off
:: Clean temporary files and caches after Playbook execution
echo Cleaning temporary files...

del /s /f /q "%TEMP%\*.*" >nul 2>&1
for /d %%x in ("%TEMP%\*") do rmdir /s /q "%%x" >nul 2>&1

del /s /f /q "C:\Windows\Temp\*.*" >nul 2>&1
for /d %%x in ("C:\Windows\Temp\*") do rmdir /s /q "%%x" >nul 2>&1

del /s /f /q "C:\ProgramData\chocolatey\lib-bad\*.*" >nul 2>&1
del /s /f /q "%LocalAppData%\Temp\*.*" >nul 2>&1

echo Cleanup completed.
