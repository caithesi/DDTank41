# Process 3: Destructible Terrain & Godot Integration

## 1. Summary of Progress (2026-02-06)

Following the infrastructure setup, we focused on the core gameplay mechanic: **Destructible Terrain**. We successfully bridged the server's bitmask-based logic with Godot's pixel-based rendering.

### Logic Extraction (DDTank.Shared)
We completed the "Backend" logic for terrain, ensuring 100% mathematical parity with the original game.
- **`Tile.cs` Ported**: Ported the complex bit-shifting logic that manages the 8-pixels-per-byte mask. This includes the `Dig`, `Add`, and `Remove` operations used for explosions.
- **`Map.cs` Ported**: Created a high-level container that manages ground and indestructible layers, implementing the `IMap` interface.
- **`IMap.cs` Refined**: Added the `Dig` method to the interface, allowing projectiles to trigger terrain changes without knowing Godot-specific details.

### Godot Implementation ("The Bridge")
We designed a synchronization strategy between Godot's `ImageTexture` and the shared logic's bitmask.
- **`DDTankMap.cs`**: Created a custom Godot C# script that translates a PNG texture into a `Tile` bitmask on startup and synchronizes pixel transparency in real-time.
- **`IMap` Implementation**: The Godot-side node now acts as the official world authority for physics objects (`BombObject`).

**Critical Implementation Details:**
1.  **Image Preparation**: Godot imports textures with VRAM compression. To modify pixels via code, the image must be decompressed using `image.Decompress()` and converted to `Image.Format.Rgba8` to ensure an alpha channel exists.
2.  **Performance**: Use `texture.Update(image)` instead of creating a new texture every frame to minimize GPU overhead.
3.  **Coordinate Alignment**: Set `Centered = false` on the terrain `Sprite2D` to align local (0,0) with pixel (0,0), simplifying bitmask-to-visual mapping.

---

## 2. Testing & Debugging Guide

This guide provides a step-by-step procedure to verify the "Dig" implementation in Godot using the `DDTankMap.cs` example.

### Prerequisites
- [ ] **Godot 4.x (.NET Edition)** installed.
- [ ] **Shared Library**: Ensure `DDTank.Shared/Tile.cs` is included in your Godot project's solution (C#).
- [ ] **Example File**: Ensure `plan/example/DDTankMap.cs` is placed in your project's script folder.

---

### Step 1: Scene Construction (Parallel Structure)
To verify the destruction logic, your Godot **Scene** tab should look like this. This "parallel" structure keeps the test logic separate from the map components.

**Editor Setup (Parallel):**
```text
Main (Node2D)                   <-- Scene Root
├── MapTester (Node2D)          <-- Attach MapTest.cs
└── DDTankMap (Node2D)          <-- Attach DDTankMap.cs
    └── TerrainSprite (Sprite2D) <-- Your map image
```

**Node Configuration:**
1.  **MapTester (Logic)**:
    -   **Script**: `plan/example/tester/MapTest.cs`.
    -   **Terrain Sprite Path**: Point to `../DDTankMap/TerrainSprite`.
    -   **Map Bridge Path**: Point to `../DDTankMap`.
2.  **DDTankMap (The Bridge)**:
    -   **Script**: `plan/example/DDTankMap.cs`.
3.  **TerrainSprite (Visuals)**:
    -   **Texture**: Load a large PNG.
    -   **Centered**: **OFF** (Uncheck this).
    -   **Position**: `(0, 0)`.

---

### Step 2: Understanding the File Roles
To run this test, your project must include these three components:

| File | Role | Case for Use |
| :--- | :--- | :--- |
| **`DDTank.Shared/`** | **Core Logic** | The "Brain". Handles the 1-bit-per-pixel math. Used for physics parity with the server. |
| **`DDTankMap.cs`** | **The Bridge** | The "Renderer". Synchronizes the C# bitmask with the Godot `ImageTexture`. |
| **`MapTest.cs`** | **The Tester** | The "Interface". Captures clicks, creates test masks, and triggers the bridge. |

