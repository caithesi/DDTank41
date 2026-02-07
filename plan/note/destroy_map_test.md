# Destructible Terrain implementation Notes (Godot 4.x)

When implementing destructible terrain by manipulating a `Sprite2D`'s texture via `Image.SetPixel()`, several critical steps must be followed to ensure transparency works and coordinates align correctly.

## 1. Image Format and Compression
By default, Godot imports textures with VRAM compression, which makes them read-only for `SetPixel()`. Even if uncompressed, they might lack an Alpha channel.

**Required Steps in Code:**
```csharp
Image image = sprite.Texture.GetImage();

// 1. Decompress if necessary
if (image.IsCompressed()) {
    image.Decompress();
}

// 2. Convert to a format that supports transparency (RGBA8)
image.Convert(Image.Format.Rgba8);

// 3. Create a dynamic ImageTexture
var texture = ImageTexture.CreateFromImage(image);
sprite.Texture = texture;
```

## 2. Visual Synchronization
After modifying the `Image` object with `SetPixel()`, you must push those changes back to the GPU.
- Use `texture.Update(image)` instead of recreating the texture for better performance.

## 3. Coordinate Mapping (Centering)
Godot's `Sprite2D` has `Centered = true` by default. This makes the local (0,0) coordinate the *center* of the sprite.
- **Recommendation:** Set `Centered = false` on the terrain sprite.
- This ensures that local coordinates (0,0) match pixel coordinates (0,0) at the top-left corner, making bitmask math much simpler.
- If centering is required, you must offset your calculations by `texture_size / 2`.

## 4. Bounds Checking
Always perform bounds checking before calling `SetPixel()` to prevent crashes when digging near the edges of the map:
```csharp
if (targetX >= 0 && targetX < image.GetWidth() && 
    targetY >= 0 && targetY < image.GetHeight()) {
    image.SetPixel(targetX, targetY, Colors.Transparent);
}
```