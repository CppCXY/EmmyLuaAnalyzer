#!/bin/bash

arch=$(uname -m)
outputPath="macosx"
if [ -d "$outputPath" ]; then
    rm -rf "$outputPath"
fi
mkdir -p "$outputPath"

if [[ $arch == "x86_64" ]]; then
    # x86_64 架构编译命令
    echo "Compiling for x86_64..."
    dotnet publish ../LanguageServer -c Release --output "$outputPath" -r macOS-x64
elif [[ $arch == "arm64" ]]; then
    echo "Compiling for arm64..."
    dotnet publish ../LanguageServer -c Release --output "$outputPath" -r macOS-arm64
else
    echo "Unsupported architecture: $arch"
    exit 1
fi
