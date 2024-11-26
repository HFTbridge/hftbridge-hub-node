#!/bin/bash

# Get the directory of the script
scriptPath="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# Path to the project
projectPath="$scriptPath/HFTbridge.Agent.csproj"

# Output directory
outDir="$scriptPath/out"

dotnet build $projectPath -c Release -o $outDir

# Build the project
dotnet publish $projectPath -r win-x64 -p:PublishSingleFile=true --self-contained true -o $outDir /p:UseAppHost=true

echo "Build successful. Output located in 'out' directory."

# # Path to the clicker project
# projectPathClicker="$scriptPath/HFTbridge.Node.DC.Spot/HFTbridge.Node.DC.Spot.csproj"

# # Build the clicker project
# dotnet publish $projectPathClicker -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true -o $outDir /p:UseAppHost=true






