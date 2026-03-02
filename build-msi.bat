@echo off
title WinClean - Build MSI
echo ========================================
echo   WinClean MSI Build Script
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
echo [DONE] Published successfully
echo.

:: Step 2: Build MSI
echo [2/2] Building MSI installer...
if not exist output mkdir output
wix build installer\msi\WinClean.wxs -o output\WinClean_Setup_1.0.0.msi -arch x64
if %errorlevel% neq 0 (
    echo [ERROR] MSI build failed
    pause
    exit /b 1
)
echo [DONE] MSI installer created
echo.

echo ========================================
echo   Build complete!
echo ========================================
echo.
echo   Portable: publish\WinClean.exe
echo   MSI:      output\WinClean_1.0.0.msi
echo.
pause
