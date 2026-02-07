# Legacy Terrain: Bitmask Destruction

DDTank uses a high-performance bitmask system for terrain, allowing for pixel-perfect destruction without the overhead of complex polygon clipping.

## 1. Data Structure (`Tile.cs`)
The map is represented by the `Tile` class (`Game.Logic/Phy/Maps/Tile.cs`).

- **Internal Storage**: `byte[] _data`.
- **Packing**: 8 pixels per byte.
- **Bit Logic**: `1 = Solid`, `0 = Empty`.
- **Alignment**: Bits are stored in Big-Endian order within the byte (the most significant bit is the leftmost pixel).
  - Pixel 0 of a byte: `(byte)(flag << 7)`
  - Pixel 7 of a byte: `(byte)(flag << 0)`

## 2. The Dig Operation
Destruction is achieved by "subtracting" a weapon's explosion mask from the map's mask.

### Bitwise Subtract (`Remove` method)
To destroy terrain at `(x, y)` using a mask `tile`:
1. Calculate the byte offset in the map.
2. Align the mask bits with the map's byte boundaries (requires bit-shifting).
3. Apply `map_byte &= ~mask_byte`.

### Mask Loading
Masks are loaded from `.bomb` files or generated from Bitmaps:
```csharp
// Alpha threshold for solid terrain
byte flag = (byte)(bitmap.GetPixel(i, j).A <= 100 ? 0 : 1);
```

## 3. Collision Detection
- **`IsEmpty(x, y)`**: Checks if a specific pixel is empty.
  ```csharp
  byte flag = (byte)(0x01 << (7 - x % 8));
  return (_data[y * _bw + x / 8] & flag) == 0;
  ```
- **`FindNotEmptyPoint`**: Used by projectiles to find the exact impact point along a trajectory.

## 4. Godot Migration Notes
- **Texture Update**: When `Dig` is called, the C# logic updates the `_data` array. The Godot client must then update the corresponding `ImageTexture`.
- **Optimization**: For large maps, consider splitting the `Tile` into chunks to avoid re-uploading the entire map texture to the GPU on every explosion.
