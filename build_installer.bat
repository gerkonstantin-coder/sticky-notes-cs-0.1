@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ╔══════════════════════════════════════════════════════════════════════╗
echo ║      Sticky Notes - Полная сборка инсталлера (C# версия)            ║
echo ╚══════════════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

:: ============================================
:: 1. Проверка наличия EXE файла
:: ============================================
echo [1/2] Проверка EXE файла...

if not exist "output\StickyNotes.exe" (
    echo       EXE файл не найден. Компиляция...
    echo.
    call compile.bat
    
    if not exist "output\StickyNotes.exe" (
        echo.
        echo [ОШИБКА] Не удалось создать EXE файл!
        pause
        exit /b 1
    )
) else (
    echo       ✓ EXE файл найден: output\StickyNotes.exe
)

:: ============================================
:: 2. Создание инсталлера
:: ============================================
echo.
echo [2/2] Создание инсталлера...

:: Проверяем Inno Setup
set "ISCC="
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set "ISCC=C:\Program Files\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files (x86)\Inno Setup 5\ISCC.exe" (
    set "ISCC=C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
) else (
    echo.
    echo ┌──────────────────────────────────────────────────────────────────┐
    echo │  [ОШИБКА] Inno Setup не найден!                                  │
    echo │                                                                  │
    echo │  Скачайте и установите Inno Setup 6:                            │
    echo │  https://jrsoftware.org/isdl.php                                │
    echo │                                                                  │
    echo │  После установки запустите этот скрипт снова                    │
    echo └──────────────────────────────────────────────────────────────────┘
    echo.
    pause
    exit /b 1
)

echo       Используется: %ISCC%
echo.

"%ISCC%" installer.iss

if exist "Output\StickyNotesSetup.exe" (
    echo.
    echo ╔══════════════════════════════════════════════════════════════════════╗
    echo ║                    ✅ ИНСТАЛЛЕР ГОТОВ!                               ║
    echo ╠══════════════════════════════════════════════════════════════════════╣
    echo ║  Файл: Output\StickyNotesSetup.exe                                   ║
    echo ║                                                                      ║
    echo ║  Установка Python НЕ требуется!                                      ║
    echo ║  Работает на любом Windows с .NET Framework (встроен в Win10/11)     ║
    echo ╚══════════════════════════════════════════════════════════════════════╝
    echo.
    explorer "Output"
) else (
    echo.
    echo [ОШИБКА] Не удалось создать инсталлер
    echo.
)

pause
