@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ╔══════════════════════════════════════════════════════════════════════╗
echo ║           Sticky Notes - Компиляция (C# версия)                      ║
echo ╚══════════════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

:: ============================================
:: Поиск csc.exe (компилятор C#)
:: ============================================
echo [1/2] Поиск компилятора C#...

set "CSC="

:: .NET Framework 4.x (обычно в Windows 10/11)
if exist "%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" (
    set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
)
if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe" (
    set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

:: .NET Framework 3.5
if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework64\v3.5\csc.exe" (
    set "CSC=%WINDIR%\Microsoft.NET\Framework64\v3.5\csc.exe"
)
if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework\v3.5\csc.exe" (
    set "CSC=%WINDIR%\Microsoft.NET\Framework\v3.5\csc.exe"
)

if not defined CSC (
    echo.
    echo [ОШИБКА] Компилятор C# не найден!
    echo.
    echo Установите .NET Framework 3.5 или выше:
    echo Windows 10/11: Панель управления -^> Программы -^> Включение компонентов Windows
    echo.
    pause
    exit /b 1
)

echo       ✓ Найден: %CSC%

:: ============================================
:: Создание папки output
:: ============================================
if not exist "output" (
    mkdir output
    echo       ✓ Создана папка: output
)

:: ============================================
:: Компиляция
:: ============================================
echo.
echo [2/2] Компиляция StickyNotes.exe...

"%CSC%" /target:winexe /out:output\StickyNotes.exe StickyNotes.cs

if errorlevel 1 (
    echo.
    echo [ОШИБКА] Компиляция не удалась!
    echo.
    echo Убедитесь, что файл StickyNotes.cs находится в текущей папке:
    echo %CD%
    echo.
    pause
    exit /b 1
)

if exist "output\StickyNotes.exe" (
    echo       ✓ Создан: output\StickyNotes.exe
    
    :: Показываем размер файла
    for %%F in ("output\StickyNotes.exe") do (
        echo       Размер: %%~zF байт
    )
) else (
    echo.
    echo [ОШИБКА] Файл не был создан!
    pause
    exit /b 1
)

echo.
echo ══════════════════════════════════════════════════════════════════════
echo   Готово! Файл: output\StickyNotes.exe
echo   Требования: Windows 10/11 (или любой Windows с .NET Framework)
echo   Python НЕ требуется!
echo ══════════════════════════════════════════════════════════════════════
echo.

pause
