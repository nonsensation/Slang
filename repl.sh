#!/bin/bash

# Vars
slndir="$(dirname "${BASH_SOURCE[0]}")/src"

# Restore + Build
dotnet build "$slndir/NonConTroll.Repl" --nologo || exit

# Run
dotnet run -p "$slndir/NonConTroll.Repl" --no-build
