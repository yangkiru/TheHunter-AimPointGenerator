# GitHub 레포지토리에 푸시하는 스크립트
# 사용법: .\GITHUB_PUSH.ps1 -Username 본인GitHub사용자명
# 예: .\GITHUB_PUSH.ps1 -Username yangkiru
#
# 1. https://github.com/new 에서 새 레포지토리 생성
#    - Repository name: TheHunter-AimPointGenerator (또는 원하는 이름)
#    - Public 선택
#    - "Create repository" 클릭 (README 등 추가하지 않기)
# 2. 이 스크립트 실행

param(
    [Parameter(Mandatory=$true)]
    [string]$Username
)

$repoName = "TheHunter-AimPointGenerator"
$remoteUrl = "https://github.com/$Username/$repoName.git"

Set-Location $PSScriptRoot

git remote add origin $remoteUrl 2>$null
if ($LASTEXITCODE -ne 0) {
    git remote set-url origin $remoteUrl
}

git branch -M main
git push -u origin main

Write-Host "`n푸시 완료! 레포지토리: https://github.com/$Username/$repoName" -ForegroundColor Green
