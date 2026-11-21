@echo off
REM ============================================================
REM  Inicia PawVet en local (sin Docker), cada parte en su ventana:
REM  API Clinica (5001), API Agenda (5002) y cliente React (5173).
REM  Requisitos: haber ejecutado configurar-variables.bat y tener
REM  las migraciones creadas (ver README.md, paso 3).
REM ============================================================
cd /d "%~dp0"

start "PawVet - API Clinica (5001)" cmd /k "cd api-clinica && dotnet run"
start "PawVet - API Agenda (5002)" cmd /k "cd api-agenda && dotnet run"
start "PawVet - Cliente React (5173)" cmd /k "cd cliente-react && (if not exist node_modules npm install) && npm run dev"

echo.
echo Se abrieron tres ventanas. Cuando terminen de cargar:
echo   Cliente web:  http://localhost:5173   (admin@pawvet.cl / Admin123!)
echo   API Clinica:  http://localhost:5001/swagger
echo   API Agenda:   http://localhost:5002/swagger
echo.
echo Para detener el sistema, cierra las tres ventanas.
pause
