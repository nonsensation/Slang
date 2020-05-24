#!/bin/bash

# Vars
slndir="$(dirname "${BASH_SOURCE[0]}")/src"

# Restore + Build
dotnet build "$slndir/non.sln" --nologo || exit

# Test
dotnet test "$slndir/NonConTroll.Tests" --nologo --no-build
