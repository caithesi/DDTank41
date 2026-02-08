# Project Overview & Progress Tracking

This document tracks the current implementation status of the DDTank migration to Godot 4.x, specifically focusing on the "C# Bridge" strategy for deterministic systems like destructible terrain.

## 1. Current Implementation Status

As of February 8, 2026, the following milestones have been reached:

- [x] **Godot C# Environment**: Created a Godot 4.x project configured for C# (.NET).
- [x] **Shared Logic Integration**: Successfully integrated the `DDTank.Shared` library into the Godot project.
- [x] **Terrain Bridge (`DDTankMap.cs`)**: Implemented the core logic for bridging the C# bitmask (`Tile.cs`) with Godot's visual systems (`ImageTexture`).
- [x] **Destruction Testing**: Implemented `MapTest.cs` and `RealTerrainTest.cs` to verify "Dig" operations.
- [x] **Legacy Logic Recovery**: Identified and verified original binary formats for `.map` (Terrain) and `.bomb` (Explosion) files.

## 2. Current Blockers

- [ ] **Missing Visual Assets**: The repository contains logical data (`.map`) but **lacks visual textures** (`show.png`, `fore.png`). These were historically served via CDN and are required for a complete visual representation of legacy maps in Godot.
- [ ] **Asset Extraction**: Need to identify a source or tool to "rip" the original PNG textures from a DDTank client to match the logic bitmasks.

## 3. Project Folder Structure (Godot Client)

```text
/new-game-project/
├── project.godot          # Main Godot project configuration
├── New Game Project.csproj # C# Project file
├── New Game Project.sln    # C# Solution file
├── node_2d.tscn           # Main testing scene (Entry point)
├── DDTankMap.cs           # Core terrain logic (Bitmask-to-Image bridge)
├── MapTest.cs             # Basic terrain testing script
├── MapTestWithBallInfo.cs # Enhanced testing script with debug visualization
├── vecteezy_3d-fox...png  # Primary terrain source texture
├── assets/
│   ├── bomb/                  # Shape bitmask definitions (.bomb files)
│   └── sound/                 # Sound effects
├── note/                  # Documentation and development notes
│   ├── destroy_map_test.md # Best practices for destructible terrain
│   └── overview.md        # This file
├── .editorconfig          # Editor configuration
└── .gitignore             # Git ignore rules
```

## 3. Scene Hierarchy (`node_2d.tscn`)

The scene is designed to separate logical management from visual presentation and input handling:

- **Node2D** (Root)
    - **DDTankMap** (`DDTankMap.cs`)
        - Manages the conversion of the static Sprite into an editable texture.
        - Handles the `Dig` logic to update both physics bitmasks and visual pixels.
        - **TerrainSprite** (`Sprite2D`)
            - Displays the terrain texture.
            - `Centered: false` (aligned to top-left for 1:1 pixel mapping).
    - **MapTest** (`MapTestWithBallInfo.cs`)
        - Detects mouse clicks and manages debug visualization.
        - Calls `_mapBridge.Dig()` at the mouse position using circular or `.bomb` masks.
        - References both `TerrainSprite` and `DDTankMap` via `NodePaths`.

## 4. Key Mechanics Verified

### Destructible Terrain
- **Bitmask Parity**: The system uses the same `DDTank.Shared.Tile` logic as the server, ensuring collision and visuals stay in sync.
- **Dynamic Updates**: Visual updates are performed by manipulating the `Image` data and updating the `ImageTexture` region.
- **Flexible Testing**: Supports both procedural circles and legacy `.bomb` binary masks, with the ability to cycle through all available shapes using the **Mouse Wheel** in the test scene.

## 5. Next Steps & Current Investigation
- **Shape Reproduction**: Verifying that irregular shapes and different sizes are correctly "punched" into the visuals.
- **Sub-byte Alignment**: Ensuring that the complex bit-shifting in `Tile.cs` matches the visual pixel manipulation, especially for unaligned coordinates.
- **Performance Optimization**: Investigating sub-region texture updates to maintain high FPS during rapid explosions.
