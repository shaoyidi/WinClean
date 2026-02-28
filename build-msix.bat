@echo off
title WinClean - MSIX Build

echo ========================================
echo   WinClean MSIX Package Builder
echo ========================================
echo.

:: Check MSBuild
where msbuild >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] MSBuild not found.
    echo         Please run this script from "Developer Command Prompt for VS"
    echo         or "x64 Native Tools Command Prompt for VS"
    echo.
    echo         If Visual Studio is not installed, you need:
    echo         - Visual Studio 2022 with ".NET desktop development" workload
    echo         - Windows 10 SDK (10.0.22621.0 or later)
    echo.
    pause
    exit /b 1
)

:: Step 1: Generate Store assets
echo [1/3] Generating Store image assets...
powershell -ExecutionPolicy Bypass -File "tools\generate-store-assets.ps1"
if %errorlevel% neq 0 (
    echo [ERROR] Asset generation failed
    pause
    exit /b 1
)
echo.

:: Step 2: Build MSIX package
echo [2/3] Building MSIX package...
msbuild src\WinClean.Package\WinClean.Package.wapproj /p:Configuration=Release /p:Platform=x64 /p:UapAppxPackageBuildMode=SideloadOnly /p:AppxPackageSigningEnabled=false /restore
if %errorlevel% neq 0 (
    echo [ERROR] MSIX build failed
    pause
    exit /b 1
)
echo.

:: Step 3: Locate output
echo [3/3] Locating package...
echo.
echo ========================================
echo   MSIX Build Complete!
echo ========================================
echo.
echo   The .msix package is in:
echo   src\WinClean.Package\AppPackages\
echo.
echo   To submit to Microsoft Store:
echo   1. Sign in to https://partner.microsoft.com/
echo   2. Create a new app submission
echo   3. Upload the .msixupload file
echo.
echo   For local testing (sideload):
echo   Right-click the .msix file and select "Install"
echo.
pause
