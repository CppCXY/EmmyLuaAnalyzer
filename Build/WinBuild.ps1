$outputPath = "win32"

if (Test-Path $outputPath) {
    Remove-Item -Path $outputPath -Recurse -Force | Out-Null
}
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

dotnet publish ../LanguageServer -c Release --output $outputPath -r win-x64 