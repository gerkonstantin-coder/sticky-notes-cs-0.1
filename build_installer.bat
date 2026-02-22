@echo off
chcp 1251 >nul

echo.
echo ===============================================
echo   Sticky Notes - Build Installer (C#)
echo ===============================================
echo.

cd /d "%~dp0"

:: Check EXE
echo [1/2] Checking EXE file...

if not exist "output\StickyNotes.exe" (
    echo       EXE not found. Compiling...
    echo.
    call compile.bat
    
    if not exist "output\StickyNotes.exe" (
        echo.
        echo [ERROR] Could not create EXE file!
        pause
        exit /b 1
    )
) else (
    echo       Found: output\StickyNotes.exe
)

:: Find Inno Setup
echo.
echo [2/2] Creating installer...

set "ISCC="

if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)
if not defined ISCC (
    if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
        set "ISCC=C:\Program Files\Inno Setup 6\ISCC.exe"
    )
)
if not defined ISCC (
    if exist "C:\Program Files (x86)\Inno Setup 5\ISCC.exe" (
        set "ISCC=C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
    )
)

if not defined ISCC (
    echo.
    echo ===============================================
    echo   [ERROR] Inno Setup not found!
    echo.
    echo   Download Inno Setup 6:
    echo   https://jrsoftware.org/isdl.php
    echo.
    echo   After installation run this script again
    echo ===============================================
    echo.
    pause
    exit /b 1
)

echo       Using: %ISCC%
echo.

"%ISCC%" installer.iss

if exist "Output\StickyNotesSetup.exe" (
    echo.
    echo ===============================================
    echo               SUCCESS!
    echo ===============================================
    echo.
    echo   File: Output\StickyNotesSetup.exe
    echo.
    echo   Python NOT required!
    echo   Works on any Windows with .NET Framework
    echo.
    echo ===============================================
    echo.
    explorer "Output"
) else (
    echo.
    echo [ERROR] Could not create installer
    echo.
)

pause
