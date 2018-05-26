#!/usr/bin/env bash

# Exit on any error
set -e

# Try to compile all examples
find ./examples -name *.csproj -exec dotnet build {} \;

# Build main project
dotnet build ./src

# Execute Unit tests
cd tests
dotnet build
dotnet test
