@echo off
title WinClean - Build

echo ========================================
echo   WinClean Build Script
echo ========================================
echo.

:: Step 1: Publish
echo [1/2] Publishing application...
dotnet publish src\WinClean\WinClean.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish
if %errorlevel% neq 0 (
    echo [ERROR] Publish failed
    pause
    exit /b 1
)
echo [DONE] Published to publish\ directory
echo.

:: Step 2: Build installer (search for ISCC.exe)
set "ISCC="
where iscc >nul 2>&1 && set "ISCC=iscc"
if "%ISCC%"=="" if exist "D:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC=D:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if "%ISCC%"=="" if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if "%ISCC%"=="" if exist "C:\Program Files\Inno Setup 6\ISCC.exe" set "ISCC=C:\Program Files\Inno Setup 6\ISCC.exe"

if "%ISCC%"=="" (
    echo [2/2] Inno Setup not found, skipping installer
    echo       To build installer, install Inno Setup: https://jrsoftware.org/isdownload.php
    echo.
    echo       You can use publish\WinClean.exe as a portable version
    goto :done
)

echo [2/2] Building installer...
"%ISCC%" installer\setup.iss
if not exist output\WinClean_Setup_1.0.0.exe (
    echo [ERROR] Installer build failed
    pause
    exit /b 1
)
echo [DONE] Installer created in output\ directory

:done
echo.
echo ========================================
echo   Build complete!
echo ========================================
echo.
echo   Portable:  publish\WinClean.exe
if exist output\WinClean_Setup_1.0.0.exe (
    echo   Installer: output\WinClean_Setup_1.0.0.exe
)
echo.
pause
