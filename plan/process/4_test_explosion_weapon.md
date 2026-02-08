# Plan 4: Explosions & Weapon Parity (Hole Shapes and Sounds)

## Purpose
The goal of this phase is to transition from generic procedural map destruction to a **data-driven system** that accurately reflects the legacy game's mechanics. In DDTank, every weapon (ball) has a specific identity defined in the database, which dictates:
1.  **The Physical Impact**: The specific `.bomb` bitmask used to "stamp" the terrain.
2.  **The Audio Identity**: The unique sound played upon impact (`BombSound`).

By implementing this, the map tester becomes a high-fidelity tool for verifying that the Godot client's physics and visual/audio feedback match the original experience 1:1.

---

## Process of Thinking

### 1. The "Identity" Problem
Previously, the tester just grabbed random `.bomb` files from a folder. However, in the real game, a "Thunder" weapon shouldn't use a "Love" bomb crater. We need a bridge between the **Asset** (the file) and the **Metadata** (the Ball ID).

### 2. Synchronization with Server Logic
The server uses `BallInfo` to determine the logic radius and particle effects. By adopting the same `BallInfo` structure in our tester, we ensure that if we later add damage calculation, the data is already aligned.

### 3. Sensory Feedback (Audio)
Terrain destruction is not just a visual change; it's a gameplay event. Adding `BombSound` integration ensures that the "Impact" feels correct and helps identify issues where a small hole might mistakenly trigger a "Big Explosion" sound.

### Pre-requisite: Asset Preparation
The sound files required for this test are originally embedded within the legacy Flash client.
- **Original Source**: `Source Flash/FlashSV1/audio.swf`.
- **Extraction Status**: [DONE] Successfully extracted to `audio_decomplied/sounds/`.
- **Current Format**: Files are named `[ID]_Sound[SoundID].mp3` (e.g., `90_Sound093.mp3`).
- **Target Action**: Copy the contents of `audio_decomplied/sounds/` to `res://sound/` in the Godot project. 
- **Note**: Manual renaming is **NOT** required; the revised `MapTestWithBallInfo.cs` uses automated pattern matching to find files like `*Sound093.mp3`.

---

## Implementation Steps

### Step 1: Data Integration (`BallInfo` Loader)
We need to parse `BallList.xml` from the legacy backup to create a local list of "Ball" identities.
- **Source**: `GameAdmin/Backup/XMLReader/XMLImport/BallList.xml`.
- **Target**: A `BallDataLoader` class in Godot that converts XML nodes into a list of `BallInfo` objects.

### Step 2: Mapping Assets to Metadata
Each `BallInfo` contains a `Crater` ID and a `BombSound` ID.
- **Shape**: Map `Crater` ID to `res://bomb/{Crater}.bomb`.
- **Sound**: Map `BombSound` ID to `res://sound/` using pattern `*Sound{ID}.mp3`.

### Step 3: Godot Integration & Setup
To implement the tester in your current Godot environment (as defined in `migrate_target_overview.md`):

#### 1. File Placement
- Copy `MapTestWithBallInfo.cs` into the project root (alongside `DDTankMap.cs`).
- Ensure the `res://sound/` directory exists and contains the decompiled `.mp3` files.

#### 2. Scene Configuration (`node_2d.tscn`)
- **Node Selection**: Select the `MapTester` node in the Scene tree.
- **Script Swap**: Detach the generic `MapTester.cs` and attach the new `MapTestWithBallInfo.cs`.
- **Inspector Setup**:
    - **Terrain Sprite Path**: Assign the `TerrainSprite` node.
    - **Map Bridge Path**: Assign the `DDTankMap` node.
    - **Bomb Folder**: Set to `res://bomb/`.
    - **Sound Folder**: Set to `res://sound/`.

#### 3. C# Project Sync
- Ensure the Godot solution is rebuilt so the new script is recognized by the .NET assembly.

### Step 4: Verification
- Verify that selecting ID `1` (Normal Bomb) creates the correct standard crater and plays the standard explosion sound.
- Verify that selecting a "Special" ID (e.g., a thematic event bomb) creates its unique shape (e.g., a heart or star).
- Ensure the bitmask alignment remains perfect even with irregular shapes.