---

### Step 3: Interaction & Initialization
The `MapTest.cs` script automates the setup. On `_Ready()`, it initializes the `DDTankMap` bridge and generates a procedural 30px circular "bomb" mask.

**How to trigger a Dig:**
Simply click the Left Mouse Button anywhere on the terrain. The following flow occurs:
1. `MapTest` detects the click and gets local coordinates.
2. It calls `_mapBridge.Dig()`.
3. `DDTankMap` updates the logical bitmask (for physics) and wipes the corresponding pixels in the `Sprite2D` (for visuals).

---

### Advanced: Loading Real Masks (.bomb files)
While `MapTest.cs` creates a circle by default, you can use the original game's binary masks for more accurate testing.

**How to swap in a real mask:**
Update the `_Ready` method in `MapTest.cs` to load a file instead of generating one:
```csharp
// Instead of _bombMask = CreateCircleMask(30);
_bombMask = new Tile("res://assets/bombs/1.bomb", false); 
```

**Recommended Files for Testing:**
| File | Type | Best For |
| :--- | :--- | :--- |
| `1.bomb` | **Standard** | Basic grenade. Perfect for initial logic verification. |
| `6.bomb` | **Large** | High explosive. Best for testing **performance** of texture updates. |
| `3.bomb` | **Irregular** | Specialized weapon. Good for testing non-circular bitmask accuracy. |

*Note: The `Tile(string path, bool digable)` constructor in `DDTank.Shared` handles the binary parsing of the width, height, and bitmask data.*

---

### Step 4: Verification Checklist (What to expect)
Run the scene (**F5**) and check for these behaviors:

1.  **Visual Transparency**: Does clicking on the terrain create a transparent hole?
2.  **Boundary Safety**: Try clicking near the edges of the map. Does the application crash or handle the bounds gracefully?
3.  **Logic Sync**: (Advanced) Add a `GD.Print` inside `DDTankMap.Dig` to verify `_terrainLogic.IsEmpty(x, y)` returns `true` after the dig.
4.  **Performance**: Rapidly click 20-30 times. The update should feel instantaneous. If there is lag, refer to **Step 4 (Shaders)** in `architect/terrain_destruction.md`.

### Troubleshooting
-   **Texture not updating**: Ensure you are using `ImageTexture` and calling `texture.Update(image)`. 
-   **Wrong Position**: Remember that `Tile.Dig` expects the **Center** coordinates of the hole.
-   **Pink/Glitchy Texture**: Ensure the initial PNG has an Alpha channel (RGBA8 format).

---

## 3. Execution & Troubleshooting

### Verification Steps
1.  Press **F5** (Run Project).
2.  **Interaction**: Click anywhere on the ground texture.
3.  **Expected Result**: A circular hole should appear instantly. Multiple clicks should overlap correctly.

### Troubleshooting (Easy Debugging)
- **No holes appear?** 
    - Check the **Output** tab in Godot. If you see "Dug hole at...", the logic is working but the visual isn't.
    - Ensure your PNG has a transparent background (Sky/Background area must be empty).
- **Compilation error?** 
    - Ensure `DDTank.Shared` is built and the DLL is accessible.
- **Hole is offset?** 
    - Ensure the `Sprite2D` child of `DDTankMap` is at position `(0,0)` and its `Centered` property is **Off**.

---

## 4. Roadmap & Next Steps

### Roadmap Adjustment
We decided to skip the legacy Phase 4 (Binary Playback Script) and move directly to **Phase 5: Asset Extraction**. It is more efficient to have the real visual assets (sprites, animations) and the terrain system ready first, then build the modernized playback "glue" later using Godot's native Timeline/Tween features.

### Next Steps
- **Phase 5: Asset Extraction**: Extracting "Hole" shapes, ground textures, and player sprites from the original SWFs to replace the procedural test assets.