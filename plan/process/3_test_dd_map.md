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

---

## 2. Testing & Debugging Guide

This guide provides a step-by-step procedure to verify the "Dig" implementation in Godot using the `DDTankMap.cs` example.

### Prerequisites
- [ ] **Godot 4.x (.NET Edition)** installed.
- [ ] **Shared Library**: Ensure `DDTank.Shared/Tile.cs` is included in your Godot project's solution (C#).
- [ ] **Example File**: Ensure `plan/example/DDTankMap.cs` is placed in your project's script folder.

---

### Step 1: Scene Construction
1.  Create a new **2D Scene**.
2.  Add a `Node2D` as the root and rename it to `TestMap`.
3.  Attach a new C# script to the root node (e.g., `MapTester.cs`).
4.  Add a `Sprite2D` as a child of `TestMap`:
    -   Rename it to `TerrainSprite`.
    -   Assign a large PNG (e.g., 1000x600) as its texture. This will be your "Destructible Terrain".
    -   Set its `Centered` property to `false` and position it at `(0,0)`.

---

### Step 2: Prepare a Mask (Mock or Real)
You can either create a procedural mask for quick testing or load a real `.bomb` file from the original game assets.

#### Option A: Procedural Circle (Mock)
For testing purposes, you can manually create a small circular `Tile` to act as a "Bomb".

```csharp
public Tile CreateCircleMask(int radius) {
    int size = radius * 2;
    Tile mask = new Tile(size, size, true); 
    for (int y = 0; y < size; y++) {
        for (int x = 0; x < size; x++) {
            double dist = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2));
            if (dist < radius) {
                // Set the bit to solid in the mask
                int idx = y * (size / 8 + 1) + x / 8;
                mask.Data[idx] |= (byte)(0x01 << (7 - x % 8));
            }
        }
    }
    return mask;
}
```

#### Option B: Real Binary `.bomb` File
If you have extracted assets, you can load the exact crater shape used in the original game.

```csharp
// Inside MapTester.cs
// Load a real bomb mask (e.g., from a 'bomb' folder in your Godot project)
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

### Step 3: Initialization
In your `MapTester.cs` script, initialize the `DDTankMap` bridge.

```csharp
public partial class MapTester : Node2D {
    private DDTankMap _mapBridge;
    private Tile _bombMask;

    public override void _Ready() {
        var sprite = GetNode<Sprite2D>("TerrainSprite");
        
        // 1. Initialize logical terrain (initially all solid)
        var terrainLogic = new Tile((int)sprite.Texture.GetWidth(), (int)sprite.Texture.GetHeight(), true);
        
        // 2. Setup the bridge
        _mapBridge = new DDTankMap();
        AddChild(_mapBridge); // Add as child so it can process
        _mapBridge.Initialize(terrainLogic, sprite);

        // 3. Prepare our test hole
        _bombMask = CreateCircleMask(30); 
    }
}
```

---

### Step 4: Implementing Interaction
Add a mouse click handler to trigger the destruction at the click position.

```csharp
public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
        // Trigger the Dig!
        Vector2 pos = GetLocalMousePosition();
        _mapBridge.Dig((int)pos.X, (int)pos.Y, _bombMask, null);
    }
}
```

---

### Step 5: Verification Checklist
Run the scene and perform the following checks:

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