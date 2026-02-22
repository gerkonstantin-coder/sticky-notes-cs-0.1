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
:: 1. Компиляция EXE
:: ============================================
echo [1/2] Компиляция EXE файла...

:: Проверяем наличие исходного файла
if not exist "StickyNotes.cs" (
    echo [ОШИБКА] Файл StickyNotes.cs не найден!
    echo Текущая папка: %CD%
    echo.
    pause
    exit /b 1
)

:: Компилируем
call compile.bat

:: Проверяем результат компиляции
if not exist "output\StickyNotes.exe" (
    echo.
    echo [ОШИБКА] EXE файл не был создан!
    echo Проверьте сообщения об ошибках выше.
    echo.
    pause
    exit /b 1
)

echo       ✓ EXE файл готов

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
)

if not defined ISCC (
    echo.
    echo ══════════════════════════════════════════════════════════════════════
    echo   [ОШИБКА] Inno Setup не найден!
    echo.
    echo   EXE файл готов: output\StickyNotes.exe
    echo   Вы можете использовать его напрямую.
    echo.
    echo   Для создания инсталлера:
    echo   1. Скачайте: https://jrsoftware.org/isdl.php
    echo   2. Установите Inno Setup
    echo   3. Запустите этот скрипт снова
    echo ══════════════════════════════════════════════════════════════════════
    echo.
    explorer "output"
    pause
    exit /b 0
)

echo       Используется: %ISCC%
echo.
echo Компиляция инсталлера...

"%ISCC%" installer.iss

if errorlevel 1 (
    echo.
    echo [ОШИБКА] Не удалось создать инсталлер
    echo.
    pause
    exit /b 1
)

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
    echo [ОШИБКА] Инсталлер не был создан
    echo.
)

pause
