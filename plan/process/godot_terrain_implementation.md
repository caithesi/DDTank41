# Godot Destructible Terrain Implementation Guide

This guide explains how to connect the `DDTank.Shared` logic to Godot's visual system to create destructible terrain.

## Visual Strategy: Pixel-to-Bitmask Sync
Since `DDTank.Shared.Tile` uses a bitmask (1 byte per 8 pixels), we will maintain a 1:1 synchronization between a Godot `ImageTexture` and the `Tile` logic.

### 1. The Godot Terrain Node
Create a `Node2D` (e.g., `DDTankMap`) and attach a `Sprite2D` as a child. This child will hold the visual representation of the ground.

### 2. Implementation Script (C#)
Copy this script to your Godot project. It handles the conversion of a Godot texture into a `Tile` and updates the visuals when a hole is dug.

```csharp
using Godot;
using System;
using DDTank.Shared;

public partial class DDTankMap : Node2D, IMap
{
    [Export] public Texture2D GroundTexture;
    
    private DDTank.Shared.Map _simMap;
    private Image _groundImage;
    private ImageTexture _visualTexture;
    private Sprite2D _displaySprite;

    // IMap implementation
    public float Gravity => 9.8f;
    public float Wind => 0.0f;
    public float AirResistance => 0.1f;

    public override void _Ready()
    {
        _displaySprite = GetNode<Sprite2D>("Sprite2D");
        InitializeMap();
    }

    private void InitializeMap()
    {
        _groundImage = GroundTexture.GetImage();
        int width = _groundImage.GetWidth();
        int height = _groundImage.GetHeight();

        // 1. Create the bitmask data from the Godot Image
        // Bit 1 = Solid, Bit 0 = Empty (Alpha < 128)
        byte[] maskData = new byte[(width / 8 + 1) * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = _groundImage.GetPixel(x, y);
                if (pixel.A > 0.5f) // Threshold for solid ground
                {
                    int index = y * (width / 8 + 1) + (x / 8);
                    maskData[index] |= (byte)(1 << (7 - (x % 8)));
                }
            }
        }

        // 2. Initialize Shared Logic
        DDTank.Shared.Tile groundTile = new DDTank.Shared.Tile(maskData, width, height, true);
        _simMap = new DDTank.Shared.Map(groundTile, null);

        // 3. Setup Visual Texture
        _visualTexture = ImageTexture.CreateFromImage(_groundImage);
        _displaySprite.Texture = _visualTexture;
    }

    // Called by the BombObject when it hits something
    public void Dig(int cx, int cy, DDTank.Shared.Tile surface, DDTank.Shared.Tile border)
    {
        // 1. Update Logic (Bitmask)
        _simMap.Dig(cx, cy, surface, border);

        // 2. Update Visuals (Pixels)
        // We iterate over the area of the 'surface' (the hole) and clear pixels
        int xStart = cx - (surface.Width / 2);
        int yStart = cy - (surface.Height / 2);

        for (int y = 0; y < surface.Height; y++)
        {
            for (int x = 0; x < surface.Width; x++)
            {
                int worldX = xStart + x;
                int worldY = yStart + y;
                
                // If logic says it's empty, make it transparent in Godot
                if (_simMap.Ground.IsEmpty(worldX, worldY))
                {
                    if (worldX >= 0 && worldX < _simMap.Bound.Width && 
                        worldY >= 0 && worldY < _simMap.Bound.Height)
                    {
                        _groundImage.SetPixel(worldX, worldY, Color.FromHtml("#00000000"));
                    }
                }
            }
        }

        // 3. Push pixel changes back to GPU
        _visualTexture.Update(_groundImage);
    }

    public bool IsRectangleEmpty(DDTank.Shared.Rectangle rect) => _simMap.IsRectangleEmpty(rect);
    public bool IsOutMap(int x, int y) => _simMap.IsOutMap(x, y);
    public DDTank.Shared.Physics[] FindPhysicalObjects(DDTank.Shared.Rectangle rect, DDTank.Shared.Physics except) 
        => _simMap.FindPhysicalObjects(rect, except);
}
```

## Performance Tips
1.  **Partial Updates**: In the `Dig` method, we only update the pixels inside the bounding box of the hole. This prevents full-texture reloads which would be slow for large maps (e.g., 2000x1000).
2.  **Thread Safety**: If you use a multi-threaded server/backend, ensure the `Dig` and `Update` calls happen on the Main Thread (Godot UI thread).
3.  **Collision Parity**: Because the `IMap.IsRectangleEmpty` call is directly linked to the bitmask in `_simMap`, your `BombObject` will always "see" the holes exactly as they are rendered.

## Next Steps
- Implement `SimpleBomb.cs` (or a Godot wrapper) that calls `map.Dig()` on impact.
- Extract "Hole" textures (the circular masks) from the original assets to pass as the `surface` parameter in `Dig`.
