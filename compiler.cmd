@echo off

REM Vars
set "SLNDIR=%~dp0src"

REM Restore + Build
dotnet build "%SLNDIR%\NonConTroll.Compiler" --nologo || exit /b

REM Run
dotnet run -p "%SLNDIR%\NonConTroll.Compiler" --no-build -- %*
