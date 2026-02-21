@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ╔══════════════════════════════════════════════════════════════════════╗
echo ║           Sticky Notes - Сборка (C# версия)                          ║
echo ╚══════════════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0"

:: ============================================
:: Поиск csc.exe (компилятор C#)
:: ============================================
echo [1/3] Поиск компилятора C#...

set "CSC="
set "FRAMEWORK_PATH="

:: .NET Framework 4.x (обычно в Windows 10/11)
for /f "delims=" %%i in ('dir /b /s "%WINDIR%\Microsoft.NET\Framework64\v4*csc.exe" 2^>nul') do set "CSC=%%i"
if not defined CSC for /f "delims=" %%i in ('dir /b /s "%WINDIR%\Microsoft.NET\Framework\v4*csc.exe" 2^>nul') do set "CSC=%%i"

:: Если не нашли в Framework 4.x
if not defined CSC (
    :: Ищем в .NET Framework 3.5
    if exist "%WINDIR%\Microsoft.NET\Framework64\v3.5\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework64\v3.5\csc.exe"
    if not defined CSC if exist "%WINDIR%\Microsoft.NET\Framework\v3.5\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework\v3.5\csc.exe"
)

:: Если всё ещё не нашли
if not defined CSC (
    echo.
    echo [ОШИБКА] Компилятор C# не найден!
    echo.
    echo Убедитесь, что установлен .NET Framework 3.5 или выше.
    echo Windows 10/11: Панель управления -^> Программы -^> Включение компонентов Windows
    echo.
    pause
    exit /b 1
)

echo       ✓ Найден: %CSC%

:: ============================================
:: Создание иконки (если есть PNG)
:: ============================================
echo.
echo [2/3] Подготовка ресурсов...

set ICON_PARAM=
if exist "icon.ico" (
    echo       ✓ Используется иконка: icon.ico
    set ICON_PARAM=/win32icon:icon.ico
) else if exist "icon.png" (
    echo       ⚠ PNG иконка найдена. Для использования конвертируйте в ICO.
) else (
    echo       ⚠ Иконка не найдена, будет использоваться стандартная
)

:: ============================================
:: Компиляция
:: ============================================
echo.
echo [3/3] Компиляция...

:: Создаем папку output
if not exist "output" mkdir output

:: Компилируем
"%CSC%" /target:winexe /out:output\StickyNotes.exe %ICON_PARAM% /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Runtime.Serialization.dll StickyNotes.cs

if errorlevel 1 (
    echo.
    echo [ОШИБКА] Компиляция не удалась!
    pause
    exit /b 1
)

echo       ✓ Создан: output\StickyNotes.exe

:: ============================================
:: Результат
:: ============================================
echo.
echo ╔══════════════════════════════════════════════════════════════════════╗
echo ║                      ✅ Успешно!                                     ║
echo ╠══════════════════════════════════════════════════════════════════════╣
echo ║  Готовый файл: output\StickyNotes.exe                                ║
echo ║                                                                      ║
echo ║  Требования: Windows 10/11 (или любой Windows с .NET Framework)     ║
echo ║  Установка Python НЕ требуется!                                      ║
echo ╚══════════════════════════════════════════════════════════════════════╝
echo.

:: Открываем папку с результатом
explorer "output"

pause
