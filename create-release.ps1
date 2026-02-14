# GitHub Release v1.1.2 생성
# 먼저 실행: gh auth login

$ErrorActionPreference = "Stop"
$zipPath = Join-Path $PSScriptRoot "AimPointGenerator-v1.1.2-win-x64.zip"

if (-not (Test-Path $zipPath)) {
    Write-Host "Creating zip..." -ForegroundColor Yellow
    Push-Location (Join-Path $PSScriptRoot "AimPointGenerator")
    dotnet publish -p:PublishProfile=Simple
    Pop-Location
}

Write-Host "Creating release v1.1.2..." -ForegroundColor Cyan
gh release create v1.1.2 $zipPath `
  --title "v1.1.2" `
  --notes @"
## AimPoint Generator v1.1.2

theHunter: Call of the Wild game AimPoint image generator

### Changes
- Save confirmation MessageBox
- Exe path fix for single-file (Environment.ProcessPath)

### Download
- **AimPointGenerator-v1.1.2-win-x64.zip** - Windows 64-bit
- No .NET installation required (self-contained)
"@

Write-Host "Done! https://github.com/yangkiru/TheHunter-AimPointGenerator/releases" -ForegroundColor Green
