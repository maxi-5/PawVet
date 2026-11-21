@echo off
REM ============================================================
REM  Guarda los SECRETOS como variables de entorno de Windows
REM  (no se suben al repositorio). Ejecutar UNA sola vez.
REM  Usa LocalDB, que viene con Visual Studio y no pide permisos
REM  de administrador. Si usas SQL Express cambia el Server por .\SQLEXPRESS
REM ============================================================
set "CONEXION_CLINICA=Server=(localdb)\MSSQLLocalDB;Database=PawVetClinicaDB;Trusted_Connection=True;TrustServerCertificate=True;"
set "CONEXION_AGENDA=Server=(localdb)\MSSQLLocalDB;Database=PawVetAgendaDB;Trusted_Connection=True;TrustServerCertificate=True;"

for /f %%k in ('powershell -NoProfile -Command "[guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')"') do set "LLAVE=%%k"

setx ConnectionStrings__ClinicaDB "%CONEXION_CLINICA%" >nul
setx ConnectionStrings__AgendaDB "%CONEXION_AGENDA%" >nul
setx Jwt__Key "%LLAVE%" >nul

echo.
echo Variables de entorno guardadas:
echo   ConnectionStrings__ClinicaDB = %CONEXION_CLINICA%
echo   ConnectionStrings__AgendaDB  = %CONEXION_AGENDA%
echo   Jwt__Key = (llave aleatoria de 64 caracteres, la misma para las dos APIs)
echo.
echo IMPORTANTE: cierra y vuelve a abrir Visual Studio para que las lea.
pause
