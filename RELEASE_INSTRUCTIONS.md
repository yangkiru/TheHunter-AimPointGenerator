# GitHub Release 생성 방법

exe 파일을 GitHub Release로 올리면 사람들이 쉽게 다운로드할 수 있습니다.

## 1. GitHub Releases 페이지 열기

https://github.com/yangkiru/TheHunter-AimPointGenerator/releases/new

## 2. Release 정보 입력

- **Choose a tag**: `v1.0.0` 선택 (이미 푸시됨)
- **Release title**: `v1.0.0`
- **Description** (예시):
  ```
  ## AimPoint Generator v1.0.0
  
  theHunter: Call of the Wild 게임용 AimPoint 이미지 생성기
  
  ### 다운로드
  - **AimPointGenerator-v1.0.0-win-x64.zip** - Windows 64비트 (약 75MB, 압축)
  - .NET 설치 불필요 (독립 실행)
  
  ### 사용법
  1. zip 압축 해제
  2. AimPointGenerator.exe 실행
  ```

## 3. 파일 업로드

- "Attach binaries by dropping them here or selecting them" 영역에
- `Publish\AimPointGenerator-v1.0.0-win-x64.zip` 파일을 드래그 앤 드롭

## 4. "Publish release" 클릭

완료! 이제 릴리스 페이지에서 다운로드 링크가 표시됩니다.
