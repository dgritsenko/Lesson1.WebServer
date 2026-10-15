@echo off
echo [TASK MANAGER] Запуск без сборки...
del /f /q tasks.db 2>nul
dotnet run --urls=http://localhost:5171 --no-build
pause