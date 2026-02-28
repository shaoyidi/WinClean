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

:: Step 2: Build installer (requires Inno Setup)
where iscc >nul 2>&1
if %errorlevel% equ 0 (
    echo [2/2] Building installer...
    iscc installer\setup.iss
    if %errorlevel% neq 0 (
        echo [ERROR] Installer build failed
        pause
        exit /b 1
    )
    echo [DONE] Installer created in output\ directory
) else (
    echo [2/2] Inno Setup not found, skipping installer
    echo       To build installer, install Inno Setup: https://jrsoftware.org/isdownload.php
    echo       Add ISCC.exe directory to PATH, then re-run this script
    echo.
    echo       You can use publish\WinClean.exe as a portable version
)

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
