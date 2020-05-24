@echo off

REM Vars
set "SLNDIR=%~dp0src"

REM Restore + Build
dotnet build "%SLNDIR%\non.sln" --nologo || exit /b

REM Test
dotnet test "%SLNDIR%\NonConTroll.Tests" --nologo --no-build
