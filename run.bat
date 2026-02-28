@echo off
title WinClean

echo ========================================
echo   WinClean - Disk Cleanup Tool
echo ========================================
echo.

:: Check .NET 8 runtime
dotnet --list-runtimes >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET runtime not found. Please install .NET 8 SDK
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

:: Restore NuGet packages on first run
if not exist "src\WinClean\bin" (
    echo [INFO] First run, restoring dependencies...
    dotnet restore src\WinClean\WinClean.csproj
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to restore dependencies
        pause
        exit /b 1
    )
    echo.
)

echo [INFO] Starting WinClean...
echo.
dotnet run --project src\WinClean\WinClean.csproj -c Release
