# Terrain Destruction Architecture

This document details the "Dig" system used in DDTank to handle real-time destructible terrain, ensuring mathematical consistency between the C# server and the Godot/Flash client.

## 1. What It Is
The terrain system uses a **Binary Bitmask** approach rather than standard physics engines. The map is represented as an array of bytes where each bit corresponds to one pixel (1 = Solid, 0 = Empty).

- **Pre-baked Hole Shapes**: Unlike many games that use procedural circles, DDTank uses pre-calculated bitmasks for every weapon. These are stored in `.bomb` files.
- **Multi-Layer Logic**: The map consists of multiple "Tiles" (layers). Each layer has a `digable` flag that determines if it can be modified by explosions.

## 2. Why It Matters
- **Determinism**: By using bitwise operations on a fixed-size grid, the server and client arrive at the exact same terrain state without floating-point errors.
- **Visual Fidelity**: Pre-baked masks allow weapons to have unique, jagged, or thematic explosion craters (e.g., a "love" bomb creating a heart-shaped hole).
- **Gameplay Depth**: Indestructible layers (`digable: false`) allow designers to create "safe zones" or "anchors" that players cannot be "dug out" from, even if the surrounding ground is destroyed.

## 3. Origin and Implementation
The logic is ported from the original C# source:
- **Core Logic**: `DDTank.Shared/Tile.cs` and `Game.Logic/Phy/Maps/Tile.cs`.
- **Managers**: `BallMgr.cs` handles loading the `.bomb` masks; `MapMgr.cs` handles loading the `fore.map` (destructible) and `dead.map` (indestructible) layers.
- **Data Source**: Binary files are generated from PNGs using a threshold of `Alpha > 100` (approx 40% opacity) to determine if a pixel is solid.

## 4. Implementation Guidelines (Godot)

To migrate this system to Godot while maintaining the dynamic nature of weapon-specific holes, follow these detailed steps:

### Step 1: Asset Pipeline (.bomb Files)
The original game uses `.bomb` files for weapon-specific craters. These files contain a header and a raw bitmask representing the "solid" pixels of the explosion shape.

#### Binary Structure
The file is a simple binary blob:
| Offset | Type | Description |
| :--- | :--- | :--- |
| 0x00 | `Int32` | **Width**: The width of the mask in pixels. |
| 0x04 | `Int32` | **Height**: The height of the mask in pixels. |
| 0x08 | `byte[]` | **MaskData**: The bitmask payload. |

#### Data Packing & Alignment
- **Stride (Row Width)**: The number of bytes per row is calculated as `(Width / 8) + 1`. This extra byte padding is often used in the legacy code to simplify bit-shifting operations during "Dig" calculations.
- **Payload Size**: Total bytes = `Stride * Height`.
- **Bit Order**: Big-endian bit order within each byte. The most significant bit (MSB, `0x80`) represents the leftmost pixel in a group of 8.
    - Example: `1000 0000` (`0x80`) means only the 1st pixel is solid.
    - Example: `0000 0001` (`0x01`) means only the 8th pixel is solid.

#### Bit-Shifting Alignment
Because the `Dig` impact point `(x, y)` is unlikely to be aligned to an 8-pixel boundary, the `Tile` class must perform bit-shifting to align the `holeMask` with the map's byte grid.
- **The Padding Secret**: The `(Width / 8) + 1` stride ensures there is always a "overflow" byte. When shifting a byte by `n` bits to the right, the bits that shift out are stored and applied to the *next* byte in the row during the logical `AND NOT` (Remove) or `OR` (Add) operations. This prevents data loss and avoids complex boundary checks within the inner loops.

#### Tile Class Integration
Ensure your `Tile` class implements a constructor that matches this binary format:
```csharp
public Tile(byte[] data, int width, int height, bool digable) {
    this._data = data;
    this._width = width;
    this._height = height;
    this._bw = (width / 8) + 1; // Stride
    this._bh = height;
}
```
In Godot, when loading these files, use `FileAccess.Get32()` for the header and `FileAccess.GetBuffer()` for the payload.

### Step 2: Weapon-to-Mask Mapping
Holes are not fixed because each weapon (`BallID`) has its own mask.
- **Manager**: Create a `BallManager` node in Godot.
- **Logic**: When a projectile packet arrives from the server, it contains a `BallID`. Use this ID to fetch the corresponding `Tile` mask from your `BallManager`.
- **Variety**: Some weapons create large circular holes, others create "V" shapes or unique thematic shapes (hearts, stars).

### Step 3: The Multi-Layer "Dig" Loop
When an explosion occurs, the `DDTankMap` must iterate through all active tiles. 

- **Implementation Reference**: See [plan/example/DDTankMap.cs](../plan/example/DDTankMap.cs) for a complete example of how to bridge shared logic with Godot visuals.
- **Key Logic**:
    1.  **Update Logic**: Call `Tile.Dig()` on the logical bitmask (from `DDTank.Shared`).
    2.  **Sync Visuals**: Translate the bitmask changes to the Godot `ImageTexture` using the pixel manipulation logic shown in the example.

### Step 4: Visual Synchronization (Performance)
Updating a 2048x1024 texture every time someone shoots is expensive. 
- **Sub-Region Updates**: Only update the rectangular area of the `ImageTexture` affected by the `holeMask`.
- **Direct Buffer Manipulation**: Use `Image.GetData()` to get the raw byte array, manipulate the alpha channel bits based on the bitmask, and then use `Image.SetData()` or `Texture2D.UpdateRegion()`.
- **Alternative (Shaders)**: Instead of modifying the texture, pass the "Dig" coordinates and mask to a shader that performs the alpha masking in real-time. This is significantly faster but requires managing a "Destruction Map" texture.

### Step 5: Handling "Borders"
The `Dig` method accepts a `border` tile. 
- **Purpose**: Original DDTank uses this to add a jagged or "burnt" edge to the hole. 
- **Implementation**: The logic is `Map.Dig(cx, cy, surface, border)`. The `surface` removes terrain, while the `border` adds a specialized "edge" layer. If you only want simple destruction, you can pass `null` for the border.

### Visual Guide: Dig Logic
1.  **Server calculates impact**: Sends Packet 91 (Combat Script) with impact `(x, y)` and `BallID`.
2.  **Client fetches mask**: `BallManager.FindTile(BallID)` returns the specific `.bomb` bitmask.
3.  **Client modifies logic**: The bitmask is applied to the bit-array at the impact point.
4.  **Client modifies visuals**: The pixels in the Godot Sprite corresponding to the bitmask are set to `Color(0,0,0,0)` (Transparent).
