# theHunter: Call of the Wild - AimPoint Generator

A program to generate AimPoint (reticle) images for use in the game.

## Download

**[Latest Release](https://github.com/yangkiru/TheHunter-AimPointGenerator/releases/latest)** - Windows 64-bit exe (no .NET installation required)

## How to Run

```bash
cd AimPointGenerator
dotnet run
```

## Usage

1. **Scope Data** (top): Enter ammunition name, effective range, scope name
2. **Aim Point Data** (bottom): Enter target distance, zeroing distance, point, Min/Max Zoom
3. Add items with the **Add Aim Point** button
4. The image updates automatically when data changes
5. **Load Data** / **Save**: Save as JSON file (filename: ammunitionname_scopename.json)
6. **Save Image**: Save AimPoint image as PNG

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
