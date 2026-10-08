@echo off
setlocal

:: --- НАСТРОЙКИ ---
:: Имя готового файла
set "OutName=Nexulation.apbx"

:: Папка, которую пакуем
set "SourceDir=Playbook"

:: Пароль для архива
set "ArchivePass=malte"

:: Путь к 7-Zip (ищем внутри плейбука или в системе)
set "Zipper=.\Playbook\Executables\7zr.exe"
:: -----------------

:: 1. Проверяем наличие 7-Zip
if not exist "%Zipper%" (
    echo [INFO] Portable 7zr not found inside Playbook. Checking Program Files...
    if exist "C:\Program Files\7-Zip\7z.exe" (
        set "Zipper=C:\Program Files\7-Zip\7z.exe"
    ) else (
        echo [ERROR] 7-Zip not found! Put 7zr.exe in Playbook\Executables.
        pause
        exit /b
    )
)

:: 2. Удаляем старый файл, если есть
if exist "%OutName%" del "%OutName%"

:: 3. ЗАПАКОВКА С ПАРОЛЕМ
:: -p... = пароль
:: -mhe=on = шифровать заголовки файлов (чтобы даже имена файлов не были видны без пароля)
echo Packaging %SourceDir% into %OutName% with password...

"%Zipper%" a -t7z -mx0 -p"%ArchivePass%" -mhe=on "%OutName%" ".\%SourceDir%\*" -xr!.git -xr!*.apbx

echo.
if exist "%OutName%" (
    echo [SUCCESS] Password protected file created: %OutName%
) else (
    echo [FAIL] Something went wrong.
)

pause