@echo off

dotnet build .\src\non.sln /nologo
dotnet test .\src\NonConTroll.Tests/NonConTroll.Tests.csproj
