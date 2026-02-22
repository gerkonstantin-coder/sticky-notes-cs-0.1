@echo off
chcp 1251 >nul

echo.
echo ===============================================
echo      Sticky Notes - Compilation (C#)
echo ===============================================
echo.

cd /d "%~dp0"

:: Find csc.exe
echo [1/2] Finding C# compiler...

set "CSC="

for /f "delims=" %%i in ('dir /b /s "%WINDIR%\Microsoft.NET\Framework64\v4*csc.exe" 2^>nul') do set "CSC=%%i"
if not defined CSC for /f "delims=" %%i in ('dir /b /s "%WINDIR%\Microsoft.NET\Framework\v4*csc.exe" 2^>nul') do set "CSC=%%i"

if not defined CSC (
    if exist "%WINDIR%\Microsoft.NET\Framework64\v3.5\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework64\v3.5\csc.exe"
)
if not defined CSC (
    if exist "%WINDIR%\Microsoft.NET\Framework\v3.5\csc.exe" set "CSC=%WINDIR%\Microsoft.NET\Framework\v3.5\csc.exe"
)

if not defined CSC (
    echo.
    echo [ERROR] C# compiler not found!
    echo.
    echo Make sure .NET Framework 3.5 or higher is installed.
    echo Windows 10/11: Control Panel - Programs - Turn Windows features on
    echo.
    pause
    exit /b 1
)

echo       Found: %CSC%

:: Prepare
echo.
echo [2/2] Compiling...

if not exist "output" mkdir output

"%CSC%" /target:winexe /out:output\StickyNotes.exe /r:System.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Runtime.Serialization.dll StickyNotes.cs

if errorlevel 1 (
    echo.
    echo [ERROR] Compilation failed!
    pause
    exit /b 1
)

echo       Created: output\StickyNotes.exe

echo.
echo ===============================================
echo                   SUCCESS!
echo ===============================================
echo.
echo   File: output\StickyNotes.exe
echo.
echo   Requirements: Windows 10/11 (or any Windows with .NET)
echo   Python NOT required!
echo.
echo ===============================================
echo.

explorer "output"

pause
