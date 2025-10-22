@echo off
echo ========================================
echo    GENERATORE LICENZE CGEASY
echo ========================================
echo.
echo Avvio generatore di licenze...
echo.

cd /d "%~dp0"
dotnet run --project src\CGEasy.LicenseGenerator\CGEasy.LicenseGenerator.csproj

pause


