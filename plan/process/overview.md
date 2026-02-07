# Project Overview & Progress Tracking

This document tracks the current implementation status of the DDTank migration to Godot 4.x, specifically focusing on the "C# Bridge" strategy for deterministic systems like destructible terrain.

## 1. Current Implementation Status

As of February 7, 2026, the following milestones have been reached:

- [x] **Godot C# Environment**: Created a Godot 4.x project configured for C# (.NET).
- [x] **Shared Logic Integration**: Successfully integrated the `DDTank.Shared` library into the Godot project.
- [x] **Terrain Bridge (`DDTankMap.cs`)**: Implemented the core logic for bridging the C# bitmask (`Tile.cs`) with Godot's visual systems (`ImageTexture`).
- [x] **Destruction Testing**: Implemented `MapTester.cs` to verify real-time "Dig" operations via user input.

## 2. Project Folder Structure (Godot Client)

```text
/new-game-project/
├── project.godot          # Main Godot project configuration
├── New Game Project.csproj # C# Project file
├── node_2d.tscn           # Main testing scene
├── DDTankMap.cs           # Logic for terrain manipulation (Visual/Logical bridge)
├── MapTest.cs             # User input handler for testing "Dig" operations
├── vecteezy_3d-fox...png  # The terrain source texture
├── bomb/                  # Directory containing numerous .bomb bitmask files
├── note/                  # Documentation
│   └── destroy_map_test.md # Best practices for destructible terrain
└── build.log              # Build output logs
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
    - **MapTester** (`MapTester.cs`)
        - Detects mouse clicks.
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
