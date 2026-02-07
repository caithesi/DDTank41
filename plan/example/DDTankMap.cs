using Godot;
using DDTank.Shared;
using System;

namespace DDTank.Godot.Example
{
    /// <summary>
    /// Example implementation of a Godot Map node that handles destructible terrain.
    /// This class bridges the Shared Tile logic with Godot's visual rendering by
    /// synchronizing a logical bitmask with a Godot ImageTexture.
    /// </summary>
    public partial class DDTankMap : Node2D
    {
        private Tile _terrainLogic;    // Shared logic bitmask (Official Physics Authority)
        private Sprite2D _view;        // Godot visual representation
        private Image _image;          // The raw Godot image used for pixel manipulation
        private ImageTexture _texture; // The texture assigned to the Sprite2D

        /// <summary>
        /// Initializes the map bridge with the logical terrain and a target sprite.
        /// Converts the sprite's texture into a modifiable ImageTexture.
        /// </summary>
        /// <param name="initialTerrain">The logical bitmask representing the map.</param>
        /// <param name="sprite">The Godot Sprite2D that displays the terrain.</param>
        public void Initialize(Tile initialTerrain, Sprite2D sprite)
        {
            _terrainLogic = initialTerrain;
            _view = sprite;
            
            // Get raw image data from the existing texture to allow per-pixel modification.
            _image = _view.Texture.GetImage();

            // 1. Decompress if necessary (Godot imports textures with VRAM compression by default)
            if (_image.IsCompressed())
            {
                _image.Decompress();
            }

            // 2. Convert to a format that supports transparency (RGBA8)
            _image.Convert(Image.Format.Rgba8);
            
            // Create a new ImageTexture that can be updated dynamically.
            _texture = ImageTexture.CreateFromImage(_image);
            
            // Replace the static texture with our dynamic one.
            _view.Texture = _texture;
        }

        /// <summary>
        /// Executes the "Dig" operation on both logical (Shared) and visual (Godot) layers.
        /// </summary>
        /// <param name="x">Impact center X coordinate in pixels.</param>
        /// <param name="y">Impact center Y coordinate in pixels.</param>
        /// <param name="holeMask">The bitmask of the crater/hole to remove.</param>
        /// <param name="borderMask">Optional bitmask for a decorative border/edge.</param>
        public void Dig(int x, int y, Tile holeMask, Tile borderMask)
        {
            // 1. Update the logical bitmasks (Shared Logic).
            // This ensures that future physics checks (e.g., player movement) reflect the hole.
            _terrainLogic.Dig(x, y, holeMask, borderMask);

            // 2. Sync Visuals (Godot).
            // Calculate the top-left starting position of the modification area.
            int startX = x - (holeMask.Width / 2);
            int startY = y - (holeMask.Height / 2);
            
            // Apply the visual transparency update.
            UpdateMapTexture(startX, startY, holeMask);
        }

        /// <summary>
        /// Synchronizes the Godot ImageTexture by making pixels transparent where the mask is solid.
        /// </summary>
        /// <param name="startX">Top-left X of the update region.</param>
        /// <param name="startY">Top-left Y of the update region.</param>
        /// <param name="mask">The mask representing the area to be cleared.</param>
        private void UpdateMapTexture(int startX, int startY, Tile mask)
        {
            // Iterate through every pixel in the hole mask.
            for (int dy = 0; dy < mask.Height; dy++)
            {
                for (int dx = 0; dx < mask.Width; dx++)
                {
                    // If the mask bit is 1 (solid), it means we remove this pixel from the terrain.
                    if (!mask.IsEmpty(dx, dy))
                    {
                        int targetX = startX + dx;
                        int targetY = startY + dy;

                        // Bounds check against the map dimensions to prevent crashes.
                        if (targetX >= 0 && targetX < _terrainLogic.Width && 
                            targetY >= 0 && targetY < _terrainLogic.Height)
                        {
                            // Set pixel to fully transparent in the Godot Image.
                            _image.SetPixel(targetX, targetY, Colors.Transparent);
                        }
                    }
                }
            }

            // Push the modified image data back to the GPU texture.
            // Note: Update() is more efficient than recreating the texture.
            _texture.Update((_image));
        }
    }
}