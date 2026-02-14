# GitHub Release v1.1.1 생성
# 먼저 실행: gh auth login

$ErrorActionPreference = "Stop"
$zipPath = Join-Path $PSScriptRoot "AimPointGenerator-v1.1.1-win-x64.zip"

if (-not (Test-Path $zipPath)) {
    Write-Host "Creating zip..." -ForegroundColor Yellow
    Push-Location (Join-Path $PSScriptRoot "AimPointGenerator")
    dotnet publish -p:PublishProfile=Simple
    Pop-Location
}

Write-Host "Creating release v1.1.1..." -ForegroundColor Cyan
gh release create v1.1.1 $zipPath `
  --title "v1.1.1" `
  --notes @"
## AimPoint Generator v1.1.1

theHunter: Call of the Wild game AimPoint image generator

### Changes
- Min/Max placement: Min on left, Max on right
- Arithmetic input: e.g. 2*2, 150/2, .5 -> 0.5
- Zeroing 0 handling: Target only when all zeroing is 0
- Single exe publish with zip

### Download
- **AimPointGenerator-v1.1.1-win-x64.zip** - Windows 64-bit
- No .NET installation required (self-contained)
"@

Write-Host "Done! https://github.com/yangkiru/TheHunter-AimPointGenerator/releases" -ForegroundColor Green
