# theHunter: Call of the Wild - AimPoint Generator

A program to generate AimPoint (reticle) images for use in the game.

## Download

**[Latest Release](https://github.com/yangkiru/TheHunter-AimPointGenerator/releases/latest)** - Windows 64-bit single exe (no .NET installation required)

## How to Run

**From release:** Extract the zip and run `AimPointGenerator.exe`

**From source:**
```bash
cd AimPointGenerator
dotnet run
```

## Usage

1. **Scope Data** (left panel): Enter ammunition name, effective range, scope name
2. **Aim Point Data**: Enter target distance, zeroing distance, point, Min/Max Zoom
3. Add items with the **Add Aim Point** button (drag handle to reorder)
4. The image updates automatically when data changes
5. **Load Data** / **Save** / **Save As**: Save as JSON file (filename: ammunitionname_scopename.json)
6. **Save Image**: Save AimPoint image as PNG

## Image Colors (Color Picker)

Customize colors of the generated image:
- **Reticle**: Crosshair/reticle color
- **Text**: Label text color
- **Background**: Background color (supports alpha/transparency)

Settings are saved in `AimPointData/theme.json` and persist between sessions.

## Combine Images

1. Click **Select Images** to choose multiple PNG files
2. Set **Columns** for the grid layout
3. Click **Combine & Save** to merge into a single image
4. **Open Directory** opens the data folder

## AimPoint Image Layout

- All information displayed inside a **circle**
- **Top left**: Ammunition name, effective range (line break)
- **Top right**: Scope name, Target:Zeroing format hint
- **Above center line left**: Min Zoom
- **Above center line right**: Max Zoom
- **Left**: Min Zoom aim point data
- **Right**: Max Zoom aim point data

## Zeroing:Target Distance Format

e.g. `150:200` → Zeroing 150m, Target distance 200m

## Min/Max Toggle

- **Max**: Aim point for maximum zoom (default)
- **Min**: Aim point for minimum zoom

## Data Storage

- Data files and images are stored in `AimPointData` folder (created next to the exe)
- `theme.json`: Image color settings
- `lastfile.txt`: Last opened/saved file path

## Requirements

- Windows 64-bit
- .NET 7.0 (only when running from source; release exe is self-contained)
